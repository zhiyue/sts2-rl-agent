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

	/// <summary>
	/// If non-null, then the default sort when displayed in the selector will use this comparer. Otherwise, cards will
	/// be displayed as ordered in the list.
	/// In most cases the order passed to the CardSelectCmd must be stable across machines. Therefore, we have to do any
	/// culture-variant sorts on the frontend only.
	/// This is currently only supported when using FromSimpleGrid.
	/// </summary>
	public Comparison<CardModel>? Comparison { get; init; }

	/// <summary>
	/// If set, card previews displayed during the selection will be unaffected by any external powers or hooks. This
	/// includes enchantments and afflictions!
	/// Only applies to hand card selections.
	/// </summary>
	public bool UnpoweredPreviews { get; init; }

	/// <summary>
	/// If set, energy/star cost color highlighting will be suppressed for cards that wouldn't be able to be played due
	/// to insufficient energy/stars.
	/// Only applicable to hand card selection.
	/// </summary>
	public bool PretendCardsCanBePlayed { get; init; }

	/// <summary>
	/// If set, cards that pass the predicate will glow gold.
	/// Only applicable to hand card selection.
	/// </summary>
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
