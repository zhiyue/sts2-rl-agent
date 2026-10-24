using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

/// <summary>
/// Seeds: Enamored by the growth and stability of flora and fauna.
/// </summary>
public class CustomAndSeedsEpoch : EpochModel
{
	public override string Id => "CUSTOM_AND_SEEDS_EPOCH";

	public override EpochEra Era => EpochEra.Seeds0;

	public override int EraPosition => 0;

	public override string StoryId => "Magnum_Opus";

	public override void QueueUnlocks()
	{
		LocString locString = new LocString("epochs", Id + ".unlock");
		NTimelineScreen.Instance.QueueMiscUnlock(locString.GetFormattedText() ?? "");
	}
}
