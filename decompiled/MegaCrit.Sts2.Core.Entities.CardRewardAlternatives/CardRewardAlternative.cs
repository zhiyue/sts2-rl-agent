using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;

/// <summary>
/// Manages the extra, non card, options in a card reward screen.
/// i.e. Skip (default option), Sacrifice (for Pael  Wing), Heal +2 (for Dream Catcher)
/// </summary>
public class CardRewardAlternative
{
	public string OptionId { get; }

	public LocString Title => new LocString("card_reward_ui", "OPTION_" + OptionId.ToUpperInvariant() + ".name");

	public string Hotkey { get; }

	/// <summary>
	/// Action to perform when the alternative option is selected.
	/// </summary>
	public Func<Task> OnSelect { get; private set; }

	/// <summary>
	/// The action to take after the alternate reward is selected. See the values for more information.
	/// </summary>
	public PostAlternateCardRewardAction AfterSelected { get; private set; }

	public CardRewardAlternative(string optionId, PostAlternateCardRewardAction afterSelected)
		: this(optionId, () => Task.CompletedTask, afterSelected)
	{
	}

	public CardRewardAlternative(string optionId, Func<Task> onSelect, PostAlternateCardRewardAction afterSelected)
	{
		OptionId = optionId;
		OnSelect = onSelect;
		AfterSelected = afterSelected;
		Hotkey = ((afterSelected == PostAlternateCardRewardAction.EndSelectionAndDoNotCompleteReward) ? MegaInput.cancel : MegaInput.viewExhaustPileAndTabRight);
	}

	/// <summary>
	/// Generates a list of extra options to use for the next card reward screen.
	/// This list will include extra options added by models like the Pael Wing relic if the player has them.
	/// </summary>
	/// <param name="cardReward">The reward for which alternatives are being generated.</param>
	/// <returns>List of rest extra options options.</returns>
	public static IReadOnlyList<CardRewardAlternative> Generate(CardReward cardReward)
	{
		List<CardRewardAlternative> list = new List<CardRewardAlternative>();
		if (cardReward.CanSkip)
		{
			list.Add(new CardRewardAlternative("Skip", PostAlternateCardRewardAction.EndSelectionAndDoNotCompleteReward));
		}
		if (cardReward.CanReroll)
		{
			list.Add(new CardRewardAlternative("REROLL", delegate
			{
				cardReward.Reroll();
				return Task.CompletedTask;
			}, PostAlternateCardRewardAction.DoNothing));
		}
		Hook.ModifyCardRewardAlternatives(cardReward.Player.RunState, cardReward.Player, cardReward, list);
		if (list.Count > 2)
		{
			throw new InvalidOperationException("More than 2 card reward alternatives are not supported.");
		}
		return list;
	}
}
