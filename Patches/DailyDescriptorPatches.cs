using FSPRO;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using Nickel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.AccessControl;
using static SharedArt;

namespace TheJazMaster.MoreDifficulties;

internal static class DailyDescriptorPatches
{
	private static Manifest Instance => Manifest.Instance;

	private static readonly string AltStartersDailyKey = "AltStartersDaily";

	public static void Apply(Harmony harmony)
	{
		harmony.TryPatch(
			logger: Instance.Logger!,
			original: typeof(DailyDescriptor).GetMethod("Create", AccessTools.all),
			prefix: new HarmonyMethod(typeof(DailyDescriptorPatches).GetMethod("DailyDescriptor_Create_Prefix", AccessTools.all)),
			postfix: new HarmonyMethod(typeof(DailyDescriptorPatches).GetMethod("DailyDescriptor_Create_Postfix", AccessTools.all)),
			finalizer: new HarmonyMethod(typeof(DailyDescriptorPatches).GetMethod("DailyDescriptor_Create_Finalizer", AccessTools.all))
		);
	}

	private static void DailyDescriptor_Create_Prefix(int daySince1970)
	{
		State s = MG.inst.g.state;

		allCharsOld = NewRunOptions.allChars;
		allShipsOld = StarterShip.ships;

		NewRunOptions.allChars = NewRunOptions.allChars.Where(deck => !Manifest.Instance.LockAndBan.IsBanned(s, deck)).ToList();
		StarterShip.ships = StarterShip.ships.Where(ship => !Manifest.Instance.ShipLockAndBan.IsBanned(s, ship.Key)).ToDictionary();
	}

	private static void DailyDescriptor_Create_Finalizer(int daySince1970)
	{
		if (allCharsOld != null) NewRunOptions.allChars = allCharsOld;
		if (allShipsOld != null) StarterShip.ships = allShipsOld;

		allCharsOld = null;
		allShipsOld = null;
	}

	private static readonly int[] difficultyPerDayOfWeek = new int[7] {Manifest.Easy, 0, 1, 2, 3, Manifest.Difficulty1, Manifest.Difficulty2};
	private static void DailyDescriptor_Create_Postfix(int daySince1970, DailyDescriptor __result)
	{
		Rand rand = new((uint)daySince1970);

		foreach (Deck character in __result.crew) {
			if (Instance.AltStarters.HasAltStarters(character)) {
				Manifest.Instance.KokoroApi.SetExtensionData(__result, Key(character), rand.Next() >= 0.5);
			}
		}

		__result.difficulty = difficultyPerDayOfWeek[(daySince1970 + 3) % 7];
	}

	internal static string Key(Deck deck) => AltStartersDailyKey + deck.Key();

	static List<Deck>? allCharsOld = null;
	static Dictionary<string, StarterShip>? allShipsOld = null;
}