using HarmonyLib;
using Nanoray.Shrike.Harmony;
using Nanoray.Shrike;
using System.Reflection.Emit;
using System.Reflection;
using Nickel;
using System.Collections.Generic;
using System;
using System.Linq;

namespace TheJazMaster.MoreDifficulties;

[HarmonyPatch]
internal static class RunSummaryRoutePatches
{
	private static ModEntry Instance => ModEntry.Instance;
	private static IModData ModData => Instance.Helper.ModData;

	[HarmonyTranspiler]
	[HarmonyPatch(typeof(RunSummaryRoute), nameof(RunSummaryRoute.Render))]
	private static IEnumerable<CodeInstruction> RunSummaryRoute_Render_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
	{
		var elem = new SequenceBlockMatcher<CodeInstruction>(instructions)
			.Find(
				ILMatches.Ldsfld("difficulties"),
				ILMatches.Ldarg(0),
				ILMatches.Ldfld("runSummary"),
				ILMatches.Ldfld("difficulty"),
				ILMatches.Call("ElementAtOrDefault")
			)
			.PointerMatcher(SequenceMatcherRelativeElement.Last)
			.Replace(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(RunSummaryRoutePatches), nameof(RunSummaryRoute_Render_Transpiler_GetDifficulty))))
			.AllElements();

		return new SequenceBlockMatcher<CodeInstruction>(elem)
			.Find(
				ILMatches.Newobj(typeof(Character).GetConstructor(Array.Empty<Type>())!)
			)
			.Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.IncludingInsertion, [
				new(OpCodes.Dup),
				new(OpCodes.Ldarg_0),
				new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(RunSummaryRoutePatches), nameof(RunSummaryRoute_Render_Transpiler_SetRunSummaryRoute)))
			])
			.AllElements();
	}

	private static void RunSummaryRoute_Render_Transpiler_SetRunSummaryRoute(Character character, RunSummaryRoute route)
	{
		ModData.SetModData(character, "runSummaryRoute", route);
	}

	private static NewRunOptions.DifficultyLevel RunSummaryRoute_Render_Transpiler_GetDifficulty(List<NewRunOptions.DifficultyLevel> difficulties, int level)
		=> difficulties.FirstOrDefault(d => d.level == level) ?? difficulties.FirstOrDefault(d => d.level == 0) ?? difficulties[0];
}
