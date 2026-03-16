using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class OrobasEpoch : EpochModel
{
	public override string Id => "OROBAS_EPOCH";

	public override EpochEra Era => EpochEra.Flourish0;

	public override int EraPosition => 2;

	public override string StoryId => "Tales_From_The_Spire";

	public override bool IsArtPlaceholder => false;

	public override void QueueUnlocks()
	{
		LocString locString = new LocString("epochs", Id + ".unlock");
		NTimelineScreen.Instance.QueueMiscUnlock(locString.GetFormattedText() ?? "");
	}
}
