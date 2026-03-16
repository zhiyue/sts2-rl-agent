using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Colorless1Epoch : EpochModel
{
	public override string Id => "COLORLESS1_EPOCH";

	public override EpochEra Era => EpochEra.Prehistoria0;

	public override int EraPosition => 0;

	public override string StoryId => "Magnum_Opus";

	public static List<CardModel> Cards
	{
		get
		{
			int num = 3;
			List<CardModel> list = new List<CardModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<CardModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = ModelDb.Card<Automation>();
			num2++;
			span[num2] = ModelDb.Card<Entropy>();
			num2++;
			span[num2] = ModelDb.Card<Catastrophe>();
			return list;
		}
	}

	public override string UnlockText => CreateCardUnlockText(Cards);

	public override EpochModel[] GetTimelineExpansion()
	{
		return new EpochModel[3]
		{
			EpochModel.Get(EpochModel.GetId<Potion1Epoch>()),
			EpochModel.Get(EpochModel.GetId<Relic1Epoch>()),
			EpochModel.Get(EpochModel.GetId<UnderdocksEpoch>())
		};
	}

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueCardUnlock(Cards);
		EpochModel.QueueTimelineExpansion(GetTimelineExpansion());
	}
}
