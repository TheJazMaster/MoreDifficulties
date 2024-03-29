﻿using HarmonyLib;
using System.Reflection;
using static System.Reflection.BindingFlags;

namespace TheJazMaster.MoreDifficulties;

internal static class RunConfigPatches
{
	private static Manifest Instance => Manifest.Instance;

	public static void Apply(Harmony harmony)
	{
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(RunConfig).GetMethod("SetDifficulty", AccessTools.all),
			postfix: new HarmonyMethod(typeof(RunConfigPatches).GetMethod("RunConfig_SetDifficulty_Postfix", AccessTools.all))
		);
	}

	private static void RunConfig_SetDifficulty_Postfix(RunConfig __instance, int x)
		=> __instance.difficulty = x;
}
