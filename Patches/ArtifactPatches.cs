using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Nanoray.PluginManager;
using Nickel;
using TheJazMaster.MoreDifficulties.Cards;

namespace TheJazMaster.MoreDifficulties;

[HarmonyPatch]
internal static class ArtifactPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Artifact), nameof(Artifact.GetLocName))]
	private static void Artifact_GetLocName_Postfix(Artifact __instance, ref string __result)
	{
		if (__instance is not HARDMODE hardmode || hardmode.difficulty != ModEntry.Easy)
			return;
		__result = Loc.T(I18n.easyNameLoc, I18n.easyNameLocEn);
	}
	
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Artifact), nameof(Artifact.OnPlayerDeckShuffle))]
	private static void Artifact_OnPlayerDeckShuffle_Postfix(Artifact __instance, State state, Combat combat) {
        if (__instance is HARDMODE hardmode && hardmode.difficulty >= ModEntry.Difficulty1 && state.deck.Count > 0)
            combat.QueueImmediate(new AAddCard {
                amount = 1,
                card = new Fatigue(),
				artifactPulse = __instance.Key()
            });
    }
}
