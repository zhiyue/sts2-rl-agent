namespace MegaCrit.Sts2.Core.Entities.Creatures;

public static class HpDisplayExtensions
{
	/// <summary>
	/// Does this HP display indicate infinite HP?
	/// </summary>
	public static bool IsInfinite(this HpDisplay display)
	{
		if ((uint)(display - 1) <= 1u)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Does this HP display show Current/Max HP numbers?
	/// </summary>
	public static bool ShowsNumbers(this HpDisplay display)
	{
		if ((uint)display <= 1u)
		{
			return true;
		}
		return false;
	}
}
