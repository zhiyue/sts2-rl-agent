using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Necrobinder2Epoch : EpochModel
{
	public override string Id => "NECROBINDER2_EPOCH";

	public override EpochEra Era => EpochEra.Invitation0;

	public override int EraPosition => 2;

	public override string StoryId => "Necrobinder";

	public static List<CardModel> Cards
	{
		get
		{
			int num = 3;
			List<CardModel> list = new List<CardModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<CardModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = ModelDb.Card<Scourge>();
			num2++;
			span[num2] = ModelDb.Card<Oblivion>();
			num2++;
			span[num2] = ModelDb.Card<Countdown>();
			return list;
		}
	}

	public override string UnlockText => CreateCardUnlockText(Cards);

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueCardUnlock(Cards);
	}
}
