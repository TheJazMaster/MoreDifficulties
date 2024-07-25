using CobaltCoreModding.Definitions;
using TheJazMaster.MoreDifficulties.Cards;
using HarmonyLib;
using System.Reflection;

namespace TheJazMaster.MoreDifficulties.AIPatches;

[HarmonyPatch]
public static class Z2BossMechaPossumPatch {

	[HarmonyPatch(typeof(Z2BossMechaPossum), nameof(Z2BossMechaPossum.BuildShipForSelf))]
	[HarmonyPostfix]
	private static void BuildShipForSelf_Postfix(PirateBoss __instance, Ship __result, State s) {
		if (s.GetDifficulty() >= Manifest.Difficulty2) {
			__result.parts[4].stunModifier = PStunMod.unstunnable;
			__result.hull += 5;
			__result.hullMax += 5;
			__result.shieldMaxBase += 2;
		}
	}

	[HarmonyPatch(typeof(Z2BossMechaPossum), nameof(Z2BossMechaPossum.PickNextIntent))]
	[HarmonyPrefix]
	public static bool PickNextIntent_Prefix(Z2BossMechaPossum __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (s.GetDifficulty() < Manifest.Difficulty2) return true;
		
		Ship ownShip2 = ownShip;
		State s2 = s;
		__result = AIUtils.MoveSet(__instance.aiCounter++, delegate
		{
			Part part2 = ownShip2.GetPart("center");
			if (part2 != null)
			{
				part2.active = false;
			}
			ownShip2.RemoveParts("center", new HashSet<string> { "r.gap1", "r.gap2" });
			ownShip2.InsertParts(s2, "l.cockpit", "center", after: false, new List<Part>
			{
				new Part
				{
					key = "l.gap1",
					type = PType.empty
				},
				new Part
				{
					key = "l.gap2",
					type = PType.empty
				}
			});
			return new EnemyDecision
			{
				actions = AIHelpers.MoveToAimAt(s2, ownShip2, s2.ship, "center"),
				intents = new List<Intent>
				{
					new IntentAttack
					{
						damage = 2,
						key = "r.cannon",
						dialogueTag = "mechaPossumShout"
					},
					new IntentMissile
					{
						missileType = MissileType.punch,
						xDir = 1,
						key = "l.fist",
						bubbleShield = true
					},
					new IntentAttack
					{
						damage = 2,
						key = "l.cannon"
					}
				}
			};
		}, delegate
		{
			ownShip2.RemoveParts("center", new HashSet<string> { "l.gap1", "l.gap2" });
			ownShip2.InsertParts(s2, "r.cockpit", "center", after: true, new List<Part>
			{
				new Part
				{
					key = "r.gap1",
					type = PType.empty
				},
				new Part
				{
					key = "r.gap2",
					type = PType.empty
				}
			});
			return new EnemyDecision
			{
				actions = AIHelpers.MoveToAimAt(s2, ownShip2, s2.ship, "center"),
				intents = new List<Intent>
				{
					new IntentAttack
					{
						damage = 3,
						key = "r.cannon"
					},
					new IntentMissile
					{
						missileType = MissileType.punch,
						xDir = -1,
						key = "r.fist",
						bubbleShield = true
					},
					new IntentAttack
					{
						damage = 2,
						key = "l.cannon"
					}
				}
			};
		}, delegate
		{
			ownShip2.RemoveParts("center", new HashSet<string> { "l.gap1", "l.gap2", "r.gap1", "r.gap2" });
			Part part = ownShip2.GetPart("center");
			if (part != null)
			{
				part.active = true;
			}
			return new EnemyDecision
			{
				actions = AIHelpers.MoveToAimAt(s2, ownShip2, s2.ship, "center"),
				intents = new List<Intent>
				{
					new IntentAttack
					{
						damage = 3,
						key = "r.cannon"
					},
					new IntentAttack
					{
						damage = (s2.GetHarderBosses() ? 5 : 4),
						key = "center"
					},
					new IntentAttack
					{
						damage = 2,
						key = "l.cannon"
					}
				}
			};
		});
		return false;
	}
}