using System;

namespace MegaCrit.Sts2.Core.Hooks;

[Flags]
public enum ModifyDamageHookType
{
	None = 0,
	Additive = 2,
	Multiplicative = 4,
	All = 6
}
