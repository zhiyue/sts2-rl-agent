namespace MegaCrit.Sts2.Core.Entities.Gold;

/// <summary>
/// Used to help flavor how the gold was lost in the Map History.
/// </summary>
public enum GoldLossType
{
	None,
	/// <summary>
	/// Gold was spent in a transaction. Typically from a shop.
	/// </summary>
	Spent,
	/// <summary>
	/// Gold was lost without involving another entity (i.e. dropping it down a hole in an event).
	/// </summary>
	Lost,
	/// <summary>
	/// Gold was taken by someone (i.e. <see cref="T:MegaCrit.Sts2.Core.Models.Powers.ThieveryPower" />)
	/// </summary>
	Stolen
}
