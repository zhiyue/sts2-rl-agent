using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace MegaCrit.Sts2.Core.Timeline.Stories;

public sealed class TheReopeningStory : StoryModel
{
	protected override string Id => "REOPENING";

	public override EpochModel[] Epochs => new EpochModel[6]
	{
		EpochModel.Get<Potion2Epoch>(),
		EpochModel.Get<Relic5Epoch>(),
		EpochModel.Get<Relic3Epoch>(),
		EpochModel.Get<Potion1Epoch>(),
		EpochModel.Get<NeowEpoch>(),
		EpochModel.Get<Event2Epoch>()
	};
}
