using System.Collections.Generic;

namespace TheJazMaster.MoreDifficulties.Cards;

[CardMeta(deck = Deck.colorless, dontOffer = true, rarity = Rarity.common, upgradesTo = new Upgrade[] {Upgrade.A, Upgrade.B})]
public class BasicDefences : DodgeColorless
{
	private static readonly Lazy<Spr> art = new Lazy<Spr>(() => Enum.Parse<Spr>("cards_Shield")); 

	public override string Name()
	{
		return "Basic Defences";
	}

	public override CardData GetData(State state)
	{
		return new CardData {
			cost = upgrade == Upgrade.B ? 0 : 1,
			art = art.Value,
			artTint = "40a4fc",
			exhaust = upgrade == Upgrade.B
		};
	}

	public override List<CardAction> GetActions(State s, Combat c)
	{
		switch (upgrade) {
			case Upgrade.None:
				return new List<CardAction> {
					new AStatus {
						status = Status.shield,
						statusAmount = 1,
						targetPlayer = true
					}
				};
			case Upgrade.A:
				return new List<CardAction> {
					new AStatus {
						status = Status.shield,
						statusAmount = 1,
						targetPlayer = true
					},
					new AStatus
					{
						status = Status.tempShield,
						statusAmount = 1,
						targetPlayer = true
					}
				};
			case Upgrade.B:
				return new List<CardAction> {
					new AStatus {
						status = Status.shield,
						statusAmount = 2,
						targetPlayer = true
					}
				};
			default:
				return new List<CardAction>();
		}
	}
}