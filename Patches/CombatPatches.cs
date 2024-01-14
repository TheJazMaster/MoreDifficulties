using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.Shrike.Harmony;
using Nanoray.Shrike;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using FSPRO;
using static System.Reflection.BindingFlags;

namespace TheJazMaster.MoreDifficulties;

internal static class CombatPatches
{
	private static Manifest Instance => Manifest.Instance;

	public static void Apply(Harmony harmony)
	{
        // Reshuffling is no longer limited
        // harmony.TryPatch(
        //     logger: Instance.Logger!,
        //     original: typeof(Combat).GetMethod("DrawCards", AccessTools.all),
        //     transpiler: new HarmonyMethod(typeof(CombatPatches).GetMethod("Combat_DrawCards_Transpiler", AccessTools.all))
        // );
	}

    private static IEnumerable<CodeInstruction> Combat_DrawCards_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase originalMethod) {
        try
		{
            Label newLabel = il.DefineLabel();
			return new SequenceBlockMatcher<CodeInstruction>(instructions)
				.Find(
					ILMatches.Ldarg(1),
					ILMatches.Ldfld("deck"),
					ILMatches.Call("get_Count"),
					ILMatches.Brtrue
				)
                .Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.IncludingInsertion, new List<CodeInstruction> {
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, typeof(CombatPatches).GetMethod("CanShuffle", AccessTools.all)),
                    new CodeInstruction(OpCodes.Brtrue, newLabel),
                    new CodeInstruction(OpCodes.Ret),
                    new CodeInstruction(OpCodes.Nop).WithLabels(newLabel),
                })
                .AllElements();
		}
		catch (Exception ex)
		{
			Instance.Logger!.LogError("Could not patch method {Method} - {Mod} probably won't work.\nReason: {Exception}", originalMethod, Instance.Name, ex);
			return instructions;
		}
	}

    private static bool CanShuffle(State s, Combat c) {
        // Reshuffling is no longer limited
        // foreach (Artifact item in s.EnumerateAllArtifacts())
        // {
        //     if (item is HARDMODE hardmode && hardmode.difficulty >= Manifest.Difficulty1 && c.energy <= 0) {
        //         item.Pulse();
        //         Audio.Play(Event.ZeroEnergy);
        //         c.pulseEnergyBad = 0.5;
        //         return false;
        //     }
        // }
        return true;
    }
}