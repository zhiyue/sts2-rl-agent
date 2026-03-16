using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Models.Achievements;

public class SkillRegent1Achievement : AchievementModel
{
	private const int _damageThreshold = 999;

	public override Task AfterForge(decimal amount, Player forger, AbstractModel? source)
	{
		if (!LocalContext.IsMe(forger))
		{
			return Task.CompletedTask;
		}
		if (AchievementsUtil.IsUnlocked(Achievement.CharacterSkillRegent1))
		{
			return Task.CompletedTask;
		}
		foreach (SovereignBlade item in forger.PlayerCombatState.AllCards.OfType<SovereignBlade>())
		{
			if (item.DynamicVars.Damage.BaseValue >= 999m)
			{
				AchievementsUtil.Unlock(Achievement.CharacterSkillRegent1, forger);
			}
		}
		return Task.CompletedTask;
	}
}
