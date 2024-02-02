using CobaltCoreModding.Definitions.ExternalItems;

namespace TheJazMaster.MoreDifficulties;

public sealed class ApiImplementation : IMoreDifficultiesApi
{
	private static Manifest Instance => Manifest.Instance;


	public void RegisterAltStarters(Deck deck, StarterDeck starterDeck)
		=> Instance.AltStarters.RegisterAltStarters(deck, starterDeck);
	public bool HasAltStarters(Deck deck)
		=> Instance.AltStarters.HasAltStarters(deck);
	public bool AreAltStartersEnabled(State state, Deck deck)
		=> Instance.AltStarters.AreAltStartersEnabled(state, deck);
		
	public int Difficulty1 => Manifest.Difficulty1;
	public int Difficulty2 => Manifest.Difficulty2;

	public ExternalCard BasicOffencesCard => Manifest.BasicOffencesCard;
    public ExternalCard BasicDefencesCard => Manifest.BasicDefencesCard;
    public ExternalCard BasicManeuversCard => Manifest.BasicManeuversCard;
    public ExternalCard BasicBroadcastCard => Manifest.BasicBroadcastCard;
    public ExternalCard BegCard => Manifest.BegCard;
    public ExternalCard FatigueCard => Manifest.FatigueCard;

}
