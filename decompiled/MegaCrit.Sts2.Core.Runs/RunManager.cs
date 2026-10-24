using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Daily;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Replay;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Runs.Metrics;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.MapDrawing;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace MegaCrit.Sts2.Core.Runs;

public class RunManager : IRunLobbyListener
{
	private long _startTime;

	private long _prevRunTime;

	private long _sessionStartTime;

	private bool _runHistoryWasUploaded;

	private int _numReloads;

	public Action? debugAfterCombatRewardsOverride;

	public static RunManager Instance { get; } = new RunManager();

	/// <summary>
	/// The ascension manager for this run.
	/// </summary>
	public AscensionManager AscensionManager { get; private set; }

	/// <summary>
	/// Whether or not this run should be saved.
	/// This will always be true for normal players.
	/// For developers, it will be true when starting/continuing a run from the main menu, but false when starting
	/// from SceneBootstrapper or from tests.
	/// </summary>
	public bool ShouldSave { get; private set; }

	/// <summary>
	/// If set to true, the final run score will be uploaded to the daily run leaderboard for today.
	/// </summary>
	public DateTimeOffset? DailyTime { get; private set; }

	/// <summary>
	/// Is there currently a run in progress?
	/// True when in a run, false when on the main menu or submenus.
	/// You shouldn't usually have to check this, because most run-dependent things are only executed within the
	/// context of a run, but this is a good escape valve if you need it.
	/// </summary>
	public bool IsInProgress => State != null;

	/// <summary>
	/// Is the run currently being cleaned up? Only true in the brief period of time after the player hits Save and Quit
	/// or abandon and IsInProgress is still true.
	/// IsInProgress maybe should also check this, but I'm scared because we're about to launch.
	/// </summary>
	public bool IsCleaningUp { get; private set; }

	/// <summary>
	/// Discovery order modifications (e.g. boss order or enemy order) is usually disabled in multiplayer and tests.
	/// Set this to true to force them to be applied.
	/// </summary>
	public bool ForceDiscoveryOrderModifications { get; set; }

	public bool IsGameOver
	{
		get
		{
			if (IsInProgress)
			{
				return State.IsGameOver;
			}
			return false;
		}
	}

	public bool IsAbandoned { get; private set; }

	public RunHistory? History { get; set; }

	public INetGameService NetService { get; private set; }

	public ChecksumTracker ChecksumTracker { get; private set; }

	public RunLocationTargetedMessageBuffer RunLocationTargetedBuffer { get; private set; }

	public CombatReplayWriter CombatReplayWriter { get; private set; }

	public RunLobby? RunLobby { get; private set; }

	public CombatStateSynchronizer CombatStateSynchronizer { get; private set; }

	public MapSelectionSynchronizer MapSelectionSynchronizer { get; private set; }

	public ActChangeSynchronizer ActChangeSynchronizer { get; private set; }

	public PlayerChoiceSynchronizer PlayerChoiceSynchronizer { get; private set; }

	public EventSynchronizer EventSynchronizer { get; private set; }

	public RewardSynchronizer RewardSynchronizer { get; private set; }

	public RewardsSetSynchronizer RewardsSetSynchronizer { get; private set; }

	public RestSiteSynchronizer RestSiteSynchronizer { get; private set; }

	public OneOffSynchronizer OneOffSynchronizer { get; private set; }

	public TreasureRoomRelicSynchronizer TreasureRoomRelicSynchronizer { get; private set; }

	public FlavorSynchronizer FlavorSynchronizer { get; private set; }

	public PeerInputSynchronizer InputSynchronizer { get; private set; }

	public HoveredModelTracker HoveredModelTracker { get; private set; }

	public ActionQueueSet ActionQueueSet { get; private set; }

	public ActionExecutor ActionExecutor { get; private set; }

	public ActionQueueSynchronizer ActionQueueSynchronizer { get; private set; }

	/// <summary>
	/// The run time when you beat the Act 3 boss.
	/// </summary>
	public long WinTime { get; set; }

	/// <summary>
	/// The total play time of the current run. If the run has been won, will use the WinTime instead.
	/// </summary>
	public long RunTime
	{
		get
		{
			if (WinTime > 0)
			{
				return WinTime;
			}
			return DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _sessionStartTime + _prevRunTime;
		}
	}

	/// <summary>
	/// This flag returns true if we are in a singleplayer session or we are faking a multiplayer session.
	/// Sometimes, for testing, we add dummy players through the bootstrapper. In those cases, we still want end turn
	/// and other functions not to wait for multiple players, because there's only one acting player.
	/// </summary>
	public bool IsSingleplayerOrFakeMultiplayer
	{
		get
		{
			if (IsInProgress)
			{
				return NetService.Type == NetGameType.Singleplayer;
			}
			return false;
		}
	}

	public SerializableMapDrawings? MapDrawingsToLoad { get; set; }

	/// <summary>
	/// Saved maps from a loaded run, indexed by act index.
	/// Used to restore exact map topology instead of regenerating from RNG.
	/// Cleared after use to allow relics to regenerate maps normally.
	/// </summary>
	public Dictionary<int, SerializableActMap>? SavedMapsToLoad { get; set; }

	private RunState? State { get; set; }

	public event Action<RunState>? RunStarted;

	public event Action? RoomEntered;

	public event Action? RoomExited;

	public event Action? ActEntered;

	private RunManager()
	{
	}

	/// <summary>
	/// Set up a brand-new singleplayer run.
	/// This includes running initialization code for things that should happen at the start of a run (obtaining the
	/// characters' starting deck and relic, setting an empty potion belt, etc.).
	/// </summary>
	/// <param name="state">RunState that should be used for the run.</param>
	/// <param name="shouldSave">
	/// Whether or not the run should be saved to disk.
	/// True during normal gameplay, false during tests and bootstrap.
	/// </param>
	/// <param name="dailyTime">
	/// If non-null, then the final run score will be uploaded to the daily run leaderboard for the passed time.
	/// </param>
	public void SetUpNewSingleplayer(RunState state, bool shouldSave, DateTimeOffset? dailyTime = null)
	{
		if (State != null)
		{
			throw new InvalidOperationException("State is already set.");
		}
		State = state;
		INetGameService netService = new NetSingleplayerGameService();
		InitializeShared(netService, new PeerInputSynchronizer(netService), shouldSave, dailyTime, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 0L, 0L, 0);
		InitializeRunLobby(netService, state);
		InitializeNewRun();
		GenerateRooms();
	}

	/// <summary>
	/// Set up a brand-new multiplayer run.
	/// This includes running initialization code for things that should happen at the start of a run (obtaining the
	/// characters' starting deck and relic, setting an empty potion belt, etc.).
	/// </summary>
	/// <param name="state">RunState that should be used for the run.</param>
	/// <param name="lobby">The multiplayer lobby containing the players that will go on the run together.</param>
	/// <param name="shouldSave">
	/// Whether or not the run should be saved to disk.
	/// True during normal gameplay, false during tests and bootstrap.
	/// </param>
	/// <param name="dailyTime">
	/// If non-null, then the final run score will be uploaded to the daily run leaderboard for the passed time.
	/// </param>
	public void SetUpNewMultiplayer(RunState state, StartRunLobby lobby, bool shouldSave, DateTimeOffset? dailyTime = null)
	{
		if (State != null)
		{
			throw new InvalidOperationException("State is already set.");
		}
		State = state;
		InitializeShared(lobby.NetService, lobby.InputSynchronizer, shouldSave, dailyTime, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 0L, 0L, 0);
		InitializeRunLobby(lobby.NetService, state);
		InitializeNewRun();
		GenerateRooms();
	}

