using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Relic5Epoch : EpochModel
{
	public override string Id => "RELIC5_EPOCH";

	public override EpochEra Era => EpochEra.Flourish1;

	public override int EraPosition => 1;

	public override string StoryId => "Reopening";

	public static List<RelicModel> Relics
	{
		get
		{
			int num = 3;
			List<RelicModel> list = new List<RelicModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<RelicModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = ModelDb.Relic<TinyMailbox>();
			num2++;
			span[num2] = ModelDb.Relic<JossPaper>();
			num2++;
			span[num2] = ModelDb.Relic<BeatingRemnant>();
			return list;
		}
	}

	public override string UnlockText => CreateRelicUnlockText(Relics);

	public override EpochModel[] GetTimelineExpansion()
	{
		return new EpochModel[2]
		{
			EpochModel.Get(EpochModel.GetId<Event2Epoch>()),
			EpochModel.Get(EpochModel.GetId<Event3Epoch>())
		};
	}

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueRelicUnlock(Relics);
		EpochModel.QueueTimelineExpansion(GetTimelineExpansion());
	}
}
