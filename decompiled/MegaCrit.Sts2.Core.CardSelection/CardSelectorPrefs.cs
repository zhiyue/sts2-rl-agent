using System;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.CardSelection;

public struct CardSelectorPrefs
{
	private const string _cardSelectionLocFilePath = "card_selection";

	public static LocString TransformSelectionPrompt => new LocString("card_selection", "TO_TRANSFORM");

	public static LocString ExhaustSelectionPrompt => new LocString("card_selection", "TO_EXHAUST");

	public static LocString RemoveSelectionPrompt => new LocString("card_selection", "TO_REMOVE");

	public static LocString EnchantSelectionPrompt => new LocString("card_selection", "TO_ENCHANT");

	public static LocString DiscardSelectionPrompt => new LocString("card_selection", "TO_DISCARD");

	public static LocString UpgradeSelectionPrompt => new LocString("card_selection", "TO_UPGRADE");

	public LocString Prompt { get; }

	public int MinSelect { get; }

	public int MaxSelect { get; }

	public bool RequireManualConfirmation { get; init; }

	public bool Cancelable { get; init; }

	public bool UnpoweredPreviews { get; init; }

	public bool PretendCardsCanBePlayed { get; init; }

	public Func<CardModel, bool>? ShouldGlowGold { get; set; }

	public CardSelectorPrefs(LocString prompt, int selectCount)
		: this(prompt, selectCount, selectCount)
	{
		Prompt.Add("Amount", selectCount);
	}

	public CardSelectorPrefs(LocString prompt, int minCount, int maxCount)
	{
		this = default(CardSelectorPrefs);
		Prompt = prompt;
		Prompt.Add("Amount", maxCount);
		Prompt.Add("MinCount", minCount);
		Prompt.Add("MaxCount", maxCount);
		MaxSelect = maxCount;
		MinSelect = minCount;
		RequireManualConfirmation = MinSelect >= 0 && MinSelect != MaxSelect;
	}
}
