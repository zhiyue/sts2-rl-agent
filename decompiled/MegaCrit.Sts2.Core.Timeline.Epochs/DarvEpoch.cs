using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class DarvEpoch : EpochModel
{
	public override string Id => "DARV_EPOCH";

	public override EpochEra Era => EpochEra.Invitation4;

	public override int EraPosition => 1;

	public override string StoryId => "Tales_From_The_Spire";

	public override void QueueUnlocks()
	{
		LocString locString = new LocString("epochs", Id + ".unlock");
		NTimelineScreen.Instance.QueueMiscUnlock(locString.GetFormattedText() ?? "");
	}
}
