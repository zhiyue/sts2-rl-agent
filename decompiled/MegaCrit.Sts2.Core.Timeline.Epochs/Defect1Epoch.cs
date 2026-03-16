using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Defect1Epoch : EpochModel
{
	public override string Id => "DEFECT1_EPOCH";

	public override EpochEra Era => EpochEra.Invitation1;

	public override int EraPosition => 2;

	public override string StoryId => "Defect";

	public override EpochModel[] GetTimelineExpansion()
	{
		return new EpochModel[6]
		{
			EpochModel.Get(EpochModel.GetId<Defect2Epoch>()),
			EpochModel.Get(EpochModel.GetId<Defect3Epoch>()),
			EpochModel.Get(EpochModel.GetId<Defect4Epoch>()),
			EpochModel.Get(EpochModel.GetId<Defect5Epoch>()),
			EpochModel.Get(EpochModel.GetId<Defect6Epoch>()),
			EpochModel.Get(EpochModel.GetId<Defect7Epoch>())
		};
	}

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueCharacterUnlock<Defect>(this);
		SaveManager.Instance.Progress.PendingCharacterUnlock = ModelDb.Character<Defect>().Id;
		EpochModel.QueueTimelineExpansion(GetTimelineExpansion());
	}
}
