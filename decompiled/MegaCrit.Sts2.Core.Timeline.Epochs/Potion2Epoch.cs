using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Potion2Epoch : EpochModel
{
	public override string Id => "POTION2_EPOCH";

	public override EpochEra Era => EpochEra.Flourish0;

	public override int EraPosition => 0;

	public override string StoryId => "Reopening";

	public static List<PotionModel> Potions
	{
		get
		{
			int num = 3;
			List<PotionModel> list = new List<PotionModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<PotionModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = ModelDb.Potion<PowderedDemise>();
			num2++;
			span[num2] = ModelDb.Potion<ShipInABottle>();
			num2++;
			span[num2] = ModelDb.Potion<TouchOfInsanity>();
			return list;
		}
	}

	public override string UnlockText => CreatePotionUnlockText(Potions);

	public override EpochModel[] GetTimelineExpansion()
	{
		return new EpochModel[3]
		{
			EpochModel.Get(EpochModel.GetId<Act2BEpoch>()),
			EpochModel.Get(EpochModel.GetId<Colorless3Epoch>()),
			EpochModel.Get(EpochModel.GetId<Relic3Epoch>())
		};
	}

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueuePotionUnlock(Potions);
		EpochModel.QueueTimelineExpansion(GetTimelineExpansion());
	}
}
