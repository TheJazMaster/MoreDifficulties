using FSPRO;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.AccessControl;
using static SharedArt;

namespace TheJazMaster.MoreDifficulties;

internal static class NewRunOptionsPatches
{
	private static Manifest Instance => Manifest.Instance;

	private static readonly Lazy<Func<Rect>> DifficultyPosGetter = new(() => AccessTools.DeclaredField(typeof(NewRunOptions), "difficultyPos").EmitStaticGetter<Rect>());
	private static readonly Lazy<Action<Rect>> DifficultyPosSetter = new(() => AccessTools.DeclaredField(typeof(NewRunOptions), "difficultyPos").EmitStaticSetter<Rect>());

	private static readonly int MarginReduction = 3;

	public static void Apply(Harmony harmony)
	{
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(NewRunOptions).GetMethod("EnsureRunConfigIsGood", AccessTools.all),
			transpiler: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_EnsureRunConfigIsGood_Transpiler", AccessTools.all))
		);
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(NewRunOptions).GetMethod("OnMouseDown", AccessTools.all),
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

		elem = new SequenceBlockMatcher<CodeInstruction>(elem)
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


		return new SequenceBlockMatcher<CodeInstruction>(elem)
			.Find(
				ILMatches.Ldarg(2),
				ILMatches.Ldfld("key"),
				ILMatches.LdcI4((int)UK.newRun_clear),
				ILMatches.AnyCall,
				ILMatches.Brfalse
			)
			.Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.IncludingInsertion, new List<CodeInstruction> {
				new(OpCodes.Ldarg_1),
				new(OpCodes.Call, typeof(NewRunOptionsPatches).GetMethod("ClearAlts", AccessTools.all))
			})
			.AllElements();	
	}

	private static void ClearAlts(G g)
	{
		var state = g.state;
		foreach((Deck deck, _) in DB.decks)
		{
			Instance.KokoroApi.RemoveExtensionData(state, deck.Key());
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

	// private static void NewRunOptions_Render_Postfix(G g)
	// {
	// 	// Rect rect = default(Rect) + new Vec(404, 210);
	// 	// Draw.Text(Loc.T(I18n.altStartersLoc, I18n.altStartersLocEn), rect.x + 16, rect.y, null, Colors.buttonBoxNormal);
	// 	// ButtonResult checkBox = CheckboxBig(onMouseDown: Input.gamepadIsActiveInput ? null : MouseListener, noHover: Input.gamepadIsActiveInput, g: g, localV: new Vec(0, 0), key: new UIKey(UK.card, 1), isSelected: Instance.AltStarters.AreAltStartersEnabled(), textColor: null, boxColor: Colors.buttonBoxNormal);
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