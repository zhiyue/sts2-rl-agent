namespace MegaCrit.Sts2.Core.ValueProps;

public static class BlockProps
{
	public const ValueProp card = ValueProp.Move;

	public const ValueProp cardUnpowered = ValueProp.Unpowered | ValueProp.Move;

	public const ValueProp monsterMove = ValueProp.Move;

	public const ValueProp nonCardUnpowered = ValueProp.Unpowered;
}
