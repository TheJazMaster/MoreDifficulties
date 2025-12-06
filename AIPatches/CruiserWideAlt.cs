using HarmonyLib;

namespace TheJazMaster.MoreDifficulties.AIPatches;

/*
Attacks 1 and 3 are stickier
*/

[HarmonyPatch]
public static class WideCruiserAltPatch {

	[HarmonyPatch(typeof(WideCruiserAlt), nameof(WideCruiserAlt.PickNextIntent))]
	[HarmonyPrefix]
	private static bool PickNextIntent_Prefix(WideCruiserAlt __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (!AIUtils.AreEnemiesEvenHarder(s)) return true;
		
		MissileType missileType = (s.GetHarderEnemies() && __instance.aiCounter > 3) ? MissileType.heavy : MissileType.normal;
		__result = AIUtils.MoveSet(__instance.aiCounter++, () => new EnemyDecision
		{
			actions = AIUtils.MoveToAimAtPincer(s, ownShip, s.ship, "cannonMidL", 2),
			intents = [
                new IntentMissile
				{
					key = "cannonOuterL",
				},
				new IntentAttack
				{
					damage = 3,
					key = "cannonMidL",
				},
				new IntentAttack
				{
					damage = 3,
					key = "cannonMidR",
				},
				new IntentMissile
				{
					key = "cannonOuterR",
				}
			]
		}, () => new EnemyDecision
		{
			actions = AIUtils.MoveToAimAtPincer(s, ownShip, s.ship, "cannonInnerL", 2),
			intents = [
                new IntentMissile
				{
					key = "cannonMidL",
					missileType = missileType
				},
				new IntentAttack
				{
					damage = 2,
					key = "cannonInnerL",
				},
				new IntentAttack
				{
					damage = 2,
					key = "cannonInnerR",
				},
				new IntentMissile
				{
					key = "cannonMidR",
					missileType = missileType
				}
			]
		}, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, "cannonInnerL"),
			intents = [
                new IntentMissile
				{
					key = "cannonInnerL",
					missileType = MissileType.heavy
				},
				new IntentMissile
				{
					key = "cannonInnerR",
					missileType = MissileType.heavy
				},
				new IntentAttack
				{
					damage = 2,
					key = "cannonOuterL",
				},
				new IntentAttack
				{
					damage = 2,
					key = "cannonOuterR",
				}
			]
		});

		return false;
	}
}