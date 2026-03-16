using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards.Mocks;

public sealed class MockPowerCard : MockCardModel
{
	protected override int CanonicalEnergyCost => 1;

	public override CardType Type => CardType.Power;

	public override TargetType TargetType => TargetType.Self;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new PowerVar<StrengthPower>(2m),
		new BlockVar(0m, ValueProp.Move)
	});

	public override MockCardModel MockBlock(int block)
	{
		AssertMutable();
		base.DynamicVars.Block.BaseValue = block;
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
		decimal baseValue = base.DynamicVars.Strength.BaseValue;
		await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, baseValue, base.Owner.Creature, this);
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
			base.DynamicVars.Strength.UpgradeValueBy(1m);
		}
	}
}
