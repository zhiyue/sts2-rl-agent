using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Events;

public class EventOption
{
	public string TextKey { get; private set; }

	public LocString Title { get; private set; }

	public LocString Description { get; private set; }

	private Func<Task>? OnChosen { get; }

	public IEnumerable<IHoverTip> HoverTips { get; set; }

	public bool IsLocked { get; }

	public bool IsProceed { get; private set; }

	public bool WasChosen { get; private set; }

	public RelicModel? Relic { get; private set; }

	private bool DisableOnChosen { get; }

	public Func<Player, bool>? WillKillPlayer { get; private set; }

	public bool ShouldSaveChoiceToHistory { get; private set; } = true;

	public LocString HistoryName { get; private set; }

	public bool ShouldSaveVariablesToHistory { get; private set; } = true;

	public event Func<EventOption, Task>? BeforeChosen;

	public EventOption(EventModel eventModel, Func<Task>? onChosen, LocString title, LocString description, string textKey, IEnumerable<IHoverTip> hoverTips)
	{
		TextKey = textKey;
		OnChosen = onChosen;
		Title = title;
		Description = description;
		HoverTips = hoverTips;
		IsLocked = OnChosen == null;
		DisableOnChosen = true;
		HistoryName = title;
	}

	public EventOption(EventModel eventModel, Func<Task>? onChosen, string textKey, IEnumerable<IHoverTip> hoverTips)
	{
		TextKey = textKey;
		OnChosen = onChosen;
		Title = eventModel.GetOptionTitle(textKey);
		Description = eventModel.GetOptionDescription(textKey);
		HoverTips = hoverTips;
		IsLocked = OnChosen == null;
		DisableOnChosen = true;
		HistoryName = Title;
	}

	public EventOption(EventModel eventModel, Func<Task>? onChosen, string textKey, params IHoverTip[] hoverTips)
		: this(eventModel, onChosen, textKey, hoverTips.ToList())
	{
	}

	public EventOption(EventModel eventModel, Func<Task>? onChosen, string textKey, bool disableOnChosen = true, bool isProceed = false, params IHoverTip[] hoverTips)
		: this(eventModel, onChosen, textKey, hoverTips.ToList())
	{
		IsProceed = isProceed;
		DisableOnChosen = disableOnChosen;
	}

	public static EventOption FromRelic(RelicModel relic, EventModel eventModel, Func<Task>? onChosen, string textKey)
	{
		LocString title = eventModel.GetOptionTitle(textKey) ?? relic.Title;
		LocString description = eventModel.GetOptionDescription(textKey) ?? relic.DynamicEventDescription;
		return new EventOption(eventModel, onChosen, title, description, textKey, relic.HoverTipsExcludingRelic).WithRelic(relic);
	}

	public EventOption WithRelic<T>(Player? owner) where T : RelicModel
	{
		RelicModel relicModel = ModelDb.Relic<T>().ToMutable();
		if (owner != null)
		{
			relicModel.Owner = owner;
		}
		return WithRelic(relicModel);
	}

	public EventOption WithRelic(RelicModel relic)
	{
		relic.AssertMutable();
		Relic = relic;
		return this;
	}

	public async Task Chosen()
	{
		if (OnChosen != null && (!DisableOnChosen || !WasChosen))
		{
			WasChosen = true;
			if (this.BeforeChosen != null)
			{
				await this.BeforeChosen(this);
			}
			await OnChosen();
		}
	}

	public EventOption WithOverridenHistoryName(LocString historyName)
	{
		HistoryName = historyName;
		return this;
	}

	public EventOption ThatDoesDamage(decimal damage)
	{
		return ThatWillKillPlayerIf((Player p) => (decimal)p.Creature.CurrentHp <= damage);
	}

	public EventOption ThatWillKillPlayerIf(Func<Player, bool> willKillPlayer)
	{
		WillKillPlayer = willKillPlayer;
		return this;
	}

	public EventOption ThatHasDynamicTitle()
	{
		ShouldSaveVariablesToHistory = true;
		return this;
	}

	public EventOption ThatWontSaveToChoiceHistory()
	{
		ShouldSaveChoiceToHistory = false;
		return this;
	}

	public override string ToString()
	{
		return $"{"EventOption"} title: {Title.GetRawText()} description: {Description.GetRawText()} textKey: {TextKey}";
	}
}
