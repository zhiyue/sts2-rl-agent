using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Timeline.Epochs;

public class Event3Epoch : EpochModel
{
	public override string Id => "EVENT3_EPOCH";

	public override EpochEra Era => EpochEra.Blight1;

	public override int EraPosition => 0;

	public override string StoryId => "Magnum_Opus";

	public static List<EventModel> Events
	{
		get
		{
			int num = 1;
			List<EventModel> list = new List<EventModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<EventModel> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = ModelDb.Event<ColorfulPhilosophers>();
			return list;
		}
	}

	public override string UnlockText
	{
		get
		{
			LocString locString = new LocString("epochs", Id + ".unlockText");
			locString.Add("Event", Events[0].Title);
			return locString.GetFormattedText();
		}
	}

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueMiscUnlock(UnlockText);
	}
}
