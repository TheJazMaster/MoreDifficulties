using HarmonyLib;
using TheJazMaster.MoreDifficulties.Cards;
using TheJazMaster.MoreDifficulties.Actions;
using System.Reflection;
using static System.Reflection.BindingFlags;
using Microsoft.Extensions.Logging;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using System.Reflection.Emit;
using System.Text;
using System.Runtime.CompilerServices;

namespace TheJazMaster.MoreDifficulties;

internal static class CharacterPatches
{
	private static Manifest Instance => Manifest.Instance;

	public static void Apply(Harmony harmony)
	{
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(Character).GetMethod("Render", AccessTools.all),
			postfix: new HarmonyMethod(typeof(CharacterPatches).GetMethod("Character_Render_Postfix", AccessTools.all)),
			transpiler: new HarmonyMethod(typeof(CharacterPatches).GetMethod("Character_Render_Transpiler", AccessTools.all))
		);
	}

	private static void Character_Render_Postfix(Character __instance, G g, int x, int y, bool flipX, bool mini, bool? isSelected, bool autoFocus, UIKey rightHint, UIKey leftHint, UIKey downHint, UIKey upHint)
	{
		var deckType = __instance.deckType;

		if (!mini || !__instance.deckType.HasValue || !Instance.AltStarters.HasAltStarters(__instance.deckType.Value)) return;

		bool altStartersEnabled = Instance.AltStarters.AreAltStartersEnabled(g.state, __instance.deckType.Value);
		Spr sprite = altStartersEnabled ? (Spr)Manifest.AltStartersMarker.Id! : (Spr)Manifest.AltStartersMarkerOff.Id!;

		Rect rect = new(x, y, 35, 33);
		UIKey uiKey = new(UK.char_mini, (int)__instance.deckType.GetValueOrDefault() + 1000);
		
		Box box = g.Push(uiKey, rect, null, autoFocus, noHoverSound: false, gamepadUntargetable: false, ReticleMode.Quad, null, null, null, null, 0, rightHint, leftHint, upHint, downHint);
		Vec pos = box.rect.xy;

		Draw.Sprite(sprite, pos.x, pos.y, flipX: false, flipY: false, 0.0, null, null, null, new Rect(0.0, 0.0, 29.0, 29.0), DB.decks[deckType!.Value].color.fadeAlpha((!isSelected.HasValue || isSelected.Value) ? 1 : 0.5));

		g.Pop();
	}
	private static IEnumerable<CodeInstruction> Character_Render_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
	{
		try
		{
			return new SequenceBlockMatcher<CodeInstruction>(instructions)
				.Find(
					ILMatches.Ldarg(1),
					ILMatches.Ldfld("tooltips"),
					ILMatches.Ldloc<Vec>(originalMethod).CreateLdlocInstruction(out var ldlocPos)
				)
				.Find(
					ILMatches.Ldstr(".desc"),
					ILMatches.Call("AppendLiteral"),
					ILMatches.AnyLdloca,
					ILMatches.Call("ToStringAndClear"),
					ILMatches.Call("AddGlossary")
				)
				.PointerMatcher(SequenceMatcherRelativeElement.Last)
				.Insert(
					SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.IncludingInsertion,
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Ldarg_1),
					ldlocPos,
					new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(CharacterPatches), nameof(AddTooltips)))
				)
				.AllElements();
		}
		catch (Exception ex)
		{
			Instance.Logger!.LogError("Could not patch method {Method} - {Mod} probably won't work.\nReason: {Exception}", originalMethod, Instance.Name, ex);
			return instructions;
		}
	}

	private static void AddTooltips(Character character, G g, Vec pos)
	{
		if (character.deckType is not { } deck)
			return;

		if (Instance.AltStarters.AreAltStartersEnabled(g.state, deck))
			g.tooltips.AddText(pos, Loc.T(I18n.altStartersDescLoc, I18n.altStartersDescLocEn));
	}
}
