using System;
using System.Collections.Generic;

namespace TheJazMaster.MoreDifficulties.Cards;

[CardMeta(deck = Deck.colorless, dontOffer = true, rarity = Rarity.common, upgradesTo = new Upgrade[] {Upgrade.A, Upgrade.B})]
public class BasicOffences : CannonColorless
{
	private static readonly Lazy<Spr> art = new Lazy<Spr>(() => Enum.Parse<Spr>("cards_Cannon")); 

	public override string Name()
	{
		return "Basic Offences";
	}

	public override CardData GetData(State state)
	{
		return new CardData {
			cost = 1,
			art = art.Value,
			artTint = "ff3366"
		};
	}

	public override List<CardAction> GetActions(State s, Combat c)
	{
		switch (upgrade) {
			case Upgrade.None:
				return new List<CardAction> {
					new AAttack
					{
						damage = GetDmg(s, 1)
					}
				};
			case Upgrade.A:
				return new List<CardAction>
				{
					new AAttack
					{
						damage = GetDmg(s, 2)
					}
				};
			case Upgrade.B:
				return new List<CardAction> {
					new AAttack
					{
						damage = GetDmg(s, 1),
						piercing = true
					}
				};
			default:
				return new List<CardAction>();
		}
	}
}
