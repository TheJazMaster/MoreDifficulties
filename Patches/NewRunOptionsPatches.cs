using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static System.Reflection.BindingFlags;

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
			original: typeof(NewRunOptions).GetMethod("EnsureRunConfigIsGood", BindingFlags.Static | BindingFlags.NonPublic),
			transpiler: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_EnsureRunConfigIsGood_Transpiler", BindingFlags.Static | BindingFlags.NonPublic))
		);
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(NewRunOptions).GetMethod("OnMouseDown"),
			transpiler: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_OnMouseDown_Transpiler", BindingFlags.Static | BindingFlags.NonPublic))
		);
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(NewRunOptions).GetMethod("GetDifficultyColor"),
			postfix: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_GetDifficultyColor_Postfix", BindingFlags.Static | BindingFlags.NonPublic))
		);
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(NewRunOptions).GetMethod("GetDifficultyColorLogbook"),
			postfix: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_GetDifficultyColorLogbook_Postfix", BindingFlags.Static | BindingFlags.NonPublic))
		);
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(NewRunOptions).GetMethod("DifficultyOptions", BindingFlags.NonPublic | BindingFlags.Instance),
			transpiler: new HarmonyMethod(typeof(NewRunOptionsPatches).GetMethod("NewRunOptions_DifficultyOptions_Transpiler", BindingFlags.Static | BindingFlags.NonPublic))
		);
		
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
		try
		{
			return new SequenceBlockMatcher<CodeInstruction>(instructions)
				.AsGuidAnchorable()
				.Find(
					ILMatches.Ldloc<RunConfig>(originalMethod.GetMethodBody()!.LocalVariables),
					ILMatches.Ldloc<NewRunOptions.DifficultyLevel>(originalMethod.GetMethodBody()!.LocalVariables),
					ILMatches.Ldfld("level"),
					ILMatches.LdcI4(0),
					ILMatches.Instruction(OpCodes.Cgt).WithAutoAnchor(out Guid comparisonAnchor),
					ILMatches.Stfld("hardmode")
				)
				.PointerMatcher(comparisonAnchor)
				.Element(out var comparisonInstruction)
				.Replace(new CodeInstruction(OpCodes.Cgt_Un, comparisonInstruction.operand))
				.AllElements();
		}
		catch (Exception ex)
		{
			Instance.Logger!.LogError("Could not patch method {Method} - {Mod} probably won't work.\nReason: {Exception}", originalMethod, Instance.Name, ex);
			return instructions;
		}
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
}
