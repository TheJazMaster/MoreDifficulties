using System.Collections.Generic;
using TheJazMaster.MoreDifficulties.Actions;

namespace TheJazMaster.MoreDifficulties.Cards;

[CardMeta(deck = Deck.trash, dontOffer = true, rarity = Rarity.common)]
public class Fatigue : Card
{
	private static readonly Lazy<Spr> art = new Lazy<Spr>(() => Enum.Parse<Spr>("cards_Trash")); 

	public override string Name()
	{
		return "Fatigue";
	}

	public override CardData GetData(State state)
	{
		return new CardData {
			cost = 1,
			temporary = true,
			exhaust = true,
			art = art.Value
		};
	}

	public override List<CardAction> GetActions(State s, Combat c)
	{
		return new();
	}
}