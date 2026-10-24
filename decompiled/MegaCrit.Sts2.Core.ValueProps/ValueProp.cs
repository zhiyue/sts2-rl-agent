using System;

namespace MegaCrit.Sts2.Core.ValueProps;

/// <summary>
/// Aka damage type:
///   - Unblockable = HP loss like Poison
///   - Unpowered = Damage from relics, potions, and powers
///   - Move = Attack damage from Attack cards and Enemy creatures attacking
/// </summary>
[Flags]
public enum ValueProp
{
	Unblockable = 2,
	Unpowered = 4,
	Move = 8,
	SkipHurtAnim = 0x10
}
