using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace MegaCrit.Sts2.Core.Timeline.Stories;

public sealed class SilentStory : StoryModel
{
	protected override string Id => "SILENT";

	public override EpochModel[] Epochs => new EpochModel[7]
	{
		EpochModel.Get<Silent6Epoch>(),
		EpochModel.Get<Silent4Epoch>(),
		EpochModel.Get<Silent5Epoch>(),
		EpochModel.Get<Silent2Epoch>(),
		EpochModel.Get<Silent3Epoch>(),
		EpochModel.Get<Silent1Epoch>(),
		EpochModel.Get<Silent7Epoch>()
	};
}
