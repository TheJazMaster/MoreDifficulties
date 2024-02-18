using CobaltCoreModding.Definitions;
using HarmonyLib;
using System.Reflection;

namespace TheJazMaster.MoreDifficulties.AIPatches;

/*
Attacks 1 and 3 are stickier
*/

[HarmonyPatch]
public static class WideCruiserAltPatch {

	[HarmonyPatch(typeof(WideCruiserAlt), nameof(WideCruiserAlt.PickNextIntent))]
	[HarmonyPrefix]
	private static bool PickNextIntent_Prefix(WideCruiserAlt __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (s.GetDifficulty() < Manifest.Difficulty2) return true;
		
		MissileType missileType = (s.GetHarderEnemies() && __instance.aiCounter > 3) ? MissileType.heavy : MissileType.normal;
		__result = AIUtils.MoveSet(__instance.aiCounter++, () => new EnemyDecision
		{
			actions = AIUtils.MoveToAimAtPincer(s, ownShip, s.ship, 2, 2),
			intents = new List<Intent>
			{
				new IntentMissile
				{
					fromX = 1
				},
				new IntentAttack
				{
					damage = 3,
					fromX = 2
				},
				new IntentAttack
				{
					damage = 3,
					fromX = 6
				},
				new IntentMissile
				{
					fromX = 7
				}
			}
		}, () => new EnemyDecision
		{
			actions = AIUtils.MoveToAimAtPincer(s, ownShip, s.ship, 3, 2),
			intents = new List<Intent>
			{
				new IntentMissile
				{
					fromX = 2,
					missileType = missileType
				},
				new IntentAttack
				{
					damage = 2,
					fromX = 3
				},
				new IntentAttack
				{
					damage = 2,
					fromX = 5
				},
				new IntentMissile
				{
					fromX = 6,
					missileType = missileType
				}
			}
		}, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, 3),
			intents = new List<Intent>
			{
				new IntentMissile
				{
					fromX = 3,
					missileType = MissileType.heavy
				},
				new IntentMissile
				{
					fromX = 5,
					missileType = MissileType.heavy
				},
				new IntentAttack
				{
					damage = 2,
					fromX = 1
				},
				new IntentAttack
				{
					damage = 2,
					fromX = 7
				}
			}
		});

		return false;
	}
}