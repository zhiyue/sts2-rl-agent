using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace MegaCrit.Sts2.Core.Timeline.Stories;

public sealed class MagnumOpusStory : StoryModel
{
	protected override string Id => "MAGNUM_OPUS";

	public override EpochModel[] Epochs => new EpochModel[9]
	{
		EpochModel.Get<Colorless1Epoch>(),
		EpochModel.Get<Relic1Epoch>(),
		EpochModel.Get<Colorless2Epoch>(),
		EpochModel.Get<CustomAndSeedsEpoch>(),
		EpochModel.Get<Relic2Epoch>(),
		EpochModel.Get<Colorless3Epoch>(),
		EpochModel.Get<Colorless4Epoch>(),
		EpochModel.Get<Relic4Epoch>(),
		EpochModel.Get<Event3Epoch>()
	};
}
