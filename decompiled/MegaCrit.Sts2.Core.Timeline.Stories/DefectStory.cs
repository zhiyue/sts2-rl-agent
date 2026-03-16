using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace MegaCrit.Sts2.Core.Timeline.Stories;

public sealed class DefectStory : StoryModel
{
	protected override string Id => "DEFECT";

	public override EpochModel[] Epochs => new EpochModel[7]
	{
		EpochModel.Get<Defect6Epoch>(),
		EpochModel.Get<Defect3Epoch>(),
		EpochModel.Get<Defect4Epoch>(),
		EpochModel.Get<Defect5Epoch>(),
		EpochModel.Get<Defect1Epoch>(),
		EpochModel.Get<Defect2Epoch>(),
		EpochModel.Get<Defect7Epoch>()
	};
}
