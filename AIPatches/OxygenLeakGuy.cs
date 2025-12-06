using HarmonyLib;

namespace TheJazMaster.MoreDifficulties.AIPatches;

/*
	Armored missile bay
*/

[HarmonyPatch]
public static class OxygenLeakGuyPatch {
	private static ModEntry Instance => ModEntry.Instance;

	[HarmonyPatch(typeof(OxygenLeakGuy), nameof(OxygenLeakGuy.BuildShipForSelf))]
	[HarmonyPostfix]
	private static void BuildShipForSelf_Postfix(OxygenLeakGuy __instance, Ship __result, State s) {
		if (AIUtils.AreEnemiesEvenHarder(s))
			__result.parts[1].damageModifier = PDamMod.armor;
	}
}