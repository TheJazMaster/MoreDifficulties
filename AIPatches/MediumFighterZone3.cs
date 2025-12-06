
using HarmonyLib;

namespace TheJazMaster.MoreDifficulties.AIPatches;

[HarmonyPatch]
public static class MediumFighterZone3Patch
{

	[HarmonyPatch(typeof(MediumFighterZone3), nameof(MediumFighterZone3.OnCombatStart))]
	[HarmonyPostfix]
	private static void OnCombatStart_Postfix(MediumFighterZone3 __instance, State s, Combat c) {
		if (AIUtils.AreEnemiesEvenHarder(s))
			c.Queue(new AStatus {
				targetPlayer = false,
				status = ModEntry.GrazerStatus,
				statusAmount = 1
			});
	}

	[HarmonyPatch(typeof(MediumFighterZone3), nameof(MediumFighterZone3.PickNextIntent))]
	[HarmonyPrefix]
	private static bool PickNextIntent_Prefix(MediumFighterZone3 __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (!AIUtils.AreEnemiesEvenHarder(s)) return true;

		__result = AIUtils.MoveSet(__instance.aiCounter++, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, "cannon1"),
			intents = [
                new IntentAttack
				{
					damage = 3,
					key = "cannon1"
				},
				new IntentAttack
				{
					damage = 3,
					key = "cannon2"
				},
				new IntentStatus
				{
					status = Status.tempShield,
					amount = 2,
					targetSelf = true,
					key = "missiles1"
				}
			]
		}, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, "cannon1"),
			intents = [
                new IntentGiveCard
				{
					card = new TrashUnplayable(),
					destination = CardDestination.Discard,
					key = "cannon1"
				},
				new IntentGiveCard
				{
					card = new TrashUnplayable(),
					destination = CardDestination.Discard,
					key = "cannon2"
				},
				new IntentStatus
				{
					status = Status.tempShield,
					amount = 2,
					targetSelf = true,
					key = "missiles2"
				}
			]
		});
		return false;
	}
}