using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Necrobinder1Epoch : EpochModel
{
	public override string Id => "NECROBINDER1_EPOCH";

	public override EpochEra Era => EpochEra.Invitation5;

	public override int EraPosition => 0;

	public override string StoryId => "Necrobinder";

	public override bool IsArtPlaceholder => false;

	public override EpochModel[] GetTimelineExpansion()
	{
		return new EpochModel[7]
		{
			EpochModel.Get(EpochModel.GetId<Necrobinder2Epoch>()),
			EpochModel.Get(EpochModel.GetId<Necrobinder3Epoch>()),
			EpochModel.Get(EpochModel.GetId<Necrobinder4Epoch>()),
			EpochModel.Get(EpochModel.GetId<Necrobinder5Epoch>()),
			EpochModel.Get(EpochModel.GetId<Necrobinder6Epoch>()),
			EpochModel.Get(EpochModel.GetId<Necrobinder7Epoch>()),
			EpochModel.Get(EpochModel.GetId<Defect1Epoch>())
		};
	}

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueCharacterUnlock<Necrobinder>(this);
		SaveManager.Instance.Progress.PendingCharacterUnlock = ModelDb.Character<Necrobinder>().Id;
		EpochModel.QueueTimelineExpansion(GetTimelineExpansion());
	}
}
