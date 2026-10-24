namespace MegaCrit.Sts2.Core.Models;

/// <summary>
/// Model which listens to events and unlocks an achievement when conditions are met.
/// Note only combat-related achievements are unlocked this way. Some are unlocked via calls from nodes. Look at all uses
/// of AchievementsUtil.Unlock to find all achievement unlocks.
/// </summary>
public abstract class AchievementModel : AbstractModel
{
	public override bool ShouldReceiveCombatHooks => true;
}
