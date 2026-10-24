namespace MegaCrit.Sts2.Core.ValueProps;

public static class BlockProps
{
	/// <summary>
	/// Normal block granted by a card.
	/// </summary>
	public const ValueProp card = ValueProp.Move;

	/// <summary>
	/// Unpowered block granted by a card.
	/// Example: <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Entrench" /> ("Double your Block") should not pick up extra block from <see cref="T:MegaCrit.Sts2.Core.Models.Powers.DexterityPower" />.
	/// </summary>
	public const ValueProp cardUnpowered = ValueProp.Unpowered | ValueProp.Move;

	/// <summary>
	/// Normal block granted by a monster move.
	/// </summary>
	public const ValueProp monsterMove = ValueProp.Move;

	/// <summary>
	/// Unpowered block granted by a non-card source.
	/// Example: The <see cref="T:MegaCrit.Sts2.Core.Models.Powers.PlatingPower" /> power ("At the end of your turn, gain X Block") should not pick up extra
	/// block from <see cref="T:MegaCrit.Sts2.Core.Models.Powers.DexterityPower" />.
	/// </summary>
	public const ValueProp nonCardUnpowered = ValueProp.Unpowered;
}
