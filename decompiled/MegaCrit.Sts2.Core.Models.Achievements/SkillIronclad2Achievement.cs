using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Achievements;

public class SkillIronclad2Achievement : AchievementModel
{
	private const int _damageRequirement = 999;

	public override Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		if (!LocalContext.IsMe(dealer))
		{
			return Task.CompletedTask;
		}
		if (result.UnblockedDamage < 999)
		{
			return Task.CompletedTask;
		}
		AchievementsUtil.Unlock(Achievement.CharacterSkillIronclad2, dealer.Player);
		return Task.CompletedTask;
	}
}
