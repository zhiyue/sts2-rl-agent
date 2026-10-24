namespace MegaCrit.Sts2.Core.Models;

public interface ITemporaryPower
{
	/// <summary>
	/// The canonical model that applies this power.
	/// </summary>
	/// <example><see cref="T:MegaCrit.Sts2.Core.Models.Potions.FlexPotion" /> for <see cref="T:MegaCrit.Sts2.Core.Models.Powers.FlexPotionPower" /></example>
	AbstractModel OriginModel { get; }

	/// <summary>
	/// The canonical power that this power internally applies.
	/// </summary>
	/// <example><see cref="T:MegaCrit.Sts2.Core.Models.Powers.StrengthPower" /> for <see cref="T:MegaCrit.Sts2.Core.Models.Powers.TemporaryStrengthPower" /></example>
	PowerModel InternallyAppliedPower { get; }

	/// <summary>
	/// Set the next application of this power to not actually be applied.
	/// This is used when debuffs are copied to other creatures by effects like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Misery" />.
	/// In these cases, the internal debuff gets copied along with this power, and upon copying, it should not apply the
	/// internal debuff again.
	/// </summary>
	void IgnoreNextInstance();
}
