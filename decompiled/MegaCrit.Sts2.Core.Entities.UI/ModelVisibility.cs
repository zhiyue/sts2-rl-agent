namespace MegaCrit.Sts2.Core.Entities.UI;

/// <summary>
/// Represents how visible a piece of content (RelicModel, PotionModel) is to the player.
/// </summary>
public enum ModelVisibility
{
	/// <summary>
	/// Invalid sentinel value.
	/// </summary>
	None,
	/// <summary>
	/// The content is both unlocked and has been seen by the player in a run.
	/// </summary>
	Visible,
	/// <summary>
	/// The content has been unlocked, but has not yet been seen by the player in a run.
	/// </summary>
	NotSeen,
	/// <summary>
	/// The content is locked (behind an epoch) and cannot be seen.
	/// </summary>
	Locked
}
