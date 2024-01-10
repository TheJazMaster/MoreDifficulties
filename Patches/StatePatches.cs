using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.Shrike.Harmony;
using Nanoray.Shrike;
using TheJazMaster.MoreDifficulties.Cards;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

namespace TheJazMaster.MoreDifficulties;

internal static class StatePatches
{
	private static Manifest Instance => Manifest.Instance;

	public static void Apply(Harmony harmony)
	{
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(State).GetNestedTypes(AccessTools.all).SelectMany(t => t.GetMethods(AccessTools.all)).First(m => m.Name.StartsWith("<PopulateRun>") && m.ReturnType == typeof(Route)),
			transpiler: new HarmonyMethod(typeof(StatePatches).GetMethod("State_PopulateRun_Delegate_Transpiler", AccessTools.all))
		);
        harmony.TryPatch(
			logger: Instance.Logger!,
            original: typeof(State).GetMethod("PopulateRun", AccessTools.all),
            postfix: new HarmonyMethod(typeof(StatePatches).GetMethod("State_PopulateRun_Postfix", AccessTools.all))
        );
	}

	private static IEnumerable<CodeInstruction> State_PopulateRun_Delegate_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
	{
		try
		{
			return new SequenceBlockMatcher<CodeInstruction>(instructions)
				.Find(
					ILMatches.Ldarg(0),
					ILMatches.Instruction(OpCodes.Ldfld),
					ILMatches.LdcI4(1),
					ILMatches.Blt
				)

				.PointerMatcher(SequenceMatcherRelativeElement.Last)
				.ExtractBranchTarget(out var branchTarget)

				.Encompass(SequenceMatcherEncompassDirection.Before, 1)
				.Replace(new CodeInstruction(OpCodes.Brfalse, branchTarget))

				.AllElements();
		}
		catch (Exception ex)
		{
			Instance.Logger!.LogError("Could not patch method {Method} - {Mod} probably won't work.\nReason: {Exception}", originalMethod, Instance.Name, ex);
			return instructions;
		}
	}

    private static void State_PopulateRun_Postfix(State __instance, int difficulty) {
        if (difficulty >= Manifest.Difficulty2) {
            List<Card> toRemove = new List<Card>();
            List<Card> toAdd = new List<Card>();
            foreach (Card card in __instance.deck)
            {
                if (card is CannonColorless) {
                    toRemove.Add(card);
                    toAdd.Add(new BasicOffences());
                } else if (card is DodgeColorless) {
                    toRemove.Add(card);
                    toAdd.Add(new BasicManeuvers());
                } else if (card is BasicShieldColorless) {
                    toRemove.Add(card);
                    toAdd.Add(new BasicDefences());
                } else if (card is DroneshiftColorless) {
                    toRemove.Add(card);
                    toAdd.Add(new BasicBroadcast());
                }
            }
            foreach (Card card in toRemove) {
                __instance.RemoveCardFromWhereverItIs(card.uuid);
            }
            foreach (Card card in toAdd) {
                __instance.SendCardToDeck(card, doAnimation: false, insertRandomly: true);
            }
        }
    }
}
