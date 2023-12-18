namespace TheJazMaster.MoreDifficulties;

internal static class I18n
{
	public static readonly string easyNameLoc = "artifact.HARDMODE_easy.name";
	public static readonly string easyDescLoc = "artifact.HARDMODE_easy.desc";
	public static readonly string easyNameLocEn = "EASYMODE";
	public static readonly string easyDescLocEn = "<c=artifact>EASY</c>: Gain <c=keyword>1</c> <c=status>temp shield</c> every turn.";

	public static readonly string difficultyLoc1 = "newRunOptions.difficultyMoreDifficulties1";
    public static readonly string difficultyLoc2 = "newRunOptions.difficultyMoreDifficulties2";
    public static readonly string difficultyLoc1En = "BRUTAL";
    public static readonly string difficultyLoc2En = "COSMIC";

    public static readonly string difficultyDescLoc1 = "artifact.HARDMODE_4.desc";
    public static readonly string difficultyDescLoc2 = "artifact.HARDMODE_5.desc";
    public static readonly string difficultyDescLoc1En = "<c=artifact>" + difficultyLoc1En + 
@"</c>: Take a <c=downside>downside</c> on boot sequence
Events are harder
Start combat with a <c=downside>BRITTLE</c> cockpit
+1 <c=card>Basic Shot</c>
+1 <c=card>Basic Block</c>
+1 <c=card>Basic Dodge</c>
+1 <c=card>Corrupted Core</c>
Less pre-upgraded card offerings
-3 max hull
1 less max hull gained from boss kills
Elites only offer 2 artifacts
Start with 50% hull missing
Most enemies are harder!
Shuffling your deck costs 1 <c=energy>ENERGY</c>!";
    public static readonly string difficultyDescLoc2En = "<c=artifact>" + difficultyLoc2En + 
@"</c>: Take a <c=downside>downside</c> on boot sequence
Events are harder
Start combat with a <c=downside>BRITTLE</c> cockpit
Your basic cards have worse upgrades
+1 <c=card>Basic Offences</c>
+1 <c=card>Basic Defences</c>
+1 <c=card>Basic Maneuvers</c>
+1 <c=card>Corrupted Core</c>
Less pre-upgraded card offerings
-3 max hull
1 less max hull gained from boss kills
Elites only offer 2 artifacts
Start with 50% hull missing
All enemies and bosses are EVEN HARDER!
Shuffling your deck costs 1 <c=energy>ENERGY</c>!";
}