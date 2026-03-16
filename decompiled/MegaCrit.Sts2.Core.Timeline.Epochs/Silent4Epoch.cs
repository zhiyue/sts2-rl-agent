using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Silent4Epoch : EpochModel
{
	public override string Id => "SILENT4_EPOCH";

	public override EpochEra Era => EpochEra.Blight1;

	public override int EraPosition => 3;

	public override string StoryId => "Silent";

	public override bool IsArtPlaceholder => false;

	public static List<PotionModel> Potions
	{
		get
		{
			int num = 3;
			List<PotionModel> list = new List<PotionModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<PotionModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = ModelDb.Potion<PoisonPotion>();
			num2++;
			span[num2] = ModelDb.Potion<GhostInAJar>();
			num2++;
			span[num2] = ModelDb.Potion<CunningPotion>();
			return list;
		}
	}

	public override string UnlockText => CreatePotionUnlockText(Potions);

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueuePotionUnlock(Potions);
		LocString locString = new LocString("epochs", Id + ".unlock");
		NTimelineScreen.Instance.QueueMiscUnlock(locString.GetFormattedText() ?? "");
	}
}
