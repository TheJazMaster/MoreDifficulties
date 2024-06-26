using CobaltCoreModding.Definitions;
using HarmonyLib;
using System.Reflection;
using Microsoft.Extensions.Logging;
using FSPRO;

namespace TheJazMaster.MoreDifficulties.AIPatches;

/*
Avoids mines
*/
[HarmonyPatch]
public static class GoliathDefenderPatch {
	private static Manifest Instance => Manifest.Instance;

	public static List<CardAction> MoveToAimAtAvoidingMines(State s, Ship movingShip, Ship targetShip, string key, string mineAvoidKey, string? backupKey = null)
	{
		bool avoidedMine = false;
		Route route = s.route;
		if (route is not Combat c)
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
		var list = (from pair in targetShip.parts.Select((Part part, int x) => new
		{
			part,
			x,
			drone = c.stuff.TryGetValue(x + targetShip.x, out value) ? value : null
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
			return pair.drone is not SpaceMine;
		}).ToList();
		if (list2.Count > 0)
		{
			list = list2;
		}
		list.Shuffle(s.rngAi);
		var anon = list[0];
		Part? part = movingShip.parts.Find(part => part.key == key);
		int alignPartLocalX = part != null ? movingShip.parts.IndexOf(part) : 0;
		Part? mineAvoidPart = movingShip.parts.Find(part => part.key == mineAvoidKey);
		int mineAvoidPartLocalX = mineAvoidPart != null ? movingShip.parts.IndexOf(mineAvoidPart) : 0;
		Part? backupPart = movingShip.parts.Find(part => part.key == backupKey);
		int? backupAlignPartLocalX = backupKey == null ? null : (backupPart != null ? movingShip.parts.IndexOf(backupPart) : 0);
		foreach (var data in list) {
			anon = data;
			if (!c.stuff.TryGetValue(data.x + targetShip.x + (mineAvoidPartLocalX - alignPartLocalX), out value) || value is not SpaceMine) {
				avoidedMine = true;
				break;
			}
		}
		if (!avoidedMine && backupAlignPartLocalX.HasValue) {
			foreach (var data in list) {
				anon = data;
				if (!c.stuff.TryGetValue(data.x + targetShip.x + (mineAvoidPartLocalX - backupAlignPartLocalX.Value), out value) || !(value is SpaceMine)) {
					avoidedMine = true;
					break;
				}
			}
		}
		int num = targetShip.x + anon.x - (movingShip.x + alignPartLocalX);
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
			actions = MoveToAimAtAvoidingMines(s, ownShip, s.ship, "cannon2", "missiles", "cannon1"),
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
			actions = MoveToAimAtAvoidingMines(s, ownShip, s.ship, "cannon1", "missiles"),
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