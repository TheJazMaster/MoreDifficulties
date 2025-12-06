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
using System.Text.RegularExpressions;
using Nickel;
using TheJazMaster.MoreDifficulties.Artifacts;

namespace TheJazMaster.MoreDifficulties;

internal static class StatePatches
{
	private static ModEntry Instance => ModEntry.Instance;
	private static IModData ModData => Instance.Helper.ModData;

	private static Type displayClass = null!;
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
        harmony.TryPatch(
			logger: Instance.Logger!,
            original: typeof(State).GetMethod("StartDailyRun", AccessTools.all, [typeof(DailyDescriptor)]),
            prefix: new HarmonyMethod(typeof(StatePatches).GetMethod("State_StartDailyRun_Prefix", AccessTools.all))
        );
        harmony.TryPatch(
			logger: Instance.Logger!,
            original: typeof(State).GetMethod("CleanupDailyRun", AccessTools.all),
            postfix: new HarmonyMethod(typeof(StatePatches).GetMethod("State_CleanupDailyRun_Postfix", AccessTools.all))
        );

		displayClass = (typeof(State).GetMembers().Where((MemberInfo m) => m.MemberType == MemberTypes.NestedType && (m as Type)!.GetMethods().Any((MethodInfo me) => Regex.Match(me.Name, ".*AddStartersForCharacter.*").Success)).First() as Type)!;
        harmony.TryPatch(
			logger: Instance.Logger!,
            original: displayClass.GetMethods().Where((MethodInfo me) => Regex.Match(me.Name, ".*AddStartersForCharacter.*").Success).First(),
            prefix: new HarmonyMethod(typeof(StatePatches).GetMethod("State_AddStartersForCharacter_Prefix", AccessTools.all))
        );
	}

	private static IEnumerable<CodeInstruction> State_PopulateRun_Delegate_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod, ILGenerator il)
	{
		try
		{
			var instr = new SequenceBlockMatcher<CodeInstruction>(instructions)
				.Find(
					ILMatches.Ldarg(0),
					ILMatches.Ldfld<State>()
				).PointerMatcher(SequenceMatcherRelativeElement.Last).Element();
			
			var easy = new SequenceBlockMatcher<CodeInstruction>(instructions).Find(
					ILMatches.Ldarg(0),
					ILMatches.Instruction(OpCodes.Ldfld),
					ILMatches.LdcI4(1),
					ILMatches.Blt
				)

				.PointerMatcher(SequenceMatcherRelativeElement.Last)
				.TryGetBranchTarget(out var branchTarget)

				.Encompass(SequenceMatcherEncompassDirection.Before, 1)
				.Replace(new CodeInstruction(OpCodes.Brfalse, branchTarget!.Value))

				.AllElements();
			
			return new SequenceBlockMatcher<CodeInstruction>(easy)
				.Find(
					ILMatches.Ldarg(0),
					ILMatches.Ldfld("chars"),
					ILMatches.LdcI4((int)Deck.colorless),
					ILMatches.Call("Contains"),
					ILMatches.Brfalse.GetBranchTarget(out var branch)
				)
				.Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.IncludingInsertion, [
					new(OpCodes.Ldarg_0),
					new(OpCodes.Ldfld, instr.operand),
					new(OpCodes.Call, typeof(StatePatches).GetMethod("ShouldCancelCatExeCards", AccessTools.all)),
					new(OpCodes.Brtrue, branch.Value)
				])
				.AllElements();
		}
		catch (Exception ex)
		{
			Instance.Logger!.LogError("Could not patch method {Method} - {Mod} probably won't work.\nReason: {Exception}", originalMethod, "MoreDifficulties", ex);
			return instructions;
		}
	}

	private static bool ShouldCancelCatExeCards(State state) {
		return AltStarters.AreAltStartersEnabled(state, Deck.colorless);
	}

    private static void State_PopulateRun_Postfix(State __instance, int difficulty) {
        if (difficulty >= ModEntry.Difficulty1) {
            List<Card> toRemove = [];
            List<Card> toAdd = [];
            foreach (Card card in __instance.deck)
            {
				Type type = card.GetType();
                if (type == typeof(CannonColorless)) {
                    toRemove.Add(card);
                    toAdd.Add(new BasicOffences {
						upgrade = card.upgrade
					});
                } else if (type == typeof(DodgeColorless)) {
                    toRemove.Add(card);
                    toAdd.Add(new BasicManeuvers {
						upgrade = card.upgrade
					});
                } else if (type == typeof(BasicShieldColorless)) {
                    toRemove.Add(card);
                    toAdd.Add(new BasicDefences {
						upgrade = card.upgrade
					});
                } else if (type == typeof(DroneshiftColorless)) {
                    toRemove.Add(card);
                    toAdd.Add(new BasicBroadcast {
						upgrade = card.upgrade
					});
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

	private static void State_StartDailyRun_Prefix(DailyDescriptor descriptor, State __instance) {
		foreach (Deck character in NewRunOptions.allChars) {
			ModData.RemoveModData(__instance, DailyDescriptorPatches.StorageKey(character));
		}
		foreach (Deck character in descriptor.crew) {
			// Store for when daily is over
			ModData.SetModData(__instance, DailyDescriptorPatches.StorageKey(character), AltStarters.AreAltStartersEnabled(__instance, character));

			if (ModData.TryGetModData(descriptor, DailyDescriptorPatches.Key(character), out bool altStarters))
				AltStarters.SetAltStarters(__instance, character, altStarters);
		}
	}

	private static void State_CleanupDailyRun_Postfix(State __instance) {
		foreach (Deck character in NewRunOptions.allChars) {
			if (ModData.TryGetModData(__instance, DailyDescriptorPatches.StorageKey(character), out bool altStarters)) {
				AltStarters.SetAltStarters(__instance, character, altStarters);
				ModData.RemoveModData(__instance, DailyDescriptorPatches.StorageKey(character));
			}
		}
	}

	private static bool State_AddStartersForCharacter_Prefix(object __instance, Deck d)
	{
		FieldInfo stateField = displayClass.GetFields().Where((FieldInfo f) => f.FieldType == typeof(State)).First();
		State state = (State) stateField!.GetValue(__instance)!;
		if (AltStarters.AreAltStartersEnabled(state, d)) {

			if (!AltStarters.altStarters.TryGetValue(d, out var value))
				StarterDeck.starterSets.TryGetValue(d, out value!);
			
			foreach (Card item4 in value.cards.Select((Card c) => c.CopyWithNewId()))
			{
				state.SendCardToDeck(item4);
			}
			foreach (Artifact item5 in value.artifacts.Select((Artifact r) => Mutil.DeepCopy(r)))
			{
				state.SendArtifactToChar(item5);
			}
			return false;
		}
		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(State), nameof(State.GetHarderEnemies))]
	private static void State_GetHarderEnemies_Postfix(ref bool __result, State __instance) {
        __result = __result || __instance.EnumerateAllArtifacts().Any(a => a is HarderEnemies || a is EvenHarderEnemies);
    }

	[HarmonyPostfix]
	[HarmonyPatch(typeof(State), nameof(State.GetHarderElites))]
	private static void State_GetHarderElites_Postfix(ref bool __result, State __instance) {
        __result = __result || __instance.EnumerateAllArtifacts().Any(a => a is HarderEnemies || a is EvenHarderEnemies);
    }

	[HarmonyPostfix]
	[HarmonyPatch(typeof(State), nameof(State.GetHarderBosses))]
	private static void State_GetHarderBosses_Postfix(ref bool __result, State __instance) {
        __result = __result || __instance.EnumerateAllArtifacts().Any(a => a is HarderEnemies || a is EvenHarderEnemies);
    }
}
