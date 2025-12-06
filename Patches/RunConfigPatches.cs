using HarmonyLib;

namespace TheJazMaster.MoreDifficulties;

[HarmonyPatch]
internal static class RunConfigPatches
{
	private static ModEntry Instance => ModEntry.Instance;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RunConfig), nameof(RunConfig.SetDifficulty))]
	private static void RunConfig_SetDifficulty_Postfix(RunConfig __instance, int x)
		=> __instance.difficulty = x;
}
