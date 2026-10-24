using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Models.Achievements;

/// <summary>
/// Grants an achievement when the player applies a large amount of poison to a single creature.
/// </summary>
public class SkillSilent2Achievement : AchievementModel
{
	private const int _poisonThreshold = 99;

	public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (!LocalContext.IsMe(applier))
		{
			return Task.CompletedTask;
		}
		if (power is PoisonPower && power.Amount >= 99)
		{
			AchievementsUtil.Unlock(Achievement.CharacterSkillSilent2, applier.Player);
		}
		return Task.CompletedTask;
	}
}
