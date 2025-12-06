using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TheJazMaster.MoreDifficulties.Cards;

namespace TheJazMaster.MoreDifficulties;

[HarmonyPatch]
internal static class DailyThinDeckPatches
{
	private static readonly List<Card> replaceThese = [
        new BasicBroadcast(),
		new BasicDefences(),
		new BasicOffences(),
		new BasicManeuvers()
	];

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DailyThinDeck), nameof(DailyThinDeck.OnReceiveArtifact))]
	private static void DailyThinDeck_OnReceiveArtifact_Postfix(State state)
	{
		List<Card> list = [];
		foreach (Card card in state.deck)
		{
			if (replaceThese.Any(bannedCard => bannedCard.GetType() == card.GetType()))
			{
				list.Add(card);
			}
		}
		foreach (Card item in list)
		{
			state.deck.Remove(item);
		}
	}
}
