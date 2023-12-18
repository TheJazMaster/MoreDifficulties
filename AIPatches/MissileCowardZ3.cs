using CobaltCoreModding.Definitions;
using HarmonyLib;
using System.Reflection;

namespace TheJazMaster.MoreDifficulties.AIPatches;

/*
	Armored cannon, more damage
*/

[HarmonyPatch]
public static class MissileCowardZ3Patch {

	[HarmonyPatch(typeof(MissileCowardZ3), nameof(MissileCowardZ3.BuildShipForSelf))]
	[HarmonyPostfix]
	private static void BuildShipForSelf_Postfix(MissileCowardZ3 __instance, Ship __result, State s) {
		if (s.GetDifficulty() >= 5)
			__result.parts[1].damageModifier = PDamMod.armor;
	}

	[HarmonyPatch(typeof(MissileCowardZ3), nameof(MissileCowardZ3.PickNextIntent))]
	[HarmonyPrefix]
	private static bool PickNextIntent_Prefix(MissileCowardZ3 __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (s.GetDifficulty() < Manifest.Difficulty2) return true;
		
		if (c.otherShip.hull < 4)
		{
			__result = AIUtils.MoveSet(s.rngAi, () => new EnemyDecision
			{
				actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, 0),
				intents = new List<Intent>
				{
					new IntentAttack
					{
						fromX = 1,
						damage = 3
					},
					new IntentAttack
					{
						fromX = 3,
						damage = 3
					},
					new IntentEscape
					{
						fromX = 7,
						dialogueTag = "batboyIsACoward"
					},
					new IntentEscape
					{
						fromX = 8,
						dialogueTag = "batboyIsACoward"
					}
				}
			});
			return false;
		}
		__result = AIUtils.MoveSet(s.rngAi, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, 2),
			intents = new List<Intent>
			{
				new IntentAttack
				{
					fromX = 1,
					damage = 2
				},
				new IntentGiveCard
				{
					card = new TrashFumes(),
					destination = CardDestination.Discard,
					fromX = 2,
					amount = 2
				},
				new IntentAttack
				{
					fromX = 3,
					damage = 2
				},
				new IntentMissile
				{
					fromX = 7,
					missileType = MissileType.seeker
				}
			}
		});

		return false;
	}
}