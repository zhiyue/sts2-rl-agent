using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Necrobinder3Epoch : EpochModel
{
	public override string Id => "NECROBINDER3_EPOCH";

	public override EpochEra Era => EpochEra.Invitation1;

	public override int EraPosition => 3;

	public override string StoryId => "Necrobinder";

	public static List<RelicModel> Relics
	{
		get
		{
			int num = 3;
			List<RelicModel> list = new List<RelicModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<RelicModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = ModelDb.Relic<BoneFlute>();
			num2++;
			span[num2] = ModelDb.Relic<FuneraryMask>();
			num2++;
			span[num2] = ModelDb.Relic<BookRepairKnife>();
			return list;
		}
	}

	public override string UnlockText => CreateRelicUnlockText(Relics);

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueRelicUnlock(Relics);
	}
}
