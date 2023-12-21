using CobaltCoreModding.Definitions;
using HarmonyLib;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Extensions.Logging;
using static System.Reflection.BindingFlags;

namespace TheJazMaster.MoreDifficulties.AIPatches;

/*
	Fires breachers every 3 turns
*/

[HarmonyPatch]
public static class OxygenLeakGuyPatch {
	private static Manifest Instance => Manifest.Instance;

	[HarmonyPatch(typeof(OxygenLeakGuy), nameof(OxygenLeakGuy.BuildShipForSelf))]
	[HarmonyPostfix]
	private static void BuildShipForSelf_Postfix(OxygenLeakGuy __instance, Ship __result, State s) {
		if (s.GetDifficulty() >= 5)
			__result.parts[1].damageModifier = PDamMod.armor;
	}

	// [HarmonyPatch(typeof(AMissileHit), nameof(AMissileHit.Update))]
	// [HarmonyTranspiler]
	// private static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod) {
	// 	try
	// 	{
	// 		return new SequenceBlockMatcher<CodeInstruction>(instructions)
	// 			.Find(
	// 				ILMatches.Ldarg(0),
	// 				ILMatches.Ldfld("weaken"),
	// 				ILMatches.Ldloc<bool>(originalMethod.GetMethodBody()!.LocalVariables),
	// 				ILMatches.Instruction(OpCodes.And)
	// 			)
    //             .Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.IncludingInsertion, new List<CodeInstruction> {
	// 				new CodeInstruction(OpCodes.Ldarg_2),
    //                 new CodeInstruction(OpCodes.Ldarg_0),
    //                 new CodeInstruction(OpCodes.Ldfld, typeof(AMissileHit).GetField("worldX")),
    //                 new CodeInstruction(OpCodes.Ldarg_0),
    //                 new CodeInstruction(OpCodes.Ldfld, typeof(AMissileHit).GetField("targetPlayer")),
    //                 new CodeInstruction(OpCodes.Call, typeof(OxygenLeakGuyPatch).GetMethod("ShouldWeaken", BindingFlags.NonPublic | BindingFlags.Static)),
    //                 new CodeInstruction(OpCodes.And),
    //             })
    //             .AllElements();
	// 	}
	// 	catch (Exception ex)
	// 	{
	// 		Instance.Logger!.LogError("Could not patch method {Method} - {Mod} probably won't work.\nReason: {Exception}", originalMethod, Instance.Name, ex);
	// 		return instructions;
	// 	}
	// }

	// private static bool ShouldWeaken(State s, int worldX, bool targetPlayer) {
	// 	if (!targetPlayer)
	// 		return true;
	// 	Part? partAtWorldX = s.ship.GetPartAtWorldX(worldX);
	// 	if (partAtWorldX != null) {
	// 		return partAtWorldX.damageModifier != PDamMod.brittle;
	// 	}
	// 	return true;
	// }

	// [HarmonyPatch(typeof(OxygenLeakGuy), nameof(OxygenLeakGuy.PickNextIntent))]
	// [HarmonyPrefix]
	// private static bool PickNextIntent_Prefix(OxygenLeakGuy __instance, ref EnemyDecision __result, State s, Combat c, Ship ownShip) {
	// 	if (s.GetDifficulty() < Manifest.Difficulty2) return true;
		
	// 	__result = AIUtils.MoveSet(__instance.aiCounter++, () => new EnemyDecision
	// 	{
	// 		actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, 1),
	// 		intents = new List<Intent>
	// 		{
	// 			new IntentAttack
	// 			{
	// 				fromX = 0,
	// 				damage = 1,
	// 				cardOnHit = new OxygenLeak()
	// 			},
	// 			new IntentMissile
	// 			{
	// 				fromX = 1,
	// 				missileType = MissileType.seeker
	// 			},
	// 			new IntentAttack
	// 			{
	// 				fromX = 5,
	// 				damage = 1,
	// 				cardOnHit = new OxygenLeak()
	// 			}
	// 		}
	// 	});

	// 	return false;
	// }
}