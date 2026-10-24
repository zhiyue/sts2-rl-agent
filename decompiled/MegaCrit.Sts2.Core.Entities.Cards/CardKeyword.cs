namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// Card Keywords are extra "automatic behaviors" you can add to a card, which also automatically add some extra
/// text to the card's description.
///
/// Examples of things that SHOULD be CardKeywords:
/// * Exhaust: automatically exhausts when played, puts "Exhaust" in the card description.
/// * Unplayable: blocks the card from being played, puts "Unplayable" in the card description.
///
/// Examples of things that should NOT be CardKeywords:
/// * Strike: this has no logic or extra text, just tells Perfected Strike (and some other models) that it's a
///     Strike card.
/// * Block: while this is used on cards that have "Block" in their descriptions, it doesn't automatically add
///     it, nor does it automatically grant block; it's just for HoverTips and effects that care about which cards
///     give block (like the Nimble enchantment, which should only apply to cards that give block).
/// </summary>
public enum CardKeyword
{
	None,
	Exhaust,
	Ethereal,
	Innate,
	Unplayable,
	Retain,
	Sly,
	Eternal
}
