using HarmonyLib;

namespace TheJazMaster.MoreDifficulties;

internal static class CharacterPatches
{
	private static Manifest Instance => Manifest.Instance;

	public static void Apply(Harmony harmony)
	{
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(Character).GetMethod("Render", AccessTools.all),
			postfix: new HarmonyMethod(typeof(CharacterPatches).GetMethod("Character_Render_Postfix", AccessTools.all))
		);
	}

	private static void Character_Render_Postfix(Character __instance, G g, int x, int y, bool flipX, bool mini, bool? isSelected, bool autoFocus, UIKey rightHint, UIKey leftHint, UIKey downHint, UIKey upHint, bool renderLocked, bool canFocus, bool showTooltips)
	{
		RenderBoxes(__instance, g, x, y, flipX, mini, isSelected, autoFocus, rightHint, leftHint, downHint, upHint);
		RenderTooltips(__instance, g, mini, renderLocked, canFocus, showTooltips);
	}

	private static void RenderTooltips(Character __instance, G g, bool mini, bool renderLocked, bool canFocus, bool showTooltips)
	{
		if (!showTooltips || !canFocus || renderLocked || __instance.deckType is not { } deck)
			return;
		
		var key = new UIKey(mini ? StableUK.char_mini : StableUK.character, (int)deck, __instance.type);
		if (g.boxes.FirstOrDefault(b => b.key == key) is not { } box)
			return;
		if (!box.IsHover())
			return;

		if (Instance.AltStarters.AreAltStartersEnabled(g.state, deck))
			g.tooltips.AddText(g.tooltips.pos, Loc.T(I18n.altStartersDescLoc, I18n.altStartersDescLocEn));
	}

	private static void RenderBoxes(Character __instance, G g, int x, int y, bool flipX, bool mini, bool? isSelected, bool autoFocus, UIKey rightHint, UIKey leftHint, UIKey downHint, UIKey upHint)
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

}
