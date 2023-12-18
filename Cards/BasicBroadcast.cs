using System.Collections.Generic;

namespace TheJazMaster.MoreDifficulties.Cards;

[CardMeta(deck = Deck.colorless, dontOffer = true, rarity = Rarity.common, upgradesTo = new Upgrade[] {Upgrade.A, Upgrade.B})]
public class BasicBroadcast : DroneshiftColorless
{
	private static readonly Lazy<Spr> art = new Lazy<Spr>(() => Enum.Parse<Spr>("cards_Dodge")); 

	public override string Name()
	{
		return "Basic Broadcast";
	}

	public override CardData GetData(State state)
	{
		return new CardData {
			cost = upgrade == Upgrade.B ? 0 : 1,
			artTint = "59f790",
			art = art.Value,
			exhaust = upgrade == Upgrade.B
		};
	}

	public override List<CardAction> GetActions(State s, Combat c)
	{
		switch (upgrade) {
			case Upgrade.None:
				return new List<CardAction>
				{
					new AStatus
					{
						status = Status.droneShift,
						statusAmount = 1,
						targetPlayer = true
					}
				};
			case Upgrade.A:
				return new List<CardAction>
				{
					new ADroneMove
					{
						dir = 1,
						// isRandom = true
					},
					new AStatus
					{
						status = Status.droneShift,
						statusAmount = 1,
						targetPlayer = true
					}
				};
			case Upgrade.B:
				return new List<CardAction>
				{
					new AStatus
					{
						status = Status.droneShift,
						statusAmount = 2,
						targetPlayer = true
					}
				};
			default:
				return new List<CardAction>();
		}
	}
}
