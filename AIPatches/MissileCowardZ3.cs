using HarmonyLib;

namespace TheJazMaster.MoreDifficulties.AIPatches;

/*
	Armored cannon, more damage
*/

[HarmonyPatch]
public static class MissileCowardZ3Patch {

	[HarmonyPatch(typeof(MissileCowardZ3), nameof(MissileCowardZ3.BuildShipForSelf))]
	[HarmonyPostfix]
	private static void BuildShipForSelf_Postfix(MissileCowardZ3 __instance, Ship __result, State s) {
		if (AIUtils.AreEnemiesEvenHarder(s))
			__result.parts[1].damageModifier = PDamMod.armor;
	}

	[HarmonyPatch(typeof(MissileCowardZ3), nameof(MissileCowardZ3.PickNextIntent))]
	[HarmonyPrefix]
	private static bool PickNextIntent_Prefix(MissileCowardZ3 __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (!AIUtils.AreEnemiesEvenHarder(s)) return true;
		
		if (c.otherShip.hull < 4)
		{
			__result = AIUtils.MoveSet(s.rngAi, () => new EnemyDecision
			{
				actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, "wing1"),
				intents = [
                    new IntentAttack
					{
						key = "cannon1",
						damage = 3
					},
					new IntentAttack
					{
						key = "cannon2",
						damage = 3
					},
					new IntentEscape
					{
						key = "missiles",
						dialogueTag = "batboyIsACoward"
					},
					new IntentEscape
					{
						key = "wing2",
						dialogueTag = "batboyIsACoward"
					}
				]
			});
			return false;
		}
		__result = AIUtils.MoveSet(s.rngAi, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, "cockpit"),
			intents = [
                new IntentAttack
				{
					key = "cannon1",
					damage = 2
				},
				new IntentGiveCard
				{
					card = new TrashFumes(),
					destination = CardDestination.Discard,
					key = "cockpit",
					amount = 2
				},
				new IntentAttack
				{
					key = "cannon2",
					damage = 2
				},
				new IntentMissile
				{
					key = "missiles",
					missileType = MissileType.seeker
				}
			]
		});

		return false;
	}
}