	/// <summary>
	/// Set up a singleplayer run that's been loaded from a save file.
	/// No start-of-run initialization code will be run here, since we're loading an existing state.
	/// </summary>
	/// <param name="state">RunState that should be used for the run.</param>
	/// <param name="save">
	/// The serialized version of the run. This may contain extra data that's not part of the deserialized RunState.
	/// </param>
	public async Task SetUpSavedSingleplayer(RunState state, SerializableRun save)
	{
		if (State != null)
		{
			throw new InvalidOperationException("State is already set.");
		}
		State = state;
		if (TestMode.IsOff)
		{
			await SaveManager.Instance.IncrementNumReloads(save, isMultiplayer: false);
		}
		INetGameService netService = new NetSingleplayerGameService();
		InitializeShared(netService, new PeerInputSynchronizer(netService), shouldSave: true, save.DailyTime, save.StartTime, save.RunTime, save.WinTime, save.NumReloads);
		InitializeRunLobby(netService, state);
		InitializeSavedRun(save);
	}

	/// <summary>
	/// Set up a multiplayer run that's been loaded from a save file.
	/// No start-of-run initialization code will be run here, since we're loading an existing state.
	/// </summary>
	/// <param name="state">RunState that should be used for the run.</param>
	/// <param name="lobby">
	/// The multiplayer lobby containing the players that will go on the run together.
	/// The lobby also contains the serialized version of the run. This may contain extra data that's not part of the
	/// deserialized RunState.
	/// </param>
	public async Task SetUpSavedMultiplayer(RunState state, LoadRunLobby lobby)
	{
		if (State != null)
		{
			throw new InvalidOperationException("State is already set.");
		}
		State = state;
		SerializableRun save = lobby.Run;
		if (lobby.NetService.Type == NetGameType.Host && TestMode.IsOff)
		{
			await SaveManager.Instance.IncrementNumReloads(save, isMultiplayer: true);
		}
		InitializeShared(lobby.NetService, lobby.InputSynchronizer, shouldSave: true, save.DailyTime, save.StartTime, save.RunTime, save.WinTime, save.NumReloads);
		InitializeRunLobby(lobby.NetService, state);
		InitializeSavedRun(save);
	}

	/// <summary>
	/// Set up a run that's been loaded from a CombatReplay file.
	/// No start-of-run initialization code will be run here, since we're loading an existing state.
	/// </summary>
	/// <param name="state">RunState that should be used for the run.</param>
	/// <param name="replay">
	/// CombatReplay that is being replayed in this run.
	/// The replay also contains the serialized version of the run. This may contain extra data that's not part of the
	/// deserialized RunState.
	/// </param>
	public void SetUpReplay(RunState state, CombatReplay replay)
	{
		if (State != null)
		{
			throw new InvalidOperationException("State is already set.");
		}
		State = state;
		SerializableRun serializableRun = replay.serializableRun;
		ulong netId = serializableRun.Players[0].NetId;
		NetReplayGameService netService = new NetReplayGameService(netId);
		InitializeShared(netService, new PeerInputSynchronizer(netService), shouldSave: true, serializableRun.DailyTime, serializableRun.StartTime, serializableRun.RunTime, serializableRun.WinTime, serializableRun.NumReloads);
		InitializeRunLobby(netService, state);
		InitializeSavedRun(serializableRun);
	}

	/// <summary>
	/// Set up a brand-new run to be used in an automated test. Should only be used in the test project.
	/// This includes running initialization code for things that should happen at the start of a run (obtaining the
	/// characters' starting deck and relic, setting an empty potion belt, etc.).
	/// </summary>
	/// <param name="state">RunState that should be used for the run.</param>
	/// <param name="gameService">The mock INetGameService to use.</param>
	/// <param name="disableCombatStateSync">
	/// Whether or not combat state synchronization should be disabled. This is only relevant if you are testing
	/// multiplayer scenarios.
	/// </param>
	/// <param name="shouldSave">
	/// If true, saving will be performed. Remember to call <see cref="M:MegaCrit.Sts2.Core.Saves.SaveManager.MockInstanceForTesting(MegaCrit.Sts2.Core.Saves.SaveManager)" />.
	/// </param>
	public void SetUpTest(RunState state, INetGameService gameService, bool disableCombatStateSync = true, bool shouldSave = false)
	{
		if (State != null)
		{
			throw new InvalidOperationException("State is already set.");
		}
		State = state;
		InitializeShared(gameService, new PeerInputSynchronizer(gameService), shouldSave, null, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 0L, 0L, 0);
		InitializeRunLobby(gameService, state);
		CombatStateSynchronizer.IsDisabled = disableCombatStateSync;
		InitializeNewRun();
	}

	private void InitializeShared(INetGameService netService, PeerInputSynchronizer inputSynchronizer, bool shouldSave, DateTimeOffset? dailyTime, long startTime, long runTime, long winTime, int numReloads)
	{
		if (State == null)
		{
			throw new InvalidOperationException("State is not set.");
		}
		NetService = netService;
		ulong netId = NetService.NetId;
		ChecksumTracker = new ChecksumTracker(NetService, State);
		ChecksumTracker checksumTracker = ChecksumTracker;
		bool flag = !TestMode.IsOn;
		bool flag2 = flag;
		if (flag2)
		{
			NetGameType type = NetService.Type;
			bool flag3 = (uint)(type - 2) <= 2u;
			flag2 = flag3;
		}
		checksumTracker.IsEnabled = flag2;
		RunLocationTargetedBuffer = new RunLocationTargetedMessageBuffer(NetService);
		FlavorSynchronizer = new FlavorSynchronizer(NetService, State, netId);
		ActionQueueSet = new ActionQueueSet(State.Players);
		ActionExecutor = new ActionExecutor(ActionQueueSet);
		ActionQueueSynchronizer = new ActionQueueSynchronizer(State, ActionQueueSet, RunLocationTargetedBuffer, NetService);
		PlayerChoiceSynchronizer = new PlayerChoiceSynchronizer(NetService, State);
		MapSelectionSynchronizer = new MapSelectionSynchronizer(NetService, ActionQueueSynchronizer, State);
		ActChangeSynchronizer = new ActChangeSynchronizer(State);
		EventSynchronizer = new EventSynchronizer(RunLocationTargetedBuffer, NetService, State, netId, State.Rng.Seed);
		RewardSynchronizer = new RewardSynchronizer(RunLocationTargetedBuffer, NetService, State, netId);
		RewardsSetSynchronizer = new RewardsSetSynchronizer(RunLocationTargetedBuffer, NetService, State, netId);
		RestSiteSynchronizer = new RestSiteSynchronizer(RunLocationTargetedBuffer, NetService, State, netId);
		OneOffSynchronizer = new OneOffSynchronizer(RunLocationTargetedBuffer, NetService, State, netId);
		TreasureRoomRelicSynchronizer = new TreasureRoomRelicSynchronizer(State, netId, ActionQueueSynchronizer, State.SharedRelicGrabBag, State.Rng.TreasureRoomRelics);
		CombatReplayWriter = new CombatReplayWriter(PlayerChoiceSynchronizer, RewardsSetSynchronizer, ActionQueueSet, ActionQueueSynchronizer, ChecksumTracker);
		CombatReplayWriter.IsEnabled = !TestMode.IsOn;
		ActionExecutor.JustBeforeActionFinishedExecuting += SendPostActionChecksum;
		ChecksumTracker.StateDiverged += StateDiverged;
		ActionExecutor.Pause();
		IsAbandoned = false;
		AscensionManager = new AscensionManager(State.AscensionLevel);
		ShouldSave = shouldSave;
		DailyTime = dailyTime;
		_startTime = startTime;
		_prevRunTime = runTime;
		WinTime = winTime;
		_numReloads = numReloads;
		_sessionStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		InputSynchronizer = inputSynchronizer;
		HoveredModelTracker = new HoveredModelTracker(InputSynchronizer, State);
	}

