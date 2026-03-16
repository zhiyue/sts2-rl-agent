using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards.Mocks;

public sealed class MockAttackCard : MockCardModel
{
	private int _hitCount = 1;

	private bool _fromOsty;

	private TargetType _targetingType = TargetType.AnyEnemy;

	public override CardType Type => CardType.Attack;

	public override TargetType TargetType => _targetingType;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new DamageVar(6m, ValueProp.Move),
		new OstyDamageVar(6m, ValueProp.Move),
		new BlockVar(0m, ValueProp.Move)
	});

	public override MockAttackCard MockBlock(int block)
	{
		AssertMutable();
		base.DynamicVars.Block.BaseValue = block;
		return this;
	}

	public MockAttackCard MockDamage(decimal damage)
	{
		AssertMutable();
		base.DynamicVars.Damage.BaseValue = damage;
		return this;
	}

	public MockAttackCard MockOstyDamage(decimal damage)
	{
		AssertMutable();
		base.DynamicVars.OstyDamage.BaseValue = damage;
		return this;
	}

	public MockAttackCard MockHitCount(int hitCount)
	{
		AssertMutable();
		_hitCount = hitCount;
		return this;
	}

	public MockAttackCard MockFromOsty()
	{
		AssertMutable();
		_fromOsty = true;
		MockTag(CardTag.OstyAttack);
		return this;
	}

	public MockAttackCard MockTargetingType(TargetType targetingType)
	{
		AssertMutable();
		_targetingType = targetingType;
		return this;
	}

	public MockAttackCard MockUnpoweredDamage()
	{
		AssertMutable();
		base.DynamicVars.Damage.Props = ValueProp.Unpowered;
		base.DynamicVars.OstyDamage.Props = ValueProp.Unpowered;
		return this;
	}

	protected override int GetBaseBlock()
	{
		return base.DynamicVars.Block.IntValue;
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (_mockSelfHpLoss > 0)
		{
			await CreatureCmd.Damage(choiceContext, base.Owner.Creature, _mockSelfHpLoss, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);
		}
		if (base.DynamicVars.Block.BaseValue > 0m)
		{
			await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		}
		int hitCount = (base.EnergyCost.CostsX ? ResolveEnergyXValue() : ((!HasStarCostX) ? _hitCount : ResolveStarXValue()));
		AttackCommand attackCommand = DamageCmd.Attack(_fromOsty ? base.DynamicVars.OstyDamage.BaseValue : base.DynamicVars.Damage.BaseValue).WithHitCount(hitCount);
		attackCommand = ((!_fromOsty) ? attackCommand.FromCard(this) : attackCommand.FromOsty(base.Owner.Osty, this));
		switch (_targetingType)
		{
		case TargetType.AnyEnemy:
			ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
			attackCommand = attackCommand.Targeting(cardPlay.Target);
			break;
		case TargetType.AllEnemies:
			attackCommand = attackCommand.TargetingAllOpponents(base.CombatState);
			break;
		case TargetType.RandomEnemy:
			attackCommand = attackCommand.TargetingRandomOpponents(base.CombatState);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (base.DynamicVars.Damage.Props.HasFlag(ValueProp.Unpowered))
		{
			attackCommand = attackCommand.Unpowered();
		}
		await attackCommand.Execute(choiceContext);
		if (_mockExtraLogic != null)
		{
			await _mockExtraLogic(this);
		}
	}

	protected override void OnUpgrade()
	{
		if (_mockUpgradeLogic != null)
		{
			_mockUpgradeLogic(this);
		}
		else
		{
			base.DynamicVars.Damage.UpgradeValueBy(3m);
		}
	}
}
