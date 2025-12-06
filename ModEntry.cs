using HarmonyLib;
using Microsoft.Extensions.Logging;
using TheJazMaster.MoreDifficulties.Cards;
using Nickel;
using Nanoray.PluginManager;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Nickel.ModSettings;
using System;
using static TheJazMaster.MoreDifficulties.ArtifactPatches;
using Shockah.CustomRunOptions;
using TheJazMaster.MoreDifficulties.Artifacts;

namespace TheJazMaster.MoreDifficulties;

public class ModEntry : SimpleMod
{
    internal static ModEntry Instance { get; private set; } = null!;
    internal Harmony Harmony { get; }
    internal ApiImplementation Api { get; private set; } = null!;

    internal IEssentialsApi EssentialsApi { get; private set; } = null!;
    internal IModSettingsApi SettingsApi { get; private set; } = null!;

    internal AltStarters AltStarters { get; private set; } = null!;
    internal LockAndBan LockAndBan { get; private set; } = null!;
    internal ShipLockAndBan ShipLockAndBan { get; private set; } = null!;

	internal ILocalizationProvider<IReadOnlyList<string>> AnyLocalizations { get; }
	internal ILocaleBoundNonNullLocalizationProvider<IReadOnlyList<string>> Localizations { get; }

    internal static Spr BegIcon;

    internal static Spr EasyModeArtifactSprite;
    internal static Spr DifficultyArtifactSprite1;
    internal static Spr DifficultyArtifactSprite2;
    internal static Spr AltStartersMarker;
    internal static Spr AltStartersMarkerOff;
    internal static Spr LockBorder;
    internal static Spr BanBorder;
    internal static Spr BanBorderAlt;
    internal static Spr ShipLockIcon;
    internal static Spr ShipBanIcon;

    internal static Status GrazerStatus;

	public const int Easy = -1;
    public const int Difficulty1 = 4;
    public const int Difficulty2 = 5;

	// internal Settings ModSettings = new();


    public ModEntry(IPluginPackage<IModManifest> package, IModHelper helper, ILogger logger) : base(package, helper, logger)
    {
        Instance = this;
        Harmony = new(package.Manifest.UniqueName);
        // ModSettings = helper.Storage.LoadJson<Settings>(helper.Storage.GetMainStorageFile("json"));

        AltStarters = new AltStarters();
        LockAndBan = new LockAndBan();
        ShipLockAndBan = new ShipLockAndBan();

        Api = new ApiImplementation();
        EssentialsApi = helper.ModRegistry.GetApi<IEssentialsApi>("Nickel.Essentials")!;
        SettingsApi = helper.ModRegistry.GetApi<IModSettingsApi>("Nickel.ModSettings")!;

        AnyLocalizations = new JsonLocalizationProvider(
            tokenExtractor: new SimpleLocalizationTokenExtractor(),
            localeStreamFunction: locale => package.PackageRoot.GetRelativeFile($"I18n/{locale}.json").OpenRead()
        );
        Localizations = new MissingPlaceholderLocalizationProvider<IReadOnlyList<string>>(
            new CurrentLocaleOrEnglishLocalizationProvider<IReadOnlyList<string>>(AnyLocalizations)
        );

        // SettingsApi.RegisterModSettings();

        BegIcon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/beg_icon.png")).Sprite;

        EasyModeArtifactSprite = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/Artifact-EasyMode.png")).Sprite;
        DifficultyArtifactSprite1 = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/HARDMODE_brutal.png")).Sprite;
        DifficultyArtifactSprite2 = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/HARDMODE_cosmic.png")).Sprite;

        AltStartersMarker = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/altMarker.png")).Sprite;
        AltStartersMarkerOff = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/altMarkerOff.png")).Sprite;
        LockBorder = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/lockBorder.png")).Sprite;
        BanBorder = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/banBorder.png")).Sprite;
        BanBorderAlt = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/banBorderAlt.png")).Sprite;
        ShipLockIcon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/lockShip.png")).Sprite;
        ShipBanIcon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/banShip.png")).Sprite;

        GrazerStatus = helper.Content.Statuses.RegisterStatus("Grazer", new()
        {
            Definition = new()
            {
                icon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/grazer.png")).Sprite,
                color = new("ff99d0"),
                isGood = true
            },
            Name = AnyLocalizations.Bind(["status", "grazer", "name"]).Localize,
            Description = AnyLocalizations.Bind(["status", "grazer", "description"]).Localize
        }).Status;

        GetType().Assembly.GetTypes().Where(t => !t.IsInterface && t.IsAssignableTo(typeof(IRegisterableCard))).Do(
            t => AccessTools.DeclaredMethod(t, nameof(IRegisterableCard.Register)).Invoke(null, [helper, package])
        );
        GetType().Assembly.GetTypes().Where(t => !t.IsInterface && t.IsAssignableTo(typeof(IRegisterableSettingsArtifact))).Do(
            t => AccessTools.DeclaredMethod(t, nameof(IRegisterableSettingsArtifact.Register)).Invoke(null, [helper, package])
        );
        helper.ModRegistry.AwaitApi<ICustomRunOptionsApi>("Shockah.CustomRunOptions", api =>
        {
            api.RegisterCustomRunOption(new CustomRunArtifacts(api));
        });

        helper.Events.OnLoadStringsForLocale += (_, args) =>
        {
            args.Localizations.Add(I18n.easyNameLoc, I18n.easyNameLocEn);
            args.Localizations.Add(I18n.easyDescLoc, I18n.easyDescLocEn);

            args.Localizations.Add(I18n.difficultyLoc1, I18n.difficultyLoc1En);
            args.Localizations.Add(I18n.difficultyLoc2, I18n.difficultyLoc2En);

            args.Localizations.Add(I18n.difficultyDescLoc1, I18n.difficultyDescLoc1En);
            args.Localizations.Add(I18n.difficultyDescLoc2, I18n.difficultyDescLoc2En);

            args.Localizations.Add(I18n.altStartersLoc, I18n.altStartersLocEn);
            args.Localizations.Add(I18n.altStartersDescLoc, I18n.altStartersDescLocEn);
        };

        NewRunOptionsPatches.Apply(Harmony);
        StatePatches.Apply(Harmony);
        Harmony.PatchAll();
    }

