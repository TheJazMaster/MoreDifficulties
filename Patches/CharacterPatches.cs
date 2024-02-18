using HarmonyLib;
using Microsoft.Extensions.Logging;

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
		RenderLockAndBan(__instance, g, x, y, mini, isSelected, autoFocus, rightHint, leftHint, downHint, upHint);
	}

	private static void RenderTooltips(Character character, G g, bool mini, bool renderLocked, bool canFocus, bool showTooltips)
	{
		if (!showTooltips || !canFocus || renderLocked || character.deckType is not { } deck)
			return;
		
		var key = new UIKey(mini ? StableUK.char_mini : StableUK.character, (int)deck, character.type);
		if (g.boxes.FirstOrDefault(b => b.key == key) is not { } box)
			return;
		if (!box.IsHover())
			return;

		if (Instance.AltStarters.AreAltStartersEnabled(g.state, deck))
			g.tooltips.AddText(g.tooltips.pos, Loc.T(I18n.altStartersDescLoc, I18n.altStartersDescLocEn));
	}

	private static void RenderBoxes(Character character, G g, int x, int y, bool flipX, bool mini, bool? isSelected, bool autoFocus, UIKey rightHint, UIKey leftHint, UIKey downHint, UIKey upHint)
	{
		if (!mini || character.deckType is not { } deck || g.state.route is not { } route || !Instance.AltStarters.HasAltStarters(character.deckType.Value)) return;

		Instance.KokoroApi.TryGetExtensionData<RunSummaryRoute>(character, "runSummaryRoute", out var runSummaryRoute);
		bool altStartersEnabled = runSummaryRoute != null && Instance.KokoroApi.TryGetExtensionData<bool>(runSummaryRoute.runSummary, AltStarters.Key(deck), out var altOn) ? 
			altOn : Instance.AltStarters.AreAltStartersEnabled(g.state, character.deckType.Value);
		Spr sprite = altStartersEnabled ? (Spr)Manifest.AltStartersMarker.Id! : (Spr)Manifest.AltStartersMarkerOff.Id!;

		Rect rect = new(x, y, 35, 33);
		UIKey uiKey = new(UK.char_mini, (int)character.deckType.GetValueOrDefault(), "altStartersBox");
		
		Box box = g.Push(uiKey, rect, null, autoFocus, noHoverSound: false, gamepadUntargetable: false, ReticleMode.Quad, null, null, null, null, 0, rightHint, leftHint, upHint, downHint);
		Vec pos = box.rect.xy;

		Draw.Sprite(sprite, pos.x, pos.y, flipX: false, flipY: false, 0.0, null, null, null, new Rect(0, 0, 33, 33), DB.decks[deck].color.fadeAlpha((!isSelected.HasValue || isSelected.Value) ? 1 : 0.5));

		g.Pop();
	}

	private static void RenderLockAndBan(Character character, G g, int x, int y, bool mini, bool? isSelected, bool autoFocus, UIKey rightHint, UIKey leftHint, UIKey downHint, UIKey upHint)
	{
		var deckType = character.deckType;

		if (!mini || character.deckType is not { } deck || g.state.route is not NewRunOptions) return;

		if (Instance.LockAndBan.IsLocked(g.state, deck)) {
			Spr sprite = (Spr)Manifest.LockBorder.Id!;

			Rect rect = new(x, y, 35 + x, 33 + y);
			UIKey uiKey = new(UK.char_mini, (int)character.deckType.GetValueOrDefault(), "lockBorder");
			
			Box box = g.Push(uiKey, rect, null, autoFocus, noHoverSound: false, gamepadUntargetable: false, ReticleMode.Quad, null, null, null, null, 0, rightHint, leftHint, upHint, downHint);
			Vec pos = box.rect.xy;

			Draw.Sprite(sprite, pos.x, pos.y, flipX: false, flipY: false, 0.0, null, null, null, new Rect(0, 0, 33, 33), DB.decks[deckType!.Value].color.fadeAlpha((!isSelected.HasValue || isSelected.Value) ? 1 : 0.5));

			g.Pop();
		}
		if (Instance.LockAndBan.IsBanned(g.state, deck)) {
			Spr sprite = Manifest.Instance.AltStarters.HasAltStarters(deck) ? (Spr)Manifest.BanBorderAlt.Id! : (Spr)Manifest.BanBorder.Id!;

			Rect rect = new(x, y, 35, 33);
			UIKey uiKey = new(UK.char_mini, (int)character.deckType.GetValueOrDefault(), "banBorder");
			
			Box box = g.Push(uiKey, rect, null, autoFocus, noHoverSound: false, gamepadUntargetable: false, ReticleMode.Quad, null, null, null, null, 0, rightHint, leftHint, upHint, downHint);
			Vec pos = box.rect.xy;

			Draw.Sprite(sprite, pos.x, pos.y, flipX: false, flipY: false, 0.0, null, null, null, new Rect(0, 0, 33, 33), DB.decks[deckType!.Value].color.fadeAlpha((!isSelected.HasValue || isSelected.Value) ? 1 : 0.5));

			g.Pop();
		}
	}
}
