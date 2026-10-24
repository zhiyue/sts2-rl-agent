using System;

namespace MegaCrit.Sts2.Core.Hooks;

/// <summary>
/// Represents the type(s) of ModifyDamage hooks that should be run for a given instance of damage.
/// </summary>
[Flags]
public enum ModifyDamageHookType
{
	None = 0,
	/// <summary>
	/// Additive damage hooks from effects like <see cref="T:MegaCrit.Sts2.Core.Models.Powers.StrengthPower" />.
	/// Including this will cause <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyDamageAdditive(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" /> and
	/// <see cref="M:MegaCrit.Sts2.Core.Models.EnchantmentModel.EnchantDamageAdditive(System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp)" /> to be called.
	/// At the time of this writing (5/15/2026), we never use this type alone.
	/// If that changes, please update this comment.
	/// </summary>
	Additive = 2,
	/// <summary>
	/// Multiplicative damage hooks from effects like <see cref="T:MegaCrit.Sts2.Core.Models.Powers.VulnerablePower" />.
	/// Including this will cause <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyDamageMultiplicative(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" /> and
	/// <see cref="M:MegaCrit.Sts2.Core.Models.EnchantmentModel.EnchantDamageMultiplicative(System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp)" /> to be called.
	/// At the time of this writing (5/15/2026), we never use this type alone.
	/// If that changes, please update this comment.
	/// </summary>
	Multiplicative = 4,
	/// <summary>
	/// Damage-capping hooks from effects like <see cref="T:MegaCrit.Sts2.Core.Models.Powers.IntangiblePower" />.
	/// Including this will cause <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyDamageCap(MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" /> to be called.
	/// At the time of this writing (5/15/2026), we never use this type alone.
	/// If that changes, please update this comment.
	/// </summary>
	Cap = 8,
	/// <summary>
	/// Include all ModifyDamage hooks.
	/// Most back-end ModifyDamage hook calls will use this.
	/// </summary>
	All = 0xE
}
