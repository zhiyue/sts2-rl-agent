using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Silent3Epoch : EpochModel
{
	public override string Id => "SILENT3_EPOCH";

	public override EpochEra Era => EpochEra.Flourish3;

	public override int EraPosition => 0;

	public override string StoryId => "Silent";

	public static List<RelicModel> Relics
	{
		get
		{
			int num = 3;
			List<RelicModel> list = new List<RelicModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<RelicModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = ModelDb.Relic<ToughBandages>();
			num2++;
			span[num2] = ModelDb.Relic<PaperKrane>();
			num2++;
			span[num2] = ModelDb.Relic<Tingsha>();
			return list;
		}
	}

	public override string UnlockText => CreateRelicUnlockText(Relics);

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueRelicUnlock(Relics);
	}
}
