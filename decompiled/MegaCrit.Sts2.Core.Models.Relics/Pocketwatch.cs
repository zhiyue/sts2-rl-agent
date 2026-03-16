using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Pocketwatch : RelicModel
{
	private const string _cardThresholdKey = "CardThreshold";

	private int _cardsPlayedThisTurn;

	private int _cardsPlayedLastTurn;

	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool ShowCounter => CombatManager.Instance.IsInProgress;

	public override int DisplayAmount => _cardsPlayedThisTurn;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("CardThreshold", 3m),
		new CardsVar(3)
	});

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (!CombatManager.Instance.IsInProgress)
		{
			return Task.CompletedTask;
		}
		_cardsPlayedThisTurn++;
		RefreshCounter();
		return Task.CompletedTask;
	}

	public override decimal ModifyHandDraw(Player player, decimal count)
	{
		if (player.Creature.CombatState.RoundNumber == 1)
		{
			return count;
		}
		if (player != base.Owner)
		{
			return count;
		}
		if ((decimal)_cardsPlayedLastTurn > base.DynamicVars["CardThreshold"].BaseValue)
		{
			return count;
		}
		return count + base.DynamicVars.Cards.BaseValue;
	}

	public override Task AfterModifyingHandDraw()
	{
		Flash();
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side == base.Owner.Creature.Side)
		{
			_cardsPlayedLastTurn = _cardsPlayedThisTurn;
			_cardsPlayedThisTurn = 0;
		}
		return Task.CompletedTask;
	}

	public override Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side == base.Owner.Creature.Side)
		{
			RefreshCounter();
		}
		return Task.CompletedTask;
	}

	private void RefreshCounter()
	{
		base.Status = (((decimal)_cardsPlayedThisTurn <= base.DynamicVars["CardThreshold"].BaseValue) ? RelicStatus.Active : RelicStatus.Normal);
		InvokeDisplayAmountChanged();
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		_cardsPlayedThisTurn = 0;
		_cardsPlayedLastTurn = 0;
		base.Status = RelicStatus.Normal;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}
}
