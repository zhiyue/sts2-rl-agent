using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Play 20 cards in a single turn.
/// </summary>
public class CccComboModel : BadgeModel
{
	private int _cardsPlayedThisTurn;

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (!LocalContext.IsMine(cardPlay.Card))
		{
			return Task.CompletedTask;
		}
		_cardsPlayedThisTurn++;
		if (_cardsPlayedThisTurn >= 20)
		{
			Player owner = cardPlay.Card.Owner;
			if (!owner.ExtraFields.CccomboBadgeUnlocked)
			{
				Log.Info("Player played 20 cards in a single turn, ccccombo unlocked");
			}
			owner.ExtraFields.CccomboBadgeUnlocked = true;
		}
		return Task.CompletedTask;
	}

	public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (side != CombatSide.Player)
		{
			return Task.CompletedTask;
		}
		_cardsPlayedThisTurn = 0;
		return Task.CompletedTask;
	}
}
