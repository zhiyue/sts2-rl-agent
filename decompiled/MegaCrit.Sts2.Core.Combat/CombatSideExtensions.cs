using System;

namespace MegaCrit.Sts2.Core.Combat;

internal static class CombatSideExtensions
{
	public static CombatSide GetOppositeSide(this CombatSide side)
	{
		return side switch
		{
			CombatSide.None => CombatSide.None, 
			CombatSide.Player => CombatSide.Enemy, 
			CombatSide.Enemy => CombatSide.Player, 
			_ => throw new ArgumentOutOfRangeException("side", side, null), 
		};
	}
}
