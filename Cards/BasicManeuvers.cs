using System.Collections.Generic;

namespace TheJazMaster.MoreDifficulties.Cards;

[CardMeta(deck = Deck.colorless, dontOffer = true, rarity = Rarity.common, upgradesTo = new Upgrade[] {Upgrade.A, Upgrade.B})]
public class BasicManeuvers : DodgeColorless
{
	private static readonly Lazy<Spr> art = new Lazy<Spr>(() => Enum.Parse<Spr>("cards_Dodge")); 

	public override string Name()
	{
		return "Basic Maneuvers";
	}

	public override CardData GetData(State state)
	{
		return new CardData {
			cost = upgrade == Upgrade.B ? 0 : 1,
			art = art.Value,
			artTint = "7a78ff",
			flippable = upgrade == Upgrade.A,
			exhaust = upgrade == Upgrade.B
		};
	}

	public override List<CardAction> GetActions(State s, Combat c)
	{
		switch (upgrade) {
			case Upgrade.None:
				return new List<CardAction> {
					new AStatus {
						status = Status.evade,
						statusAmount = 1,
						targetPlayer = true
					}
				};
			case Upgrade.A:
				return new List<CardAction> {
					new AMove {
						dir = 1,
						// isRandom = true,
						targetPlayer = true
					},
					new AStatus {
						status = Status.evade,
						statusAmount = 1,
						targetPlayer = true
					}
				};
			case Upgrade.B:
				return new List<CardAction> {
					new AStatus {
						status = Status.evade,
						statusAmount = 2,
						targetPlayer = true
					}
				};
			default:
				return new List<CardAction>();
		}
	}
}