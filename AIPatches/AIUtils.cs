using FSPRO;
using Microsoft.Extensions.Logging;

namespace TheJazMaster.MoreDifficulties.AIPatches;

public class AIUtils {

	private static Manifest Instance => Manifest.Instance;

	public static EnemyDecision MoveSet(int counter, params Func<EnemyDecision>[] generators)
	{
		return generators.GetModulo(counter)();
	}

	public static EnemyDecision MoveSet(Rand rngSource, params Func<EnemyDecision>[] generators)
	{
		return generators.GetModulo(rngSource.NextInt())();
	}

	public static List<CardAction> MoveToAimAtPincer(State s, Ship movingShip, Ship targetShip, int alignPartLocalX, int maxLocalXFromEdge = 99, bool rightEdge = false, int maxMove = 99, bool movesFast = false, bool? attackWeakPoints = null, bool avoidAsteroids = false, bool avoidMines = true)
	{
		Ship targetShip2 = targetShip;
		Route route = s.route;
		Combat? c = route as Combat;
		if (c == null)
		{
			return new List<CardAction>();
		}
		if (movingShip.Get(Status.engineStall) > 0)
		{
			Audio.Play(Event.Status_PowerDown);
			movingShip.Add(Status.engineStall, -1);
			movingShip.shake += 1.0;
			return new List<CardAction>();
		}
		StuffBase? value;
		var cutoff = rightEdge ? (targetShip.parts.Count - maxLocalXFromEdge - 1) : maxLocalXFromEdge;
		var list = (from pair in targetShip2.parts.Select((Part part, int x) => new
			{
				part = part,
				x = x,
				drone = (c.stuff.TryGetValue(x + targetShip2.x, out value) ? value : null)
			})
			where pair.part.type != PType.empty && (!rightEdge && pair.x <= cutoff || rightEdge && pair.x >= cutoff)
			select pair).ToList();
		var list2 = list.Where(pair =>
		{
			if (pair.drone == null)
			{
				return true;
			}
			if (pair.drone is ShieldDrone && !pair.drone.targetPlayer)
			{
				return false;
			}
			if (pair.drone is AttackDrone && pair.drone.targetPlayer)
			{
				return false;
			}
			if (pair.drone is Asteroid && avoidAsteroids)
			{
				return false;
			}
			return !(pair.drone is SpaceMine && avoidMines);
		}).ToList();
		if (list2.Count > 0)
		{
			list = list2;
		}
		if (attackWeakPoints == true)
		{
			var list3 = list.Where(pair => pair.part.GetDamageModifier() == PDamMod.weak || pair.part.GetDamageModifier() == PDamMod.brittle).ToList();
			if (list3.Count > 0)
			{
				list = list3;
			}
		}
		else if (attackWeakPoints == false)
		{
			var list4 = list.Where(pair => pair.part.GetDamageModifier() != PDamMod.weak && pair.part.GetDamageModifier() != PDamMod.brittle).ToList();
			if (list4.Count > 0)
			{
				list = list4;
			}
		}
		var anon = list.Random(s.rngAi);
		int num = targetShip2.x + anon.x - (movingShip.x + alignPartLocalX);
		if (Math.Abs(num) > maxMove)
		{
			num = maxMove * Math.Sign(num);
		}
		if (movesFast)
		{
			return new List<CardAction>
			{
				new AMove
				{
					dir = num,
					targetPlayer = false
				}
			};
		}
		return AIHelpers.Move(num);
	}

	public static int GetUnderhang(State s, Combat c, Ship ownShip)
	{
		int partCount = s.ship.parts.Count;
		int leftUnderhang = Math.Max(partCount + Math.Min(s.ship.x - ownShip.x, 0), 0);
		int rightUnderhang = Math.Max(partCount - Math.Max(s.ship.x + partCount - (ownShip.x + ownShip.parts.Count), 0), 0);
		if (leftUnderhang < partCount && rightUnderhang == partCount)
		{
			return leftUnderhang;
		}
		if (leftUnderhang == s.ship.parts.Count && rightUnderhang < partCount)
		{
			return rightUnderhang;
		}
		return 999;
	}
}