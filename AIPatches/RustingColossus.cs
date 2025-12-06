using HarmonyLib;

namespace TheJazMaster.MoreDifficulties.AIPatches;

[HarmonyPatch]
public static class RustingColossusPatch {
	
	[HarmonyPatch(typeof(RustingColossus), nameof(RustingColossus.BuildShipForSelf))]
	[HarmonyPostfix]
	private static void BuildShipForSelf_Postfix(RustingColossus __instance, Ship __result, State s) {
		if (AIUtils.AreEnemiesEvenHarder(s)) {
			__result.hullMax += 20;
			__result.hull += 20;
		}
	}
}