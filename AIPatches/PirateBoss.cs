using CobaltCoreModding.Definitions;
using TheJazMaster.MoreDifficulties.Cards;
using HarmonyLib;
using System.Reflection;

namespace TheJazMaster.MoreDifficulties.AIPatches;

[HarmonyPatch]
public static class PirateBossPatch {

	[HarmonyPatch(typeof(PirateBoss), nameof(PirateBoss.BuildShipForSelf))]
	[HarmonyPostfix]
	private static void BuildShipForSelf_Postfix(PirateBoss __instance, Ship __result, State s) {
		if (s.GetDifficulty() >= 5) {
			__result.parts[7].stunModifier = PStunMod.unstunnable;
			__result.hull += 6;
			__result.hullMax += 6;
			__result.shieldMaxBase += 3;
		}
	}

	[HarmonyPatch(typeof(PirateBoss), nameof(PirateBoss.PickNextIntent))]
	[HarmonyPrefix]
	public static bool PickNextIntent_Prefix(PirateBoss __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (s.GetDifficulty() < Manifest.Difficulty2) return true;
		
		int moveDir = (__instance.PlayerIsLeftishOfBoss(s, c, ownShip) ? (__instance.moveDistance * -1) : __instance.moveDistance);
		__result = AIUtils.MoveSet(__instance.aiCounter++, delegate
		{
			List<Intent> list = new List<Intent>();
			for (int i = 0; i < ownShip.parts.Count; i++)
			{
				if (ownShip.parts[i].type == PType.cannon || ownShip.parts[i].skin == "wing_lawless")
				{
					list.Add(new IntentAttack
					{
						damage = ((!(ownShip.parts[i].skin == "cannon_lawless")) ? 1 : 2),
						fromX = i,
						fast = true
					});
				}
				if (ownShip.parts[i].type == PType.cockpit && !ownShip.parts[i].flip && __instance.aiCounter % 2 == 1)
				{
					list.Add(new IntentGiveCard
					{
						destination = CardDestination.Hand,
						card = new Beg(),
						fromX = i
					});
				}
				if (ownShip.parts[i].type == PType.missiles)
				{
					if (__instance.aiCounter % 2 == 0)
					{
						list.Add(new IntentStatus
						{
							targetSelf = true,
							status = Status.tempShield,
							amount = 1,
							fromX = i
						});
					}
					else
					{
						list.Add(new IntentMissile
						{
							missileType = __instance.aiCounter % 4 == 1 ? MissileType.heavy : MissileType.seeker,
							fromX = i
						});
					}
				}
			}
			if (AIUtils.GetUnderhang(s, c, ownShip) < 2)
			{
				if (__instance.enraged)
				{
					moveDir = s.ship.x + s.ship.parts.Count / 2 + 1 - (ownShip.x + ownShip.parts.Count / 2);
					foreach (Intent item in list)
					{
						if (item is IntentMissile intentMissile)
						{
							intentMissile.missileType = MissileType.heavy;
						}
						if (item is IntentAttack intentAttack)
						{
							intentAttack.damage++;
						}
					}
					if (list[1] is IntentAttack intentAttack2)
					{
						intentAttack2.dialogueTag = "hardRiggsGoesAggro";
					}
					return new EnemyDecision
					{
						actions = new List<CardAction>
						{
							new AMove
							{
								targetPlayer = false,
								dir = moveDir
							},
							new AStatus
							{
								targetPlayer = false,
								status = Status.heat,
								statusAmount = Math.Abs(moveDir)
							},
							new AHurt
							{
								targetPlayer = false,
								hurtAmount = 1
							}
						},
						intents = list
					};
				}
				if (!__instance.enraged && list[1] is IntentAttack intentAttack3)
				{
					intentAttack3.dialogueTag = "hardRiggsGetsMad";
				}
				__instance.enraged = true;
			}
			return new EnemyDecision
			{
				actions = AIHelpers.Move(moveDir),
				intents = list
			};
		});

		return false;
	}
}