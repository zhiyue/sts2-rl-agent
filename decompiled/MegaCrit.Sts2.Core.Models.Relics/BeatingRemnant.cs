using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BeatingRemnant : RelicModel
{
	private const string _maxHpLossKey = "MaxHpLoss";

	private decimal _damageReceivedThisTurn;

	public override RelicRarity Rarity => RelicRarity.Rare;

	private decimal DamageReceivedThisTurn
	{
		get
		{
			return _damageReceivedThisTurn;
		}
		set
		{
			AssertMutable();
			_damageReceivedThisTurn = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("MaxHpLoss", 20m));

	/// <summary>
	/// Note: We can't use <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyDamageCap(MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" /> because we may be taking "trample" damage from Osty.
	/// The ModifyDamageX hooks will have already run for that initial damage to Osty, and we currently can't re-run
	/// them for the trample damage.
	/// </summary>
	public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (!CombatManager.Instance.IsInProgress)
		{
			return amount;
		}
		if (target != base.Owner.Creature)
		{
			return amount;
		}
		return Math.Min(amount, base.DynamicVars["MaxHpLoss"].BaseValue - DamageReceivedThisTurn);
	}

	public override Task AfterModifyingHpLostAfterOsty()
	{
		Flash();
		return Task.CompletedTask;
	}

	public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (!CombatManager.Instance.IsInProgress)
		{
			return Task.CompletedTask;
		}
		if (target != base.Owner.Creature)
		{
			return Task.CompletedTask;
		}
		DamageReceivedThisTurn += (decimal)result.UnblockedDamage;
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(base.Owner.Creature))
		{
			return Task.CompletedTask;
		}
		DamageReceivedThisTurn = 0m;
		return Task.CompletedTask;
	}
}
