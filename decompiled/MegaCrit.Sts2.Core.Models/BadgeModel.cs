namespace MegaCrit.Sts2.Core.Models;

/// <summary>
/// Model which listens to events and keeps track of stats for badges.
/// This holds no backing data for the badge itself. See <see cref="T:MegaCrit.Sts2.Core.Models.Badges.Badge" /> for that.
/// Only a few badges use this because most unlocks can be computed from the SerializedRun at the end of the run.
/// Badge could probably get merged into this at some point, but be wary, because the IDs don't match up with the IDs
/// generated for models.
/// </summary>
public abstract class BadgeModel : AbstractModel
{
	public override bool ShouldReceiveCombatHooks => true;
}
