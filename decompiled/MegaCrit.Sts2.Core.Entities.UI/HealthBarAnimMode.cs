namespace MegaCrit.Sts2.Core.Entities.UI;

public enum HealthBarAnimMode
{
	None,
	/// <summary>
	/// Enemy was spawned at combat start, and their HP bar should animate in after a randomized small delay
	/// </summary>
	SpawnedAtCombatStart,
	/// <summary>
	/// Enemy was spawned in a new space during combat, and their HP bar should animate in immediately, but slowly
	/// </summary>
	SpawnedDuringCombat,
	/// <summary>
	/// Enemy already exists and we're animating its HP bar in from a hidden state
	/// </summary>
	FromHidden
}
