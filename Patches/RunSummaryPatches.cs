using HarmonyLib;
using Nanoray.Shrike.Harmony;
using Nanoray.Shrike;
using System.Reflection.Emit;
using System.Reflection;
using Nickel;
using System.Collections.Generic;
using System;

namespace TheJazMaster.MoreDifficulties;

[HarmonyPatch]
internal static class RunSummaryPatches
{
	private static ModEntry Instance => ModEntry.Instance;
	private static IModData ModData => Instance.Helper.ModData;

	[HarmonyTranspiler]
	[HarmonyPatch(typeof(RunSummary), nameof(RunSummary.SaveFromState))]
	private static IEnumerable<CodeInstruction> RunSummary_SaveFromState_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
	{
		return new SequenceBlockMatcher<CodeInstruction>(instructions)
			.Find(
				ILMatches.Newobj(typeof(RunSummary).GetConstructor(Array.Empty<Type>())!)
			)
			.EncompassUntil(SequenceMatcherPastBoundsDirection.After, ILMatches.Stfld("decks"))
			.Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.IncludingInsertion, [
				new(OpCodes.Dup),
				new(OpCodes.Ldarg_0),
				new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(RunSummaryPatches), nameof(RunSummary_SaveFromState_Transpiler_SetModData)))
			])
			.AllElements();
	}
	private static void RunSummary_SaveFromState_Transpiler_SetModData(RunSummary summary, State s)
	{
		foreach (Deck deck in summary.decks) {
			ModData.SetModData(summary, AltStarters.Key(deck), AltStarters.AreAltStartersEnabled(s, deck));
		}
	}
}