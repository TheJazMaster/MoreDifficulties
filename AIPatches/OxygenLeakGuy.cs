using CobaltCoreModding.Definitions;
using HarmonyLib;
using System.Reflection;

namespace TheJazMaster.MoreDifficulties.AIPatches;

/*
	Fires breachers every 3 turns
*/

[HarmonyPatch]
public static class OxygenLeakGuyPatch {

	[HarmonyPatch(typeof(OxygenLeakGuy), nameof(OxygenLeakGuy.PickNextIntent))]
	[HarmonyPrefix]
	private static bool PickNextIntent_Prefix(OxygenLeakGuy __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
		if (s.GetDifficulty() < Manifest.Difficulty2) return true;
		
		var breacherTurn = 2;
		__result = AIUtils.MoveSet(__instance.aiCounter++, () => new EnemyDecision
		{
			actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, 1),
			intents = new List<Intent>
			{
				new IntentAttack
				{
					fromX = 0,
					damage = 1,
					cardOnHit = new OxygenLeak()
				},
				new IntentMissile
				{
					fromX = 1,
					missileType = ((__instance.aiCounter > breacherTurn && __instance.aiCounter % 3 == 2) ? MissileType.breacher : MissileType.seeker)
				},
				new IntentAttack
				{
					fromX = 5,
					damage = 1,
					cardOnHit = new OxygenLeak()
				}
			}
		});

		return false;
	}
}