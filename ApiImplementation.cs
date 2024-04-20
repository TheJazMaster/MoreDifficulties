using TheJazMaster.MoreDifficulties.Cards;

namespace TheJazMaster.MoreDifficulties;

public sealed class ApiImplementation : IMoreDifficultiesApi
{
	private static Manifest Instance => Manifest.Instance;


	public void RegisterAltStarters(Deck deck, StarterDeck starterDeck)
		=> Instance.AltStarters.RegisterAltStarters(deck, starterDeck);
	public bool HasAltStarters(Deck deck)
		=> Instance.AltStarters.HasAltStarters(deck);
	public StarterDeck? GetAltStarters(Deck deck)
		=> Instance.AltStarters.GetAltStarters(deck);
	public bool AreAltStartersEnabled(State state, Deck deck)
		=> Instance.AltStarters.AreAltStartersEnabled(state, deck);

	public bool IsBanned(State state, Deck deck)
		=> Instance.LockAndBan.IsBanned(state, deck);
	public bool IsLocked(State state, Deck deck)
		=> Instance.LockAndBan.IsLocked(state, deck);
		
	public int Difficulty1 => Manifest.Difficulty1;
	public int Difficulty2 => Manifest.Difficulty2;

	public Type BasicOffencesCardType => typeof(BasicOffences);
    public Type BasicDefencesCardType => typeof(BasicDefences);
    public Type BasicManeuversCardType => typeof(BasicManeuvers);
    public Type BasicBroadcastCardType => typeof(BasicBroadcast);
    public Type BegCardType => typeof(Beg);
    public Type FatigueCardType => typeof(Fatigue);

}
