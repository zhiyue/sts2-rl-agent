using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Ironclad3Epoch : EpochModel
{
	public override string Id => "IRONCLAD3_EPOCH";

	public override EpochEra Era => EpochEra.Seeds3;

	public override int EraPosition => 0;

	public override string StoryId => "Ironclad";

	public static List<RelicModel> Relics
	{
		get
		{
			int num = 3;
			List<RelicModel> list = new List<RelicModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<RelicModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = ModelDb.Relic<RedSkull>();
			num2++;
			span[num2] = ModelDb.Relic<PaperPhrog>();
			num2++;
			span[num2] = ModelDb.Relic<RuinedHelmet>();
			return list;
		}
	}

	public override string UnlockText => CreateRelicUnlockText(Relics);

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueRelicUnlock(Relics);
	}
}
