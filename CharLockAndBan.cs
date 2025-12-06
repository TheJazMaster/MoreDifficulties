using Nickel;

namespace TheJazMaster.MoreDifficulties;

internal class LockAndBan
{
	internal static ModEntry Instance => ModEntry.Instance;
	private static IModData ModData => Instance.Helper.ModData;

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

	internal static bool IsLocked(State state, Deck deck)
	{
		return ModData.TryGetModData<bool>(state, KeyLock(deck), out var on) && on;
	}
	internal static bool IsBanned(State state, Deck deck)
	{
		return ModData.TryGetModData<bool>(state, KeyBan(deck), out var on) && on;
	}

	internal static void SetLock(State state, Deck deck, bool on)
	{
		ModData.SetModData(state, KeyLock(deck), on);
	}
	internal static void SetBan(State state, Deck deck, bool on)
	{
		ModData.SetModData(state, KeyBan(deck), on);
	}
}