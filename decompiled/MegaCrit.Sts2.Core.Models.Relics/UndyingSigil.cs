using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

/// <summary>
/// This relic doesn't actually _do_ anything; Doom checks for its existence and makes enemy Doom trigger at the
/// start of the enemy's turn instead of the end of the enemy's turn (so enemies die before they can attack).
/// </summary>
public sealed class UndyingSigil : RelicModel
{
	private const string _damageDecrease = "DamageDecrease";

	public override RelicRarity Rarity => RelicRarity.Shop;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("DamageDecrease", 0.5m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<DoomPower>());

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (dealer == null)
		{
			return 1m;
		}
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
		if (target != base.Owner.Creature)
		{
			return 1m;
		}
		if (dealer == base.Owner.Creature)
		{
			return 1m;
		}
		if (dealer.CurrentHp > dealer.GetPowerAmount<DoomPower>())
		{
			return 1m;
		}
		return base.DynamicVars["DamageDecrease"].BaseValue;
	}
}
