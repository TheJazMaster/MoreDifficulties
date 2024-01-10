using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.Shrike.Harmony;
using Nanoray.Shrike;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System;
using System.Linq;
using static System.Reflection.BindingFlags;

namespace TheJazMaster.MoreDifficulties;

internal static class RunSummaryRoutePatches
{
	private static Manifest Instance => Manifest.Instance;

	public static void Apply(Harmony harmony)
	{
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(RunSummaryRoute).GetMethod("Render", AccessTools.all),
			transpiler: new HarmonyMethod(typeof(RunSummaryRoutePatches).GetMethod("RunSummaryRoute_Render_Transpiler", AccessTools.all))
		);
	}

	private static IEnumerable<CodeInstruction> RunSummaryRoute_Render_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
	{
		try
		{
			return new SequenceBlockMatcher<CodeInstruction>(instructions)
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
		}
		catch (Exception ex)
		{
			Instance.Logger!.LogError("Could not patch method {Method} - {Mod} probably won't work.\nReason: {Exception}", originalMethod, Instance.Name, ex);
			return instructions;
		}
	}

	private static NewRunOptions.DifficultyLevel RunSummaryRoute_Render_Transpiler_GetDifficulty(List<NewRunOptions.DifficultyLevel> difficulties, int level)
		=> difficulties.FirstOrDefault(d => d.level == level) ?? difficulties.FirstOrDefault(d => d.level == 0) ?? difficulties[0];
}
