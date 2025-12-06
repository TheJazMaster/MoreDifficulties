using System.Collections.Generic;
using System.Linq;
using FSPRO;
using Nickel;

namespace TheJazMaster.MoreDifficulties.Actions;

public class ABeg : CardAction
{
	public override void Begin(G g, State s, Combat c)
	{
		foreach (StuffBase item in c.stuff.Values.ToList())
		{
			if (item is Missile missile && missile.missileType != MissileType.normal) {
				c.stuff.Remove(item.x);
				Missile value = new() {
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
        return [
			new GlossaryTooltip("action.beg") {
                TitleColor = Colors.action,
                Icon = ModEntry.BegIcon,
				Title = ModEntry.Instance.Localizations.Localize(["action", "beg", "name"]),
				Description = ModEntry.Instance.Localizations.Localize(["action", "beg", "description"]),
            },
			.. new Missile().GetTooltips()
        ];
	}

    public override Icon? GetIcon(State s) => new Icon(ModEntry.BegIcon, null, Colors.textMain);
}