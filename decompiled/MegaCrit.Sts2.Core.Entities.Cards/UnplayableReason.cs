using System;

namespace MegaCrit.Sts2.Core.Entities.Cards;

[Flags]
public enum UnplayableReason
{
	None = 0,
	HasUnplayableKeyword = 2,
	BlockedByHook = 4,
	BlockedByCardLogic = 8,
	EnergyCostTooHigh = 0x10,
	StarCostTooHigh = 0x20,
	NoLivingAllies = 0x40
}
