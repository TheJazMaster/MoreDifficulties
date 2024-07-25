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

internal class ShipLockAndBan
{
	internal static Manifest Instance => Manifest.Instance;

	internal static readonly string ShipLockKey = "ShipLock";
	internal static readonly string ShipBanKey = "ShipBan";

	internal static string KeyLock(string key)
	{
		return ShipLockKey + key;
	}
	internal static string KeyBan(string key)
	{
		return ShipBanKey + key;
	}

	internal bool IsLocked(State state, string key)
	{
		return Instance.KokoroApi.TryGetExtensionData<bool>(state, KeyLock(key), out var on) && on;
	}
	internal bool IsBanned(State state, string key)
	{
		return Instance.KokoroApi.TryGetExtensionData<bool>(state, KeyBan(key), out var on) && on;
	}

	internal void SetLock(State state, string key, bool on)
	{
		Instance.KokoroApi.SetExtensionData(state, KeyLock(key), on);
	}
	internal void SetBan(State state, string key, bool on)
	{
		Instance.KokoroApi.SetExtensionData(state, KeyBan(key), on);
	}
}