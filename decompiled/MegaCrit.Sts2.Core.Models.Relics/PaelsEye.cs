using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PaelsEye : RelicModel
{
	private bool _usedThisCombat;

	private bool _anyCardsPlayedThisTurn;

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

	private bool AnyCardsPlayedThisTurn
	{
		get
		{
			return _anyCardsPlayedThisTurn;
		}
		set
		{
			AssertMutable();
			_anyCardsPlayedThisTurn = value;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (!CombatManager.Instance.IsInProgress)
		{
			return Task.CompletedTask;
		}
		if (AnyCardsPlayedThisTurn || UsedThisCombat)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card.Owner != base.Owner)
		{
			return Task.CompletedTask;
		}
		base.Status = RelicStatus.Normal;
		AnyCardsPlayedThisTurn = true;
		return Task.CompletedTask;
	}

	public override Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side != base.Owner.Creature.Side)
		{
			return Task.CompletedTask;
		}
		if (UsedThisCombat)
		{
			return Task.CompletedTask;
		}
		base.Status = RelicStatus.Active;
		AnyCardsPlayedThisTurn = false;
		return Task.CompletedTask;
	}

	public override bool ShouldTakeExtraTurn(Player player)
	{
		if (!UsedThisCombat && !AnyCardsPlayedThisTurn)
		{
			return player == base.Owner;
		}
		return false;
	}

	public override async Task BeforeTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (UsedThisCombat || AnyCardsPlayedThisTurn || side != CombatSide.Player)
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
}
