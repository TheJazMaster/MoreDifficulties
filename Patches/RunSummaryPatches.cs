using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.Shrike.Harmony;
using Nanoray.Shrike;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace TheJazMaster.MoreDifficulties;

internal static class RunSummaryPatches
{
	private static Manifest Instance => Manifest.Instance;

	public static void Apply(Harmony harmony)
	{
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(RunSummary).GetMethod("SaveFromState", AccessTools.all),
			transpiler: new HarmonyMethod(typeof(RunSummaryPatches).GetMethod("RunSummary_SaveFromState_Transpiler", AccessTools.all))
		);
	}

	private static IEnumerable<CodeInstruction> RunSummary_SaveFromState_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
	{
		return new SequenceBlockMatcher<CodeInstruction>(instructions)
			.Find(
				ILMatches.Newobj(typeof(RunSummary).GetConstructor(Array.Empty<Type>())!)
			)
			.EncompassUntil(SequenceMatcherPastBoundsDirection.After, ILMatches.Stfld("decks"))
			.Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.IncludingInsertion, new List<CodeInstruction> {
				new(OpCodes.Dup),
				new(OpCodes.Ldarg_0),
				new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(RunSummaryPatches), nameof(RunSummary_SaveFromState_Transpiler_SetModData)))
			})
			.AllElements();
	}
	private static void RunSummary_SaveFromState_Transpiler_SetModData(RunSummary summary, State s)
	{
		foreach (Deck deck in summary.decks) {
			Instance.KokoroApi.SetExtensionData(summary, AltStarters.Key(deck), Instance.AltStarters.AreAltStartersEnabled(s, deck));
		}
	}
}