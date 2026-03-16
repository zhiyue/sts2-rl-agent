using System;

namespace MegaCrit.Sts2.Core.ValueProps;

[Flags]
public enum ValueProp
{
	Unblockable = 2,
	Unpowered = 4,
	Move = 8,
	SkipHurtAnim = 0x10
}
