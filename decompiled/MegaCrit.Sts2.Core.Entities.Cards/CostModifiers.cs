using System;

namespace MegaCrit.Sts2.Core.Entities.Cards;

[Flags]
public enum CostModifiers
{
	None = 0,
	Local = 2,
	Global = 4,
	All = -1
}
