using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards.Mocks;

public sealed class MockSkillCard : MockCardModel
{
	private struct PowerApplication
	{
		public Type powerType;

		public int amount;

		public TargetType targetType;
	}

	private const string _drawKey = "Draw";

	private const string _discardKey = "Discard";

	private int _blockCount = 1;

	private TargetType _targetType = TargetType.Self;

	private List<PowerApplication> _powerApplications = new List<PowerApplication>();

	public override CardType Type => CardType.Skill;

	public override TargetType TargetType => _targetType;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[6]
	{
		new BlockVar(5m, ValueProp.Move),
		new CardsVar("Draw", 0),
		new CardsVar("Discard", 0),
		new ForgeVar(0),
		new StarsVar(0),
		new SummonVar(0m)
	});

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_powerApplications = _powerApplications.ToList();
	}

	public override MockSkillCard MockBlock(int block)
	{
		AssertMutable();
		base.DynamicVars.Block.BaseValue = block;
		return this;
	}

	public MockSkillCard MockBlockCount(int blockCount)
	{
		AssertMutable();
		_blockCount = blockCount;
		return this;
	}

	public MockSkillCard MockDraw(int cards)
	{
		AssertMutable();
		base.DynamicVars["Draw"].BaseValue = cards;
		return this;
	}

	public MockSkillCard MockSummon(int summons)
	{
		AssertMutable();
		base.DynamicVars.Summon.BaseValue = summons;
		return this;
	}

	public MockSkillCard MockDiscard(int cards)
	{
		AssertMutable();
		base.DynamicVars["Discard"].BaseValue = cards;
		return this;
	}

	public MockSkillCard MockForge(decimal forge)
	{
		AssertMutable();
		base.DynamicVars.Forge.BaseValue = forge;
		return this;
	}

	public MockSkillCard MockStarGain(decimal stars)
	{
		AssertMutable();
		base.DynamicVars.Stars.BaseValue = stars;
		return this;
	}

	public MockSkillCard MockPower<TPower>(int amount, TargetType targetType) where TPower : PowerModel
	{
		AssertMutable();
		if (_powerApplications.Any((PowerApplication a) => a.targetType != targetType))
		{
			throw new InvalidOperationException("Cannot have multiple power applications with different target types.");
		}
		_targetType = targetType;
		_powerApplications.Add(new PowerApplication
		{
			powerType = typeof(TPower),
			amount = amount,
			targetType = targetType
		});
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
		for (int i = 0; i < _blockCount; i++)
		{
			await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		}
		if (base.DynamicVars["Draw"].IntValue > 0)
		{
			await CardPileCmd.Draw(choiceContext, base.DynamicVars["Draw"].IntValue, base.Owner);
		}
		if (base.DynamicVars["Discard"].IntValue > 0)
		{
			await CardCmd.Discard(choiceContext, await CardSelectCmd.FromHandForDiscard(prefs: new CardSelectorPrefs(new LocString("cards", "MOCK_SKILL_CARD.discardSelectionPrompt"), base.DynamicVars["Discard"].IntValue), context: choiceContext, player: base.Owner, filter: null, source: this));
		}
		if (base.DynamicVars.Forge.BaseValue > 0m)
		{
			await ForgeCmd.Forge(base.DynamicVars.Forge.BaseValue, base.Owner, this);
		}
		if (base.DynamicVars.Stars.BaseValue > 0m)
		{
			await PlayerCmd.GainStars(base.DynamicVars.Stars.BaseValue, base.Owner);
		}
		if (base.DynamicVars.Summon.BaseValue > 0m)
		{
			await OstyCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);
		}
		foreach (PowerApplication application in _powerApplications)
		{
			foreach (Creature item in application.targetType switch
			{
				TargetType.AllEnemies => base.CombatState.Enemies, 
				TargetType.AnyEnemy => new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(cardPlay.Target), 
				TargetType.Self => new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(base.Owner.Creature), 
				_ => throw new ArgumentOutOfRangeException("targetType", application.targetType, null), 
			})
			{
				PowerModel power = ModelDb.GetById<PowerModel>(ModelDb.GetId(application.powerType)).ToMutable();
				await PowerCmd.Apply(power, item, application.amount, base.Owner.Creature, this);
			}
		}
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
			base.DynamicVars.Block.UpgradeValueBy(3m);
		}
	}
}
