namespace MegaCrit.Sts2.Core.ValueProps;

internal static class ValuePropExtensions
{
	public static bool IsPoweredAttack(this ValueProp props)
	{
		if (props.HasFlag(ValueProp.Move))
		{
			return !props.HasFlag(ValueProp.Unpowered);
		}
		return false;
	}

	public static bool IsPoweredCardOrMonsterMoveBlock(this ValueProp props)
	{
		if (props.HasFlag(ValueProp.Move))
		{
			return !props.HasFlag(ValueProp.Unpowered);
		}
		return false;
	}

	public static bool IsCardOrMonsterMove(this ValueProp props)
	{
		return props.HasFlag(ValueProp.Move);
	}
}
