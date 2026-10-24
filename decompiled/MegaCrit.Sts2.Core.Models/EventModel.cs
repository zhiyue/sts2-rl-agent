using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models;

public abstract class EventModel : AbstractModel
{
	protected const string _initialPageKey = "INITIAL";

	/// <summary>
	/// Generated via <see cref="M:MegaCrit.Sts2.Core.Models.EventModel.GenerateInternalCombatState(MegaCrit.Sts2.Core.Runs.IRunState)" /> for combat-style events.
	/// </summary>
	private EncounterModel? _mutableEncounter;

	/// <summary>
	/// Generated via <see cref="M:MegaCrit.Sts2.Core.Models.EventModel.GenerateInternalCombatState(MegaCrit.Sts2.Core.Runs.IRunState)" /> for combat-style events.
	/// </summary>
	protected CombatState? _combatStateForCombatLayout;

	private List<EventOption>? _currentOptions;

	private bool _isFinished;

	private bool _cleanupCalled;

	private DynamicVarSet? _dynamicVars;

	private EventModel _canonicalInstance;

	public virtual Color ButtonColor => new Color(1f, 1f, 1f, 0.9f);

	/// <summary>
	/// Deterministic events send out a checksum at their end. It's skipped for non-deterministic ones.
	/// - All standard events should be deterministic
	/// - All shared events are combat-related, and we don't trust end-of-combat rewards to be deterministic
	/// - One-off events which grant rewards are non-deterministic, such as Crystal Sphere
	/// </summary>
	public virtual bool IsDeterministic => !IsShared;

	public override bool ShouldReceiveCombatHooks => false;

	public virtual string LocTable => "events";

	public LocString Title => L10NLookup(base.Id.Entry + ".title");

	public virtual LocString InitialDescription => L10NLookup(base.Id.Entry + ".pages.INITIAL.description");

	/// <summary>
	/// Get the owner of this particular event instance.
	/// When players enter an event in a multiplayer run, a separate EventModel instance is created with each player as
	/// the owner. Only the event owned by the local player is displayed in <see cref="T:MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom" />, but the other
	/// instances still exist on each player's device, so keep that in mind when using this property.
	/// Will be null in canonical events.
	/// </summary>
	public Player? Owner { get; private set; }

	/// <summary>
	/// In non-shared events, multiplayer players may choose options independently.
	/// In shared events, multiplayer players vote on an option. Then, the top-voted option is executed for all players.
	/// There is no difference in singleplayer.
	/// All events that transition to other rooms (e.g. Dense Vegetation) must be shared.
	/// </summary>
	public virtual bool IsShared => false;

	public LocString? Description { get; private set; }

	/// <summary>
	/// The encounter that should be displayed when entering this event.
	/// Only relevant when <see cref="P:MegaCrit.Sts2.Core.Models.EventModel.LayoutType" /> is <see cref="F:MegaCrit.Sts2.Core.Events.EventLayoutType.Combat" />.
	/// </summary>
	public virtual EncounterModel? CanonicalEncounter => null;

	public bool IsFinished
	{
		get
		{
			return _isFinished;
		}
		private set
		{
			AssertMutable();
			_isFinished = value;
		}
	}

	public IReadOnlyList<EventOption> CurrentOptions
	{
		get
		{
			AssertMutable();
			if (_currentOptions == null)
			{
				_currentOptions = new List<EventOption>();
			}
			return _currentOptions;
		}
	}

	public DynamicVarSet DynamicVars
	{
		get
		{
			if (_dynamicVars != null)
			{
				return _dynamicVars;
			}
			_dynamicVars = new DynamicVarSet(CanonicalVars);
			_dynamicVars.InitializeWithOwner(this);
			return _dynamicVars;
		}
	}

	protected virtual IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	/// <summary>
	/// A per-event RNG that we can use to do random rolls in the event independently of the run's centralized RNG.
	/// This is safe to do in events because we don't need to keep track of a given event's RNG state once it's over.
	/// Null in canonical events, but we should never be using it there, so we mark it as non-nullable.
	/// </summary>
	public Rng Rng { get; private set; }

