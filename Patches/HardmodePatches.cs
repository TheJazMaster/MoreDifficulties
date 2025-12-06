using System.Collections.Generic;
using HarmonyLib;

namespace TheJazMaster.MoreDifficulties;

[HarmonyPatch]
internal static class HardmodePatches
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(HARDMODE), nameof(HARDMODE.GetSprite))]
	private static void HARDMODE_GetSprite_Postfix(HARDMODE __instance, ref Spr __result)
	{
		if (__instance.difficulty == ModEntry.Easy) {
			__result = ModEntry.EasyModeArtifactSprite;
		}

		if (__instance.difficulty == ModEntry.Difficulty1) {
            __result = ModEntry.DifficultyArtifactSprite1;
        }

        if (__instance.difficulty == ModEntry.Difficulty2) {
            __result = ModEntry.DifficultyArtifactSprite2;
        }
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(HARDMODE), nameof(HARDMODE.GetExtraTooltips))]
	private static void HARDMODE_GetExtraTooltips_Postfix(HARDMODE __instance, ref List<Tooltip>? __result)
	{
		__result ??= [];
		if (__instance.difficulty == ModEntry.Easy) {
			__result.RemoveAll(t => t is TTText textTooltip && textTooltip.text == "???");
			__result.Add(new TTText(Loc.T(I18n.easyDescLoc, I18n.easyDescLocEn)));
		}

		if (__instance.difficulty == ModEntry.Difficulty1) {
            __result.RemoveAt(0);
            __result.Add(new TTText(Loc.T(I18n.difficultyDescLoc1, I18n.difficultyDescLoc1En)));
        }

        if (__instance.difficulty == ModEntry.Difficulty2) {
            __result.RemoveAt(0);
            __result.Add(new TTText(Loc.T(I18n.difficultyDescLoc2, I18n.difficultyDescLoc2En)));
        }
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(HARDMODE), nameof(HARDMODE.OnTurnStart))]
	private static void HARDMODE_OnTurnStart_Postfix(HARDMODE __instance, Combat combat)
	{
		if (combat.turn < 1)
			return;
		if (__instance.difficulty != ModEntry.Easy)
			return;

		combat.QueueImmediate(new AStatus
		{
			status = Status.tempShield,
			statusAmount = 1,
			targetPlayer = true
		});
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(HARDMODE), nameof(HARDMODE.OnReceiveArtifact))]
    private static void HARDMODE_OnReceiveArtifact_Postfix(HARDMODE __instance, State state) {
        if (__instance.difficulty >= ModEntry.Difficulty1) {
			if (!state.ship.evadeMax.HasValue || state.ship.evadeMax > 6) {
				state.ship.evadeMax = 7;
			}
		}
    }
}
