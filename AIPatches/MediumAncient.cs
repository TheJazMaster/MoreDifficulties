using CobaltCoreModding.Definitions;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using static System.Reflection.BindingFlags;

namespace TheJazMaster.MoreDifficulties.AIPatches;

[HarmonyPatch]
public static class MediumAncientPatch {

	[HarmonyPatch(typeof(MediumAncient), nameof(MediumAncient.OnCombatStart))]
	[HarmonyPostfix]
	private static void OnCombatStart_Postfix(MediumAncient __instance, State s, Combat c) {
		if (s.GetDifficulty() >= 5)
			c.Queue(new AStatus
			{
				targetPlayer = false,
				status = (Status)Manifest.GrazerStatus!.Id!,
				statusAmount = 1
			});
	}


	private static int? GetFromX(AAttack attack, State s, Combat c)
	{
		if (attack.fromX.HasValue)
		{
			return attack.fromX;
		}
		int num = (attack.targetPlayer ? c.otherShip : s.ship).parts.FindIndex((Part p) => p.type == PType.cannon && p.active);
		if (num != -1)
		{
			return num;
		}
		return null;
	}

	[HarmonyPatch(typeof(AAttack), nameof(AAttack.Begin))]
	[HarmonyPrefix]
	private static void Begin_Prefix(AAttack __instance, G g, State s, Combat c) {
		Ship ship = (__instance.targetPlayer ? s.ship : c.otherShip);
		var targetPlayer = __instance.targetPlayer;

		int? num = GetFromX(__instance, s, c);
		RaycastResult? raycastResult = (__instance.fromDroneX.HasValue ? CombatUtils.RaycastGlobal(c, ship, fromDrone: true, __instance.fromDroneX.Value) : (num.HasValue ? CombatUtils.RaycastFromShipLocal(s, c, num.Value, targetPlayer) : null));
		if (raycastResult != null && !__instance.isBeam)
		{
			if (!raycastResult.hitShip && !raycastResult.hitDrone) {
				var shipGrazer = s.ship.Get((Status)Manifest.GrazerStatus!.Id!);
				var otherShipGrazer = c.otherShip.Get((Status)Manifest.GrazerStatus!.Id!);
				if (targetPlayer && otherShipGrazer > 0 || !targetPlayer && shipGrazer > 0) {
					bool graze = false;
					for (int i = -1; i <= 1; i += 2)
					{
						if (CombatUtils.RaycastGlobal(c, ship, fromDrone: true, raycastResult.worldX + i).hitShip)
						{
							graze = true;
							break;
						}
					}
					if (graze)
					{
						c.QueueImmediate(new AHurt {
							targetPlayer = targetPlayer,
							hurtAmount = targetPlayer ? otherShipGrazer : shipGrazer,
							hurtShieldsFirst = true
						});		
					}
				}
			}
		}
	}
}