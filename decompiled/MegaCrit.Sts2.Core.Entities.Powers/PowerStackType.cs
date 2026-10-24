namespace MegaCrit.Sts2.Core.Entities.Powers;

public enum PowerStackType
{
	None,
	/// <summary>
	/// Amount is visible, and must be manually incremented/decremented via an action.
	/// </summary>
	Counter,
	/// <summary>
	/// Amount is hidden, and is always 1.
	/// </summary>
	Single
}
