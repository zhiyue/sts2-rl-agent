namespace MegaCrit.Sts2.Core.Entities.Cards;

public static class ActionTargetExtensions
{
	public static bool IsSingleTarget(this TargetType targetType)
	{
		switch (targetType)
		{
		case TargetType.Self:
		case TargetType.AnyEnemy:
		case TargetType.AnyPlayer:
		case TargetType.AnyAlly:
		case TargetType.TargetedNoCreature:
			return true;
		default:
			return false;
		}
	}
}
