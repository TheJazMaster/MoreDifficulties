using System;
using TheJazMaster.MoreDifficulties.Cards;

namespace TheJazMaster.MoreDifficulties;

public sealed class ApiImplementation : IMoreDifficultiesApi
{
	public void DisableCharacterExtrasRendering() {
		CharacterPatches.skipRenderingCharacterExtras = true;
	}
	public void ReenableCharacterExtrasRendering() {
		CharacterPatches.skipRenderingCharacterExtras = false;
	}

	public void RegisterAltStarters(Deck deck, StarterDeck starterDeck)
		=> AltStarters.RegisterAltStarters(deck, starterDeck);
	public bool HasAltStarters(Deck deck)
		=> AltStarters.HasAltStarters(deck);
	public StarterDeck? GetAltStarters(Deck deck)
		=> AltStarters.GetAltStarters(deck);
	public bool AreAltStartersEnabled(State state, Deck deck)
		=> AltStarters.AreAltStartersEnabled(state, deck);

	public bool IsBanned(State state, Deck deck)
		=> LockAndBan.IsBanned(state, deck);
	public bool IsLocked(State state, Deck deck)
		=> LockAndBan.IsLocked(state, deck);
		
	public int Difficulty1 => ModEntry.Difficulty1;
	public int Difficulty2 => ModEntry.Difficulty2;

	public Type BasicOffencesCardType => typeof(BasicOffences);
    public Type BasicDefencesCardType => typeof(BasicDefences);
    public Type BasicManeuversCardType => typeof(BasicManeuvers);
    public Type BasicBroadcastCardType => typeof(BasicBroadcast);
    public Type BegCardType => typeof(Beg);
    public Type FatigueCardType => typeof(Fatigue);

}
