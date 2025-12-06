
using HarmonyLib;

namespace TheJazMaster.MoreDifficulties.AIPatches;

[HarmonyPatch]
public static class CrabGuyPatch {

	[HarmonyPatch(typeof(CrabGuy), nameof(CrabGuy.PickNextIntent))]
	[HarmonyPrefix]
	private static bool PickNextIntent_Prefix(CrabGuy __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (!AIUtils.AreEnemiesEvenHarder(s)) return true;

		bool bubbledSeekers = true;
		if (c.turn == 0)
		{
			__result = AIUtils.MoveSet(0, () => new EnemyDecision
			{
				actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, "cannon1", 99, movesFast: true, null, avoidAsteroids: false, avoidMines: false),
				intents = [
                    new IntentSpawn
					{
						key = "missiles1",
						thing = new AttackDrone
						{
							targetPlayer = true,
							upgraded = true,
							bubbleShield = true
						}
					},
					new IntentSpawn
					{
						key = "missiles2",
						thing = new AttackDrone
						{
							targetPlayer = true,
							upgraded = true,
							bubbleShield = true
						}
					}
				]
			});
			return false;
		}
		if (c.turn == 1)
		{
			bool left = s.rngAi.Next() > 0.5;
			__result = AIUtils.MoveSet(0, () => new EnemyDecision
			{
				actions = [
                    new AMove
					{
						dir = left ? (-5) : 5,
						targetPlayer = false
					}
				],
				intents = [
                    new IntentMissile
					{
						key = !left ? "missiles1" : "missiles2",
						missileType = MissileType.seeker,
						bubbleShield = bubbledSeekers
					}
				]
			});
			return false;
		}
		__result = AIUtils.MoveSet(__instance.aiCounter++, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, "missiles1", 99, movesFast: true, null, avoidAsteroids: false, avoidMines: false),
			intents = [
                new IntentSpawn
				{
					key = "missiles1",
					thing = new AttackDrone
					{
						targetPlayer = true,
						upgraded = true,
						bubbleShield = true
					}
				},
				new IntentSpawn
				{
					key = "missiles2",
					thing = new AttackDrone
					{
						targetPlayer = true,
						upgraded = true,
						bubbleShield = true
					}
				}
			]
		}, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, "cannon1", 99, movesFast: true, null, avoidAsteroids: false, avoidMines: false),
			intents = [
                new IntentAttack
				{
					damage = 1,
					status = Status.drawLessNextTurn,
					statusAmount = 1,
					key = "wing1",
				},
				new IntentAttack
				{
					damage = 2,
					key = "cannon1",
				},
				new IntentAttack
				{
					damage = 2,
					key = "cannon2",
				},
				new IntentAttack
				{
					damage = 1,
					status = Status.drawLessNextTurn,
					statusAmount = 1,
					key = "wing2",
				}
			]
		}, () => new EnemyDecision
		{
			actions = [
                new AMove
				{
					dir = 6,
					isRandom = true,
					targetPlayer = false
				}
			],
			intents = [
                new IntentMissile
				{
					key = "missiles1",
					missileType = MissileType.seeker,
					bubbleShield = bubbledSeekers
				},
				new IntentMissile
				{
					key = "missiles2",
					missileType = MissileType.seeker,
					bubbleShield = bubbledSeekers
				}
			]
		});
		
		return false;
	}
}