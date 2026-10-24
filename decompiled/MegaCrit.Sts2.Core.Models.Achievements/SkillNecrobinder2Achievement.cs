using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Models.Achievements;

/// <summary>
/// Grants an achievement when the player applies a large amount of Strength to Osty.
/// </summary>
public class SkillNecrobinder2Achievement : AchievementModel
{
	private const int _strengthThreshold = 50;

	public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (!LocalContext.IsMe(applier))
		{
			return Task.CompletedTask;
		}
		if (power is StrengthPower && power.Owner.Monster is Osty && power.Amount >= 50)
		{
			AchievementsUtil.Unlock(Achievement.CharacterSkillNecrobinder2, applier.Player);
		}
		return Task.CompletedTask;
	}
}