	private void InitializeRunLobby(INetGameService netService, RunState state)
	{
		if (netService.Type.IsMultiplayer())
		{
			RunLobby = new RunLobby(state.GameMode, netService, this, state, state.Players.Select((Player p) => p.NetId));
			RunLobby.RemotePlayerDisconnected += RemotePlayerDisconnected;
		}
		CombatStateSynchronizer = new CombatStateSynchronizer(NetService, RunLobby, state);
	}

	/// <summary>
	/// Call this when starting a brand new run, not when loading a saved run.
	/// </summary>
	private void InitializeNewRun()
	{
		State.SharedRelicGrabBag.Populate(ModelDb.RelicPool<SharedRelicPool>().GetUnlockedRelics(State.UnlockState), State.Rng.UpFront);
		foreach (Player player in State.Players)
		{
			player.PopulateRelicGrabBagIfNecessary(State.Rng.UpFront);
		}
		SetStartedWithNeowFlag();
		foreach (ModifierModel modifier in State.Modifiers)
		{
			modifier.OnRunCreated(State);
		}
		foreach (Player player2 in State.Players)
		{
			ApplyAscensionEffects(player2);
		}
	}

	/// <summary>
	/// Call this when loading a saved run, not when starting a brand new run.
	/// </summary>
	private void InitializeSavedRun(SerializableRun save)
	{
		foreach (ActModel act in State.Acts)
		{
			act.ValidateRoomsAfterLoad(State.Rng.UpFront);
		}
		AfterMapLocationChanged();
		MapDrawingsToLoad = save.MapDrawings;
		SavedMapsToLoad = null;
		for (int i = 0; i < save.Acts.Count; i++)
		{
			SerializableActMap savedMap = save.Acts[i].SavedMap;
			if (savedMap != null)
			{
				if (SavedMapsToLoad == null)
				{
					Dictionary<int, SerializableActMap> dictionary = (SavedMapsToLoad = new Dictionary<int, SerializableActMap>());
				}
				SavedMapsToLoad[i] = savedMap;
			}
		}
		foreach (ModifierModel modifier in State.Modifiers)
		{
			modifier.OnRunLoaded(State);
		}
	}

	private void SendPostActionChecksum(GameAction action)
	{
		if (CombatManager.Instance.IsInProgress && ((!(action is EndPlayerTurnAction) && !(action is ReadyToBeginEnemyTurnAction)) || 1 == 0))
		{
			ChecksumTracker.GenerateChecksum($"finished action execution {action}", action);
		}
	}

	/// <summary>
	/// Call this when we start a new run to set the StartedWithNeow extra field.
	/// </summary>
	private void SetStartedWithNeowFlag()
	{
		State.ExtraFields.StartedWithNeow = State.UnlockState.IsEpochRevealed<NeowEpoch>();
	}

	/// <summary>
	/// Call this to validate a save can be loaded without actually creating a run.
	/// This performs deep validation by instantiating save components to ensure all data is valid.
	///
	/// This validation occurs AFTER JSON parsing and migration, so the save file has already been
	/// successfully deserialized. However, the content may still be invalid due to:
	/// - Removed modifiers that exist in the save
	/// - Invalid card/relic/potion IDs
	/// - Corrupted game state data
	///
	/// If the save has any deprecated content in it, the save will be mutated so that the content is replaced with its
	/// deprecated version. This is done so that it can be sent over the network correctly in multiplayer scenarios.
	///
	/// Unlike MigrationManager.LoadSave(), this method does not automatically handle corruption.
	/// Callers must handle exceptions and rename corrupt files as needed.
	/// </summary>
	/// <param name="save">The save to attempt to load.</param>
	/// <param name="localPlayerId">The player ID of the local (hosting) player. If it is not in the save file, an
	/// exception is thrown.</param>
	public static SerializableRun CanonicalizeSave(SerializableRun save, ulong localPlayerId)
	{
		if (save.Players.FirstOrDefault((SerializablePlayer p) => p.NetId == localPlayerId) == null)
		{
			throw new InvalidOperationException($"Save is invalid! Players does not contain local player Id. IDs in save file: {string.Join(",", save.Players.Select((SerializablePlayer p) => p.NetId))}. Local ID: {localPlayerId}.");
		}
		RunState runState = RunState.FromSerializable(save);
		int latestSchemaVersion = SaveManager.Instance.GetLatestSchemaVersion<SerializableRun>();
		SerializableRun serializableRun = new SerializableRun
		{
			SchemaVersion = latestSchemaVersion,
			Acts = runState.Acts.Zip(save.Acts, delegate(ActModel act, SerializableActModel savedAct)
			{
				SerializableActModel serializableActModel = act.ToSave();
				serializableActModel.SavedMap = savedAct.SavedMap;
				return serializableActModel;
			}).ToList(),
			Modifiers = runState.Modifiers.Select((ModifierModel m) => m.ToSerializable()).ToList(),
			DailyTime = save.DailyTime,
			GameMode = runState.GameMode,
			CurrentActIndex = runState.CurrentActIndex,
			EventsSeen = runState.VisitedEventIds.ToList(),
			SerializableOdds = runState.Odds.ToSerializable(),
			SerializableSharedRelicGrabBag = runState.SharedRelicGrabBag.ToSerializable(),
			Players = runState.Players.Select((Player p) => p.ToSerializable()).ToList(),
			SerializableRng = runState.Rng.ToSerializable(),
			VisitedMapCoords = runState.VisitedMapCoords.ToList(),
			MapPointHistory = runState.MapPointHistory.Select((IReadOnlyList<MapPointHistoryEntry> l) => l.ToList()).ToList(),
			SaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
			StartTime = save.StartTime,
			RunTime = save.RunTime,
			WinTime = save.WinTime,
			Ascension = runState.AscensionLevel,
			PlatformType = save.PlatformType,
			MapDrawings = save.MapDrawings,
			NumReloads = save.NumReloads,
			ExtraFields = runState.ExtraFields.ToSerializable(),
			PreFinishedRoom = save.PreFinishedRoom
		};
		PacketWriter packetWriter = new PacketWriter();
		packetWriter.Write(serializableRun);
		return serializableRun;
	}

	/// <summary>
	/// Builds a blacklist of room types that should not be rolled for an Unknown map point, based on the connected map
	/// points.
	/// </summary>
	/// <param name="previousMapPointEntry">
	/// The history entry for the previous map point.
	/// Null before we've visited the second map point.
	/// </param>
	/// <param name="nextMapPoints">
	/// The next map points from the current one.
	/// Empty before we've visited the first map point, and after visiting the boss map point.
	/// </param>
	/// <returns>A set of room types that should be excluded when rolling.</returns>
	public static HashSet<RoomType> BuildRoomTypeBlacklist(MapPointHistoryEntry? previousMapPointEntry, IReadOnlyCollection<MapPoint> nextMapPoints)
	{
		HashSet<RoomType> hashSet = new HashSet<RoomType>();
		if ((previousMapPointEntry != null && previousMapPointEntry.HasRoomOfType(RoomType.Shop)) || (nextMapPoints.Count > 0 && nextMapPoints.All((MapPoint p) => p.PointType == MapPointType.Shop)))
		{
			hashSet.Add(RoomType.Shop);
		}
		return hashSet;
	}

