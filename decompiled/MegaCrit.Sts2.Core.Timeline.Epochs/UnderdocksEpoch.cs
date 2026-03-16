using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class UnderdocksEpoch : EpochModel
{
	public override string Id => "UNDERDOCKS_EPOCH";

	public override EpochEra Era => EpochEra.Invitation1;

	public override int EraPosition => 4;

	public override string StoryId => "Tales_From_The_Spire";

	public override EpochModel[] GetTimelineExpansion()
	{
		return new EpochModel[3]
		{
			EpochModel.Get(EpochModel.GetId<Colorless2Epoch>()),
			EpochModel.Get(EpochModel.GetId<Relic2Epoch>()),
			EpochModel.Get(EpochModel.GetId<Potion2Epoch>())
		};
	}

	public override void QueueUnlocks()
	{
		LocString locString = new LocString("epochs", Id + ".unlock");
		NTimelineScreen.Instance.QueueMiscUnlock(locString.GetFormattedText() ?? "");
		EpochModel.QueueTimelineExpansion(GetTimelineExpansion());
	}
}
