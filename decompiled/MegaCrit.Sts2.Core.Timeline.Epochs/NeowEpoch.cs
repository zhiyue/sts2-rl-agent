using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class NeowEpoch : EpochModel
{
	public override string Id => "NEOW_EPOCH";

	public override EpochEra Era => EpochEra.Invitation0;

	public override int EraPosition => 1;

	public override string StoryId => "Reopening";

	public override bool IsArtPlaceholder => false;

	public override EpochModel[] GetTimelineExpansion()
	{
		return new EpochModel[12]
		{
			EpochModel.Get(EpochModel.GetId<Colorless1Epoch>()),
			EpochModel.Get(EpochModel.GetId<CustomAndSeedsEpoch>()),
			EpochModel.Get(EpochModel.GetId<DailyRunEpoch>()),
			EpochModel.Get(EpochModel.GetId<DarvEpoch>()),
			EpochModel.Get(EpochModel.GetId<Ironclad2Epoch>()),
			EpochModel.Get(EpochModel.GetId<Ironclad3Epoch>()),
			EpochModel.Get(EpochModel.GetId<Ironclad4Epoch>()),
			EpochModel.Get(EpochModel.GetId<Ironclad5Epoch>()),
			EpochModel.Get(EpochModel.GetId<Ironclad6Epoch>()),
			EpochModel.Get(EpochModel.GetId<Ironclad7Epoch>()),
			EpochModel.Get(EpochModel.GetId<OrobasEpoch>()),
			EpochModel.Get(EpochModel.GetId<Silent1Epoch>())
		};
	}

	public override void QueueUnlocks()
	{
		LocString locString = new LocString("epochs", Id + ".unlock");
		NTimelineScreen.Instance.QueueMiscUnlock(locString.GetFormattedText() ?? "");
		SaveManager.Instance.ObtainEpochOverride(EpochModel.GetId<Silent1Epoch>(), EpochState.ObtainedNoSlot);
		EpochModel.QueueTimelineExpansion(GetTimelineExpansion());
	}
}
