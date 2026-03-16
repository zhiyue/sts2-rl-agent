using System;

namespace MegaCrit.Sts2.Core.Entities.Cards;

[Flags]
public enum LocalCostModifierExpiration
{
	EndOfCombat = 0,
	EndOfTurn = 2,
	WhenPlayed = 4
}
