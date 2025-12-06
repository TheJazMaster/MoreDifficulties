using HarmonyLib;

namespace TheJazMaster.MoreDifficulties.AIPatches;

/*
Attacks are stickier
*/

[HarmonyPatch]
public static class UnderwaterGuyPatch {

	[HarmonyPatch(typeof(UnderwaterGuy), nameof(UnderwaterGuy.PickNextIntent))]
	[HarmonyPrefix]
	private static bool PickNextIntent_Prefix(UnderwaterGuy __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (!AIUtils.AreEnemiesEvenHarder(s)) return true;
		
		__result = AIUtils.MoveSet(__instance.aiCounter++, () => new EnemyDecision
		{
			actions = AIUtils.MoveToAimAtPincer(s, ownShip, s.ship, "empty1", 1),
			intents = [
				new IntentAttack
				{
					damage = 3,
					cardOnHit = new Fear(),
					destination = CardDestination.Hand,
					key = "empty1"
				},
				new IntentAttack
				{
					damage = 3,
					cardOnHit = new Fear(),
					destination = CardDestination.Hand,
					key = "empty2"
				}
			]
		}, () => new EnemyDecision
		{
			actions = AIUtils.MoveToAimAtPincer(s, ownShip, s.ship, "empty2", 1, true),
			intents = [
				new IntentAttack
				{
					damage = 3,
					cardOnHit = new Fear(),
					destination = CardDestination.Hand,
					key = "empty1"
				},
				new IntentAttack
				{
					damage = 3,
					cardOnHit = new Fear(),
					destination = CardDestination.Hand,
					key = "empty2"
				}
			]
		});

		return false;
	}
}