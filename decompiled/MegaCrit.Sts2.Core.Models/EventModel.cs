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
	private const string _locTable = "events";

	protected const string _initialPageKey = "INITIAL";

	private EncounterModel? _mutableEncounter;

	protected CombatState? _combatStateForCombatLayout;

	private List<EventOption>? _currentOptions;

	private bool _isFinished;

	private bool _cleanupCalled;

	private DynamicVarSet? _dynamicVars;

	private EventModel _canonicalInstance;

	public virtual Color ButtonColor => new Color(1f, 1f, 1f, 0.9f);

	public virtual bool IsDeterministic => !IsShared;

	public override bool ShouldReceiveCombatHooks => false;

	protected virtual string LocTable => "events";

	public LocString Title => L10NLookup(base.Id.Entry + ".title");

	public virtual LocString InitialDescription => L10NLookup(base.Id.Entry + ".pages.INITIAL.description");

	public Player? Owner { get; private set; }

	public virtual bool IsShared => false;

	public LocString? Description { get; private set; }

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

	public Rng Rng { get; private set; }

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
		Rng = new Rng((uint)(Owner.RunState.Rng.Seed + (IsShared ? 0 : Owner.NetId) + (ulong)StringHelper.GetDeterministicHashCode(base.Id.Entry)));
		try
		{
			await BeforeEventStarted();
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

	protected virtual IReadOnlyList<EventOption> GenerateInitialOptionsWrapper()
	{
		AssertMutable();
		List<EventOption> list = GenerateInitialOptions().ToList();
		ReplaceNullOptions(list);
		return list;
	}

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

	protected void ClearCurrentOptions()
	{
		AssertMutable();
		if (_currentOptions == null)
		{
			_currentOptions = new List<EventOption>();
		}
		_currentOptions.Clear();
	}

	public virtual bool IsAllowed(RunState runState)
	{
		return true;
	}

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

	public PackedScene CreateBackgroundScene()
	{
		return PreloadManager.Cache.GetScene(BackgroundScenePath);
	}

	public Node2D CreateVfx()
	{
		return PreloadManager.Cache.GetScene(VfxPath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
	}

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
		_combatStateForCombatLayout = new CombatState(_mutableEncounter, runState, runState.Modifiers, runState.MultiplayerScalingModel);
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

	public virtual void OnRoomEnter()
	{
	}

	public virtual Task Resume(AbstractRoom exitedRoom)
	{
		return Task.CompletedTask;
	}

	protected void SetEventFinished(LocString description)
	{
		SetEventState(description, Array.Empty<EventOption>());
		IsFinished = true;
		EnsureCleanup();
	}

	protected virtual Task BeforeEventStarted()
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterEventStarted()
	{
		return Task.CompletedTask;
	}

	protected virtual void OnEventFinished()
	{
	}

	public void EnsureCleanup()
	{
		if (!_cleanupCalled)
		{
			_cleanupCalled = true;
			OnEventFinished();
		}
	}

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

	protected void EnterCombatWithoutExitingEvent<T>(IReadOnlyList<Reward> extraRewards, bool shouldResumeAfterCombat) where T : EncounterModel
	{
		EnterCombatWithoutExitingEvent(ModelDb.Encounter<T>().ToMutable(), extraRewards, shouldResumeAfterCombat);
	}

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
		CombatState combatState = ((LayoutType != EventLayoutType.Combat) ? new CombatState(mutableEncounter, Owner.RunState, Owner.RunState.Modifiers, Owner.RunState.MultiplayerScalingModel) : _combatStateForCombatLayout);
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

	protected EventOption RelicOption<T>(Func<Task>? onChosen, string pageName = "INITIAL") where T : RelicModel
	{
		RelicModel relic = ModelDb.Relic<T>().ToMutable();
		return RelicOption(relic, onChosen, pageName);
	}

	protected EventOption RelicOption(RelicModel relic, Func<Task>? onChosen, string pageName = "INITIAL")
	{
		relic.AssertMutable();
		relic.Owner = Owner;
		string textKey = OptionKey(pageName, relic.Id.Entry);
		return EventOption.FromRelic(relic, this, onChosen, textKey);
	}

	protected string InitialOptionKey(string optionName)
	{
		return OptionKey("INITIAL", optionName);
	}

	private string OptionKey(string pageName, string optionName)
	{
		return $"{StringHelper.Slugify(GetType().Name)}.pages.{pageName}.options.{optionName}";
	}
}
