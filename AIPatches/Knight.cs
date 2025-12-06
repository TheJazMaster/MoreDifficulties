using HarmonyLib;

namespace TheJazMaster.MoreDifficulties.AIPatches;

[HarmonyPatch]
public static class KnightPatch
{
	[HarmonyPatch(typeof(Knight), nameof(Knight.BuildShipForSelf))]
	[HarmonyPostfix]
	private static void BuildShipForSelf_Postfix(Knight __instance, Ship __result, State s) {
		if (AIUtils.AreEnemiesEvenHarder(s)) {
			__result.hullMax += 3;
			__result.hull += 3;
			__result.shieldMaxBase += 1;
		}
	}

	[HarmonyPatch(typeof(Knight), nameof(Knight.OnHitByAttack))]
	[HarmonyPrefix]
	public static bool OnHitByAttack_Prefix(Knight __instance, State s, Combat c, int worldX, AAttack attack)
	{
        if (!AIUtils.AreEnemiesEvenHarder(s)) {
            return true;
        }

        Part? partAtWorldX = c.otherShip.GetPartAtWorldX(worldX);
		if (partAtWorldX == null)
		{
			return false;
		}
		if (partAtWorldX.GetDamageModifier() == PDamMod.weak || partAtWorldX.GetDamageModifier() == PDamMod.brittle)
		{
			if (__instance.thisBattleIsHonorable && !__instance.heIsAngry)
			{
				__instance.heIsAngry = true;
				c.Queue(new AMidCombatDialogue
				{
					script = "Knight_Midcombat_Dishonor"
				});
				c.Queue(new AStatus
				{
					status = Status.powerdrive,
					targetPlayer = false,
					statusAmount = 2
				});
			}
		}
		else if (c.otherShip.hull <= 7 && __instance.thisBattleIsHonorable && !__instance.heIsAngry)
		{
			c.Queue(new AMidCombatDialogue
			{
				script = "Knight_Midcombat_YouWin"
			});
			c.Queue(new ADelay
			{
				time = 0.0,
				timer = 0.1
			});
			c.Queue(new AEscape
			{
				targetPlayer = false
			});
		}
        return false;
    }
}