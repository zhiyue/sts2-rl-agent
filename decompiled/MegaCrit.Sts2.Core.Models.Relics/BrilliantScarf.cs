using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BrilliantScarf : RelicModel
{
	private int _cardsPlayedThisTurn;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool ShowCounter
	{
		get
		{
			if (CombatManager.Instance.IsInProgress)
			{
				return CardsPlayedThisTurn < base.DynamicVars.Cards.IntValue;
			}
			return false;
		}
	}

	public override int DisplayAmount => CardsPlayedThisTurn;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(5));

	private int CardsPlayedThisTurn
	{
		get
		{
			return _cardsPlayedThisTurn;
		}
		set
		{
			AssertMutable();
			_cardsPlayedThisTurn = value;
			UpdateDisplay();
		}
	}

	private void UpdateDisplay()
	{
		int intValue = base.DynamicVars.Cards.IntValue;
		base.Status = ((CardsPlayedThisTurn == intValue - 1) ? RelicStatus.Active : RelicStatus.Normal);
		InvokeDisplayAmountChanged();
	}

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (!ShouldModifyCost(card))
		{
			return false;
		}
		modifiedCost = default(decimal);
		return true;
	}

	public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (!ShouldModifyCost(card))
		{
			return false;
		}
		modifiedCost = default(decimal);
		return true;
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side != base.Owner.Creature.Side)
		{
			return Task.CompletedTask;
		}
		CardsPlayedThisTurn = 0;
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (!CombatManager.Instance.IsInProgress)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.IsAutoPlay)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card.Owner != base.Owner)
		{
			return Task.CompletedTask;
		}
		CardsPlayedThisTurn++;
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		CardsPlayedThisTurn = 0;
		return Task.CompletedTask;
	}

	private bool ShouldModifyCost(CardModel card)
	{
		if (!CombatManager.Instance.IsInProgress)
		{
			return false;
		}
		if (card.Owner.Creature != base.Owner.Creature)
		{
			return false;
		}
		if ((decimal)CardsPlayedThisTurn != base.DynamicVars.Cards.BaseValue - 1m)
		{
			return false;
		}
		bool flag;
		switch (card.Pile?.Type)
		{
		case PileType.Hand:
		case PileType.Play:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}
}
