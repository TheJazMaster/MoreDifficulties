using CobaltCoreModding.Definitions;
using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Reflection.Emit;
using TheJazMaster.MoreDifficulties.Actions;
using TheJazMaster.MoreDifficulties.Cards;
using static System.Reflection.BindingFlags;
using FSPRO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheJazMaster.MoreDifficulties;

public class Manifest : ISpriteManifest, IModManifest, ICardManifest, IStatusManifest, IGlossaryManifest
{
    public string Name { get; init; } = typeof(Manifest).Namespace!;

    internal static Manifest Instance { get; private set; } = null!;

    public IEnumerable<DependencyEntry> Dependencies => Array.Empty<DependencyEntry>();

    public ILogger? Logger { get; set; }
    public static ILogger? TheLogger { get; set; }

    public static ExternalGlossary? BegGlossary { get; private set; }
    public static ExternalSprite? BegIcon { get; private set; }
    public static ExternalSprite? BegYesArt { get; private set; }
    public static ExternalSprite? BegNoArt { get; private set; }

    public static ExternalSprite? EasyModeArtifactSprite { get; private set; }
    public static ExternalSprite? DifficultyArtifactSprite1 { get; private set; }
    public static ExternalSprite? DifficultyArtifactSprite2 { get; private set; }

    public static ExternalSprite? GrazerStatusIcon { get; private set; }
    public static ExternalStatus? GrazerStatus { get; private set; }

    public static ExternalCard? BasicOffencesCard { get; private set; }
    public static ExternalCard? BasicDefencesCard { get; private set; }
    public static ExternalCard? BasicManeuversCard { get; private set; }
    public static ExternalCard? BasicBroadcastCard { get; private set; }
    public static ExternalCard? BegCard { get; private set; }

    public DirectoryInfo? ModRootFolder { get; set; }
    public DirectoryInfo? GameRootFolder { get; set; }

	public const int Easy = -1;
    public static readonly int Difficulty1 = 4;
    public static readonly int Difficulty2 = 5;

    private ExternalSprite RegisterSprite(ISpriteRegistry registry, string globalName, string path) {
        var sprite = new ExternalSprite(Name + "." + globalName, new FileInfo(path));
        registry.RegisterArt(sprite);
        return sprite;
    }

    private ExternalArtifact RegisterArtifact(IArtifactRegistry registry, Type type, ExternalSprite sprite, string name, string desc, List<ExternalGlossary>? extraGlossary = null)
    {
        var artifact = new ExternalArtifact(Name + ".Artifacts." + type.Name, type, sprite, extraGlossary, null);
        artifact.AddLocalisation(name, desc);
        registry.RegisterArtifact(artifact);
        return artifact!;
    }

    private ExternalCard RegisterCard(ICardRegistry registry, Type type, ExternalDeck deck, string? name = null)
    {
        var card = new ExternalCard(Name + ".Cards." + type.Name, type, ExternalSprite.GetRaw((int)Enum.Parse<Spr>("cards_colorless")), deck);
        card.AddLocalisation((name == null) ? type.Name : name);
        registry.RegisterCard(card);
        return card!;
    }

    private ExternalGlossary RegisterGlossary(IGlossaryRegisty registry, string globalName, string name, string desc, ExternalGlossary.GlossayType type, ExternalSprite icon)
    {
        var glossary = new ExternalGlossary(Name + ".Glossary." + globalName, "MoreDifficulties" + globalName, false, type, icon);
        glossary.AddLocalisation("en", "Beg", "Instantly downgrade all <c=midrow>missiles in the midrow</c> into normal <c=drone>missiles</c>.", null);
        registry.RegisterGlossary(glossary);
        return glossary;
    }

    void IGlossaryManifest.LoadManifest(IGlossaryRegisty registry)
    {
        BegGlossary = RegisterGlossary(registry, "BegAction", "Beg", "Instantly downgrade all <c=midrow>missiles in the midrow</c> into normal <c=drone>missiles</c>.", ExternalGlossary.GlossayType.action, BegIcon ?? throw new Exception("Missing Beg Icon"));
    }

