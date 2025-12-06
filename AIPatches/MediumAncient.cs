using HarmonyLib;

namespace TheJazMaster.MoreDifficulties.AIPatches;

[HarmonyPatch]
public static class MediumAncientPatch
{
	[HarmonyPatch(typeof(MediumAncient), nameof(MediumAncient.BuildShipForSelf))]
	[HarmonyPostfix]
	private static void BuildShipForSelf_Postfix(MediumAncient __instance, Ship __result, State s) {
		if (AIUtils.AreEnemiesEvenHarder(s)) {
			__result.shieldMaxBase += 2;
		}
	}
}