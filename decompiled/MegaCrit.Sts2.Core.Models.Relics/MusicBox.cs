using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class MusicBox : RelicModel
{
	private bool _wasUsedThisTurn;

	private CardModel? _cardBeingPlayed;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Ethereal));

	private bool WasUsedThisTurn
	{
		get
		{
			return _wasUsedThisTurn;
		}
		set
		{
			AssertMutable();
			_wasUsedThisTurn = value;
		}
	}

	private CardModel? CardBeingPlayed
	{
		get
		{
			return _cardBeingPlayed;
		}
		set
		{
			AssertMutable();
			_cardBeingPlayed = value;
		}
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (CardBeingPlayed != null)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card.Owner != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (WasUsedThisTurn)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card.Type != CardType.Attack)
		{
			return Task.CompletedTask;
		}
		CardBeingPlayed = cardPlay.Card;
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card == CardBeingPlayed)
		{
			Flash();
			CardModel card = cardPlay.Card.CreateClone();
			CardCmd.ApplyKeyword(card, CardKeyword.Ethereal);
			await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, base.Owner);
			WasUsedThisTurn = true;
			CardBeingPlayed = null;
		}
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(base.Owner.Creature))
		{
			return Task.CompletedTask;
		}
		WasUsedThisTurn = false;
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		WasUsedThisTurn = false;
		return Task.CompletedTask;
	}
}
