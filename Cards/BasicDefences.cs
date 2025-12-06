using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace TheJazMaster.MoreDifficulties.Cards;

public class BasicDefences : BasicShieldColorless, IRegisterableCard
{
    public static void Register(IModHelper helper, IPluginPackage<IModManifest> package) {
        IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, Deck.colorless, Rarity.common, StableSpr.cards_Shield, helper, package, true);
    }

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.B ? 0 : 1,
		artTint = "40a4fc",
		exhaust = upgrade == Upgrade.B
	};

    public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
    {
        Upgrade.A => [
            new AStatus {
				status = Status.shield,
				statusAmount = 1,
				targetPlayer = true
			},
			new AStatus {
				status = Status.tempShield,
				statusAmount = 1,
				targetPlayer = true
			}
        ],
        Upgrade.B => [
            new AStatus {
				status = Status.shield,
				statusAmount = 2,
				targetPlayer = true
			}
        ],
        _ => [
            new AStatus {
				status = Status.shield,
				statusAmount = 1,
				targetPlayer = true
			}
        ]
	};
}