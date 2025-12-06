using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace TheJazMaster.MoreDifficulties.Cards;

public class BasicBroadcast : DroneshiftColorless, IRegisterableCard
{
    public static void Register(IModHelper helper, IPluginPackage<IModManifest> package) {
        IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, Deck.colorless, Rarity.common, StableSpr.cards_Dodge, helper, package, true);
    }

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.B ? 0 : 1,
		artTint = "59f790",
		flippable = upgrade == Upgrade.A,
		exhaust = upgrade == Upgrade.B
	};

    public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
        Upgrade.A => [
            new ADroneMove {
                dir = 1
            },
            new AStatus {
                status = Status.droneShift,
                statusAmount = 1,
                targetPlayer = true
            }
        ],
        Upgrade.B => [
            new AStatus {
                status = Status.droneShift,
                statusAmount = 2,
                targetPlayer = true
            }
        ],
        _ => [
            new AStatus {
                status = Status.droneShift,
                statusAmount = 1,
                targetPlayer = true
            }
        ]
    };
}
