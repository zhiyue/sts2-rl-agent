namespace MegaCrit.Sts2.Core.ValueProps;

public static class DamageProps
{
	/// <summary>
	/// Normal damage done by a card.
	/// </summary>
	public const ValueProp card = ValueProp.Move;

	/// <summary>
	/// Unpowered damage done by a card.
	/// Generally used by Curse/Status cards that do damage to the player.
	/// </summary>
	public const ValueProp cardUnpowered = ValueProp.Unpowered | ValueProp.Move;

	/// <summary>
	/// HP loss from a card.
	/// Used by cards that have a "Lose X HP" drawback like Offering.
	/// </summary>
	public const ValueProp cardHpLoss = ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move;

	/// <summary>
	/// Normal damage done by a monster move
	/// </summary>
	public const ValueProp monsterMove = ValueProp.Move;

	/// <summary>
	/// Unpowered damage done by a source other than a card or monster move.
	/// Most commonly used by damaging powers whose damage should still be blockable, like Thorns.
	/// </summary>
	public const ValueProp nonCardUnpowered = ValueProp.Unpowered;

	/// <summary>
	/// HP loss from a source other than a card or monster move.
	/// Most commonly used by damaging powers whose damage should be unblockable, like Poison.
	/// </summary>
	public const ValueProp nonCardHpLoss = ValueProp.Unblockable | ValueProp.Unpowered;
}
