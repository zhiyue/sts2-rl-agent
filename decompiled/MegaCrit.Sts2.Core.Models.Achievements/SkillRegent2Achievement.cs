using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Models.Achievements;

public class SkillRegent2Achievement : AchievementModel
{
	private const int _starThreshold = 20;

	public override Task AfterStarsGained(int amount, Player gainer)
	{
		if (!LocalContext.IsMe(gainer))
		{
			return Task.CompletedTask;
		}
		PlayerCombatState? playerCombatState = gainer.PlayerCombatState;
		if (playerCombatState != null && playerCombatState.Stars < 20)
		{
			return Task.CompletedTask;
		}
		AchievementsUtil.Unlock(Achievement.CharacterSkillRegent2, gainer);
		return Task.CompletedTask;
	}
}
