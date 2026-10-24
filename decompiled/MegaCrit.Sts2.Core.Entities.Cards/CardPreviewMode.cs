namespace MegaCrit.Sts2.Core.Entities.Cards;

public enum CardPreviewMode
{
	/// <summary>
	/// Do not calculate any previews.
	/// This should be used on the backend when doing actual value calculations.
	/// </summary>
	None,
	/// <summary>
	/// Calculate the "normal" previews (extra damage from <see cref="T:MegaCrit.Sts2.Core.Models.Powers.StrengthPower" />, etc.)
	/// This should be the default when calculating a card's visuals.
	/// </summary>
	Normal,
	/// <summary>
	/// Calculate previews as if the card were being upgraded.
	/// This is used in <see cref="T:MegaCrit.Sts2.Core.Nodes.Cards.NUpgradePreview" /> and similar areas.
	/// </summary>
	Upgrade,
	/// <summary>
	/// Calculate previews as if multiple creatures were being targeted at once.
	/// This is relevant for cards that target all/random enemies, where all those enemies may have the same power
	/// that would apply to the card.
	/// For example, if all enemies have <see cref="T:MegaCrit.Sts2.Core.Models.Powers.VulnerablePower" />, attacks should preview the extra damage.
	/// If one enemy has it and another does not, attacks should not preview the extra damage.
	/// This is used in <see cref="T:MegaCrit.Sts2.Core.Nodes.Combat.NCardPlay" /> and similar areas.
	/// </summary>
	MultiCreatureTargeting
}
