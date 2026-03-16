using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Regent1Epoch : EpochModel
{
	public override string Id => "REGENT1_EPOCH";

	public override EpochEra Era => EpochEra.Flourish2;

	public override int EraPosition => 1;

	public override string StoryId => "Regent";

	public override bool IsArtPlaceholder => false;

	public override EpochModel[] GetTimelineExpansion()
	{
		return new EpochModel[7]
		{
			EpochModel.Get(EpochModel.GetId<Regent2Epoch>()),
			EpochModel.Get(EpochModel.GetId<Regent3Epoch>()),
			EpochModel.Get(EpochModel.GetId<Regent4Epoch>()),
			EpochModel.Get(EpochModel.GetId<Regent5Epoch>()),
			EpochModel.Get(EpochModel.GetId<Regent6Epoch>()),
			EpochModel.Get(EpochModel.GetId<Regent7Epoch>()),
			EpochModel.Get(EpochModel.GetId<Necrobinder1Epoch>())
		};
	}

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueCharacterUnlock<Regent>(this);
		SaveManager.Instance.Progress.PendingCharacterUnlock = ModelDb.Character<Regent>().Id;
		EpochModel.QueueTimelineExpansion(GetTimelineExpansion());
	}
}
