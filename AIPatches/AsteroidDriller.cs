using HarmonyLib;

namespace TheJazMaster.MoreDifficulties.AIPatches;

[HarmonyPatch]
public static class AsteroidDrillerPatch
{
	[HarmonyPatch(typeof(AsteroidDriller), nameof(AsteroidDriller.PickNextIntent))]
	[HarmonyPrefix]
	private static bool PickNextIntent_Prefix(AsteroidDriller __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (!AIUtils.AreEnemiesEvenHarder(s)) return true;

		__result = AI.MoveSet(s.rngAi, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, "drill1", 99, movesFast: false, null, avoidAsteroids: false, avoidMines: false),
			intents = [
                new IntentAttack
				{
					damage = 1,
					key = "drill1",
					multiHit = ++__instance.drillAttackCount
				},
				new IntentAttack
				{
					damage = 1,
					key = "drill2",
					multiHit = __instance.drillAttackCount
				}
			]
		}, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, "drill1", 99, movesFast: false, null, avoidAsteroids: false, avoidMines: false),
			intents = [
                new IntentAttack
				{
					damage = 1,
					key = "drill1",
					multiHit = ++__instance.drillAttackCount
				},
				new IntentAttack
				{
					damage = 1,
					key = "drill2",
					multiHit = __instance.drillAttackCount
				}
			]
		}, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, "wing", 99, movesFast: false, null, avoidAsteroids: true, avoidMines: false),
			intents = [
                new IntentStatus
				{
					amount = 4,
					key = "cockpit",
					status = Status.shield,
					targetSelf = true,
					dialogueTag = "skunkCooldown"
				},
				new IntentAttack
				{
					damage = 4,
					key = "wing"
				}
			]
		});
		return false;
	}
}