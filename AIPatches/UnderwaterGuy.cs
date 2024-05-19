using CobaltCoreModding.Definitions;
using HarmonyLib;
using System.Reflection;

namespace TheJazMaster.MoreDifficulties.AIPatches;

/*
Attacks are stickier
*/

[HarmonyPatch]
public static class UnderwaterGuyPatch {

	[HarmonyPatch(typeof(UnderwaterGuy), nameof(UnderwaterGuy.PickNextIntent))]
	[HarmonyPrefix]
	private static bool PickNextIntent_Prefix(UnderwaterGuy __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (s.GetDifficulty() < Manifest.Difficulty2) return true;
		
		__result = AIUtils.MoveSet(__instance.aiCounter++, () => new EnemyDecision
		{
			actions = AIUtils.MoveToAimAtPincer(s, ownShip, s.ship, "empty1", 1),
			intents = new List<Intent>
			{
				new IntentAttack
				{
					damage = 3,
					fromX = 1,
					cardOnHit = new Fear(),
					destination = CardDestination.Hand
				},
				new IntentAttack
				{
					damage = 3,
					fromX = 5,
					cardOnHit = new Fear(),
					destination = CardDestination.Hand
				}
			}
		}, () => new EnemyDecision
		{
			actions = AIUtils.MoveToAimAtPincer(s, ownShip, s.ship, "empty2", 1, true),
			intents = new List<Intent>
			{
				new IntentAttack
				{
					damage = 3,
					fromX = 1,
					cardOnHit = new Fear(),
					destination = CardDestination.Hand
				},
				new IntentAttack
				{
					damage = 3,
					fromX = 5,
					cardOnHit = new Fear(),
					destination = CardDestination.Hand
				}
			}
		});

		return false;
	}
}