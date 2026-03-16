namespace MegaCrit.Sts2.Core.ValueProps;

public static class DamageProps
{
	public const ValueProp card = ValueProp.Move;

	public const ValueProp cardUnpowered = ValueProp.Unpowered | ValueProp.Move;

	public const ValueProp cardHpLoss = ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move;

	public const ValueProp monsterMove = ValueProp.Move;

	public const ValueProp nonCardUnpowered = ValueProp.Unpowered;

	public const ValueProp nonCardHpLoss = ValueProp.Unblockable | ValueProp.Unpowered;
}
