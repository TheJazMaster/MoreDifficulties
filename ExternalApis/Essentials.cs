namespace TheJazMaster.MoreDifficulties;

public interface IEssentialsApi
{
	UK ShipSelectionToggleUiKey { get; }

	UK ShipSelectionUiKey { get; }

	bool IsShowingShips { get; }
	
	StarterShip? PreviewingShip { get; }
}