using HarmonyLib;

namespace TheJazMaster.MoreDifficulties.AIPatches;

[HarmonyPatch]
public static class StoneGuyPatch {
	
	[HarmonyPatch(typeof(StoneGuy), nameof(StoneGuy.BuildShipForSelf))]
	[HarmonyPostfix]
	private static void BuildShipForSelf_Postfix(StoneGuy __instance, Ship __result, State s) {
		if (AIUtils.AreEnemiesEvenHarder(s)) {
			__result.hullMax += 2;
			__result.hull += 2;
		}
	}
}