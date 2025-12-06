using HarmonyLib;
using FSPRO;
using System.Collections.Generic;
using System.Linq;

namespace TheJazMaster.MoreDifficulties.AIPatches;

/*
Avoids mines
*/
[HarmonyPatch]
public static class GoliathDefenderPatch {
	private static ModEntry Instance => ModEntry.Instance;

	public static List<CardAction> MoveToAimAtAvoidingMines(State s, Ship movingShip, Ship targetShip, string key, string mineAvoidKey, string? backupKey = null)
	{
		bool avoidedMine = false;
		Route route = s.route;
		if (route is not Combat c)
		{
			return [];
		}
		if (movingShip.Get(Status.engineStall) > 0)
		{
			Audio.Play(Event.Status_PowerDown);
			movingShip.Add(Status.engineStall, -1);
			movingShip.shake += 1.0;
			return [];
		}
        var list = (from pair in targetShip.parts.Select((Part part, int x) => new
        {
            part,
            x,
            drone = c.stuff.TryGetValue(x + targetShip.x, out StuffBase? value) ? value : null
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
			if (!c.stuff.TryGetValue(data.x + targetShip.x + (mineAvoidPartLocalX - alignPartLocalX), out var value) || value is not SpaceMine) {
				avoidedMine = true;
				break;
			}
		}
		if (!avoidedMine && backupAlignPartLocalX.HasValue) {
			foreach (var data in list) {
				anon = data;
				if (!c.stuff.TryGetValue(data.x + targetShip.x + (mineAvoidPartLocalX - backupAlignPartLocalX.Value), out var value) || value is not SpaceMine) {
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
		if (!AIUtils.AreEnemiesEvenHarder(s)) return true;
		
		if (__instance.maxMultiHit > 5)
		{
			__instance.maxMultiHit = 4;
		}
		__result = AIUtils.MoveSet(__instance.aiCounter++, () => new EnemyDecision
		{
			actions = MoveToAimAtAvoidingMines(s, ownShip, s.ship, "cannon2", "missiles", "cannon1"),
			intents = [
                new IntentAttack
				{
					damage = 2,
					key = "cannon1"
				},
				new IntentAttack
				{
					damage = 2,
					key = "cannon2"
				},
				new IntentSpawn
				{
					key = "missiles",
					thing = new SpaceMine()
				}
			]
		}, () => new EnemyDecision
		{
			actions = MoveToAimAtAvoidingMines(s, ownShip, s.ship, "cannon1", "missiles"),
			intents = [
                new IntentAttack
				{
					damage = 1,
					key = "cannon1",
					multiHit = 2
				},
				new IntentAttack
				{
					damage = 1,
					key = "cannon2",
					multiHit = ++__instance.maxMultiHit
				},
				new IntentStatus
				{
					status = Status.tempShield,
					amount = 2,
					targetSelf = true,
					key = "cockpit"
				}
			]
		});

		return false;
	}
}