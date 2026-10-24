namespace MegaCrit.Sts2.Core.Runs;

public enum CardCreationSource
{
	None,
	/// <summary>
	/// This card was created for a post-encounter card reward.
	/// </summary>
	Encounter,
	/// <summary>
	/// This card was created to be shown in a shop.
	/// </summary>
	Shop,
	/// <summary>
	/// This card was created for a reward generated from an event or relic.
	/// </summary>
	Other
}
