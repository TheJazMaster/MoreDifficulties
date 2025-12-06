using System;
using HarmonyLib;

namespace TheJazMaster.MoreDifficulties.AIPatches;


[HarmonyPatch]
public static class TheCobaltPatch {

	[HarmonyPatch(typeof(TheCobalt), nameof(TheCobalt.BuildShipForSelf))]
	[HarmonyPostfix]
	private static void BuildShipForSelf_Postfix(TheCobalt __instance, Ship __result, State s) {
		if (AIUtils.AreEnemiesEvenHarder(s)) {
			__result.parts[5].stunModifier = PStunMod.unstunnable;
			__result.x += s.rngActions.NextInt() % 5 - 1;
		}
	}

	[HarmonyPatch(typeof(TheCobalt), nameof(TheCobalt.PickNextIntent))]
	[HarmonyPrefix]
	private static bool PickNextIntent_Prefix(TheCobalt __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (!AIUtils.AreEnemiesEvenHarder(s)) return true;

		__instance.aiCounter++;

		if (__instance.aiCounter == 2) {
			int dir = (s.ship.x + s.ship.parts.Count / 2 + 1 > ownShip.x + ownShip.parts.Count / 2) ? __instance.moveDistance : (__instance.moveDistance * -1);
			__result =  new EnemyDecision {
				actions = AIHelpers.Move(dir),
				intents = [
					new IntentAttack {
						damage = 2,
						key = "wing1"
					},
					new IntentAttack {
						damage = 1,
						status = Status.drawLessNextTurn,
						statusAmount = 1,
						key = "cannon1"
					},
					new IntentAttack {
						damage = 1,
						multiHit = 2,
						key = "stuncannon1"
					},
					new IntentAttack {
						damage = 3,
						key = "bigcannon"
					},
					new IntentAttack {
						damage = 1,
						multiHit = 2,
						key = "stuncannon2"
					},
					new IntentAttack {
						damage = 1,
						status = Status.drawLessNextTurn,
						statusAmount = 1,
						key = "cannon2"
					},
					new IntentAttack {
						damage = 2,
						key = "wing2"
					}
				]
			};
			return false;
		}

		var alt = __instance.aiCounter / 2 % 2 == 0;
		__result = AIUtils.MoveSet(__instance.aiCounter, delegate
		{
			int dir = (s.ship.x + s.ship.parts.Count / 2 + 1 > ownShip.x + ownShip.parts.Count / 2) ? __instance.moveDistance : (__instance.moveDistance * -1);
			bool escaping = Math.Abs(AIUtils.GetUnderhang(s, c, ownShip)) < 2;
			return new EnemyDecision {
				actions = escaping ? [
					new AMove {
						targetPlayer = false,
						dir = s.ship.x + s.ship.parts.Count / 2 + 1 - (ownShip.x + ownShip.parts.Count / 2)
					},
					new AHurt {
						targetPlayer = false,
						hurtAmount = 1
					}
				] : AIHelpers.Move(dir),
				intents = [
					new IntentAttack {
						damage = 2,
						key = "wing1"
					},
					new IntentAttack {
						damage = 1,
						status = Status.drawLessNextTurn,
						statusAmount = 1,
						key = "cannon1"
					},
					new IntentGiveCard {
						card = alt ? new TrashExhaustOthers() : new TrashUnplayable(),
						destination = alt ? CardDestination.Discard : CardDestination.Deck,
						key = "missiles1"
					},
					new IntentAttack {
						damage = 1,
						multiHit = 2,
						key = "stuncannon1"
					},
					new IntentAttack {
						damage = 5,
						key = "bigcannon"
					},
					new IntentAttack {
						damage = 1,
						multiHit = 2,
						key = "stuncannon2"
					},
					new IntentGiveCard {
						card = alt ? new TrashUnplayable() : new TrashExhaustOthers(),
						destination = alt ? CardDestination.Deck : CardDestination.Discard,
						key = "missiles2"
					},
					new IntentAttack {
						damage = 1,
						status = Status.drawLessNextTurn,
						statusAmount = 1,
						key = "cannon2"
					},
					new IntentAttack {
						damage = 2,
						key = "wing2"
					}
				]
			};
		}, delegate
		{
			int dir = (s.ship.x + s.ship.parts.Count / 2 + 1 > ownShip.x + ownShip.parts.Count / 2) ? __instance.moveDistance : (__instance.moveDistance * -1);
			bool escaping = AIUtils.GetUnderhang(s, c, ownShip) < 2;
			return new EnemyDecision
			{
				actions = escaping ? [
					new AMove {
						targetPlayer = false,
						dir = s.ship.x + s.ship.parts.Count / 2 + 1 - (ownShip.x + ownShip.parts.Count / 2)
					},
					new AHurt {
						targetPlayer = false,
						hurtAmount = 1
					}
				] : AIHelpers.Move(dir),
				intents = [
					new IntentAttack {
						damage = 2,
						key = "wing1"
					},
					new IntentAttack {
						damage = 1,
						key = "cannon1"
					},
					new IntentMissile {
						missileType = alt ? MissileType.seeker : MissileType.heavy,
						key = "missiles1"
					},
					new IntentAttack {
						damage = 2,
						key = "stuncannon1"
					},
					new IntentAttack {
						damage = 2,
						multiHit = 2,
						key = "bigcannon"
					},
					new IntentAttack {
						damage = 2,
						key = "stuncannon2"
					},
					new IntentMissile {
						missileType = alt ? MissileType.heavy : MissileType.seeker,
						key = "missiles2"
					},
					new IntentAttack {
						damage = 1,
						key = "cannon2"
					},
					new IntentAttack {
						damage = 2,
						key = "wing2"
					}
				]
			};
		});

		return false;
	}
}