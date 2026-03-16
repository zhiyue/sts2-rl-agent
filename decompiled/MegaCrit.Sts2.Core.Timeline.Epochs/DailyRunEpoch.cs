using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class DailyRunEpoch : EpochModel
{
	public override string Id => "DAILY_RUN_EPOCH";

	public override EpochEra Era => EpochEra.Invitation6;

	public override int EraPosition => 0;

	public override string StoryId => "Tales_From_The_Spire";

	public override void QueueUnlocks()
	{
		LocString locString = new LocString("epochs", Id + ".unlock");
		NTimelineScreen.Instance.QueueMiscUnlock(locString.GetFormattedText() ?? "");
	}
}
