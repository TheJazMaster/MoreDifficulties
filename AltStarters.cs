using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.Shrike.Harmony;
using Nanoray.Shrike;
using TheJazMaster.MoreDifficulties.Cards;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;

namespace TheJazMaster.MoreDifficulties;

/*
Dizzy: Stun Shot, Boost Capacitors
Riggs: Evade Shot, Quick Thinking
Peri: Wave Charge, Feint
Isaac: Shift Shot, Shield Drone
Drake: Explosive Slug, Hot Compress
Max: Reroute, Shuffle Shot
Books: Meteor, Mining Drill
Cat: Jack of all Trades, CAT.EXE

Eddie: Interference, Power Cell
Riggs?:
Dave:
Nola:
Isabelle:
Ilya:
Phillip:
Tucker: 
*/

internal class AltStarters
{
	internal Manifest Instance => Manifest.Instance;

	internal bool AreAltStartersEnabled(State state, Deck deck)
	{
		return Instance.KokoroApi.TryGetExtensionData<bool>(state, deck.Key(), out var on) && on;
	}

	internal bool HasAltStarters(Deck deck)
	{
		return altStarters.ContainsKey(deck);
	}

	internal void SetAltStarters(State state, Deck deck, bool on)
	{
		Instance.KokoroApi.SetExtensionData(state, deck.Key(), on);
	}

	internal void RegisterAltStarters(Deck deck, StarterDeck starterDeck)
	{
		if (altStarters.ContainsKey(deck))
		{
			Manifest.Instance.Logger!.LogWarning("Deck {Name} already has registered alternative starters.", new { Name = deck.Key() });
		}
		altStarters.Add(deck, starterDeck);
	}

	internal Dictionary<Deck, StarterDeck> altStarters = new() {
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
					new ShieldDroneCard()
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
	};
}