    void ISpriteManifest.LoadManifest(ISpriteRegistry registry)
    {
        if (ModRootFolder == null)
            throw new Exception("Root Folder not set");

        EasyModeArtifactSprite = RegisterSprite(registry, "EasyModeSprite", Path.Combine(ModRootFolder!.FullName, "Sprites", "Artifact-EasyMode.png"));
        DifficultyArtifactSprite1 = RegisterSprite(registry, "DifficultyArtifactSprite1", Path.Combine(ModRootFolder.FullName, "Sprites", Path.GetFileName("HARDMODE_brutal.png")));
        DifficultyArtifactSprite2 = RegisterSprite(registry, "DifficultyArtifactSprite2", Path.Combine(ModRootFolder.FullName, "Sprites", Path.GetFileName("HARDMODE_cosmic.png")));

        BegNoArt = RegisterSprite(registry, "BegNoCardArt", Path.Combine(ModRootFolder.FullName, "Sprites", Path.GetFileName("beg_no.png")));
        BegYesArt = RegisterSprite(registry, "BegYesCardArt", Path.Combine(ModRootFolder.FullName, "Sprites", Path.GetFileName("beg_yes.png")));
        BegIcon = RegisterSprite(registry, "BegIcon", Path.Combine(ModRootFolder.FullName, "Sprites", Path.GetFileName("beg_icon.png")));

        GrazerStatusIcon = RegisterSprite(registry, "GrazerStatusIcon", Path.Combine(ModRootFolder.FullName, "Sprites", Path.GetFileName("grazer.png")));
    }

    void ICardManifest.LoadManifest(ICardRegistry registry)
    {
        var colorless = ExternalDeck.GetRaw((int)Enum.Parse<Deck>("colorless"));
        var trash = ExternalDeck.GetRaw((int)Enum.Parse<Deck>("trash"));

        BasicOffencesCard = RegisterCard(registry, typeof(BasicOffences), colorless, "Basic Offences");
        BasicDefencesCard = RegisterCard(registry, typeof(BasicDefences), colorless, "Basic Defences");
        BasicManeuversCard = RegisterCard(registry, typeof(BasicManeuvers), colorless, "Basic Maneuvers");
        BasicBroadcastCard = RegisterCard(registry, typeof(BasicBroadcast), colorless, "Basic Broadcast");
        BegCard = RegisterCard(registry, typeof(Beg), trash, "Beg");
    }

    void IStatusManifest.LoadManifest(IStatusRegistry registry) 
    {
        GrazerStatus = new ExternalStatus("MoreDifficulties.Status.Grazer", true, System.Drawing.Color.FromArgb(255, 159, 208, 255), null, GrazerStatusIcon ?? throw new Exception("Missing Grazer Icon for status"), true);
        GrazerStatus.AddLocalisation("Grazer", "Every time an attack misses the enemy by exactly 1 space, the enemy takes 1 damage.");
        registry.RegisterStatus(GrazerStatus);
    }

    public void BootMod(IModLoaderContact contact) {

        Instance = this;
		ReflectionExt.CurrentAssemblyLoadContext.LoadFromAssemblyPath(Path.Combine(ModRootFolder!.FullName, "Shrike.dll"));
		ReflectionExt.CurrentAssemblyLoadContext.LoadFromAssemblyPath(Path.Combine(ModRootFolder!.FullName, "Shrike.Harmony.dll"));

        Harmony harmony = new("MoreDifficulties");
        harmony.TryPatch(
            logger: Logger!,
			original: typeof(DB).GetMethod("SetLocale", AccessTools.all),
			postfix: new HarmonyMethod(typeof(Manifest).GetMethod("DB_SetLocale_Postfix", AccessTools.all))
		);

		ArtifactPatches.Apply(harmony);
		HardmodePatches.Apply(harmony);
		CombatPatches.Apply(harmony);
		NewRunOptionsPatches.Apply(harmony);
		RunConfigPatches.Apply(harmony);
		RunSummaryRoutePatches.Apply(harmony);
		StatePatches.Apply(harmony);

		harmony.PatchAll(typeof(Manifest).Assembly);
    }

    private static void DB_SetLocale_Postfix() {
        DB.currentLocale.strings[I18n.easyNameLoc] = I18n.easyNameLocEn;
        DB.currentLocale.strings[I18n.easyDescLoc] = I18n.easyDescLocEn;

        DB.currentLocale.strings[I18n.difficultyLoc1] = I18n.difficultyLoc1En;
        DB.currentLocale.strings[I18n.difficultyLoc2] = I18n.difficultyLoc2En;

        DB.currentLocale.strings[I18n.difficultyDescLoc1] = I18n.difficultyDescLoc1En;
        DB.currentLocale.strings[I18n.difficultyDescLoc2] = I18n.difficultyDescLoc2En;
    }

}