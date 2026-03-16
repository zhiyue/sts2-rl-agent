using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MegaCrit.Sts2.Core.Models.Cards.Mocks;

public abstract class MockCardModel : CardModel
{
	protected int _mockEnergyCost = 1;

	protected CardMultiplayerConstraint _mockMultiplayerConstraint;

	protected bool _mockEnergyCostX;

	protected int _mockStarCost;

	protected bool _mockStarCostX;

	protected Func<CardModel, Task>? _mockExtraLogic;

	protected int _mockMaxUpgradeLevel = 1;

	protected CardRarity _mockRarity = CardRarity.Common;

	protected int _mockSelfHpLoss;

	protected HashSet<CardTag>? _mockTags;

	protected CardPoolModel? _mockPool;

	protected Action<CardModel>? _mockUpgradeLogic;

	protected override int CanonicalEnergyCost => _mockEnergyCost;

	protected override bool HasEnergyCostX => _mockEnergyCostX;

	public override int CanonicalStarCost => _mockStarCost;

	public override bool HasStarCostX => _mockStarCostX;

	public override CardMultiplayerConstraint MultiplayerConstraint => _mockMultiplayerConstraint;

	public override bool GainsBlock => GetBaseBlock() > 0;

	public override int MaxUpgradeLevel => _mockMaxUpgradeLevel;

	public override CardRarity Rarity => _mockRarity;

	public override CardPoolModel Pool => _mockPool ?? base.Pool;

	public override IEnumerable<CardTag> Tags => _mockTags ?? new HashSet<CardTag>();

	protected MockCardModel()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	public abstract MockCardModel MockBlock(int block);

	public MockCardModel MockCanonical()
	{
		AssertMutable();
		base.CombatState?.RemoveCard(this);
		NeverEverCallThisOutsideOfTests_ClearOwner();
		NeverEverCallThisOutsideOfTests_SetIsMutable(isMutable: false);
		return this;
	}

	public MockCardModel MockEnergyCost(int cost)
	{
		AssertMutable();
		_mockEnergyCost = cost;
		_mockEnergyCostX = false;
		MockSetEnergyCost(new CardEnergyCost(this, cost, costsX: false));
		return this;
	}

	public MockCardModel MockMultiplayerType(CardMultiplayerConstraint constraint)
	{
		AssertMutable();
		_mockMultiplayerConstraint = constraint;
		return this;
	}

	public MockCardModel MockEnergyCostX()
	{
		AssertMutable();
		_mockEnergyCostX = true;
		_mockEnergyCost = 0;
		MockSetEnergyCost(new CardEnergyCost(this, 0, costsX: true));
		return this;
	}

	public MockCardModel MockStarCost(int cost)
	{
		AssertMutable();
		_mockStarCost = cost;
		_mockStarCostX = false;
		return this;
	}

	public MockCardModel MockStarCostX()
	{
		AssertMutable();
		_mockStarCostX = true;
		_mockStarCost = 0;
		return this;
	}

	public MockCardModel MockExtraLogic(Func<CardModel, Task> extraLogic)
	{
		AssertMutable();
		_mockExtraLogic = extraLogic;
		return this;
	}

	public MockCardModel MockKeyword(CardKeyword keyword)
	{
		AssertMutable();
		AddKeyword(keyword);
		return this;
	}

	public MockCardModel MockReplay(int count)
	{
		AssertMutable();
		base.BaseReplayCount = count;
		return this;
	}

	public MockCardModel MockRarity(CardRarity rarity)
	{
		AssertMutable();
		_mockRarity = rarity;
		return this;
	}

	public MockCardModel MockPool<T>() where T : CardPoolModel
	{
		AssertMutable();
		_mockPool = ModelDb.CardPool<T>();
		return this;
	}

	public MockCardModel MockTag(CardTag tag)
	{
		AssertMutable();
		if (_mockTags == null)
		{
			_mockTags = new HashSet<CardTag>();
		}
		_mockTags.Add(tag);
		return this;
	}

	public MockCardModel MockSelfHpLoss(int hpLoss)
	{
		AssertMutable();
		_mockSelfHpLoss = hpLoss;
		return this;
	}

	public MockCardModel MockUnUpgradable()
	{
		AssertMutable();
		_mockMaxUpgradeLevel = 0;
		return this;
	}

	public MockCardModel MockUpgradeLogic(Action<CardModel> upgradeLogic)
	{
		AssertMutable();
		_mockUpgradeLogic = upgradeLogic;
		return this;
	}

	protected abstract int GetBaseBlock();
}
