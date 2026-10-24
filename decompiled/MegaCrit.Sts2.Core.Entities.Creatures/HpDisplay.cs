namespace MegaCrit.Sts2.Core.Entities.Creatures;

public enum HpDisplay
{
	/// <summary>
	/// Normal HP display (red background with poison/doom overlays, Current/Max HP numbers).
	/// </summary>
	Normal,
	/// <summary>
	/// Purple "infinite" background with Current/Max HP numbers.
	/// </summary>
	InfiniteWithNumbers,
	/// <summary>
	/// Purple "infinite" background with infinity symbol instead of Current/Max HP numbers.
	/// </summary>
	InfiniteWithoutNumbers
}