	public SerializableRun ToSave(AbstractRoom? preFinishedRoom)
	{
		int latestSchemaVersion = SaveManager.Instance.GetLatestSchemaVersion<SerializableRun>();
		List<SerializableActModel> list = new List<SerializableActModel>();
		for (int i = 0; i < State.Acts.Count; i++)
		{
			SerializableActModel serializableActModel = State.Acts[i].ToSave();
			if (i == State.CurrentActIndex && State.Map != null)
			{
				serializableActModel.SavedMap = SerializableActMap.FromActMap(State.Map);
			}
			list.Add(serializableActModel);
		}
		return new SerializableRun
		{
			SchemaVersion = latestSchemaVersion,
			Acts = list,
			Modifiers = State.Modifiers.Select((ModifierModel m) => m.ToSerializable()).ToList(),
			DailyTime = DailyTime,
			CurrentActIndex = State.CurrentActIndex,
			EventsSeen = State.VisitedEventIds.ToList(),
			GameMode = State.GameMode,
			SerializableOdds = State.Odds.ToSerializable(),
			SerializableSharedRelicGrabBag = State.SharedRelicGrabBag.ToSerializable(),
			Players = State.Players.Select((Player p) => p.ToSerializable()).ToList(),
			SerializableRng = State.Rng.ToSerializable(),
			VisitedMapCoords = State.VisitedMapCoords.ToList(),
			MapPointHistory = State.MapPointHistory.Select((IReadOnlyList<MapPointHistoryEntry> l) => l.ToList()).ToList(),
			SaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
			StartTime = _startTime,
			RunTime = RunTime,
			WinTime = WinTime,
			NumReloads = _numReloads,
			Ascension = State.AscensionLevel,
			PlatformType = NetService.Platform,
			MapDrawings = NRun.Instance?.GlobalUi.MapScreen.Drawings.GetSerializableMapDrawings(),
			ExtraFields = State.ExtraFields.ToSerializable(),
			PreFinishedRoom = preFinishedRoom?.ToSerializable()
		};
	}

	public RunState Launch()
	{
		LocalContext.NetId = NetService.NetId;
		NetService.SetBufferMessages(bufferMessages: false);
		this.RunStarted?.Invoke(State);
		UpdateRichPresence();
		return State;
	}

	/// <summary>
	/// Finalize the relics at the start of a run.
	/// This is called when creating a new run, but not before loading a saved run, since
	/// <see cref="M:MegaCrit.Sts2.Core.Models.RelicModel.AfterObtained" /> is not idempotent.
	/// </summary>
	public async Task FinalizeStartingRelics()
	{
		if (State == null)
		{
			return;
		}
		foreach (Player player in State.Players)
		{
			foreach (RelicModel relic in player.Relics)
			{
				await relic.AfterObtained();
			}
		}
	}

	/// <summary>
	/// Initialize the rooms for the run. Don't call this in Start so we can skip it in tests.
	/// </summary>
	public void GenerateRooms()
	{
		List<AncientEventModel> list = State.UnlockState.SharedAncients.ToList().UnstableShuffle(State.Rng.UpFront);
		foreach (ActModel item in State.Acts.Skip(1))
		{
			int count = State.Rng.UpFront.NextInt(list.Count + 1);
			List<AncientEventModel> list2 = list.Take(count).ToList();
			list = list.Except(list2).ToList();
			item.SetSharedAncientSubset(list2);
		}
		for (int i = 0; i < State.Acts.Count; i++)
		{
			ActModel act = State.Acts[i];
			act.GenerateRooms(State.Rng.UpFront, State.UnlockState, State.Players.Count > 1);
			if (ShouldApplyTutorialModifications())
			{
				act.ApplyDiscoveryOrderModifications(State.UnlockState);
			}
			if (i == State.Acts.Count - 1 && AscensionManager.HasLevel(AscensionLevel.DoubleBoss))
			{
				EncounterModel secondBossEncounter = State.Rng.UpFront.NextItem(act.AllBossEncounters.Where((EncounterModel e) => e.Id != act.BossEncounter.Id));
				act.SetSecondBossEncounter(secondBossEncounter);
			}
		}
	}

