
using CobaltCoreModding.Definitions;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using static System.Reflection.BindingFlags;

namespace TheJazMaster.MoreDifficulties.AIPatches;

[HarmonyPatch]
public static class CrabGuyPatch {

	[HarmonyPatch(typeof(CrabGuy), nameof(CrabGuy.PickNextIntent))]
	[HarmonyPrefix]
	private static bool PickNextIntent_Prefix(CrabGuy __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (s.GetDifficulty() < Manifest.Difficulty2) return true;

		bool bubbledSeekers = true;
		if (c.turn == 0)
		{
			__result = AIUtils.MoveSet(0, () => new EnemyDecision
			{
				actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, 2, 99, movesFast: true, null, avoidAsteroids: false, avoidMines: false),
				intents = new List<Intent>
				{
					new IntentSpawn
					{
						fromX = 1,
						thing = new AttackDrone
						{
							targetPlayer = true,
							upgraded = true,
							bubbleShield = true
						}
					},
					new IntentSpawn
					{
						fromX = 4,
						thing = new AttackDrone
						{
							targetPlayer = true,
							upgraded = true,
							bubbleShield = true
						}
					}
				}
			});
			return false;
		}
		if (c.turn == 1)
		{
			bool left = s.rngAi.Next() > 0.5;
			__result = AIUtils.MoveSet(0, () => new EnemyDecision
			{
				actions = new List<CardAction>
				{
					new AMove
					{
						dir = (left ? (-5) : 5),
						targetPlayer = false
					}
				},
				intents = new List<Intent>
				{
					new IntentMissile
					{
						fromX = ((!left) ? 1 : 4),
						missileType = MissileType.seeker,
						bubbleShield = bubbledSeekers
					}
				}
			});
			return false;
		}
		__result = AIUtils.MoveSet(__instance.aiCounter++, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, 1, 99, movesFast: true, null, avoidAsteroids: false, avoidMines: false),
			intents = new List<Intent>
			{
				new IntentSpawn
				{
					fromX = 1,
					thing = new AttackDrone
					{
						targetPlayer = true,
						upgraded = true,
						bubbleShield = true
					}
				},
				new IntentSpawn
				{
					fromX = 4,
					thing = new AttackDrone
					{
						targetPlayer = true,
						upgraded = true,
						bubbleShield = true
					}
				}
			}
		}, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, 2, 99, movesFast: true, null, avoidAsteroids: false, avoidMines: false),
			intents = new List<Intent>
			{
				new IntentAttack
				{
					damage = 1,
					status = Status.drawLessNextTurn,
					statusAmount = 1,
					fromX = 0
				},
				new IntentAttack
				{
					damage = 2,
					fromX = 2
				},
				new IntentAttack
				{
					damage = 2,
					fromX = 3
				},
				new IntentAttack
				{
					damage = 1,
					status = Status.drawLessNextTurn,
					statusAmount = 1,
					fromX = 5
				}
			}
		}, () => new EnemyDecision
		{
			actions = new List<CardAction>
			{
				new AMove
				{
					dir = 6,
					isRandom = true,
					targetPlayer = false
				}
			},
			intents = new List<Intent>
			{
				new IntentMissile
				{
					fromX = 1,
					missileType = MissileType.seeker,
					bubbleShield = bubbledSeekers
				},
				new IntentMissile
				{
					fromX = 4,
					missileType = MissileType.seeker,
					bubbleShield = bubbledSeekers
				}
			}
		});
		
		return false;
	}
}