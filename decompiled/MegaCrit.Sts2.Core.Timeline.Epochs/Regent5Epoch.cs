using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Regent5Epoch : EpochModel
{
	public override string Id => "REGENT5_EPOCH";

	public override EpochEra Era => EpochEra.Flourish3;

	public override int EraPosition => 2;

	public override string StoryId => "Regent";

	public static List<CardModel> Cards
	{
		get
		{
			int num = 3;
			List<CardModel> list = new List<CardModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<CardModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = ModelDb.Card<Begone>();
			num2++;
			span[num2] = ModelDb.Card<Arsenal>();
			num2++;
			span[num2] = ModelDb.Card<Supermassive>();
			return list;
		}
	}

	public override string UnlockText => CreateCardUnlockText(Cards);

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueCardUnlock(Cards);
	}
}
