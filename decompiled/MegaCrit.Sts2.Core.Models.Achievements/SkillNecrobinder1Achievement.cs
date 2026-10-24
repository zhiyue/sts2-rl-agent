using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Models.Achievements;

/// <summary>
/// Grants an achievement when a player applies a large amount of Doom to a single creature.
/// </summary>
public class SkillNecrobinder1Achievement : AchievementModel
{
	private const int _doomThreshold = 999;

	public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (!LocalContext.IsMe(applier))
		{
			return Task.CompletedTask;
		}
		if (power is DoomPower && power.Amount >= 999)
		{
			AchievementsUtil.Unlock(Achievement.CharacterSkillNecrobinder1, applier.Player);
		}
		return Task.CompletedTask;
	}
}
