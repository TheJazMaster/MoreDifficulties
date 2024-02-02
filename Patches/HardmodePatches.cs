using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using static System.Reflection.BindingFlags;

namespace TheJazMaster.MoreDifficulties;

internal static class HardmodePatches
{
	private static Manifest Instance => Manifest.Instance;

	public static void Apply(Harmony harmony)
	{
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(HARDMODE).GetMethod("GetSprite", AccessTools.all),
			postfix: new HarmonyMethod(typeof(HardmodePatches).GetMethod("HARDMODE_GetSprite_Postfix", AccessTools.all))
		);
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(HARDMODE).GetMethod("GetExtraTooltips", AccessTools.all),
			postfix: new HarmonyMethod(typeof(HardmodePatches).GetMethod("HARDMODE_GetExtraTooltips_Postfix", AccessTools.all))
		);
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(HARDMODE).GetMethod("OnTurnStart", AccessTools.all),
			postfix: new HarmonyMethod(typeof(HardmodePatches).GetMethod("HARDMODE_OnTurnStart_Postfix", AccessTools.all))
		);
        harmony.TryPatch(
            logger: Instance.Logger!,
            original: typeof(HARDMODE).GetMethod("OnReceiveArtifact", AccessTools.all),
            postfix: new HarmonyMethod(typeof(HardmodePatches).GetMethod("HARDMODE_OnReceiveArtifact_Postfix", AccessTools.all))
        );
	}

	private static void HARDMODE_GetSprite_Postfix(HARDMODE __instance, ref Spr __result)
	{
		if (__instance.difficulty == Manifest.Easy) {
			__result = (Spr)Manifest.EasyModeArtifactSprite!.Id!;
		}

		if (__instance.difficulty == Manifest.Difficulty1) {
            __result = (Spr)Manifest.DifficultyArtifactSprite1!.Id!;
        }

        if (__instance.difficulty == Manifest.Difficulty2) {
            __result = (Spr)Manifest.DifficultyArtifactSprite2!.Id!;
        }
	}

	private static void HARDMODE_GetExtraTooltips_Postfix(HARDMODE __instance, ref List<Tooltip>? __result)
	{
		__result ??= new();
		if (__instance.difficulty == Manifest.Easy) {
			__result.RemoveAll(t => t is TTText textTooltip && textTooltip.text == "???");
			__result.Add(new TTText(Loc.T(I18n.easyDescLoc, I18n.easyDescLocEn)));
		}

		if (__instance.difficulty == Manifest.Difficulty1) {
            __result.RemoveAt(0);
            __result.Add(new TTText(Loc.T(I18n.difficultyDescLoc1, I18n.difficultyDescLoc1En)));
        }

        if (__instance.difficulty == Manifest.Difficulty2) {
            __result.RemoveAt(0);
            __result.Add(new TTText(Loc.T(I18n.difficultyDescLoc2, I18n.difficultyDescLoc2En)));
        }
	}

	private static void HARDMODE_OnTurnStart_Postfix(HARDMODE __instance, Combat combat)
	{
		if (combat.turn < 1)
			return;
		if (__instance.difficulty != Manifest.Easy)
			return;

		combat.QueueImmediate(new AStatus
		{
			status = Status.tempShield,
			statusAmount = 1,
			targetPlayer = true
		});
	}

    private static void HARDMODE_OnReceiveArtifact_Postfix(HARDMODE __instance, State state) {
        if (__instance.difficulty >= Manifest.Difficulty1) {
			if (!state.ship.evadeMax.HasValue || state.ship.evadeMax > 6)
			{
				state.ship.evadeMax = 7;
			}
			// state.SendCardToDeck(new DodgeColorless(), doAnimation: false, insertRandomly: true);
			// state.SendCardToDeck(new BasicShieldColorless(), doAnimation: false, insertRandomly: true);
		}
    }
}
