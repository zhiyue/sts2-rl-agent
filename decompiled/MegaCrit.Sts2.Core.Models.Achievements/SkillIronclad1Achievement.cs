using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Achievements;

public class SkillIronclad1Achievement : AchievementModel
{
	private const int _exhaustRequirement = 20;

	private int _cardsExhaustedThisCombat;

	public override Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (!LocalContext.IsMine(card))
		{
			return Task.CompletedTask;
		}
		_cardsExhaustedThisCombat++;
		if (_cardsExhaustedThisCombat >= 20)
		{
			AchievementsUtil.Unlock(Achievement.CharacterSkillIronclad1, card.Owner);
		}
		return Task.CompletedTask;
	}

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		_cardsExhaustedThisCombat = 0;
		return Task.CompletedTask;
	}
}
