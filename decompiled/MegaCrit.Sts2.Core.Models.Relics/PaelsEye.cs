using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PaelsEye : RelicModel
{
	private bool _usedThisCombat;

	private bool _wasOwnerPartOfLastPlayerTurn = true;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	private bool UsedThisCombat
	{
		get
		{
			return _usedThisCombat;
		}
		set
		{
			AssertMutable();
			_usedThisCombat = value;
		}
	}

	/// <summary>
	/// If two players both have Pael's Eye in multiplayer, and only one player uses it, then only that player gets a turn.
	/// In this case, we force the other player not to draw any cards and we auto-end their turn. But even though they
	/// haven't played any cards, that "turn" shouldn't count towards Pael's Eye for them.
	/// </summary>
	private bool WasOwnerPartOfLastPlayerTurn
	{
		get
		{
			return _wasOwnerPartOfLastPlayerTurn;
		}
		set
		{
			AssertMutable();
			_wasOwnerPartOfLastPlayerTurn = value;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));

	public override Task AfterObtained()
	{
		WasOwnerPartOfLastPlayerTurn = CombatManager.Instance.IsPartOfPlayerTurn(base.Owner);
		return Task.CompletedTask;
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (!CombatManager.Instance.IsInProgress)
		{
			return Task.CompletedTask;
		}
		if (UsedThisCombat)
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
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}

	public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (side != base.Owner.Creature.Side)
		{
			return Task.CompletedTask;
		}
		if (UsedThisCombat)
		{
			return Task.CompletedTask;
		}
		if (participants.Contains(base.Owner.Creature))
		{
			base.Status = RelicStatus.Active;
			WasOwnerPartOfLastPlayerTurn = true;
		}
		else
		{
			base.Status = RelicStatus.Normal;
			WasOwnerPartOfLastPlayerTurn = false;
		}
		return Task.CompletedTask;
	}

	public override bool ShouldTakeExtraTurn(Player player)
	{
		if (!UsedThisCombat && !AnyCardsPlayedThisTurn() && WasOwnerPartOfLastPlayerTurn)
		{
			return player == base.Owner;
		}
		return false;
	}

	public override async Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(base.Owner.Creature) || UsedThisCombat || AnyCardsPlayedThisTurn() || !WasOwnerPartOfLastPlayerTurn)
		{
			return;
		}
		foreach (CardModel item in CardPile.GetCards(base.Owner, PileType.Hand).ToList())
		{
			await CardCmd.Exhaust(choiceContext, item);
		}
	}

	public override Task AfterTakingExtraTurn(Player player)
	{
		if (player != base.Owner)
		{
			return Task.CompletedTask;
		}
		Flash();
		base.Status = RelicStatus.Normal;
		UsedThisCombat = true;
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		base.Status = RelicStatus.Normal;
		UsedThisCombat = false;
		return Task.CompletedTask;
	}

	private bool AnyCardsPlayedThisTurn()
	{
		PlayerCombatState? playerCombatState = base.Owner.PlayerCombatState;
		if (playerCombatState != null && playerCombatState.TurnNumber == 1 && base.Owner.Relics.Any((RelicModel r) => r is WhisperingEarring))
		{
			return true;
		}
		return CombatManager.Instance.History.CardPlaysFinished.Any((CardPlayFinishedEntry e) => e.Actor == base.Owner.Creature && e.HappenedThisTurn(base.Owner.Creature.CombatState) && !e.CardPlay.IsAutoPlay);
	}
}
