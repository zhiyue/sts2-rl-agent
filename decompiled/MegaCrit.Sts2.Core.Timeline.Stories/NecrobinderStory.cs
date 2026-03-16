using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace MegaCrit.Sts2.Core.Timeline.Stories;

public sealed class NecrobinderStory : StoryModel
{
	protected override string Id => "NECROBINDER";

	public override EpochModel[] Epochs => new EpochModel[7]
	{
		EpochModel.Get<Necrobinder2Epoch>(),
		EpochModel.Get<Necrobinder3Epoch>(),
		EpochModel.Get<Necrobinder4Epoch>(),
		EpochModel.Get<Necrobinder5Epoch>(),
		EpochModel.Get<Necrobinder6Epoch>(),
		EpochModel.Get<Necrobinder1Epoch>(),
		EpochModel.Get<Necrobinder7Epoch>()
	};
}
