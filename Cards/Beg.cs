using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;
using TheJazMaster.MoreDifficulties.Actions;

namespace TheJazMaster.MoreDifficulties.Cards;


public class Beg : Card, IRegisterableCard
{
    internal static Spr TopSprite;
	internal static Spr BottomSprite;

    public static void Register(IModHelper helper, IPluginPackage<IModManifest> package) {
		TopSprite = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/beg_yes.png")).Sprite;
		BottomSprite = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/beg_no.png")).Sprite;

        IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, Deck.trash, Rarity.common, TopSprite, helper, package, true);
    }

	public override CardData GetData(State state) => new() {
		cost = 0,
		temporary = true,
		exhaust = true,
		floppable = true,
		art = flipped ? BottomSprite : TopSprite!
	};

	public override List<CardAction> GetActions(State s, Combat c)
	{
		return [
			new ABeg {
				disabled = flipped
			},
			new AAddCard {
				card = new Fear(),
				destination = CardDestination.Deck,
				amount = 2,
				disabled = flipped
			},
			new ADummyAction(),
			new ADummyAction(),
			new AStatus {
				targetPlayer = false,
				status = Status.overdrive,
				statusAmount = 1,
				disabled = !flipped
			}
		];
	}
}