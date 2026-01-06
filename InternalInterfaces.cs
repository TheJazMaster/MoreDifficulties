using System;
using Nanoray.PluginManager;
using Nickel;

namespace TheJazMaster.MoreDifficulties;

internal interface IRegisterableCard
{
	private static Spr RegisterSpriteOrDefault(string path, Spr defaultSprite, IModHelper helper, IPluginPackage<IModManifest> package) {
		var file = package.PackageRoot.GetRelativeFile(path);
		if (file.Exists)
			return helper.Content.Sprites.RegisterSprite(file).Sprite;
		return defaultSprite;
	}

	static ICardEntry Register(Type type, Deck deck, Rarity rarity, Spr sprite, IModHelper helper, IPluginPackage<IModManifest> package, bool dontOffer = false) {
		var name = type.Name;
		return Register(type, deck, rarity, dontOffer, name, RegisterSpriteOrDefault($"Sprites/{name}.png", sprite, helper, package), helper, package);
	}

	static abstract void Register(IModHelper helper, IPluginPackage<IModManifest> package);

	private static ICardEntry Register(Type type, Deck deck, Rarity rarity, bool dontOffer, string name, Spr sprite, IModHelper helper, IPluginPackage<IModManifest> package) {
        return helper.Content.Cards.RegisterCard(name, new()
		{
			CardType = type,
			Meta = new()
			{
				deck = deck,
				rarity = rarity,
				upgradesTo = deck == Deck.trash ? [] : [Upgrade.A, Upgrade.B],
				dontOffer = dontOffer
			},
			Art = sprite,
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", name, "name"]).Localize
		});
	}
}

internal interface IRegisterableSettingsArtifact
{
	private static Spr RegisterSpriteOrDefault(string path, Spr defaultSprite, IModHelper helper, IPluginPackage<IModManifest> package) {
		var file = package.PackageRoot.GetRelativeFile(path);
		if (file.Exists)
			return helper.Content.Sprites.RegisterSprite(file).Sprite;
		return defaultSprite;
	}

	static IArtifactEntry Register(Type type, IModHelper helper, IPluginPackage<IModManifest> package) {
		var name = type.Name;
		return Register(type, name, RegisterSpriteOrDefault($"Sprites/{name}.png", StableSpr.artifacts_Crosslink, helper, package), helper, package);
	}
	static abstract void Register(IModHelper helper, IPluginPackage<IModManifest> package);

	private static IArtifactEntry Register(Type type, string name, Spr sprite, IModHelper helper, IPluginPackage<IModManifest> package) {
		return helper.Content.Artifacts.RegisterArtifact(name, new()
		{
			ArtifactType = type,
			Meta = new()
			{
				owner = Deck.colorless,
				pools = [ArtifactPool.Unreleased],
				unremovable = true
			},
			Sprite = sprite,
			Name = ModEntry.Instance.AnyLocalizations.Bind(["artifact", name, "name"]).Localize,
			Description = ModEntry.Instance.AnyLocalizations.Bind(["artifact", name, "description"]).Localize
		});
	}
}