	/// <summary>
	/// Returns true if tutorial modifications should be applied to the current run. False otherwise.
	/// Takes into account things like game mode, test mode, and singleplayer/multiplayer.
	/// </summary>
	public bool ShouldApplyTutorialModifications()
	{
		if (ForceDiscoveryOrderModifications)
		{
			return true;
		}
		if (TestMode.IsOn)
		{
			return false;
		}
		if (State == null)
		{
			return false;
		}
		if (State.GameMode != GameMode.Standard)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Generate a new map.
	/// This is called at the start of the run, between each act, and when relics create an entirely new map.
	/// Don't call this in Start so we can skip it in tests.
	/// </summary>
	public async Task GenerateMap()
	{
		if (State == null)
		{
			throw new InvalidOperationException("State is not set.");
		}
		MapSelectionSynchronizer.BeforeMapGenerated();
		ActMap map;
		if (SavedMapsToLoad != null && SavedMapsToLoad.TryGetValue(State.CurrentActIndex, out SerializableActMap value))
		{
			map = new SavedActMap(value);
			SavedMapsToLoad.Remove(State.CurrentActIndex);
			if (SavedMapsToLoad.Count == 0)
			{
				SavedMapsToLoad = null;
			}
			map = Hook.ModifyGeneratedMapLate(State, map, State.CurrentActIndex);
			await Hook.AfterMapGenerated(State, map, State.CurrentActIndex);
		}
		else
		{
			ActMap map2 = State.Act.CreateMap(State, replaceTreasureWithElites: false);
			map = Hook.ModifyGeneratedMap(State, map2, State.CurrentActIndex);
			await Hook.AfterMapGenerated(State, map, State.CurrentActIndex);
			if (!State.ExtraFields.StartedWithNeow && State.CurrentActIndex == 0)
			{
				map.StartingMapPoint.PointType = MapPointType.Monster;
			}
		}
		State.Map = map;
		State.RemoveStaleVisitedMapCoords(map);
		NMapScreen.Instance?.SetMap(map, State.Rng.Seed, clearDrawings: true);
	}

	public Task EnterMapCoord(MapCoord coord)
	{
		if (State == null)
		{
			return Task.CompletedTask;
		}
		if (!State.AddVisitedMapCoord(coord))
		{
			return Task.CompletedTask;
		}
		return EnterMapCoordInternal(coord, null, saveGame: true);
	}

	public async Task LoadIntoLatestMapCoord(AbstractRoom? preFinishedRoom)
	{
		if (State != null)
		{
			if (State.VisitedMapCoords.Count > 0)
			{
				RunManager runManager = this;
				IReadOnlyList<MapCoord> visitedMapCoords = State.VisitedMapCoords;
				await runManager.EnterMapCoordInternal(visitedMapCoords[visitedMapCoords.Count - 1], preFinishedRoom, saveGame: false);
			}
			else
			{
				await EnterRoomInternal(new MapRoom());
			}
		}
	}

	private Task EnterMapCoordInternal(MapCoord coord, AbstractRoom? preFinishedRoom, bool saveGame)
	{
		if (State == null)
		{
			return Task.CompletedTask;
		}
		MapPoint point = State.Map.GetPoint(coord);
		return EnterMapPointInternal(coord.row + 1, point.PointType, preFinishedRoom, saveGame);
	}

	/// <summary>
	/// WARNING: This should only be called by <see cref="T:MegaCrit.Sts2.Core.Runs.RunManager" /> and in tests.
	/// </summary>
	public async Task EnterMapPointInternal(int actFloor, MapPointType pointType, AbstractRoom? preFinishedRoom, bool saveGame)
	{
		if (State == null)
		{
			return;
		}
		using (new NetLoadingHandle(NetService))
		{
			if (State.MapPointHistory.Count > 0)
			{
				UpdatePlayerStatsInMapPointHistory();
			}
			State.ActFloor = actFloor;
			await ExitCurrentRooms();
			if (preFinishedRoom == null)
			{
				CombatStateSynchronizer.StartSync();
			}
			ClearScreens();
			if (preFinishedRoom == null)
			{
				await CombatStateSynchronizer.WaitForSync();
			}
			if (saveGame)
			{
				await SaveManager.Instance.SaveRun(null);
			}
			if (CombatReplayWriter.IsEnabled)
			{
				CombatReplayWriter.RecordInitialState(ToSave(null));
			}
			RoomType roomType;
			if (pointType == MapPointType.Unknown && preFinishedRoom != null)
			{
				roomType = RoomType.Monster;
			}
			else
			{
				HashSet<RoomType> blacklist = BuildRoomTypeBlacklist(State.CurrentMapPointHistoryEntry, State.CurrentMapPoint?.Children ?? new HashSet<MapPoint>());
				roomType = RollRoomTypeFor(pointType, blacklist);
			}
			AbstractRoom abstractRoom = ((preFinishedRoom == null) ? CreateRoom(roomType, pointType) : preFinishedRoom);
			ActionExecutor.Pause();
			if (preFinishedRoom == null)
			{
				State.AppendToMapPointHistory(pointType, abstractRoom.RoomType, abstractRoom.ModelId);
			}
			if (abstractRoom is CombatRoom { IsPreFinished: not false, ParentEventId: not null } combatRoom)
			{
				EventRoom room = new EventRoom(ModelDb.GetById<EventModel>(combatRoom.ParentEventId));
				await EnterRoomInternal(room, isRestoringRoomStackBase: true);
				await EnterRoomInternal(combatRoom);
			}
			else
			{
				await EnterRoom(abstractRoom);
			}
			if (NRun.Instance != null)
			{
				NRun.Instance.GlobalUi.MapScreen.IsTraveling = false;
			}
			AfterMapLocationChanged();
			await FadeIn();
		}
	}

	private AbstractRoom CreateRoom(RoomType roomType, MapPointType mapPointType = MapPointType.Unassigned, AbstractModel? model = null)
	{
		if (State == null)
		{
			throw new InvalidOperationException("RunState is not set.");
		}
		switch (roomType)
		{
		case RoomType.Monster:
		case RoomType.Elite:
		case RoomType.Boss:
			return new CombatRoom((model as EncounterModel) ?? State.Act.PullNextEncounter(roomType).ToMutable(), State);
		case RoomType.Treasure:
			return new TreasureRoom(State.CurrentActIndex);
		case RoomType.Shop:
			return new MerchantRoom();
		case RoomType.Event:
			return new EventRoom((model as EventModel) ?? ((mapPointType == MapPointType.Ancient) ? State.Act.PullAncient() : State.Act.PullNextEvent(State)));
		case RoomType.RestSite:
			return new RestSiteRoom();
		case RoomType.Map:
			return new MapRoom();
		default:
			throw new InvalidOperationException($"Unexpected RoomType: {roomType}");
		}
	}

	/// <summary>
	/// Roll for a room type based on the specified map point type.
	/// Most map point type have an idempotent room type mapping, but unknown points need to do an RNG roll to determine
	/// room type, which is why we call this a "roll".
	/// </summary>
	/// <param name="pointType">MapPointType to roll a RoomType for.</param>
	/// <param name="blacklist">Room types that we shouldn't be able to roll for map point types with multiple options.</param>
	/// <returns>RoomType.</returns>
	private RoomType RollRoomTypeFor(MapPointType pointType, IEnumerable<RoomType> blacklist)
	{
		if (TryGetRoomTypeForTutorial(pointType, out var roomType))
		{
			return roomType;
		}
		return pointType switch
		{
			MapPointType.Unassigned => RoomType.Unassigned, 
			MapPointType.Unknown => State.Odds.UnknownMapPoint.Roll(blacklist, State), 
			MapPointType.Shop => RoomType.Shop, 
			MapPointType.Treasure => RoomType.Treasure, 
			MapPointType.RestSite => RoomType.RestSite, 
			MapPointType.Monster => RoomType.Monster, 
			MapPointType.Elite => RoomType.Elite, 
			MapPointType.Boss => RoomType.Boss, 
			MapPointType.Ancient => RoomType.Event, 
			_ => throw new ArgumentOutOfRangeException("pointType", pointType, null), 
		};
	}

	/// <summary>
	/// Overrides question mark rooms for the player's very first run.
	/// </summary>
	private bool TryGetRoomTypeForTutorial(MapPointType pointType, out RoomType roomType)
	{
		roomType = RoomType.Unassigned;
		if (!TestMode.IsOn)
		{
			RunState? state = State;
			if (state == null || state.Players.Count <= 1)
			{
				if (pointType != MapPointType.Unassigned)
				{
					return false;
				}
				if (SaveManager.Instance.Progress.NumberOfRuns > 0)
				{
					return false;
				}
				RunState? state2 = State;
				if (state2 != null && state2.MapPointHistory.SelectMany((IReadOnlyList<MapPointHistoryEntry> l) => l).Any((MapPointHistoryEntry e) => e.MapPointType == MapPointType.Unassigned))
				{
					return false;
				}
				roomType = RoomType.Event;
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Helper to call the universal "enter the room" fade in vfx.
	/// </summary>
	private async Task FadeIn(bool showTransition = true)
	{
		if (!TestMode.IsOn)
		{
			await NGame.Instance.Transition.RoomFadeIn(showTransition);
		}
	}

	/// <summary>
	/// Resets various UI elements before transitioning to the next room.
	/// </summary>
	private void ClearScreens()
	{
		if (!TestMode.IsOn)
		{
			NOverlayStack.Instance.Clear();
			NCapstoneContainer.Instance.Close();
			NMapScreen.Instance.Close(animateOut: false);
		}
	}

	/// <summary>
	/// Should only be used in tests or dev commands, never in real flows.
	/// </summary>
	public async Task EnterMapCoordDebug(MapCoord coord, RoomType roomType, MapPointType pointType = MapPointType.Unassigned, AbstractModel? model = null, bool showTransition = true)
	{
		State.AddVisitedMapCoord(coord);
		await EnterRoomDebug(roomType, pointType, model, showTransition);
	}

	/// <summary>
	/// Should only be used in tests or dev commands, never in real flows.
	/// </summary>
	public async Task<AbstractRoom> EnterRoomDebug(RoomType roomType, MapPointType pointType = MapPointType.Unassigned, AbstractModel? model = null, bool showTransition = true)
	{
		using (new NetLoadingHandle(NetService))
		{
			CombatStateSynchronizer.StartSync();
			if (model is EncounterModel encounterModel)
			{
				roomType = encounterModel.RoomType;
			}
			else if (model is EventModel)
			{
				roomType = RoomType.Event;
			}
			if (pointType == MapPointType.Unassigned)
			{
				MapPointType mapPointType = default(MapPointType);
				switch (roomType)
				{
				case RoomType.Monster:
					mapPointType = MapPointType.Monster;
					break;
				case RoomType.Elite:
					mapPointType = MapPointType.Elite;
					break;
				case RoomType.Boss:
					mapPointType = MapPointType.Boss;
					break;
				case RoomType.Treasure:
					mapPointType = MapPointType.Treasure;
					break;
				case RoomType.Shop:
					mapPointType = MapPointType.Shop;
					break;
				case RoomType.Event:
					mapPointType = MapPointType.Unknown;
					break;
				case RoomType.RestSite:
					mapPointType = MapPointType.RestSite;
					break;
				case RoomType.Unassigned:
					mapPointType = MapPointType.Unassigned;
					break;
				case RoomType.Map:
					mapPointType = MapPointType.Unassigned;
					break;
				default:
					global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(roomType);
					break;
				}
				pointType = mapPointType;
			}
			if (CombatReplayWriter.IsEnabled)
			{
				CombatReplayWriter.RecordInitialState(ToSave(null));
			}
			ClearScreens();
			State.AppendToMapPointHistory(pointType, roomType, model?.Id);
			NRun.Instance?.GlobalUi.TopBar.RoomIcon.DebugSetMapPointTypeOverride(pointType);
			if (State.Map is MockSinglePointActMap mockSinglePointActMap)
			{
				mockSinglePointActMap.MockCurrentMapPointType(pointType);
			}
			await CombatStateSynchronizer.WaitForSync();
			AbstractRoom room = CreateRoom(roomType, MapPointType.Unassigned, model);
			await EnterRoom(room);
			await FadeIn(showTransition);
			return room;
		}
	}

	private async Task ExitCurrentRooms()
	{
		if (State != null)
		{
			while (State.CurrentRoomCount > 0)
			{
				await ExitCurrentRoom();
			}
			NRun.Instance?.GlobalUi.TopBar.RoomIcon.DebugClearMapPointTypeOverride();
		}
	}

	private async Task<AbstractRoom?> ExitCurrentRoom()
	{
		if (State == null)
		{
			return null;
		}
		RewardsSetSynchronizer.BeforeLeavingRoom();
		AbstractRoom currentRoom = State.PopCurrentRoom();
		await currentRoom.Exit(State);
		this.RoomExited?.Invoke();
		return currentRoom;
	}

	/// <param name="room">Room to enter.</param>
	/// <param name="isRestoringRoomStackBase">
	/// If true, skip hooks and room visit tracking. Used when reconstructing the base of the room stack on load
	/// (e.g., pushing a parent EventRoom underneath a pre-finished CombatRoom).
	/// </param>
	private async Task EnterRoomInternal(AbstractRoom room, bool isRestoringRoomStackBase = false)
	{
		if (State == null)
		{
			return;
		}
		bool flag = isRestoringRoomStackBase;
		bool flag2 = flag;
		bool flag3;
		if (!flag2)
		{
			if (room is CombatRoom combatRoom)
			{
				if (combatRoom.IsPreFinished)
				{
					goto IL_0072;
				}
			}
			else if (room is EventRoom { IsPreFinished: not false })
			{
				goto IL_0072;
			}
			flag3 = false;
			goto IL_007a;
		}
		goto IL_007d;
		IL_007d:
		bool runExternalEffects = !flag2;
		State.PushRoom(room);
		if (runExternalEffects && !(room is MapRoom))
		{
			await Hook.BeforeRoomEntered(State, room);
		}
		await room.Enter(State, isRestoringRoomStackBase);
		if (runExternalEffects)
		{
			NRunMusicController.Instance?.UpdateTrack();
			if (State.CurrentRoomCount == 1)
			{
				State.Act.MarkRoomVisited(room.RoomType);
			}
		}
		RunLocationTargetedBuffer.OnLocationChanged(State.RunLocation);
		if (!(room is CombatRoom))
		{
			ActionExecutor.Unpause();
		}
		NRunMusicController.Instance?.UpdateAmbience();
		this.RoomEntered?.Invoke();
		return;
		IL_007a:
		flag2 = flag3;
		goto IL_007d;
		IL_0072:
		flag3 = true;
		goto IL_007a;
	}

	/// <summary>
	/// Exit all the rooms that the player is currently in and enter the specified room.
	/// NOTE: If you want to enter a room WITHOUT exiting the current rooms first, call
	/// <see cref="M:MegaCrit.Sts2.Core.Runs.RunManager.EnterRoomWithoutExitingCurrentRoom(MegaCrit.Sts2.Core.Rooms.AbstractRoom,System.Boolean)" /> instead.
	/// </summary>
	/// <param name="room">Room to enter</param>
	public async Task EnterRoom(AbstractRoom room)
	{
		await ExitCurrentRooms();
		await EnterRoomInternal(room);
	}

	/// <summary>
	/// Enter the specified room without exiting any current rooms you may be in.
	/// IMPORTANT NOTE: If you are already in a room and want to exit it first, you should call <see cref="M:MegaCrit.Sts2.Core.Runs.RunManager.EnterRoom(MegaCrit.Sts2.Core.Rooms.AbstractRoom)" />
	/// instead.
	/// If you are already in a room, this will push the new room on top of the "room stack". This is to support effects
	/// where the player should enter a new sub-room, then return to the old one when they're done.
	/// For example, if the player enters an event and selects options that start a fight, they should enter a combat
	/// room. Then, when they're done with the combat, it should be "popped off" the room stack, and they should
	/// transition back to the event.
	/// </summary>
	/// <param name="room">Room that should be entered.</param>
	/// <param name="fadeToBlack">
	/// Whether we should fade to black before entering the room.
	/// Usually true, but false when the room-change is backend only (like when transitioning from a combat-style event
	/// to a real combat).
	/// </param>
	public async Task EnterRoomWithoutExitingCurrentRoom(AbstractRoom room, bool fadeToBlack)
	{
		if (State == null)
		{
			return;
		}
		ActionExecutor.Pause();
		CombatStateSynchronizer.StartSync();
		using (new NetLoadingHandle(NetService))
		{
			if (fadeToBlack)
			{
				if (TestMode.IsOff)
				{
					await NGame.Instance.Transition.RoomFadeOut();
				}
				ClearScreens();
			}
			await CombatStateSynchronizer.WaitForSync();
			State.CurrentMapPointHistoryEntry?.Rooms.Add(new MapPointRoomHistoryEntry
			{
				RoomType = room.RoomType,
				ModelId = room.ModelId
			});
			await EnterRoomInternal(room);
			ActiveScreenContext.Instance.Update();
			if (fadeToBlack)
			{
				await FadeIn();
			}
		}
	}

	public async Task EnterNextAct()
	{
		if (State == null)
		{
			return;
		}
		using (new NetLoadingHandle(NetService))
		{
			if (State.CurrentActIndex >= State.Acts.Count - 1)
			{
				AbstractRoom currentRoom = State.CurrentRoom;
				if (currentRoom != null && currentRoom.IsVictoryRoom)
				{
					await WinRun();
					return;
				}
				if (TestMode.IsOff)
				{
					await NGame.Instance.Transition.RoomFadeOut();
				}
				ClearScreens();
				await EnterRoom(new EventRoom(ModelDb.Event<TheArchitect>()));
				await FadeIn();
			}
			else
			{
				await EnterAct(State.CurrentActIndex + 1);
			}
		}
	}

	private async Task WinRun()
	{
		if (State != null)
		{
			EventRoom eventRoom = (EventRoom)State.CurrentRoom;
			((TheArchitect)eventRoom.LocalMutableEvent).TriggerVictory();
			OnEnded(isVictory: true);
			await GuaranteeKillAllPlayers();
		}
	}

	public async Task EnterAct(int currentActIndex, bool doTransition = true)
	{
		if (State == null)
		{
			return;
		}
		if (TestMode.IsOff)
		{
			await NGame.Instance.Transition.RoomFadeOut();
		}
		using (new NetLoadingHandle(NetService))
		{
			ClearScreens();
			await ExitCurrentRooms();
			await SetActInternal(currentActIndex);
			if (currentActIndex == 0 && State.ExtraFields.StartedWithNeow)
			{
				if (NRun.Instance != null)
				{
					NMapScreen.Instance?.InitMarker(State.Map.StartingMapPoint.coord);
				}
				await EnterMapCoord(State.Map.StartingMapPoint.coord);
				NMapScreen.Instance?.RefreshAllMapPointVotes();
			}
			else
			{
				await EnterRoomInternal(new MapRoom());
				this.ActEntered?.Invoke();
				await FadeIn(doTransition);
			}
			await Hook.AfterActEntered(State);
		}
	}

	/// <summary>
	/// Should only be used in <see cref="T:MegaCrit.Sts2.Core.Runs.RunManager" /> and tests/debugging.
	/// </summary>
	/// <param name="actIndex">The act to set.</param>
	public async Task SetActInternal(int actIndex)
	{
		if (State != null)
		{
			State.CurrentActIndex = actIndex;
			State.ClearVisitedMapCoordsDebug();
			State.Odds.UnknownMapPoint.ResetToBase();
			AfterMapLocationChanged();
			await PreloadManager.LoadActAssets(State.Act);
			await GenerateMap();
			NMapScreen.Instance?.SetTravelEnabled(enabled: false);
			NRunMusicController.Instance?.UpdateMusic();
			UpdateRichPresence();
		}
	}

	/// <summary>
	/// Update rich presence on platform for the current character/act/ascension.
	/// </summary>
	private void UpdateRichPresence()
	{
		if (!TestMode.IsOn && State != null)
		{
			PlatformUtil.SetRichPresence("IN_RUN", NetService.GetRawLobbyIdentifier(), State.Players.Count);
			PlatformUtil.SetRichPresenceValue("Character", LocalContext.GetMe(State).Character.Id.Entry);
			PlatformUtil.SetRichPresenceValue("Act", State.Act.Id.Entry);
			PlatformUtil.SetRichPresenceValue("Ascension", State.AscensionLevel.ToString());
		}
	}

	/// <summary>
	/// This is called from NRewardsScreen when the screen is terminal.
	/// Runs the correct logic for proceeding depending on the state of the run.
	///
	/// A rewards screen is terminal for combat rewards and treasure rooms, where proceeding from it opens up the map so
	/// you can select the next room to travel to.
	///
	/// A rewards screen is non-terminal for things like the Calling Bell relic, which shows a rewards screen wherever
	/// you pick it up from and then lets you continue with whatever was happening in that room.
	/// </summary>
	public async Task ProceedFromTerminalRewardsScreen()
	{
		if (State == null)
		{
			return;
		}
		if (State.CurrentRoomCount > 1)
		{
			if (State.CurrentRoom is CombatRoom { ShouldResumeParentEventAfterCombat: not false })
			{
				await ResumePreviousRoom();
				return;
			}
			NMapScreen.Instance?.SetTravelEnabled(enabled: true);
			NMapScreen.Instance?.Open();
		}
		else
		{
			NMapScreen.Instance?.Open();
		}
	}

	/// <summary>
	/// Exits the current room and resumes the previous room in the stack.
	/// This is usually called if you're in an event combat and you're returning back to the event.
	/// </summary>
	private async Task ResumePreviousRoom()
	{
		if (State != null)
		{
			ClearScreens();
			AbstractRoom abstractRoom = await ExitCurrentRoom();
			if (abstractRoom != null)
			{
				await State.CurrentRoom.Resume(abstractRoom, State);
				NRunMusicController.Instance?.UpdateTrack();
				await FadeIn();
			}
			else
			{
				Log.Error("Current room returned null while exiting.");
			}
		}
	}

	private void AfterMapLocationChanged()
	{
		MapSelectionSynchronizer.OnLocationChanged(State.MapLocation);
		RunLocationTargetedBuffer.OnLocationChanged(State.RunLocation);
	}

	/// <summary>
	/// Abandons the run.
	/// If in multiplayer, you should only call this on the host, and the run abandonment will be synchronized across
	/// all peers. If called on a multiplayer client, an exception will be thrown.
	/// </summary>
	public void Abandon()
	{
		Log.Info("Abandoning an in-progress run (player-initiated)");
		if (NetService.Type == NetGameType.Singleplayer)
		{
			TaskHelper.RunSafely(AbandonInternal());
		}
		else
		{
			RunLobby.AbandonRun();
		}
	}

	void IRunLobbyListener.RunAbandoned()
	{
		Log.Info("The host told us to abandon the run");
		NMapScreen.Instance?.Close(animateOut: false);
		NCapstoneContainer.Instance?.Close();
		TaskHelper.RunSafely(AbandonInternal());
	}

	private async Task AbandonInternal()
	{
		try
		{
			NCapstoneContainer.Instance.Close();
			NMapScreen.Instance.Close(animateOut: false);
		}
		catch (Exception value)
		{
			Log.Error($"Exception thrown while trying to abandon run: {value}");
		}
		IsAbandoned = true;
		await GuaranteeKillAllPlayers();
		if (NetService.Type == NetGameType.Client)
		{
			NErrorPopup nErrorPopup = NErrorPopup.Create(new NetErrorInfo(NetError.HostAbandoned, selfInitiated: false));
			if (nErrorPopup != null)
			{
				NModalContainer.Instance.Add(nErrorPopup);
			}
		}
	}

	/// <summary>
	/// When you Abandon a Run you are sentenced to death.
	/// </summary>
	private async Task GuaranteeKillAllPlayers()
	{
		if (State == null)
		{
			return;
		}
		foreach (Player player in State.Players)
		{
			await CreatureCmd.Kill(player.Creature, force: true);
			await Cmd.CustomScaledWait(0.25f, 0.5f);
		}
	}

	private void StateDiverged(NetFullCombatState state)
	{
		if (NetService.Type != NetGameType.Replay)
		{
			Log.Info("Abandoning run and returning to main menu because our state diverged from host's");
			WriteReplay(stopRecording: false);
		}
	}

	public void WriteReplay(bool stopRecording)
	{
		string profileScopedPath = SaveManager.Instance.GetProfileScopedPath("replays/latest.mcr");
		CombatReplayWriter.WriteReplay(profileScopedPath, stopRecording);
	}

	/// <summary>
	/// Cleans up the run and disconnects us from any multiplayer peers.
	/// </summary>
	/// <param name="graceful">If true, messages are allowed to be sent before closing the multiplayer connection. Pass
	/// false if the game window is being closed.</param>
	public void CleanUp(bool graceful = true)
	{
		if (State == null)
		{
			return;
		}
		ShouldSave = false;
		IsCleaningUp = true;
		try
		{
			_runHistoryWasUploaded = false;
			ActionQueueSet.Reset();
			CardSelectCmd.Reset();
			NAudioManager.Instance?.StopAllLoops();
			NOverlayStack.Instance?.Clear();
			NCapstoneContainer.Instance?.CleanUp();
			NMapScreen.Instance?.CleanUp();
			NModalContainer.Instance?.Clear();
			CombatManager.Instance.Reset(graceful);
			if (CombatReplayWriter.IsRecordingReplay)
			{
				WriteReplay(stopRecording: true);
			}
			ActionExecutor.JustBeforeActionFinishedExecuting -= SendPostActionChecksum;
			CombatReplayWriter.Dispose();
			ActionQueueSynchronizer.Dispose();
			PlayerChoiceSynchronizer.Dispose();
			RewardSynchronizer.Dispose();
			RewardsSetSynchronizer.Dispose();
			RestSiteSynchronizer.Dispose();
			FlavorSynchronizer.Dispose();
			ChecksumTracker.Dispose();
			if (RunLobby != null)
			{
				RunLobby.RemotePlayerDisconnected -= RemotePlayerDisconnected;
				RunLobby.Dispose();
			}
			NetService.Disconnect(NetError.Quit, !graceful);
		}
		finally
		{
			IsCleaningUp = false;
			LocalContext.NetId = null;
			State = null;
		}
	}

	/// <summary>
	/// Called when the run ends.
	/// </summary>
	/// <param name="isVictory">Whether or not the run ended in a victory.</param>
	/// <returns>The serialized version of the run that just ended.</returns>
	public SerializableRun OnEnded(bool isVictory)
	{
		UpdatePlayerStatsInMapPointHistory();
		RunState state = State;
		Player me = LocalContext.GetMe(state);
		if (state.CurrentRoom is CombatRoom combatRoom)
		{
			MapPointRoomHistoryEntry mapPointRoomHistoryEntry = state.CurrentMapPointHistoryEntry.Rooms.Last();
			mapPointRoomHistoryEntry.TurnsTaken = me.PlayerCombatState?.TurnNumber ?? combatRoom.CombatState.RoundNumber;
		}
		SerializableRun serializableRun = ToSave(null);
		SerializablePlayer me2 = LocalContext.GetMe(serializableRun);
		if (_runHistoryWasUploaded)
		{
			return serializableRun;
		}
		_runHistoryWasUploaded = true;
		if (!isVictory && state.CurrentRoom is CombatRoom combatRoom2)
		{
			foreach (var monstersWithSlot in combatRoom2.Encounter.MonstersWithSlots)
			{
				MonsterModel item = monstersWithSlot.Item1;
				CheckUpdateEnemyDiscoveryAfterLoss(me, item.Id);
			}
		}
		if (ShouldSave)
		{
			using (SaveManager.Instance.BeginSaveBatch())
			{
				SaveManager.Instance.UpdateProgressWithRunData(serializableRun, isVictory);
				foreach (string discoveredEpoch in me2.DiscoveredEpochs)
				{
					if (!me.DiscoveredEpochs.Contains(discoveredEpoch))
					{
						me.DiscoveredEpochs.Add(discoveredEpoch);
					}
				}
				AchievementsHelper.AfterRunEnded(state, me, isVictory);
				RunHistoryUtilities.CreateRunHistoryEntry(serializableRun, isVictory, IsAbandoned, NetService.Platform);
				MetricUtilities.UploadRunMetrics(serializableRun, isVictory, NetService.NetId);
				if (SaveManager.Instance.Progress.NumberOfRuns == 5)
				{
					MetricUtilities.UploadSettingsMetric();
				}
				if (NetService.Type == NetGameType.Singleplayer)
				{
					SaveManager.Instance.DeleteCurrentRun();
				}
				else if (NetService.Type == NetGameType.Host)
				{
					SaveManager.Instance.DeleteCurrentMultiplayerRun();
				}
			}
			if (isVictory)
			{
				int score = ScoreUtility.CalculateScore(serializableRun, isVictory);
				StatsManager.IncrementArchitectDamage(score);
			}
		}
		if (DailyTime.HasValue)
		{
			NetGameType type = NetService.Type;
			if ((uint)(type - 1) <= 1u)
			{
				int score2 = ScoreUtility.CalculateDailyScore(serializableRun, me.NetId, isVictory);
				TaskHelper.RunSafely(DailyRunUtility.UploadScore(DailyTime.Value, score2, serializableRun.Players));
			}
			else if (NetService.Type == NetGameType.Client)
			{
				TaskHelper.RunSafely(DailyRunUtility.UploadScore(DailyTime.Value, -999999999, serializableRun.Players));
			}
		}
		return serializableRun;
	}

	/// <summary>
	/// Updates the player's DiscoveredEnemies stat when the player loses to a monster.
	/// This does _not_ update the progress save in any way! It only checks if the player has newly discovered a monster
	/// and adds it to DiscoveredEnemies if so.
	/// </summary>
	private static void CheckUpdateEnemyDiscoveryAfterLoss(Player player, ModelId monster)
	{
		EnemyStats value;
		EnemyStats enemyStats = (SaveManager.Instance.Progress.EnemyStats.TryGetValue(monster, out value) ? value : null);
		if (enemyStats == null)
		{
			player.DiscoveredEnemies.Add(monster);
		}
	}

	private void UpdatePlayerStatsInMapPointHistory()
	{
		if (TestMode.IsOn || State == null)
		{
			return;
		}
		foreach (Player player in State.Players)
		{
			PlayerMapPointHistoryEntry playerMapPointHistoryEntry = State.CurrentMapPointHistoryEntry?.GetEntry(player.NetId);
			if (playerMapPointHistoryEntry != null)
			{
				playerMapPointHistoryEntry.CurrentGold = player.Gold;
				playerMapPointHistoryEntry.CurrentHp = player.Creature.CurrentHp;
				playerMapPointHistoryEntry.MaxHp = player.Creature.MaxHp;
			}
		}
	}

	public bool HasAscension(AscensionLevel level)
	{
		if (!IsInProgress)
		{
			return false;
		}
		return AscensionManager.HasLevel(level);
	}

	public void ApplyAscensionEffects(Player player)
	{
		AscensionManager.ApplyEffectsTo(player);
	}

	public ClientRejoinResponseMessage GetRejoinMessage()
	{
		return new ClientRejoinResponseMessage
		{
			serializableRun = ToSave(null),
			combatState = NetFullCombatState.FromRun(State, null)
		};
	}

	public void LocalPlayerDisconnected(NetErrorInfo info)
	{
		foreach (Player player in State.Players)
		{
			if (!LocalContext.IsMe(player))
			{
				InputSynchronizer.OnPlayerDisconnected(player.NetId);
			}
		}
		if (info.GetReason() != NetError.QuitGameOver && !IsAbandoned && !State.IsGameOver)
		{
			TaskHelper.RunSafely(ReturnToMainMenuWithError(info));
		}
	}

	private void RemotePlayerDisconnected(ulong playerId)
	{
		InputSynchronizer.OnPlayerDisconnected(playerId);
	}

	private async Task ReturnToMainMenuWithError(NetErrorInfo info)
	{
		NCapstoneContainer.Instance?.Close();
		NMapScreen.Instance?.Close(animateOut: false);
		if (TestMode.IsOff)
		{
			await NGame.Instance.ReturnToMainMenuAfterRun();
			NErrorPopup nErrorPopup = NErrorPopup.Create(info);
			if (nErrorPopup != null)
			{
				NModalContainer.Instance.Add(nErrorPopup);
			}
		}
	}

	/// <summary>
	/// Get the energy icon prefix for the local player's character. Null if there's no run in progress.
	/// An unfortunate necessity to make energy icons render properly.
	/// </summary>
	/// <returns></returns>
	public string? GetLocalCharacterEnergyIconPrefix()
	{
		CardPoolModel cardPoolModel = LocalContext.GetMe(State)?.Character.CardPool;
		if (cardPoolModel != null)
		{
			return EnergyIconHelper.GetPrefix(cardPoolModel);
		}
		return null;
	}

	/// <summary>
	/// THIS IS TEMPORARY AND SHOULD ONLY BE USED IN TESTS
	/// </summary>
	public RunState? DebugOnlyGetState()
	{
		return State;
	}
}
