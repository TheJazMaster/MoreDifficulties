
using CobaltCoreModding.Definitions;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using static System.Reflection.BindingFlags;

namespace TheJazMaster.MoreDifficulties.AIPatches;

[HarmonyPatch]
public static class StoneGuyPatch {
	
	[HarmonyPatch(typeof(StoneGuy), nameof(StoneGuy.BuildShipForSelf))]
	[HarmonyPostfix]
	private static void BuildShipForSelf_Postfix(StoneGuy __instance, Ship __result, State s) {
		if (s.GetDifficulty() >= 5) {
			__result.hullMax += 3;
			__result.hull += 3;
		}
	}
}