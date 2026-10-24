namespace MegaCrit.Sts2.Core.Entities.Merchant;

public enum PurchaseStatus
{
	/// <summary>
	/// The purchase was successful.
	/// </summary>
	Success,
	/// <summary>
	/// The purchase failed due to lack of gold.
	/// </summary>
	FailureGold,
	/// <summary>
	/// The purchase failed due to lack of space in the player's inventory.
	/// </summary>
	FailureSpace,
	/// <summary>
	/// The purchase failed because some external effect prevented it (like <see cref="T:MegaCrit.Sts2.Core.Models.Relics.Sozu" /> for a potion purchase).
	/// </summary>
	FailureForbidden,
	/// <summary>
	/// The purchase failed because the Merchant does not have this slot stocked.
	/// </summary>
	FailureOutOfStock
}
