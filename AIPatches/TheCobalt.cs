using CobaltCoreModding.Definitions;
using HarmonyLib;
using System.Reflection;

namespace TheJazMaster.MoreDifficulties.AIPatches;


[HarmonyPatch]
public static class TheCobaltPatch {

	[HarmonyPatch(typeof(TheCobalt), nameof(TheCobalt.BuildShipForSelf))]
	[HarmonyPostfix]
	private static void BuildShipForSelf_Postfix(TheCobalt __instance, Ship __result, State s) {
		if (s.GetDifficulty() >= Manifest.Difficulty2)
			__result.parts[5].stunModifier = PStunMod.unstunnable;
	}

	[HarmonyPatch(typeof(TheCobalt), nameof(TheCobalt.PickNextIntent))]
	[HarmonyPrefix]
	private static bool PickNextIntent_Prefix(TheCobalt __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (s.GetDifficulty() < Manifest.Difficulty2) return true;
		
		__instance.aiCounter++;

		var alt = (int)(__instance.aiCounter / 2) % 2 == 0;
		__result = AIUtils.MoveSet(__instance.aiCounter, delegate
		{
			int dir = ((s.ship.x + s.ship.parts.Count / 2 + 1 > ownShip.x + ownShip.parts.Count / 2) ? __instance.moveDistance : (__instance.moveDistance * -1));
			bool escaping = Math.Abs(AIUtils.GetUnderhang(s, c, ownShip)) < 2;
			return new EnemyDecision {
				actions = escaping ? new List<CardAction> {
					new AMove {
						targetPlayer = false,
						dir = s.ship.x + s.ship.parts.Count / 2 + 1 - (ownShip.x + ownShip.parts.Count / 2)
					},
					new AStatus {
						targetPlayer = false,
						status = Status.overdrive,
						statusAmount = 1
					},
					new AHurt {
						targetPlayer = false,
						hurtAmount = 1
					}
				} : AIHelpers.Move(dir),
				intents = new List<Intent> {
					new IntentAttack {
						damage = 2,
						fromX = 0
					},
					new IntentAttack {
						damage = 1,
						status = Status.lockdown,
						fromX = 1
					},
					new IntentGiveCard {
						card = alt ? new TrashExhaustOthers() : new TrashUnplayable(),
						destination = alt ? CardDestination.Discard : CardDestination.Deck,
						fromX = 2
					},
					new IntentAttack {
						damage = 1,
						fromX = 4,
						multiHit = 2
					},
					new IntentAttack {
						damage = 5,
						fromX = 5
					},
					new IntentAttack {
						damage = 1,
						fromX = 6,
						multiHit = 2
					},
					new IntentGiveCard {
						card = alt ? new TrashUnplayable() : new TrashExhaustOthers(),
						destination = alt ? CardDestination.Deck : CardDestination.Discard,
						fromX = 8
					},
					new IntentAttack {
						damage = 1,
						status = Status.lockdown,
						fromX = 9
					},
					new IntentAttack {
						damage = 2,
						fromX = 10
					}
				}
			};
		}, delegate
		{
			int dir = (s.ship.x + s.ship.parts.Count / 2 + 1 > ownShip.x + ownShip.parts.Count / 2) ? __instance.moveDistance : (__instance.moveDistance * -1);
			bool escaping = AIUtils.GetUnderhang(s, c, ownShip) < 2;
			return new EnemyDecision
			{
				actions = escaping ? new List<CardAction> {
					new AMove {
						targetPlayer = false,
						dir = s.ship.x + s.ship.parts.Count / 2 + 1 - (ownShip.x + ownShip.parts.Count / 2)
					},
					new AStatus {
						targetPlayer = false,
						status = Status.overdrive,
						statusAmount = 1
					},
					new AHurt {
						targetPlayer = false,
						hurtAmount = 1
					}
				} : AIHelpers.Move(dir),
				intents = new List<Intent> {
					new IntentAttack {
						damage = 2,
						fromX = 0
					},
					new IntentAttack {
						damage = 1,
						fromX = 1
					},
					new IntentMissile {
						missileType = alt ? MissileType.seeker : MissileType.heavy,
						fromX = 2
					},
					new IntentAttack {
						damage = 2,
						fromX = 4
					},
					new IntentAttack {
						damage = 3,
						fromX = 5,
						multiHit = 2
					},
					new IntentAttack {
						damage = 2,
						fromX = 6
					},
					new IntentMissile {
						missileType = alt ? MissileType.heavy : MissileType.seeker,
						fromX = 8
					},
					new IntentAttack {
						damage = 1,
						fromX = 9
					},
					new IntentAttack {
						damage = 2,
						fromX = 10
					}
				}
			};
		});

		return false;
	}
}