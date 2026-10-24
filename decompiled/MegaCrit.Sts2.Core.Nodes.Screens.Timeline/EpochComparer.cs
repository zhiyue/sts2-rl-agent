using System.Collections.Generic;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Timeline;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

/// <summary>
/// Used for sorting Epochs chronologically on the Timeline.
/// Sorted whenever we reveal an Epoch.
/// Useful for navigating between Epochs in the Inspect screen.
/// </summary>
public class EpochComparer : IComparer<SerializableEpoch>
{
	public int Compare(SerializableEpoch? x, SerializableEpoch? y)
	{
		EpochModel epochModel = EpochModel.Get(x.Id);
		EpochModel epochModel2 = EpochModel.Get(y.Id);
		int num = epochModel.Era.CompareTo(epochModel2.Era);
		if (num != 0)
		{
			return num;
		}
		return epochModel.EraPosition.CompareTo(epochModel2.EraPosition);
	}
}
