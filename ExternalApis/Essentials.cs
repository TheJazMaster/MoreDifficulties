using System;

namespace TheJazMaster.MoreDifficulties;

/// <summary>
/// Provides access to <c>Nickel.Essentials</c> APIs.
/// </summary>
public interface IEssentialsApi
{
	/// <summary>The <see cref="UK"/> part of a <see cref="UIKey"/> used by the button that toggles between showing a list of ships on the <see cref="NewRunOptions"/> screen.</summary>
	UK ShipSelectionToggleUiKey { get; }

	/// <summary>The <see cref="UK"/> part of a <see cref="UIKey"/> used by each button on the list of ships on the <see cref="NewRunOptions"/> screen. The <see cref="UIKey.str"/> will be set to the ship's key.</summary>
	UK ShipSelectionUiKey { get; }
}