	/// <summary>
	/// Get all the EventOption LocStrings shown on the initial page.
	/// Used by NGameInfoUploader to upload info about the game for lookup elsewhere.
	/// Override in events with unusually dynamic options like <see cref="T:MegaCrit.Sts2.Core.Models.Events.TheFutureOfPotions" />.
	/// </summary>
	public virtual IEnumerable<LocString> GameInfoOptions
	{
		get
		{
			List<LocString> list = (from k in LocManager.Instance.GetTable(LocTable).Keys
				where k.StartsWith(base.Id.Entry + ".pages.INITIAL.options")
				select new LocString(LocTable, k)).ToList();
			if (list.Count == 0)
			{
				throw new LocException("Event Loc for " + base.Id.Entry + " does not conform to the common format");
			}
			foreach (LocString item in list)
			{
				DynamicVars.AddTo(item);
			}
			return list;
		}
	}

	public virtual EventLayoutType LayoutType => EventLayoutType.Default;

	/// <summary>
	/// The node that is being shown for this event.
	/// Null for canonical events, and before a mutable event has been initialized.
	/// </summary>
	public Control? Node { get; private set; }

	private string LayoutScenePath => LayoutType switch
	{
		EventLayoutType.Default => "res://scenes/events/default_event_layout.tscn", 
		EventLayoutType.Combat => "res://scenes/events/combat_event_layout.tscn", 
		EventLayoutType.Ancient => "res://scenes/events/ancient_event_layout.tscn", 
		EventLayoutType.Custom => SceneHelper.GetScenePath("events/custom/" + base.Id.Entry.ToLowerInvariant()), 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	public EventModel CanonicalInstance
	{
		get
		{
			if (!base.IsMutable)
			{
				return this;
			}
			return _canonicalInstance;
		}
		private set
		{
			AssertMutable();
			_canonicalInstance = value;
		}
	}

	private string InitialPortraitPath => ImageHelper.GetImagePath("events/" + base.Id.Entry.ToLowerInvariant() + ".png");

	private string InitialPhobiaModePortraitPath => ImageHelper.GetImagePath("events/" + base.Id.Entry.ToLowerInvariant() + "_phobia_mode.png");

	public bool HasPhobiaModePortrait => ResourceLoader.Exists(InitialPhobiaModePortraitPath);

	private string BackgroundScenePath => SceneHelper.GetScenePath("events/background_scenes/" + base.Id.Entry.ToLowerInvariant());

	private string VfxPath => SceneHelper.GetScenePath("vfx/events/" + base.Id.Entry.ToLowerInvariant() + "_vfx");

	public bool HasVfx => ResourceLoader.Exists(VfxPath);

	public static Vector2 VfxOffset => new Vector2(268f, 49f);

	public event Action<EventModel>? StateChanged;

	public event Action? EnteringEventCombat;

	public LocString? GetOptionTitle(string key)
	{
		return LocString.GetIfExists(LocTable, key + ".title");
	}

	public LocString? GetOptionDescription(string key)
	{
		return LocString.GetIfExists(LocTable, key + ".description");
	}

	public async Task BeginEvent(Player player, bool isPreFinished)
	{
		AssertMutable();
		if (Owner != null)
		{
			throw new InvalidOperationException("Tried to begin event, but it already has an owner!");
		}
		Owner = player;
		Rng = new Rng((uint)((uint)((int)Owner.RunState.Rng.Seed + ((!IsShared) ? Owner.RunState.GetPlayerSlotIndex(Owner) : 0)) + StringHelper.GetDeterministicHashCode(base.Id.Entry)));
		try
		{
			await BeforeEventStarted(isPreFinished);
			CalculateVars();
			if (player.Creature.IsDead)
			{
				Log.Error("The generic event death message should not appear!");
				SetEventFinished(L10NLookup("GENERIC.youAreDead.description"));
			}
			else
			{
				SetInitialEventState(isPreFinished);
			}
		}
		catch
		{
			EnsureCleanup();
			throw;
		}
	}

	protected virtual void SetInitialEventState(bool isPreFinished)
	{
		if (isPreFinished && !(this is AncientEventModel))
		{
			throw new InvalidOperationException($"Tried to load into pre-finished event {this}! Only ancient events can be pre-finished.");
		}
		IReadOnlyList<EventOption> eventOptions = GenerateInitialOptionsWrapper();
		SetEventState(InitialDescription, eventOptions);
	}

	/// <summary>
	/// Wrapper around abstract GenerateInitialOptions, similar to <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.OnPlayWrapper(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Boolean,MegaCrit.Sts2.Core.Entities.Cards.ResourceInfo,System.Boolean)" />.
	/// Making this virtual lets us change initial option generation behavior in entire categories of events (like
	/// Ancient events), while still letting concrete event subclasses override the inner GenerateInitialOptions method.
	/// </summary>
	protected virtual IReadOnlyList<EventOption> GenerateInitialOptionsWrapper()
	{
		AssertMutable();
		List<EventOption> list = GenerateInitialOptions().ToList();
		ReplaceNullOptions(list);
		return list;
	}

	/// <summary>
	/// Defensively protect against misconfigured events with null options.
	/// </summary>
	protected void ReplaceNullOptions(List<EventOption> options)
	{
		for (int i = 0; i < options.Count; i++)
		{
			EventOption eventOption = options[i];
			if (eventOption == null)
			{
				string text = $"Event {base.Id.Entry} has a null option at index {i}!";
				Log.Error(text);
				SentryService.CaptureException(new NullReferenceException(text));
				eventOption = new EventOption(this, null, "ERROR");
				options[i] = eventOption;
			}
		}
	}

	protected abstract IReadOnlyList<EventOption> GenerateInitialOptions();

	/// <summary>
	/// Used by <see cref="T:MegaCrit.Sts2.Core.Models.Events.TheArchitect" /> to stop the initial options from showing until the initial animations have
	/// played. Probably worth coming up with a more robust solution after EA release.
	/// </summary>
	protected void ClearCurrentOptions()
	{
		AssertMutable();
		if (_currentOptions == null)
		{
			_currentOptions = new List<EventOption>();
		}
		_currentOptions.Clear();
	}

	/// <summary>
	/// Whether or not this event is allowed to be entered based on the specified combat state.
	/// This will usually just return true, but some events require special conditions.
	/// For example, <see cref="T:MegaCrit.Sts2.Core.Models.Events.RelicTrader" /> can only be entered if you have 5+ relics and you're on act 2+.
	/// </summary>
	public virtual bool IsAllowed(IRunState runState)
	{
		return true;
	}

	/// <summary>
	/// Create the scene that will be used for this event.
	/// </summary>
	public PackedScene CreateScene()
	{
		return PreloadManager.Cache.GetScene(LayoutScenePath);
	}

	public void SetNode(Control node)
	{
		AssertMutable();
		if (Node != null)
		{
			throw new InvalidOperationException("Tried to set node, but it has already been set!");
		}
		Node = node;
		if (LayoutType == EventLayoutType.Custom)
		{
			((ICustomEventNode)Node).Initialize(this);
		}
	}

	public Texture2D CreateInitialPortrait()
	{
		return PreloadManager.Cache.GetTexture2D(InitialPortraitPath);
	}

	public Texture2D CreateInitialPhobiaModePortrait()
	{
		return PreloadManager.Cache.GetTexture2D(InitialPhobiaModePortraitPath);
	}

	public PackedScene CreateBackgroundScene()
	{
		return PreloadManager.Cache.GetScene(BackgroundScenePath);
	}

	public Node2D CreateVfx()
	{
		return PreloadManager.Cache.GetScene(VfxPath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
	}

	/// <summary>
	/// Create the visuals for the combat room that will be used for this event.
	/// Only relevant to <see cref="F:MegaCrit.Sts2.Core.Events.EventLayoutType.Combat" /> events.
	/// </summary>
	public ICombatRoomVisuals CreateCombatRoomVisuals(IEnumerable<Player> players, ActModel act)
	{
		if (LayoutType != EventLayoutType.Combat)
		{
			throw new InvalidOperationException("Tried to create combat room visuals for non-combat event!");
		}
		return new CombatEventVisuals(_mutableEncounter, players, act);
	}

	public void GenerateInternalCombatState(IRunState runState)
	{
		if (LayoutType != EventLayoutType.Combat)
		{
			throw new InvalidOperationException("Tried to generate internal encounter for non-combat event!");
		}
		_mutableEncounter = CanonicalEncounter.ToMutable();
		_mutableEncounter.GenerateMonstersWithSlots(runState);
		_combatStateForCombatLayout = new CombatState(_mutableEncounter, runState, runState.Modifiers, runState.BadgeModels, runState.MultiplayerScalingModel);
		foreach (Player player in runState.Players)
		{
			_combatStateForCombatLayout.AddPlayer(player);
		}
		foreach (var monstersWithSlot in _combatStateForCombatLayout.Encounter.MonstersWithSlots)
		{
			MonsterModel item = monstersWithSlot.Item1;
			string item2 = monstersWithSlot.Item2;
			Creature creature = _combatStateForCombatLayout.CreateCreature(item, CombatSide.Enemy, item2);
			_combatStateForCombatLayout.AddCreature(creature);
		}
	}

	public void ResetInternalCombatState()
	{
		if (LayoutType != EventLayoutType.Combat)
		{
			throw new InvalidOperationException("Tried to reset internal encounter for non-combat event!");
		}
		if (_combatStateForCombatLayout == null)
		{
			return;
		}
		foreach (Creature item in _combatStateForCombatLayout.Creatures.ToList())
		{
			_combatStateForCombatLayout.RemoveCreature(item);
		}
		_combatStateForCombatLayout = null;
	}

	public EventModel ToMutable()
	{
		AssertCanonical();
		EventModel eventModel = (EventModel)MutableClone();
		eventModel.CanonicalInstance = this;
		return eventModel;
	}

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_dynamicVars = DynamicVars.Clone(this);
	}

	protected override void AfterCloned()
	{
		base.AfterCloned();
		this.StateChanged = null;
		this.EnteringEventCombat = null;
		_currentOptions = null;
	}

	public virtual void CalculateVars()
	{
	}

	protected LocString L10NLookup(string entryName)
	{
		return new LocString(LocTable, entryName);
	}

	public virtual IEnumerable<string> GetAssetPaths(IRunState runState)
	{
		if (TestMode.IsOn)
		{
			return Array.Empty<string>();
		}
		int num = 1;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = LayoutScenePath;
		List<string> list2 = list;
		switch (LayoutType)
		{
		case EventLayoutType.Default:
			list2.Add(InitialPortraitPath);
			if (HasPhobiaModePortrait)
			{
				list2.Add(InitialPhobiaModePortraitPath);
			}
			if (HasVfx)
			{
				list2.Add(VfxPath);
			}
			break;
		case EventLayoutType.Combat:
			list2.AddRange(NCombatRoom.AssetPaths);
			if (_mutableEncounter != null)
			{
				list2.AddRange(_mutableEncounter.GetAssetPaths(runState));
			}
			break;
		case EventLayoutType.Ancient:
			list2.Add(BackgroundScenePath);
			break;
		}
		return list2;
	}

	/// <summary>
	/// Gets called when entering an Event. Useful for setting ambient vfx, changing music, etc
	/// </summary>
	public virtual void OnRoomEnter()
	{
	}

	/// <summary>
	/// Gets called when an event is resumed. This should handle setting up the appropriate next page.
	/// This happens when the player was in an event, entered a new room (NOT a new <see cref="T:MegaCrit.Sts2.Core.Map.MapPoint" />), and then
	/// finished that room, causing them to resume the event room.
	/// This is most common in events where a choice can start a combat, like Dense Vegetation's Rest + Fight choice.
	/// </summary>
	/// <param name="exitedRoom">The room that was exited before this was resumed.</param>
	public virtual Task Resume(AbstractRoom exitedRoom)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Call this from an event option function when the event is all finished and the player should be able to proceed.
	/// </summary>
	/// <param name="description">The description to set on the event room.</param>
	protected void SetEventFinished(LocString description)
	{
		SetEventState(description, Array.Empty<EventOption>());
		IsFinished = true;
		EnsureCleanup();
	}

	/// <summary>
	/// Virtual function for events to hook onto if they need to do stuff before the event starts.
	/// </summary>
	protected virtual Task BeforeEventStarted(bool isPreFinished)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Virtual function for events to hook onto if they need to do stuff after the event starts.
	/// </summary>
	public virtual Task AfterEventStarted()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Virtual function for events to hook onto if they need to do stuff when the event ends.
	/// </summary>
	protected virtual void OnEventFinished()
	{
	}

	/// <summary>
	/// Ensures OnEventFinished is called exactly once, preventing double-cleanup.
	/// </summary>
	public void EnsureCleanup()
	{
		if (!_cleanupCalled)
		{
			_cleanupCalled = true;
			OnEventFinished();
		}
	}

	/// <summary>
	/// Call this from an event option function when the event's description and options change to new ones.
	/// </summary>
	/// <param name="description">The new description to set on the event room.</param>
	/// <param name="eventOptions">The new choices to present to the player.</param>
	protected virtual void SetEventState(LocString description, IEnumerable<EventOption> eventOptions)
	{
		AssertMutable();
		if (_currentOptions == null)
		{
			_currentOptions = new List<EventOption>();
		}
		_currentOptions.Clear();
		_currentOptions.AddRange(eventOptions);
		Description = description;
		if (_currentOptions.Count == 0)
		{
			if (_isFinished)
			{
				throw new InvalidOperationException("Tried to set event options after event was finished!");
			}
			_isFinished = true;
		}
		this.StateChanged?.Invoke(this);
	}

	/// <summary>
	/// Enter an encounter, then return to this event once the encounter is finished.
	/// </summary>
	/// <param name="extraRewards">Extra rewards to give the player in addition to the encounter's normal rewards.</param>
	/// <param name="shouldResumeAfterCombat">
	/// Whether to resume this event after the encounter is finished.
	/// Usually true, but some events (like <see cref="T:MegaCrit.Sts2.Core.Models.Events.DenseVegetation" />) have no more options after combat ends, so
	/// they pass false to directly proceed to the next map point after combat ends.
	/// </param>
	protected void EnterCombatWithoutExitingEvent<T>(IReadOnlyList<Reward> extraRewards, bool shouldResumeAfterCombat) where T : EncounterModel
	{
		EnterCombatWithoutExitingEvent(ModelDb.Encounter<T>().ToMutable(), extraRewards, shouldResumeAfterCombat);
	}

	/// <summary>
	/// Enter an encounter, then return to this event once the encounter is finished.
	/// </summary>
	/// <param name="mutableEncounter">The mutable model of the encounter to start.</param>
	/// <param name="extraRewards">Extra rewards to give the player in addition to the encounter's normal rewards.</param>
	/// <param name="shouldResumeAfterCombat">
	/// Whether to resume this event after the encounter is finished.
	/// Usually true, but some events (like <see cref="T:MegaCrit.Sts2.Core.Models.Events.DenseVegetation" />) have no more options after combat ends, so
	/// they pass false to directly proceed to the next map point after combat ends.
	/// </param>
	protected void EnterCombatWithoutExitingEvent(EncounterModel mutableEncounter, IReadOnlyList<Reward> extraRewards, bool shouldResumeAfterCombat)
	{
		if (!IsShared)
		{
			throw new InvalidOperationException($"Tried to enter combat in non-shared event {this}!");
		}
		if (shouldResumeAfterCombat && LayoutType == EventLayoutType.Combat)
		{
			throw new InvalidOperationException($"Cannot resume event {base.Id} after combat because it has a Combat layout — " + "there is no event layout to return to.");
		}
		this.EnteringEventCombat?.Invoke();
		if (!LocalContext.IsMe(Owner))
		{
			return;
		}
		Node = null;
		CombatState combatState = ((LayoutType != EventLayoutType.Combat) ? new CombatState(mutableEncounter, Owner.RunState, Owner.RunState.Modifiers, Owner.RunState.BadgeModels, Owner.RunState.MultiplayerScalingModel) : _combatStateForCombatLayout);
		CombatRoom combatRoom = new CombatRoom(combatState)
		{
			ShouldCreateCombat = (LayoutType != EventLayoutType.Combat),
			ShouldResumeParentEventAfterCombat = shouldResumeAfterCombat,
			ParentEventId = base.Id
		};
		foreach (Reward extraReward in extraRewards)
		{
			combatRoom.AddExtraReward(extraReward.Player, extraReward);
		}
		TaskHelper.RunSafely(RunManager.Instance.EnterRoomWithoutExitingCurrentRoom(combatRoom, LayoutType != EventLayoutType.Combat));
	}

	/// <summary>
	/// Generate an event option from a relic.
	/// </summary>
	/// <remarks>
	/// By default, the passed relic's title and description will be used as the title/description for this option.
	/// However, if events.json contains a {optionKey}.title and/or {optionKey}.description entry, they'll override the
	/// relic's associated field.
	///
	/// For example, at the time of this writing, Pael's "Liquify" option grants the <see cref="T:MegaCrit.Sts2.Core.Models.Relics.PaelsClaw" /> relic. We
	/// want this option's title to be "Liquify" (not <see cref="T:MegaCrit.Sts2.Core.Models.Relics.PaelsClaw" />), but we want its description to be
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Relics.PaelsClaw" />'s description.
	/// So, we pass textKey=PAEL.pages.INITIAL.options.LIQUIFY, and we add
	/// "PAEL.pages.INITIAL.options.LIQUIFY.title": "Liquify" to events.json, but we don't add a ".description" entry.
	/// </remarks>
	/// <param name="onChosen">Function to execute when this option is chosen</param>
	/// <param name="pageName">Name of the page that this option is in, for metrics/gameinfo and loc. INITIAL by default.</param>
	/// <typeparam name="T">Relic whose title, description, and HoverTips should be used for the event option.</typeparam>
	protected EventOption RelicOption<T>(Func<Task>? onChosen, string pageName = "INITIAL") where T : RelicModel
	{
		RelicModel relic = ModelDb.Relic<T>().ToMutable();
		return RelicOption(relic, onChosen, pageName);
	}

	/// <summary>
	/// Generate an event option from a relic. See <see cref="M:MegaCrit.Sts2.Core.Models.EventModel.RelicOption``1(System.Func{System.Threading.Tasks.Task},System.String)" /> for more details.
	/// </summary>
	protected EventOption RelicOption(RelicModel relic, Func<Task>? onChosen, string pageName = "INITIAL")
	{
		relic.AssertMutable();
		relic.Owner = Owner;
		string textKey = OptionKey(pageName, relic.Id.Entry);
		return EventOption.FromRelic(relic, this, onChosen, textKey);
	}

	/// <summary>
	/// Generate a key for the specified option.
	/// </summary>
	/// <param name="optionName">Name of this option, for metrics/gameinfo and loc.</param>
	protected string InitialOptionKey(string optionName)
	{
		return OptionKey("INITIAL", optionName);
	}

	/// <summary>
	/// Generate a key for the specified page and option.
	/// </summary>
	/// <param name="optionName">Name of this option, for metrics/gameinfo and loc.</param>
	/// <param name="pageName">Name of the page that this option is in, for metrics/gameinfo and loc.</param>
	private string OptionKey(string pageName, string optionName)
	{
		return $"{StringHelper.Slugify(GetType().Name)}.pages.{pageName}.options.{optionName}";
	}
}
