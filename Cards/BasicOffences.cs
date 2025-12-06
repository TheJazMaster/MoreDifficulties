
using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace TheJazMaster.MoreDifficulties.Cards;

public class BasicOffences : CannonColorless, IRegisterableCard
{
    public static void Register(IModHelper helper, IPluginPackage<IModManifest> package) {
        IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, Deck.colorless, Rarity.common, StableSpr.cards_Cannon, helper, package, true);
    }

	public override CardData GetData(State state) => new() {
		cost = 1,
		artTint = "ff3366"
	};

    public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
        Upgrade.A => [
            new AAttack {
				damage = GetDmg(s, 2)
			}
        ],
        Upgrade.B => [
            new AAttack {
				damage = GetDmg(s, 1),
				piercing = true
			}
        ],
        _ => [
            new AAttack {
				damage = GetDmg(s, 1)
			}
        ]
	};
}
