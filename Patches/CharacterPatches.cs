using System.Linq;
using HarmonyLib;
using Nickel;

namespace TheJazMaster.MoreDifficulties;

[HarmonyPatch]
internal static class CharacterPatches
{
	private static ModEntry Instance => ModEntry.Instance;
	private static IModData ModData => Instance.Helper.ModData;

	internal static bool skipRenderingCharacterExtras = false;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Character), nameof(Character.Render))]
	private static void Character_Render_Postfix(Character __instance, G g, int x, int y, bool flipX, bool mini, bool? isSelected, bool autoFocus, UIKey rightHint, UIKey leftHint, UIKey downHint, UIKey upHint, bool renderLocked, bool canFocus, bool showTooltips, UIKey? overrideKey = null)
	{
		if (__instance.deckType is not { } deck) return;

		ModData.TryGetModData<RunSummaryRoute>(__instance, "runSummaryRoute", out var runSummaryRoute);
		bool altStartersEnabled = runSummaryRoute != null && ModData.TryGetModData<bool>(runSummaryRoute.runSummary, AltStarters.Key(deck), out var altOn) ? 
			altOn : (g.state.route is NewRunOptions nro && nro.subRoute is DailyPreview preview && preview._descriptor != null && ModData.TryGetModData(preview._descriptor, DailyDescriptorPatches.Key(deck), out bool altOnFordaily) ?
			altOnFordaily : AltStarters.AreAltStartersEnabled(g.state, deck));

		RenderBoxes(__instance, g, altStartersEnabled, deck, x, y, flipX, mini, isSelected, autoFocus, rightHint, leftHint, downHint, upHint, runSummaryRoute, overrideKey, renderLocked);
		RenderTooltips(__instance, g, altStartersEnabled, deck, mini, renderLocked, canFocus, showTooltips, overrideKey);
		RenderLockAndBan(__instance, g, altStartersEnabled, deck, x, y, mini, isSelected, autoFocus, rightHint, leftHint, downHint, upHint, runSummaryRoute, overrideKey, renderLocked);
	}

	private static void RenderTooltips(Character character, G g, bool altStartersEnabled, Deck deck, bool mini, bool renderLocked, bool canFocus, bool showTooltips, UIKey? overrideKey)
	{
		if (!showTooltips || skipRenderingCharacterExtras || !canFocus || renderLocked)
			return;
		
		var key = overrideKey != null ? overrideKey.Value : new UIKey(mini ? StableUK.char_mini : StableUK.character, (int)deck, character.type);
		if (g.boxes.FirstOrDefault(b => b.key == key) is not { } box)
			return;
		if (!box.IsHover())
			return;

		if (altStartersEnabled)
			g.tooltips.AddText(g.tooltips.pos, Loc.T(I18n.altStartersDescLoc, I18n.altStartersDescLocEn));
	}

	private static void RenderBoxes(Character character, G g, bool altStartersEnabled, Deck deck, int x, int y, bool flipX, bool mini, bool? isSelected, bool autoFocus, UIKey rightHint, UIKey leftHint, UIKey downHint, UIKey upHint, RunSummaryRoute? runSummaryRoute, UIKey? overrideKey, bool renderLocked)
	{
		if (!mini || skipRenderingCharacterExtras || renderLocked || g.state.route is not { } route || !(IsInRunOptionsScreen(g) || IsInDailyScreen(g) || IsInRunSummaryScreen(g) || AreCockpitPanelsShown(g)) || !AltStarters.HasAltStarters(deck))
			return;

		Spr sprite = altStartersEnabled ? ModEntry.AltStartersMarker : ModEntry.AltStartersMarkerOff;

		Rect rect = new(x, y, 35, 33);
		UK k = StableUK.char_mini; int v = (int)character.deckType.GetValueOrDefault(); string str = "altStartersBox";
		if (overrideKey.HasValue) {
			k = overrideKey.Value.k;
			v = overrideKey.Value.v;
			str += overrideKey.Value.str;
		}
		UIKey uiKey = new(k, v, str);

		Box box = g.Push(uiKey, rect, null, autoFocus, noHoverSound: false, gamepadUntargetable: true, ReticleMode.Quad, null, null, null, null, 0, rightHint, leftHint, upHint, downHint);
		Vec pos = box.rect.xy;

		Draw.Sprite(sprite, pos.x, pos.y, flipX: false, flipY: false, 0.0, null, null, null, new Rect(0, 0, 33, 33), DB.decks[deck].color.fadeAlpha((!isSelected.HasValue || isSelected.Value) ? 1 : 0.5));

		g.Pop();
	}

	private static void RenderLockAndBan(Character character, G g, bool altStartersEnabled, Deck deck, int x, int y, bool mini, bool? isSelected, bool autoFocus, UIKey rightHint, UIKey leftHint, UIKey downHint, UIKey upHint, RunSummaryRoute? runSummaryRoute, UIKey? overrideKey, bool renderLocked)
	{
		var deckType = character.deckType;

		if (!mini || skipRenderingCharacterExtras || renderLocked || !IsInRunOptionsScreen(g) || runSummaryRoute != null)
			return;

		if (LockAndBan.IsLocked(g.state, deck)) {
			Spr sprite = ModEntry.LockBorder;

			Rect rect = new(x, y, 35 + x, 33 + y);
			UK k = StableUK.char_mini; int v = (int)character.deckType.GetValueOrDefault(); string str = "lockBorder";
			if (overrideKey.HasValue) {
				k = overrideKey.Value.k;
				v = overrideKey.Value.v;
				str += overrideKey.Value.str;
			}
			UIKey uiKey = new(k, v, str);
			
			Box box = g.Push(uiKey, rect, null, autoFocus, noHoverSound: false, gamepadUntargetable: false, ReticleMode.Quad, null, null, null, null, 0, rightHint, leftHint, upHint, downHint);
			Vec pos = box.rect.xy;

			Draw.Sprite(sprite, pos.x, pos.y, flipX: false, flipY: false, 0.0, null, null, null, new Rect(0, 0, 33, 33), DB.decks[deckType!.Value].color.fadeAlpha((!isSelected.HasValue || isSelected.Value) ? 1 : 0.5));

			g.Pop();
		}
		if (LockAndBan.IsBanned(g.state, deck)) {
			Spr sprite = AltStarters.HasAltStarters(deck) ? ModEntry.BanBorderAlt : ModEntry.BanBorder;

			Rect rect = new(x, y, 35, 33);
			UK k = StableUK.char_mini; int v = (int)character.deckType.GetValueOrDefault(); string str = "banBorder";
			if (overrideKey.HasValue) {
				k = overrideKey.Value.k;
				v = overrideKey.Value.v;
				str += overrideKey.Value.str;
			}
			UIKey uiKey = new(k, v, str);
			
			Box box = g.Push(uiKey, rect, null, autoFocus, noHoverSound: false, gamepadUntargetable: false, ReticleMode.Quad, null, null, null, null, 0, rightHint, leftHint, upHint, downHint);
			Vec pos = box.rect.xy;

			Draw.Sprite(sprite, pos.x, pos.y, flipX: false, flipY: false, 0.0, null, null, null, new Rect(0, 0, 33, 33), DB.decks[deckType!.Value].color.fadeAlpha((!isSelected.HasValue || isSelected.Value) ? 1 : 0.5));

			g.Pop();
		}
	}

	private static bool IsInRunOptionsScreen(G g) {
		return g.state.route is NewRunOptions && g.metaRoute == null && !IsInRunSummaryScreen(g);
	}

	private static bool IsInDailyScreen(G g) {
		return (g.state.route is DailyPreview) && g.metaRoute == null && !IsInRunSummaryScreen(g);
	}

	private static bool AreCockpitPanelsShown(G g) {
		return g.metaRoute == null && (g.state.routeOverride ?? g.state.route).GetShowOverworldPanels();
	}

	// Close enough?
	private static bool IsInRunSummaryScreen(G g) {
		return g.state.route is RunSummaryRoute || (g.metaRoute?.subRoute is Codex codex && codex.subRoute is RunHistoryList);
	}
}
