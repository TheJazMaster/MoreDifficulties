namespace TheJazMaster.MoreDifficulties;

public interface IMoreDifficultiesApi
{
	void RegisterAltStarters(Deck deck, StarterDeck starterDeck);
    bool HasAltStarters(Deck deck);
	public StarterDeck? GetAltStarters(Deck deck);
    bool AreAltStartersEnabled(State state, Deck deck);

	bool IsBanned(State state, Deck deck);
	bool IsLocked(State state, Deck deck);

	int Difficulty1 { get; }
	int Difficulty2 { get; }
	Type BasicOffencesCardType { get; }
    Type BasicDefencesCardType { get; }
    Type BasicManeuversCardType { get; }
    Type BasicBroadcastCardType { get; }
    Type BegCardType { get; }
    Type FatigueCardType { get; }


	void DisableCharacterExtrasRendering();
	void ReenableCharacterExtrasRendering();
}
