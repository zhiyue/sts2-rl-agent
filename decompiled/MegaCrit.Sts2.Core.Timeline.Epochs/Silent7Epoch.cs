using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Silent7Epoch : EpochModel
{
	public override string Id => "SILENT7_EPOCH";

	public override EpochEra Era => EpochEra.Invitation5;

	public override int EraPosition => 3;

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
			span[num2] = ModelDb.Card<HiddenDaggers>();
			num2++;
			span[num2] = ModelDb.Card<BladeOfInk>();
			num2++;
			span[num2] = ModelDb.Card<PhantomBlades>();
			return list;
		}
	}

	public override string UnlockText => CreateCardUnlockText(Cards);

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueCardUnlock(Cards);
	}
}
