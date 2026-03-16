using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Silent5Epoch : EpochModel
{
	public override string Id => "SILENT5_EPOCH";

	public override EpochEra Era => EpochEra.Blight2;

	public override int EraPosition => 0;

	public override string StoryId => "Silent";

	public static List<CardModel> Cards
	{
		get
		{
			int num = 3;
			List<CardModel> list = new List<CardModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<CardModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = ModelDb.Card<Reflex>();
			num2++;
			span[num2] = ModelDb.Card<MasterPlanner>();
			num2++;
			span[num2] = ModelDb.Card<HandTrick>();
			return list;
		}
	}

	public override string UnlockText => CreateCardUnlockText(Cards);

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueCardUnlock(Cards);
	}
}
