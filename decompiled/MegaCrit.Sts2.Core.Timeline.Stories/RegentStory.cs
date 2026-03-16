using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace MegaCrit.Sts2.Core.Timeline.Stories;

public sealed class RegentStory : StoryModel
{
	protected override string Id => "REGENT";

	public override EpochModel[] Epochs => new EpochModel[7]
	{
		EpochModel.Get<Regent6Epoch>(),
		EpochModel.Get<Regent1Epoch>(),
		EpochModel.Get<Regent5Epoch>(),
		EpochModel.Get<Regent2Epoch>(),
		EpochModel.Get<Regent3Epoch>(),
		EpochModel.Get<Regent4Epoch>(),
		EpochModel.Get<Regent7Epoch>()
	};
}
