using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Relic4Epoch : EpochModel
{
	public override string Id => "RELIC4_EPOCH";

	public override EpochEra Era => EpochEra.Blight0;

	public override int EraPosition => 0;

	public override string StoryId => "Magnum_Opus";

	public static List<RelicModel> Relics
	{
		get
		{
			int num = 3;
			List<RelicModel> list = new List<RelicModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<RelicModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = ModelDb.Relic<MiniatureCannon>();
			num2++;
			span[num2] = ModelDb.Relic<TungstenRod>();
			num2++;
			span[num2] = ModelDb.Relic<WhiteStar>();
			return list;
		}
	}

	public override string UnlockText => CreateRelicUnlockText(Relics);

	public override EpochModel[] GetTimelineExpansion()
	{
		return new EpochModel[3]
		{
			EpochModel.Get(EpochModel.GetId<Event1Epoch>()),
			EpochModel.Get(EpochModel.GetId<Colorless5Epoch>()),
			EpochModel.Get(EpochModel.GetId<Relic5Epoch>())
		};
	}

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueRelicUnlock(Relics);
		EpochModel.QueueTimelineExpansion(GetTimelineExpansion());
	}
}
