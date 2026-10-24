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

	/// <summary>
	/// Create a new event option.
	/// </summary>
	/// <param name="eventModel">The event that this option belongs to.</param>
	/// <param name="onChosen">The function that will run when the option is chosen.</param>
	/// <param name="title">Option title.</param>
	/// <param name="description">Option description.</param>
	/// <param name="textKey">
	/// Localization key prefix for this option. Does not include ".title" or ".description".
	/// </param>
	/// <param name="hoverTips">HoverTips that should be shown when hovering this option.</param>
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
		AddLocVars(eventModel);
	}

	/// <summary>
	/// Create a new event option.
	/// </summary>
	/// <param name="eventModel">The event that this option belongs to.</param>
	/// <param name="onChosen">The function that will run when the option is chosen.</param>
	/// <param name="textKey">
	/// Localization key prefix for this option. Does not include ".title" or ".description".
	/// </param>
	/// <param name="hoverTips">HoverTips that should be shown when hovering this option.</param>
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
		AddLocVars(eventModel);
	}

	/// <summary>
	/// Create a new event option.
	/// </summary>
	/// <param name="eventModel">The event that this option belongs to.</param>
	/// <param name="onChosen">The function that will run when the option is chosen.</param>
	/// <param name="textKey">
	/// Localization key prefix for this option. Does not include ".title" or ".description".
	/// </param>
	/// <param name="hoverTips">HoverTips that should be shown when hovering this option.</param>
	public EventOption(EventModel eventModel, Func<Task>? onChosen, string textKey, params IHoverTip[] hoverTips)
		: this(eventModel, onChosen, textKey, hoverTips.ToList())
	{
	}

	/// <summary>
	/// Create a new event option.
	/// </summary>
	/// <param name="eventModel">The event that this option belongs to.</param>
	/// <param name="onChosen">The function that will run when the option is chosen.</param>
	/// <param name="textKey">
	/// Localization key prefix for this option. Does not include ".title" or ".description".
	/// </param>
	/// <param name="disableOnChosen">
	/// Whether this option should be disabled after clicking it.
	/// Usually true, but false for options that have a confirmation dialog that can be canceled,
	/// so the option can be re-clicked after.
	/// </param>
	/// <param name="isProceed">Whether this option is a "proceed" button.</param>
	/// <param name="hoverTips">HoverTips that should be shown when hovering this option.</param>
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

	/// <summary>
	/// Associates a relic with this event option.
	/// Should be called on custom ancient event options that give relics so that they show up in the correct category
	/// in the relic library.
	/// </summary>
	public EventOption WithRelic<T>(Player? owner) where T : RelicModel
	{
		RelicModel relicModel = ModelDb.Relic<T>().ToMutable();
		if (owner != null)
		{
			relicModel.Owner = owner;
		}
		return WithRelic(relicModel);
	}

	/// <summary>
	/// Associates a relic with this event option.
	/// Should be called on custom ancient event options that give relics so that they show up in the correct category
	/// in the relic library.
	/// </summary>
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

	/// <summary>
	/// Typically, the title of the event is used for the name shown in the run history and uploaded to metrics. This
	/// method overrides the name used.
	/// </summary>
	public EventOption WithOverridenHistoryName(LocString historyName)
	{
		HistoryName = historyName;
		return this;
	}

	/// <summary>
	/// Causes the event option to flash red if the player's HP is below the damage value passed.
	/// </summary>
	public EventOption ThatDoesDamage(decimal damage)
	{
		return ThatWillKillPlayerIf((Player p) => (decimal)p.Creature.CurrentHp <= damage);
	}

	/// <summary>
	/// Causes the event option to flash red if the player's Max HP is below the value passed.
	/// </summary>
	public EventOption ThatDecreasesMaxHp(decimal value)
	{
		return ThatWillKillPlayerIf((Player p) => (decimal)p.Creature.MaxHp <= value);
	}

	/// <summary>
	/// Causes the event option to flash red when the passed function returns true.
	/// </summary>
	public EventOption ThatWillKillPlayerIf(Func<Player, bool> willKillPlayer)
	{
		WillKillPlayer = willKillPlayer;
		return this;
	}

	/// <summary>
	/// Certain event options have dynamic titles that vary based on the DynamicVars passed to the loc string. By
	/// default, LocString DynamicVars are not saved to the save file. If this is called, those variables will be saved
	/// to the run history so that the title can be correctly displayed. This should only be used with specific
	/// events, as saving variables often saves lots of unnecessary data.
	/// </summary>
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

	private void AddLocVars(EventModel eventModel)
	{
		eventModel.Owner?.Character.AddDetailsTo(Description);
		LocString description = Description;
		Player? owner = eventModel.Owner;
		description.Add("IsMultiplayer", owner != null && owner.RunState.Players.Count > 1);
	}

	public override string ToString()
	{
		return $"{"EventOption"} title: {Title.GetRawText()} description: {Description.GetRawText()} textKey: {TextKey}";
	}
}
