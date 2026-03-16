using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline;

public class EpochSlotData
{
	public EpochModel Model { get; }

	public EpochSlotState State { get; }

	public EpochEra Era { get; }

	public int EraPosition { get; }

	public EpochSlotData(string modelId, EpochSlotState state)
	{
		Model = EpochModel.Get(modelId);
		Era = Model.Era;
		EraPosition = Model.EraPosition;
		State = state;
	}

	public EpochSlotData(EpochModel model, EpochSlotState state)
	{
		Model = model;
		Era = Model.Era;
		EraPosition = Model.EraPosition;
		State = state;
	}
}
