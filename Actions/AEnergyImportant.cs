using FSPRO;

namespace TheJazMaster.MoreDifficulties.Actions;

public class AEnergyImportant : AEnergy
{
	public double pulseAmount = 0.5;

	public override void Begin(G g, State s, Combat c)
	{
		c.energy = Math.Max(c.energy + changeAmount, 0);
		Audio.Play((changeAmount > 0) ? Event.Status_PowerUp : Event.Status_PowerDown);
		if (changeAmount < 0)
		{
			c.pulseEnergyBad = pulseAmount;
		}
		else if (changeAmount > 0)
		{
			c.pulseEnergyGood = pulseAmount;
		}
	}
}
