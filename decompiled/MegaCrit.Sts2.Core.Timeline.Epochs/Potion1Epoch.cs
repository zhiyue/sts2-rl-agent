using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Potion1Epoch : EpochModel
{
	public override string Id => "POTION1_EPOCH";

	public override EpochEra Era => EpochEra.Invitation0;

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
			span[num2] = ModelDb.Potion<BeetleJuice>();
			num2++;
			span[num2] = ModelDb.Potion<MazalethsGift>();
			num2++;
			span[num2] = ModelDb.Potion<DropletOfPrecognition>();
			return list;
		}
	}

	public override string UnlockText => CreatePotionUnlockText(Potions);

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueuePotionUnlock(Potions);
	}
}
