using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace TheJazMaster.MoreDifficulties.Cards;

public class Fatigue : Card, IRegisterableCard
{
    public static void Register(IModHelper helper, IPluginPackage<IModManifest> package) {
        IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, Deck.trash, Rarity.common, StableSpr.cards_Trash, helper, package, true);
    }

	public override CardData GetData(State state) => new() {
		cost = 1,
		temporary = true,
		exhaust = true
	};

    public override List<CardAction> GetActions(State s, Combat c) => [];
}