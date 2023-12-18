using CobaltCoreModding.Definitions;
using HarmonyLib;
using System.Reflection;
using FSPRO;

namespace TheJazMaster.MoreDifficulties.AIPatches;

/*
Avoids mines
*/
[HarmonyPatch]
public static class GoliathDefenderPatch {

	public static List<CardAction> MoveToAimAtAvoidingMines(State s, Ship movingShip, Ship targetShip, int alignPartLocalX, int mineAvoidPartLocalX)
	{
		Ship targetShip2 = targetShip;
		Route route = s.route;
		Combat? c = route as Combat;
		if (c == null)
		{
			return new List<CardAction>();
		}
		if (movingShip.Get(Status.engineStall) > 0)
		{
			Audio.Play(Event.Status_PowerDown);
			movingShip.Add(Status.engineStall, -1);
			movingShip.shake += 1.0;
			return new List<CardAction>();
		}
		StuffBase? value;
		var list = (from pair in targetShip2.parts.Select((Part part, int x) => new
			{
				part = part,
				x = x,
				drone = (c.stuff.TryGetValue(x + targetShip2.x, out value) ? value : null)
			})
			where pair.part.type != PType.empty
			select pair).ToList();
		var list2 = list.Where(pair =>
		{
			if (pair.drone == null)
			{
				return true;
			}
			if (pair.drone is ShieldDrone && !pair.drone.targetPlayer)
			{
				return false;
			}
			if (pair.drone is AttackDrone && pair.drone.targetPlayer)
			{
				return false;
			}
			return (!(pair.drone is SpaceMine)) ? true : false;
		}).ToList();
		if (list2.Count > 0)
		{
			list = list2;
		}
		list.Shuffle(s.rngAi);
		var anon = list[0];
		foreach (var data in list) {
			anon = data;
			if (!c.stuff.TryGetValue(data.x + (mineAvoidPartLocalX - alignPartLocalX), out value) || !(value is SpaceMine)) {
				break;
			}
		}
		int num = targetShip2.x + anon.x - (movingShip.x + alignPartLocalX);
		return AIHelpers.Move(num);
	}

	[HarmonyPatch(typeof(GoliathDefender), nameof(GoliathDefender.PickNextIntent))]
	[HarmonyPrefix]
	public static bool PickNextIntent_Prefix(GoliathDefender __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (s.GetDifficulty() < Manifest.Difficulty2) return true;
		
		if (__instance.maxMultiHit > 5)
		{
			__instance.maxMultiHit = 4;
		}
		__result = AIUtils.MoveSet(__instance.aiCounter++, () => new EnemyDecision
		{
			actions = MoveToAimAtAvoidingMines(s, ownShip, s.ship, 2, 4),
			intents = new List<Intent>
			{
				new IntentAttack
				{
					damage = 2,
					fromX = 1
				},
				new IntentAttack
				{
					damage = 2,
					fromX = 3
				},
				new IntentSpawn
				{
					fromX = 4,
					thing = new SpaceMine()
				}
			}
		}, () => new EnemyDecision
		{
			actions = MoveToAimAtAvoidingMines(s, ownShip, s.ship, 3, 1),
			intents = new List<Intent>
			{
				new IntentAttack
				{
					damage = 1,
					fromX = 1,
					multiHit = 2
				},
				new IntentAttack
				{
					damage = 1,
					fromX = 3,
					multiHit = ++__instance.maxMultiHit
				},
				new IntentStatus
				{
					status = Status.tempShield,
					amount = 2,
					targetSelf = true,
					fromX = 2
				}
			}
		});

		return false;
	}
}