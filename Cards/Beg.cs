using System.Collections.Generic;
using TheJazMaster.MoreDifficulties.Actions;

namespace TheJazMaster.MoreDifficulties.Cards;

[CardMeta(deck = Deck.trash, dontOffer = true, rarity = Rarity.common)]
public class Beg : Card
{
	// private static readonly Lazy<Spr> art = new Lazy<Spr>(() => Enum.Parse<Spr>("cards_Trash")); 

	public override string Name()
	{
		return "Beg";
	}

	public override CardData GetData(State state)
	{
		return new CardData {
			cost = 0,
			// art = art.Value,
			temporary = true,
			exhaust = true,
			floppable = true,
			art = flipped ? (Spr)Manifest.BegNoArt!.Id! : (Spr)Manifest.BegYesArt!.Id!
		};
	}

	public override List<CardAction> GetActions(State s, Combat c)
	{
		return new List<CardAction> {
			new ABeg {
				disabled = flipped
			},
			new AAddCard
			{
				card = new Fear(),
				destination = CardDestination.Deck,
				amount = 2,
				disabled = flipped
			},
			new ADummyAction(),
			new ADummyAction(),
			new AStatus {
				targetPlayer = false,
				status = Status.overdrive,
				statusAmount = 1,
				disabled = !flipped
			}
		};
	}
}