    public override object? GetApi(IModManifest requestingMod)
		=> new ApiImplementation();

    internal static bool IsFatigueEnabled(State s) => s.EnumerateAllArtifacts().OfType<TrashOnShuffle>().Any() || s.GetDifficulty() >= Difficulty1;
}

// class Settings {

// 	[JsonProperty]
// 	public ProfileSettings Global = new();

// 	[JsonIgnore]
// 	public ProfileBasedValue<IModSettingsApi.ProfileMode, ProfileSettings> ProfileBased;

// 	public Settings()
// 	{
// 		ProfileBased = ProfileBasedValue.Create(
// 			() => ModEntry.Instance.Helper.ModData.GetModDataOrDefault(MG.inst.g?.state ?? DB.fakeState, "ActiveProfile", IModSettingsApi.ProfileMode.Slot),
// 			profile => ModEntry.Instance.Helper.ModData.SetModData(MG.inst.g?.state ?? DB.fakeState, "ActiveProfile", profile),
// 			profile => profile switch
// 			{
// 				IModSettingsApi.ProfileMode.Global => Global,
// 				IModSettingsApi.ProfileMode.Slot => ModEntry.Instance.Helper.ModData.ObtainModData<ProfileSettings>(MG.inst.g?.state ?? DB.fakeState, "ProfileSettings"),
// 				_ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null)
// 			},
// 			(profile, data) =>
// 			{
// 				switch (profile)
// 				{
// 					case IModSettingsApi.ProfileMode.Global:
// 						Global = data;
// 						break;
// 					case IModSettingsApi.ProfileMode.Slot:
// 						ModEntry.Instance.Helper.ModData.SetModData(MG.inst.g?.state ?? DB.fakeState, "ProfileSettings", data);
// 						break;
// 					default:
// 						throw new ArgumentOutOfRangeException();
// 				}
// 			}
// 		);
// 	}
// }

// class ProfileSettings {
// 	public bool evenHarderEnemies;
//     public bool harderEnemies;
//     public bool evadeCap;
//     public bool worseBasics;
//     public bool trashOnShuffle;
// }