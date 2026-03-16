using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Achievements;

public class SkillSilent1Achievement : AchievementModel
{
	private CardModel? _firstCardOnStack;

	private int _slyCardsPlayed;

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (!LocalContext.IsMine(cardPlay.Card))
		{
			return Task.CompletedTask;
		}
		if (_firstCardOnStack == null)
		{
			_firstCardOnStack = cardPlay.Card;
		}
		return Task.CompletedTask;
	}

	public override Task BeforeCardAutoPlayed(CardModel card, Creature? target, AutoPlayType type)
	{
		if (!LocalContext.IsMine(card))
		{
			return Task.CompletedTask;
		}
		if (type != AutoPlayType.SlyDiscard)
		{
			return Task.CompletedTask;
		}
		if (_firstCardOnStack == null)
		{
			return Task.CompletedTask;
		}
		_slyCardsPlayed++;
		if (_slyCardsPlayed >= 5)
		{
			AchievementsUtil.Unlock(Achievement.CharacterSkillSilent1, card.Owner);
		}
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (!LocalContext.IsMine(cardPlay.Card))
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card == _firstCardOnStack)
		{
			_firstCardOnStack = null;
			_slyCardsPlayed = 0;
		}
		return Task.CompletedTask;
	}

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		_firstCardOnStack = null;
		_slyCardsPlayed = 0;
		return Task.CompletedTask;
	}
}
