using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace MegaCrit.Sts2.Core.Timeline.Stories;

public sealed class TalesFromTheSpireStory : StoryModel
{
	protected override string Id => "TALES_FROM_THE_SPIRE";

	public override EpochModel[] Epochs => new EpochModel[6]
	{
		EpochModel.Get<Colorless5Epoch>(),
		EpochModel.Get<OrobasEpoch>(),
		EpochModel.Get<Event1Epoch>(),
		EpochModel.Get<UnderdocksEpoch>(),
		EpochModel.Get<DarvEpoch>(),
		EpochModel.Get<DailyRunEpoch>()
	};
}
