using FSPRO;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Mono.CompilerServices.SymbolWriter;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.AccessControl;
using static SharedArt;

namespace TheJazMaster.MoreDifficulties;

internal static class NewRunOptionsPatches
{
	private static Manifest Instance => Manifest.Instance;

	private static OnMouseDownRightHandler OnMouseDownRightHandler = null!;

	private static readonly Lazy<Func<Rect>> DifficultyPosGetter = new(() => AccessTools.DeclaredField(typeof(NewRunOptions), "difficultyPos").EmitStaticGetter<Rect>());
	private static readonly Lazy<Action<Rect>> DifficultyPosSetter = new(() => AccessTools.DeclaredField(typeof(NewRunOptions), "difficultyPos").EmitStaticSetter<Rect>());

	private static readonly int MarginReduction = 3;

	public static void Apply(Harmony harmony)
	{
		OnMouseDownRightHandler = new OnMouseDownRightHandler();

		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(NewRunOptions).GetMethod("EnsureRunConfigIsGood", AccessTools.all),
			transpiler: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_EnsureRunConfigIsGood_Transpiler", AccessTools.all))
		);
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(NewRunOptions).GetMethod("OnMouseDown", AccessTools.all),
			postfix: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_OnMouseDown_Postfix", AccessTools.all)), 
			transpiler: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_OnMouseDown_Transpiler", AccessTools.all))
		);
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(NewRunOptions).GetMethod("GetDifficultyColor", AccessTools.all),
			postfix: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_GetDifficultyColor_Postfix", AccessTools.all))
		);
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(NewRunOptions).GetMethod("GetDifficultyColorLogbook", AccessTools.all),
			postfix: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_GetDifficultyColorLogbook_Postfix", AccessTools.all))
		);
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(NewRunOptions).GetMethod("DifficultyOptions", AccessTools.all),
			transpiler: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_DifficultyOptions_Transpiler", AccessTools.all))
		);
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(NewRunOptions).GetMethod("Randomize", AccessTools.all),
			postfix: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_Randomize_Postfix", AccessTools.all)),
			transpiler: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_Randomize_Transpiler", AccessTools.all))
		);
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(NewRunOptions).GetMethod("Clear", AccessTools.all),
			postfix: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_Clear_Postfix", AccessTools.all))
		);
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(NewRunOptions).GetMethod("CharSelect", AccessTools.all),
			postfix: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_CharSelect_Postfix", AccessTools.all))
		);
		// harmony.TryPatch(
		// 	logger: Instance.Logger!,
		// 	original: typeof(NewRunOptions).GetMethod("Render", AccessTools.all),
		// 	postfix: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_Render_Postfix", AccessTools.all))
		// );
		
        NewRunOptions.difficulties.Insert(Manifest.Difficulty1, new NewRunOptions.DifficultyLevel
		{
			uiKey = "difficulty_brutal",
			locKey = I18n.difficultyLoc1,
			color = NewRunOptions.GetDifficultyColor(Manifest.Difficulty1),
			level = Manifest.Difficulty1
		});
        NewRunOptions.difficulties.Insert(Manifest.Difficulty2, new NewRunOptions.DifficultyLevel
		{
			uiKey = "difficulty_cosmic",
			locKey = I18n.difficultyLoc2,
			color = NewRunOptions.GetDifficultyColor(Manifest.Difficulty2),
			level = Manifest.Difficulty2
		});
		NewRunOptions.difficulties.Insert(0, new NewRunOptions.DifficultyLevel
		{
			uiKey = "difficulty_easy",
			locKey = "newRunOptions.difficultyEasy",
			color = NewRunOptions.GetDifficultyColor(Manifest.Easy),
			level = Manifest.Easy
		});

		var difficultyPos = DifficultyPosGetter.Value();
		difficultyPos.y -= (NewRunOptions.difficulties.Count - 4) * (10 - MarginReduction);
		DifficultyPosSetter.Value(difficultyPos);
	}
	// ADD RUN HISTORY REMEMBERING ALT STARTERS
	// ALSO THEY'RE LIKE 38x38
	private static IEnumerable<CodeInstruction> NewRunOptions_EnsureRunConfigIsGood_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
	{
		try
		{
			return new SequenceBlockMatcher<CodeInstruction>(instructions)
				.Find(
					ILMatches.Ldarg(0),
					ILMatches.Ldarg(0),
					ILMatches.Ldfld("difficulty"),
					ILMatches.LdcI4(1),
					ILMatches.LdcI4(3),
					ILMatches.Call("Clamp"),
					ILMatches.Stfld("difficulty")
				)
				.Remove()
				.AllElements();
		}
		catch (Exception ex)
		{
			Instance.Logger!.LogError("Could not patch method {Method} - {Mod} probably won't work.\nReason: {Exception}", originalMethod, Instance.Name, ex);
			return instructions;
		}
	}

	private static void NewRunOptions_OnMouseDown_Postfix(G g, Box b)
	{
		RunConfig runConfig = g.state.runConfig;
		int? num = b.key?.ValueFor(UK.char_mini);
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			Deck deck = (Deck)valueOrDefault;
			if (runConfig.selectedChars.Contains(deck)) {
				if (Instance.LockAndBan.IsBanned(g.state, deck))
					Instance.LockAndBan.SetBan(g.state, deck, false);
			} else {
				if (Instance.LockAndBan.IsLocked(g.state, deck))
					Instance.LockAndBan.SetLock(g.state, deck, false);
			}
		}
	}

	private static IEnumerable<CodeInstruction> NewRunOptions_OnMouseDown_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
	{
		var elem = new SequenceBlockMatcher<CodeInstruction>(instructions)
			.Find(
				ILMatches.Ldloc<RunConfig>(originalMethod.GetMethodBody()!.LocalVariables),
				ILMatches.Ldloc<NewRunOptions.DifficultyLevel>(originalMethod.GetMethodBody()!.LocalVariables),
				ILMatches.Ldfld("level"),
				ILMatches.LdcI4(0),
				ILMatches.Instruction(OpCodes.Cgt).Anchor(out Guid comparisonAnchor),
				ILMatches.Stfld("hardmode")
			)
			.Anchors()
			.PointerMatcher(comparisonAnchor)
			.Element(out var comparisonInstruction)
			.Replace(new CodeInstruction(OpCodes.Cgt_Un, comparisonInstruction.operand))
			.AllElements();

		return new SequenceBlockMatcher<CodeInstruction>(elem)
			.Find(
				ILMatches.Ldloc<RunConfig>(originalMethod),
				ILMatches.Ldfld("selectedChars"),
				ILMatches.Ldloc<Deck>(originalMethod).CreateLdlocInstruction(out var ldLoc),
				ILMatches.Call("Contains"),
				ILMatches.Brfalse.Anchor(out var anchor)
			)
			.EncompassUntil(SequenceMatcherPastBoundsDirection.After, ILMatches.Br.GetBranchTarget(out var end))
			.Anchors()
			.PointerMatcher(anchor)
			.Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.IncludingInsertion, new List<CodeInstruction> {
				new(OpCodes.Ldarg_1),
				ldLoc.Value,
				new(OpCodes.Call, typeof(NewRunOptionsPatches).GetMethod("ToggleAltEnabled", AccessTools.all)),
				new(OpCodes.Brfalse, end.Value)
			})
			.AllElements();
	}

	private static void NewRunOptions_Clear_Postfix(State s)
	{
		ClearAlts(s);
		ReselectLocks(s);
	}

	private static void ClearAlts(State state)
	{
		foreach((Deck deck, _) in DB.decks)
		{
			if (!Instance.LockAndBan.IsLocked(state, deck))
				Instance.KokoroApi.RemoveExtensionData(state, AltStarters.Key(deck));
		}
	}
	private static void ReselectLocks(State state)
	{
		// foreach((Deck deck, _) in DB.decks)
		// {
		// 	Instance.KokoroApi.RemoveExtensionData(state, LockAndBan.KeyLock(deck));
		// }
		foreach((Deck deck, _) in DB.decks)
		{
			if (Instance.LockAndBan.IsLocked(state, deck))
				state.runConfig.selectedChars.Add(deck);
		}
	}

	private static void NewRunOptions_Randomize_Postfix(State s, Rand rng)
	{
		ClearAlts(s);
		foreach (Deck character in s.runConfig.selectedChars) {
			if (Instance.AltStarters.HasAltStarters(character) && !Instance.LockAndBan.IsLocked(s, character)) {
				Instance.AltStarters.SetAltStarters(s, character, rng.Next() >= 0.5);
			}
		}
	}

	private static bool ToggleAltEnabled(G g, Deck deck)
	{
		State state = g.state;
		bool hasAltStarters = Instance.AltStarters.HasAltStarters(deck);
		bool enabled = hasAltStarters && Instance.AltStarters.AreAltStartersEnabled(state, deck);
		if (!enabled) Audio.Play(Event.Click);
		Instance.AltStarters.SetAltStarters(state, deck, !enabled);
		return enabled || !hasAltStarters;
	}

	private static void NewRunOptions_GetDifficultyColor_Postfix(int level, ref Color __result)
	{
		if (level == Manifest.Easy)
			__result = Color.Lerp(Colors.textMain, Colors.midrow, Math.Abs(level) / 3.0);

		if (level == Manifest.Difficulty1 || level == Manifest.Difficulty2)
			__result = Color.Lerp(Colors.enemyName, new Color("ffa0a0"), (level-3) / 2.0);
	}

	private static void NewRunOptions_GetDifficultyColorLogbook_Postfix(int level, ref Color __result)
	{
		if (level == Manifest.Easy)
			__result = Color.Lerp(new Color(0.2, 0.3, 0.9), Colors.midrow, Math.Pow(Math.Abs(level) / 3.0, 2.0));

		if (level == Manifest.Difficulty1 || level == Manifest.Difficulty2)
			__result = Color.Lerp(Colors.enemyName, new Color("ffa0a0"), (level-3) / 2.0);
	}

	private static IEnumerable<CodeInstruction> NewRunOptions_DifficultyOptions_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
	{
		try
		{
			return new SequenceBlockMatcher<CodeInstruction>(instructions)
				.Find(
					ILMatches.Ldloc<int>(originalMethod.GetMethodBody()!.LocalVariables),
					ILMatches.LdcI4(20),
					new ElementMatch<CodeInstruction>("{mul}", i => i.opcode == OpCodes.Mul)
				)
				.PointerMatcher(SequenceMatcherRelativeElement.First)
				.CreateLdlocInstruction(out var ldLoc)
				.Encompass(SequenceMatcherEncompassDirection.After, 2)
				.Replace(
					ldLoc,
					new CodeInstruction(OpCodes.Ldc_I4, 20 - MarginReduction),
					new CodeInstruction(OpCodes.Mul)
				)
				.AllElements();
		}
		catch (Exception ex)
		{
			Instance.Logger!.LogError("Could not patch method {Method} - {Mod} probably won't work.\nReason: {Exception}", originalMethod, Instance.Name, ex);
			return instructions;
		}
	}

	private static IEnumerable<CodeInstruction> NewRunOptions_Randomize_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
	{
		return new SequenceBlockMatcher<CodeInstruction>(instructions)
			.Find(
				ILMatches.Ldarg(1).Anchor(out var anchor),
				ILMatches.Ldfld("storyVars"),
				ILMatches.AnyCall,
				ILMatches.Ldarg(2),
				ILMatches.Call("Shuffle"),
				ILMatches.LdcI4(3)
			)
			.EncompassUntil(SequenceMatcherPastBoundsDirection.After, ILMatches.Call("ToHashSet"))
			.Replace(new List<CodeInstruction>() {
				new(OpCodes.Ldarg_1),
				new(OpCodes.Ldarg_2),
				new(OpCodes.Call, typeof(NewRunOptionsPatches).GetMethod("GetShuffledCharactersWithLocksAndBans", AccessTools.all))
			})
			.AllElements();
	}

	private static HashSet<Deck> GetShuffledCharactersWithLocksAndBans(State s, Rand rng)
	{
		var locked = s.storyVars.GetUnlockedChars().Where(deck => Instance.LockAndBan.IsLocked(s, deck));
		return locked.Concat(s.storyVars.GetUnlockedChars().Where(deck => !Instance.LockAndBan.IsBanned(s, deck) && !Instance.LockAndBan.IsLocked(s, deck)).Shuffle(rng).Take(Math.Max(0, 3-locked.Count()))).ToHashSet();
	}

	private static void NewRunOptions_CharSelect_Postfix(NewRunOptions __instance, G g, RunConfig runConfig, HashSet<Deck> unlockedChars)
	{
		foreach (Deck deck in unlockedChars)
		{
			var key = new UIKey(StableUK.char_mini, (int)deck, deck.Key());
			if (g.boxes.FirstOrDefault(b => b.key == key) is not { } box)
				continue;

			box.onMouseDownRight = OnMouseDownRightHandler;
		}
	}

	// private static void NewRunOptions_Render_Postfix(G g)
	// {
	// 	// Rect rect = default(Rect) + new Vec(404, 210);
	// 	// Draw.Text(Loc.T(I18n.altStartersLoc, I18n.altStartersLocEn), rect.x + 16, rect.y, null, Colors.buttonBoxNormal);
	// 	// ButtonResult checkBox = CheckboxBig(onMouseDown: Input.gamepadIsActiveInput ? null : MouseListener, noHover: Input.gamepadIsActiveInput, g: g, localV: new Vec(0, 0), key: new UIKey(StableUK.card, 1), isSelected: Instance.AltStarters.AreAltStartersEnabled(), textColor: null, boxColor: Colors.buttonBoxNormal);
	// 	// if (checkBox.isHover)
	// 	// {
	// 	// 	g.tooltips.Add(checkBox.v - new Vec(110, 25), new TTText(Loc.T(I18n.altStartersDescLoc, I18n.altStartersDescLocEn)));
	// 	// }

	// 	Rect rect = new(10.0, 92.0);
	// 	g.Push(null, rect);
	// 	int ix = 0;
	// 	int iy = 0;
	// 	foreach (Deck deck in NewRunOptions.allChars)
	// 	{
	// 		rect = new Rect(34 * ix, 32 * iy);
	// 		Vec xy = g.Push(null, rect).rect.xy;
	// 		if (Instance.AltStarters.AreAltStartersEnabled(deck))
	// 		{
	// 			double x = xy.x + 26.0;
	// 			double y = xy.y + 2.0;
	// 			Color? boxColor = DB.decks[deck].color;
	// 			Draw.Sprite((Spr)Manifest.AltStartersMarker.Id!, x, y, flipX: false, flipY: false, 0.0, null, null, null, null, boxColor);
	// 		}
	// 		g.Pop();
	// 	}
	// }

	// private static void NewRunOptions_Render_Postfix(G g)
	// {
	// 	// Rect rect = default(Rect) + new Vec(404, 210);
	// 	// Draw.Text(Loc.T(I18n.altStartersLoc, I18n.altStartersLocEn), rect.x + 16, rect.y, null, Colors.buttonBoxNormal);
	// 	// ButtonResult checkBox = CheckboxBig(onMouseDown: Input.gamepadIsActiveInput ? null : MouseListener, noHover: Input.gamepadIsActiveInput, g: g, localV: new Vec(0, 0), key: new UIKey(StableUK.card, 1), isSelected: Instance.AltStarters.AreAltStartersEnabled(), textColor: null, boxColor: Colors.buttonBoxNormal);
	// 	// if (checkBox.isHover)
	// 	// {
	// 	// 	g.tooltips.Add(checkBox.v - new Vec(110, 25), new TTText(Loc.T(I18n.altStartersDescLoc, I18n.altStartersDescLocEn)));
	// 	// }

	// 	Rect rect = new(10.0, 92.0);
	// 	g.Push(null, rect);
	// 	int ix = 0;
	// 	int iy = 0;
	// 	foreach (Deck deck in NewRunOptions.allChars)
	// 	{
	// 		rect = new Rect(34 * ix, 32 * iy);
	// 		Vec xy = g.Push(null, rect).rect.xy;
	// 		if (Instance.AltStarters.AreAltStartersEnabled(deck))
	// 		{
	// 			double x = xy.x + 26.0;
	// 			double y = xy.y + 2.0;
	// 			Color? boxColor = DB.decks[deck].color;
	// 			Draw.Sprite((Spr)Manifest.AltStartersMarker.Id!, x, y, flipX: false, flipY: false, 0.0, null, null, null, null, boxColor);
	// 		}
	// 		g.Pop();
	// 	}


	// 	g.Pop();
	// }
}

sealed class OnMouseDownRightHandler : OnMouseDownRight
{
	internal static LockAndBan LockAndBan => Manifest.Instance.LockAndBan;
	public void OnMouseDownRight(G g, Box b)
	{
		int? num = b.key?.ValueFor(UK.char_mini);
		if (num.HasValue)
		{
			State s = g.state;
			RunConfig runConfig = s.runConfig;
			Deck deck = (Deck)num.GetValueOrDefault();

			if (runConfig.selectedChars.Contains(deck))
			{
				Audio.Play(Event.Click);
				LockAndBan.SetLock(g.state, deck, !LockAndBan.IsLocked(s, deck));
			}
			else
			{
				Audio.Play(Event.Click);
				LockAndBan.SetBan(g.state, deck, !LockAndBan.IsBanned(s, deck));
			}
		}
	}
}