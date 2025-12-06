using Nickel;

namespace TheJazMaster.MoreDifficulties;

internal class ShipLockAndBan
{
	internal static ModEntry Instance => ModEntry.Instance;
	private static IModData ModData => Instance.Helper.ModData;

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

	internal static bool IsLocked(State state, string key)
	{
		return ModData.TryGetModData<bool>(state, KeyLock(key), out var on) && on;
	}
	internal static bool IsBanned(State state, string key)
	{
		return ModData.TryGetModData<bool>(state, KeyBan(key), out var on) && on;
	}

	internal static void SetLock(State state, string key, bool on)
	{
		ModData.SetModData(state, KeyLock(key), on);
	}
	internal static void SetBan(State state, string key, bool on)
	{
		ModData.SetModData(state, KeyBan(key), on);
	}
}