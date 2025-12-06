using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace TheJazMaster.MoreDifficulties.Cards;

public class BasicManeuvers : DodgeColorless, IRegisterableCard
{
    public static void Register(IModHelper helper, IPluginPackage<IModManifest> package) {
        IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, Deck.colorless, Rarity.common, StableSpr.cards_Dodge, helper, package, true);
    }

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.B ? 0 : 1,
		artTint = "7a78ff",
		flippable = upgrade == Upgrade.A,
		exhaust = upgrade == Upgrade.B
	};

    public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
        Upgrade.A => [
            new AMove {
				dir = 1,
				targetPlayer = true
			},
			new AStatus {
				status = Status.evade,
				statusAmount = 1,
				targetPlayer = true
			}
        ],
        Upgrade.B => [
            new AStatus {
				status = Status.evade,
				statusAmount = 2,
				targetPlayer = true
			}
        ],
        _ => [
            new AStatus {
				status = Status.evade,
				statusAmount = 1,
				targetPlayer = true
			}
        ]
    };
}