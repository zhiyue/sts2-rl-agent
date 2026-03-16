using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Models.Achievements;

public class Play20CardsSingleTurnAchievement : AchievementModel
{
	private int _cardsPlayedThisTurn;

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (!LocalContext.IsMine(cardPlay.Card))
		{
			return Task.CompletedTask;
		}
		_cardsPlayedThisTurn++;
		if (_cardsPlayedThisTurn >= 20)
		{
			AchievementsUtil.Unlock(Achievement.Play20CardsSingleTurn, cardPlay.Card.Owner);
		}
		return Task.CompletedTask;
	}

	public override Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side != CombatSide.Player)
		{
			return Task.CompletedTask;
		}
		_cardsPlayedThisTurn = 0;
		return Task.CompletedTask;
	}
}
