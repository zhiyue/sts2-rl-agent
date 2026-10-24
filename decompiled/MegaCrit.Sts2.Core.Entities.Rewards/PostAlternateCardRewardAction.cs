namespace MegaCrit.Sts2.Core.Entities.Rewards;

public enum PostAlternateCardRewardAction
{
	None,
	/// <summary>
	/// After the alternate reward button is pressed, end card selection, but don't complete it - the player may
	/// re-enter card selection.
	/// Used for the skip button.
	/// </summary>
	EndSelectionAndDoNotCompleteReward,
	/// <summary>
	/// After the alternate reward button is pressed, end card selection and complete the card reward - the player may
	/// not go back and select the reward.
	/// Used for the Sacrifice button.
	/// </summary>
	EndSelectionAndCompleteReward,
	/// <summary>
	/// After the alternate reward button is pressed, nothing happens. The reward screen stays up and the player is
	/// allowed to select a reward.
	/// Used when card rewards are rerolled.
	/// </summary>
	DoNothing
}
