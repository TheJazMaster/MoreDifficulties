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

internal class LockAndBan
{
	internal Manifest Instance => Manifest.Instance;

	internal static readonly string LockKey = "Lock";
	internal static readonly string BanKey = "Ban";

	internal static string KeyLock(Deck deck)
	{
		return LockKey + deck.Key();
	}
	internal static string KeyBan(Deck deck)
	{
		return BanKey + deck.Key();
	}

	internal bool IsLocked(State state, Deck deck)
	{
		return Instance.KokoroApi.TryGetExtensionData<bool>(state, KeyLock(deck), out var on) && on;
	}
	internal bool IsBanned(State state, Deck deck)
	{
		return Instance.KokoroApi.TryGetExtensionData<bool>(state, KeyBan(deck), out var on) && on;
	}

	internal void SetLock(State state, Deck deck, bool on)
	{
		Instance.KokoroApi.SetExtensionData(state, KeyLock(deck), on);
	}
	internal void SetBan(State state, Deck deck, bool on)
	{
		Instance.KokoroApi.SetExtensionData(state, KeyBan(deck), on);
	}
}