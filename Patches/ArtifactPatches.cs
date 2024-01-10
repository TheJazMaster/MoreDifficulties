using HarmonyLib;
using TheJazMaster.MoreDifficulties.Cards;
using TheJazMaster.MoreDifficulties.Actions;
using System.Reflection;
using static System.Reflection.BindingFlags;

namespace TheJazMaster.MoreDifficulties;

internal static class ArtifactPatches
{
	private static Manifest Instance => Manifest.Instance;

	public static void Apply(Harmony harmony)
	{
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(Artifact).GetMethod("GetLocName", AccessTools.all),
			postfix: new HarmonyMethod(typeof(ArtifactPatches).GetMethod("Artifact_GetLocName_Postfix", AccessTools.all))
		);
        harmony.TryPatch(
            logger: Instance.Logger!,
            original: typeof(Artifact).GetMethod("OnPlayerDeckShuffle", AccessTools.all),
            postfix: new HarmonyMethod(typeof(ArtifactPatches).GetMethod("Artifact_OnPlayerDeckShuffle_Postfix", AccessTools.all))
        );
        harmony.TryPatch(
            logger: Instance.Logger!,
            original: typeof(InitialBooster).GetMethod("ModifyBaseDamage", AccessTools.all),
            postfix: new HarmonyMethod(typeof(ArtifactPatches).GetMethod("InitialBooster_ModifyBaseDamage_Postfix", AccessTools.all))
        );
	}

    private static void Artifact_OnPlayerDeckShuffle_Postfix(Artifact __instance, State state, Combat combat) {
        if (__instance is HARDMODE hardmode && hardmode.difficulty >= Manifest.Difficulty1 && state.deck.Count > 0)
            combat.QueueImmediate(new AEnergyImportant {
                changeAmount = -1,
                timer = 0.7,
                pulseAmount = 1,
				artifactPulse = __instance.Key()
            });
    }

    private static void InitialBooster_ModifyBaseDamage_Postfix(ref int __result, int baseDamage, Card? card, State state, Combat? combat, bool fromPlayer) {
		if (fromPlayer && card != null && card is BasicOffences)
		{
            __result += 1;
		}
    }

	private static void Artifact_GetLocName_Postfix(Artifact __instance, ref string __result)
	{
		if (__instance is not HARDMODE hardmode || hardmode.difficulty != Manifest.Easy)
			return;
		__result = Loc.T(I18n.easyNameLoc, I18n.easyNameLocEn);
	}
}
