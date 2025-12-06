using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Nickel;

namespace TheJazMaster.MoreDifficulties;


internal class AltStarters
{
	internal static ModEntry Instance => ModEntry.Instance;
	private static IModData ModData => Instance.Helper.ModData;

	internal static readonly string AltStartersKey = "AltStarters";

	internal static string Key(Deck deck)
	{
		return AltStartersKey + deck.Key();
	}

	internal static bool AreAltStartersEnabled(State state, Deck deck)
	{
		return HasAltStarters(deck) && ModData.TryGetModData<bool>(state, Key(deck), out var on) && on;
	}

	internal static bool HasAltStarters(Deck deck)
	{
		return altStarters.ContainsKey(deck);
	}

	internal static StarterDeck? GetAltStarters(Deck deck)
	{
		return altStarters.GetValueOrDefault(deck);
	}

	internal static void SetAltStarters(State state, Deck deck, bool on)
	{
		ModData.SetModData(state, Key(deck), on);
	}

	internal static void RegisterAltStarters(Deck deck, StarterDeck starterDeck)
	{
		if (altStarters.ContainsKey(deck))
		{
			ModEntry.Instance.Logger!.LogWarning("Deck {Name} already has registered alternative starters.", new { Name = deck.Key() });
		}
		altStarters.Add(deck, starterDeck);
	}

	internal static Dictionary<Deck, StarterDeck> altStarters = new() {
		{
			Deck.dizzy,
			new StarterDeck() {
				cards = {
					new StunShot(),
					new BoostCapacitors()
				}
			}
		},
		{
			Deck.riggs,
			new StarterDeck() {
				cards = {
					new EvasiveShot(),
					new DrawThree()
				}
			}
		},
		{
			Deck.peri,
			new StarterDeck() {
				cards = {
					new Feint(),
					new WaveCharge()
				}
			}
		},
		{
			Deck.goat,
			new StarterDeck() {
				cards = {
					new ShiftShot(),
					new SmallBoulder()
				}
			}
		},
		{
			Deck.eunice,
			new StarterDeck() {
				cards = {
					new HESlug(),
					new HotCompress()
				}
			}
		},
		{
			Deck.hacker,
			new StarterDeck() {
				cards = {
					new RerouteCard(),
					new ShuffleShot()
				}
			}
		},
		{
			Deck.shard,
			new StarterDeck() {
				cards = {
					new MiningDrillCard(),
					new MeteorCard()
				}
			}
		},
		{
			Deck.colorless,
			new StarterDeck() {
				cards = {
					new DefensiveMode(),
					new ColorlessCATSummon()
				}
			}
		},
	};
}