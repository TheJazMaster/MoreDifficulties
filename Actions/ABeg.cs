using CobaltCoreModding.Definitions;
using FSPRO;

namespace TheJazMaster.MoreDifficulties.Actions;

public class ABeg : CardAction
{
	private static readonly Lazy<Spr> icon = new Lazy<Spr>(() => (Spr)Manifest.BegIcon!.Id!); 

	public override void Begin(G g, State s, Combat c)
	{
		foreach (StuffBase item in c.stuff.Values.ToList())
		{
			if (item is Missile missile && missile.missileType != MissileType.normal) {
				c.stuff.Remove(item.x);
				Missile value = new Missile
				{
					x = item.x,
					xLerped = item.xLerped,
					bubbleShield = item.bubbleShield,
					targetPlayer = item.targetPlayer,
					age = item.age,
					missileType = MissileType.normal
				};
				c.stuff[item.x] = value;
			}
		}
		Audio.Play(Event.Status_PowerDown);
	}

	public override List<Tooltip> GetTooltips(State s)
	{
		if (s.route is Combat combat)
		{
			foreach (StuffBase value in combat.stuff.Values)
			{
				if (value is Missile missile && missile.missileType != MissileType.normal)
				value.hilight = 2;
			}
		}
		var result = new List<Tooltip>
		{
			new TTGlossary(Manifest.BegGlossary?.Head ?? throw new Exception("Missing Beg Glossary"))
		};
		// result.AddRange(new Missile().GetTooltips());
		return result;
	}

	public override Icon? GetIcon(State s)
	{
		return new Icon(icon.Value, null, Colors.textMain);
	}
}