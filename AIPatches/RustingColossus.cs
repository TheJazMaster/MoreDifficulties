
using CobaltCoreModding.Definitions;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using static System.Reflection.BindingFlags;

namespace TheJazMaster.MoreDifficulties.AIPatches;

[HarmonyPatch]
public static class RustingColossusPatch {
	
	[HarmonyPatch(typeof(RustingColossus), nameof(RustingColossus.BuildShipForSelf))]
	[HarmonyPostfix]
	private static void BuildShipForSelf_Postfix(RustingColossus __instance, Ship __result, State s) {
		if (s.GetDifficulty() >= Manifest.Difficulty2) {
			__result.hullMax += 20;
			__result.hull += 20;
		}
	}
}