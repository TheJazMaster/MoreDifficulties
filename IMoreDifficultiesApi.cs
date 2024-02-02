using CobaltCoreModding.Definitions.ExternalItems;

namespace TheJazMaster.MoreDifficulties;

public interface IMoreDifficultiesApi
{
	void RegisterAltStarters(Deck deck, StarterDeck starterDeck);
    bool HasAltStarters(Deck deck);
    bool AreAltStartersEnabled(State state, Deck deck);

	int Difficulty1 { get; }
	int Difficulty2 { get; }
	ExternalCard BasicOffencesCard { get; }
    ExternalCard BasicDefencesCard { get; }
    ExternalCard BasicManeuversCard { get; }
    ExternalCard BasicBroadcastCard { get; }
    ExternalCard BegCard { get; }
    ExternalCard FatigueCard { get; }


}
