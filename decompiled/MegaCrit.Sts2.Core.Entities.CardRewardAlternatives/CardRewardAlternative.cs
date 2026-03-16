using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;

public class CardRewardAlternative
{
	public string OptionId { get; }

	public LocString Title => new LocString("card_reward_ui", "OPTION_" + OptionId.ToUpperInvariant() + ".name");

	public string Hotkey { get; }

	public Func<Task> OnSelect { get; private set; }

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
		Hotkey = ((afterSelected == PostAlternateCardRewardAction.DismissScreenAndKeepReward) ? MegaInput.cancel : MegaInput.viewExhaustPileAndTabRight);
	}

	public static IReadOnlyList<CardRewardAlternative> Generate(CardReward cardReward)
	{
		List<CardRewardAlternative> list = new List<CardRewardAlternative>();
		if (cardReward.CanSkip)
		{
			list.Add(new CardRewardAlternative("Skip", PostAlternateCardRewardAction.DismissScreenAndKeepReward));
		}
		if (cardReward.CanReroll)
		{
			list.Add(new CardRewardAlternative("REROLL", cardReward.Reroll, PostAlternateCardRewardAction.DoNothing));
		}
		Hook.ModifyCardRewardAlternatives(cardReward.Player.RunState, cardReward.Player, cardReward, list);
		if (list.Count > 2)
		{
			throw new InvalidOperationException("More than 2 card reward alternatives are not supported.");
		}
		return list;
	}
}
