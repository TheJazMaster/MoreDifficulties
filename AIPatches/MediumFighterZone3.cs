
using CobaltCoreModding.Definitions;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using static System.Reflection.BindingFlags;

namespace TheJazMaster.MoreDifficulties.AIPatches;

[HarmonyPatch]
public static class MediumFighterZone3Patch {

	[HarmonyPatch(typeof(MediumFighterZone3), nameof(MediumFighterZone3.PickNextIntent))]
	[HarmonyPrefix]
	private static bool PickNextIntent_Prefix(MediumFighterZone3 __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (s.GetDifficulty() < Manifest.Difficulty2) return true;

		__result = AIUtils.MoveSet(__instance.aiCounter++, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, 2),
			intents = new List<Intent>
			{
				new IntentAttack
				{
					damage = 3,
					fromX = 2
				},
				new IntentAttack
				{
					damage = 3,
					fromX = 3
				},
				new IntentStatus
				{
					status = Status.tempShield,
					amount = 3,
					targetSelf = true,
					fromX = 1
				}
			}
		}, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, 2),
			intents = new List<Intent>
			{
				new IntentGiveCard
				{
					card = new TrashUnplayable(),
					destination = CardDestination.Discard,
					fromX = ownShip.GetStatusOrigin() + 1
				},
				new IntentGiveCard
				{
					card = new TrashUnplayable(),
					destination = CardDestination.Discard,
					fromX = ownShip.GetStatusOrigin() + 2
				},
				new IntentStatus
				{
					status = Status.tempShield,
					amount = 3,
					targetSelf = true,
					fromX = 4
				}
			}
		});
		return false;
	}
}