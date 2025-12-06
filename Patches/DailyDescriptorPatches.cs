using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Nickel;

namespace TheJazMaster.MoreDifficulties;

[HarmonyPatch]
internal static class DailyDescriptorPatches
{
	private static ModEntry Instance => ModEntry.Instance;
	private static IModData ModData => Instance.Helper.ModData;

	private static readonly string AltStartersDailyKey = "AltStartersDaily";
	private static readonly string AltStartersStorageKey = "AltStartersStorage";


	[HarmonyPrefix]
	[HarmonyPatch(typeof(DailyDescriptor), nameof(DailyDescriptor.Create))]
	private static void DailyDescriptor_Create_Prefix(int daySince1970)
	{
		State s = MG.inst.g.state;

		allCharsOld = NewRunOptions.allChars;
		allShipsOld = StarterShip.ships;

		NewRunOptions.allChars = [.. NewRunOptions.allChars.Where(deck => !LockAndBan.IsBanned(s, deck))];
		StarterShip.ships = StarterShip.ships.Where(ship => !ShipLockAndBan.IsBanned(s, ship.Key)).ToDictionary();
	}

	[HarmonyFinalizer]
	[HarmonyPatch(typeof(DailyDescriptor), nameof(DailyDescriptor.Create))]
	private static void DailyDescriptor_Create_Finalizer(int daySince1970)
	{
		if (allCharsOld != null) NewRunOptions.allChars = allCharsOld;
		if (allShipsOld != null) StarterShip.ships = allShipsOld;

		allCharsOld = null;
		allShipsOld = null;
	}

	private static readonly int[] difficultyPerDayOfWeek = [ModEntry.Easy, 0, 1, 2, 3, ModEntry.Difficulty1, ModEntry.Difficulty2];

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DailyDescriptor), nameof(DailyDescriptor.Create))]
	private static void DailyDescriptor_Create_Postfix(int daySince1970, DailyDescriptor __result)
	{
		Rand rand = new((uint)daySince1970);

		foreach (Deck character in __result.crew) {
			if (AltStarters.HasAltStarters(character)) {
				ModData.SetModData(__result, Key(character), rand.Next() >= 0.5);
			}
		}

		__result.difficulty = difficultyPerDayOfWeek[(daySince1970 + 3) % 7];
	}

	internal static string Key(Deck deck) => AltStartersDailyKey + deck.Key();
	internal static string StorageKey(Deck deck) => AltStartersStorageKey + deck.Key();

	static List<Deck>? allCharsOld = null;
	static Dictionary<string, StarterShip>? allShipsOld = null;
}