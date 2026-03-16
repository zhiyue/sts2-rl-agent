using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace MegaCrit.Sts2.Core.Timeline.Stories;

public sealed class IroncladStory : StoryModel
{
	protected override string Id => "IRONCLAD";

	public override EpochModel[] Epochs => new EpochModel[6]
	{
		EpochModel.Get<Ironclad4Epoch>(),
		EpochModel.Get<Ironclad3Epoch>(),
		EpochModel.Get<Ironclad2Epoch>(),
		EpochModel.Get<Ironclad5Epoch>(),
		EpochModel.Get<Ironclad6Epoch>(),
		EpochModel.Get<Ironclad7Epoch>()
	};
}
