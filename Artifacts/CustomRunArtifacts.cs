using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FMOD;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using Nickel.ModSettings;
using Shockah.CustomRunOptions;
using TheJazMaster.MoreDifficulties.Cards;

namespace TheJazMaster.MoreDifficulties.Artifacts;

public class CustomRunArtifacts : ICustomRunOptionsApi.ICustomRunOption
{
    private static ICustomRunOptionsApi api = null!;

    public CustomRunArtifacts(ICustomRunOptionsApi api) {
        CustomRunArtifacts.api = api;

        ModEntry.Instance.Harmony.TryPatch(
            logger: ModEntry.Instance.Logger,
            original: AccessTools.DeclaredMethod(typeof(State), nameof(State.PopulateRun)),
            postfix: AccessTools.DeclaredMethod(GetType(), nameof(State_PopulateRun_Postfix))
        );
    }

    internal static IModSettingsApi SettingsApi => ModEntry.Instance.SettingsApi;
    internal static IModData ModData => ModEntry.Instance.Helper.ModData;
    internal static ILocaleBoundNonNullLocalizationProvider<IReadOnlyList<string>> Localizations => ModEntry.Instance.Localizations;

    internal static readonly string CustomRunOptionKey = "CustomRunArtifact";

    public static readonly List<string> Modifiers = ["EvadeCap", "WorseBasics", "TrashOnShuffle", "HarderEnemies", "EvenHarderEnemies"];

    private static Artifact ModifierKeyToArtifact(string s) => s switch {
        "EvadeCap" => new EvadeCap(),
        "WorseBasics" => new WorseBasics(),
        "TrashOnShuffle" => new TrashOnShuffle(),
        "HarderEnemies" => new HarderEnemies(),
        "EvenHarderEnemies" => new EvenHarderEnemies(),
        _ => throw new NotImplementedException()
    };

    public IReadOnlyList<ICustomRunOptionsApi.INewRunOptionsElement> GetNewRunOptionsElements(G g, RunConfig config) {
        var ret = new List<ICustomRunOptionsApi.INewRunOptionsElement>();
        foreach (string s in Modifiers) {
            if (ModData.GetModDataOrDefault(config, CustomRunOptionKey + s, false)) ret.Add(api.MakeArtifactNewRunOptionsElement(ModifierKeyToArtifact(s)));
        }
        return ret;
    }

    public IModSettingsApi.IModSetting MakeCustomRunSettings(NewRunOptions baseRoute, G g, RunConfig config)
    {
        return SettingsApi.MakeList([
            SettingsApi.MakePadding(SettingsApi.MakeText(() => $"<c=white>{ModEntry.Instance.Localizations.Localize(["settings", "customRunCategory"])}</c>"
				).SetFont(DB.thicket), 8, 4),
            .. Modifiers.Select<string, IModSettingsApi.IModSetting>(s =>
                api.MakeIconAffixModSetting(
                    SettingsApi.MakeCheckbox(
                        () => ModEntry.Instance.Localizations.Localize(["artifact", s, "name"]),
                        () => ModData.GetModDataOrDefault(config, CustomRunOptionKey + s, false),
                        (_, _, to) => ModData.SetModData(config, CustomRunOptionKey + s, to)
                    ).SetTooltips(() => [
                        new GlossaryTooltip($"settings.{ModEntry.Instance.Package.Manifest.UniqueName}::{s}")
                        {
                            TitleColor = Colors.textBold,
                            Title = Localizations.Localize(["artifact", s, "name"]),
                            Description = Localizations.Localize(["artifact", s, "description"])
                        }
                    ])
                ).SetLeftIcon(api.MakeIconAffixModSettingIconSetting().SetIcon(ModifierKeyToArtifact(s).GetSprite()))
            )]
        );
    }

	private static void State_PopulateRun_Postfix(State __instance)
	{
		if (!api.IsStartingNormalRun)
			return;
		
		foreach (string s in Modifiers)
			if (ModData.GetModDataOrDefault(__instance.runConfig, CustomRunOptionKey + s, false)) {
                __instance.SendArtifactToChar(ModifierKeyToArtifact(s));
            }
				
	}
}

internal sealed class EvadeCap : Artifact, IRegisterableSettingsArtifact
{
    public static void Register(IModHelper helper, IPluginPackage<IModManifest> package)
    {
        IRegisterableSettingsArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, helper, package);
    }

    public override void OnReceiveArtifact(State state)
    {
        if (!state.ship.evadeMax.HasValue || state.ship.evadeMax > 6) {
            state.ship.evadeMax = 7;
        }
    }
}

internal sealed class WorseBasics : Artifact, IRegisterableSettingsArtifact
{
    public static void Register(IModHelper helper, IPluginPackage<IModManifest> package)
    {
        IRegisterableSettingsArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, helper, package);
    }

    public override void OnReceiveArtifact(State state)
    {
        List<Card> toRemove = [];
        List<Card> toAdd = [];
        foreach (Card card in state.deck)
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
            state.RemoveCardFromWhereverItIs(card.uuid);
        }
        foreach (Card card in toAdd) {
            state.SendCardToDeck(card, doAnimation: false, insertRandomly: true);
        }
    }
}

internal sealed class TrashOnShuffle : Artifact, IRegisterableSettingsArtifact
{
    public static void Register(IModHelper helper, IPluginPackage<IModManifest> package)
    {
        IRegisterableSettingsArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, helper, package);
    }

    public override void OnPlayerDeckShuffle(State state, Combat combat)
    {
        combat.QueueImmediate(new AAddCard {
                amount = 1,
                card = new Fatigue(),
				artifactPulse = Key()
            });
    }

    public override List<Tooltip>? GetExtraTooltips() => [
        new TTCard() {
            card = new Fatigue()
        }
    ];
}

internal sealed class EvenHarderEnemies : Artifact, IRegisterableSettingsArtifact
{
    public static void Register(IModHelper helper, IPluginPackage<IModManifest> package)
    {
        IRegisterableSettingsArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, helper, package);
    }

    public override void OnReceiveArtifact(State state)
    {
        if (state.EnumerateAllArtifacts().OfType<HarderEnemies>().FirstOrDefault() is {} artifact) ALoseArtifact.DoRemoveArtifact(state, artifact.Key());
    }
}

internal sealed class HarderEnemies : Artifact, IRegisterableSettingsArtifact
{
    public static void Register(IModHelper helper, IPluginPackage<IModManifest> package)
    {
        IRegisterableSettingsArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, helper, package);
    }

    public override void OnReceiveArtifact(State state)
    {
        if (state.EnumerateAllArtifacts().OfType<EvenHarderEnemies>().Any()) ALoseArtifact.DoRemoveArtifact(state, Key());
    }
}