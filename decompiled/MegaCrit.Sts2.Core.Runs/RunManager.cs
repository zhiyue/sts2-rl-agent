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

	public Action? debugAfterCombatRewardsOverride;

	public static RunManager Instance { get; } = new RunManager();

	public AscensionManager AscensionManager { get; private set; }

	public bool ShouldSave { get; private set; }

	public DateTimeOffset? DailyTime { get; private set; }

	public bool IsInProgress => State != null;

	public bool IsCleaningUp { get; private set; }

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

	public RestSiteSynchronizer RestSiteSynchronizer { get; private set; }

	public OneOffSynchronizer OneOffSynchronizer { get; private set; }

	public TreasureRoomRelicSynchronizer TreasureRoomRelicSynchronizer { get; private set; }

	public FlavorSynchronizer FlavorSynchronizer { get; private set; }

	public PeerInputSynchronizer InputSynchronizer { get; private set; }

	public HoveredModelTracker HoveredModelTracker { get; private set; }

	public ActionQueueSet ActionQueueSet { get; private set; }

	public ActionExecutor ActionExecutor { get; private set; }

	public ActionQueueSynchronizer ActionQueueSynchronizer { get; private set; }

	public long WinTime { get; set; }

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

	public bool IsSinglePlayerOrFakeMultiplayer
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

	public Dictionary<int, SerializableActMap>? SavedMapsToLoad { get; set; }

	public bool ShouldIgnoreUnlocks
	{
		get
		{
			if (IsInProgress)
			{
				return !IsSinglePlayerOrFakeMultiplayer;
			}
			return false;
		}
	}

	private RunState? State { get; set; }

	private GameMode GameMode
	{
		get
		{
			RunState? state = State;
			if (state != null && state.Modifiers.Count > 0)
			{
				if (!DailyTime.HasValue)
				{
					return GameMode.Custom;
				}
				return GameMode.Daily;
			}
			return GameMode.Standard;
		}
	}

	public event Action<RunState>? RunStarted;

	public event Action? RoomEntered;

	public event Action? RoomExited;

	public event Action? ActEntered;

	private RunManager()
	{
	}

	public void SetUpNewSinglePlayer(RunState state, bool shouldSave, DateTimeOffset? dailyTime = null)
	{
		if (State != null)
		{
			throw new InvalidOperationException("State is already set.");
		}
		State = state;
		INetGameService netService = new NetSingleplayerGameService();
		InitializeShared(netService, new PeerInputSynchronizer(netService), shouldSave, dailyTime, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 0L, 0L);
		InitializeRunLobby(netService, state);
		InitializeNewRun();
		GenerateRooms();
	}

	public void SetUpNewMultiPlayer(RunState state, StartRunLobby lobby, bool shouldSave, DateTimeOffset? dailyTime = null)
	{
		if (State != null)
		{
			throw new InvalidOperationException("State is already set.");
		}
		State = state;
		InitializeShared(lobby.NetService, lobby.InputSynchronizer, shouldSave, dailyTime, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 0L, 0L);
		InitializeRunLobby(lobby.NetService, state);
		InitializeNewRun();
		GenerateRooms();
	}

	public void SetUpSavedSinglePlayer(RunState state, SerializableRun save)
	{
		if (State != null)
		{
			throw new InvalidOperationException("State is already set.");
		}
		State = state;
		INetGameService netService = new NetSingleplayerGameService();
		InitializeShared(netService, new PeerInputSynchronizer(netService), shouldSave: true, save.DailyTime, save.StartTime, save.RunTime, save.WinTime);
		InitializeRunLobby(netService, state);
		InitializeSavedRun(save);
	}

	public void SetUpSavedMultiPlayer(RunState state, LoadRunLobby lobby)
	{
		if (State != null)
		{
			throw new InvalidOperationException("State is already set.");
		}
		State = state;
		SerializableRun run = lobby.Run;
		InitializeShared(lobby.NetService, lobby.InputSynchronizer, shouldSave: true, run.DailyTime, run.StartTime, run.RunTime, run.WinTime);
		InitializeRunLobby(lobby.NetService, state);
		InitializeSavedRun(run);
	}

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
		InitializeShared(netService, new PeerInputSynchronizer(netService), shouldSave: true, serializableRun.DailyTime, serializableRun.StartTime, serializableRun.RunTime, serializableRun.WinTime);
		InitializeRunLobby(netService, state);
		InitializeSavedRun(serializableRun);
	}

	public void SetUpTest(RunState state, INetGameService gameService, bool disableCombatStateSync = true, bool shouldSave = false)
	{
		if (State != null)
		{
			throw new InvalidOperationException("State is already set.");
		}
		State = state;
		InitializeShared(gameService, new PeerInputSynchronizer(gameService), shouldSave, null, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 0L, 0L);
		InitializeRunLobby(gameService, state);
		CombatStateSynchronizer.IsDisabled = disableCombatStateSync;
		InitializeNewRun();
	}

	private void InitializeShared(INetGameService netService, PeerInputSynchronizer inputSynchronizer, bool shouldSave, DateTimeOffset? dailyTime, long startTime, long runTime, long winTime)
	{
		if (State == null)
		{
			throw new InvalidOperationException("State is not set.");
		}
		NetService = netService;
		ulong netId = NetService.NetId;
		ChecksumTracker = new ChecksumTracker(NetService, State);
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
		RestSiteSynchronizer = new RestSiteSynchronizer(RunLocationTargetedBuffer, NetService, State, netId);
		OneOffSynchronizer = new OneOffSynchronizer(RunLocationTargetedBuffer, NetService, State, netId);
		TreasureRoomRelicSynchronizer = new TreasureRoomRelicSynchronizer(State, netId, ActionQueueSynchronizer, State.SharedRelicGrabBag, State.Rng.TreasureRoomRelics);
		CombatReplayWriter = new CombatReplayWriter(PlayerChoiceSynchronizer, ActionQueueSet, ActionQueueSynchronizer, ChecksumTracker);
		CombatReplayWriter.IsEnabled = !TestMode.IsOn;
		ActionExecutor.AfterActionExecuted += SendPostActionChecksum;
		ChecksumTracker.StateDiverged += StateDiverged;
		ActionExecutor.Pause();
		IsAbandoned = false;
		AscensionManager = new AscensionManager(State.AscensionLevel);
		ShouldSave = shouldSave;
		DailyTime = dailyTime;
		_startTime = startTime;
		_prevRunTime = runTime;
		WinTime = winTime;
		_sessionStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		InputSynchronizer = inputSynchronizer;
		HoveredModelTracker = new HoveredModelTracker(InputSynchronizer, State);
	}

	public void InitializeRunLobby(INetGameService netService, RunState state)
	{
		if (netService.Type.IsMultiplayer())
		{
			RunLobby = new RunLobby(GameMode, netService, this, state, state.Players.Select((Player p) => p.NetId));
			RunLobby.RemotePlayerDisconnected += RemotePlayerDisconnected;
		}
		CombatStateSynchronizer = new CombatStateSynchronizer(NetService, RunLobby, state);
	}

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

	private void InitializeSavedRun(SerializableRun save)
	{
		foreach (ActModel act in State.Acts)
		{
			act.ValidateRoomsAfterLoad(State.Rng.UpFront);
		}
		AfterLocationChanged();
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
			ChecksumTracker.GenerateChecksum($"after executing action {action}", action);
		}
	}

	private void SetStartedWithNeowFlag()
	{
		State.ExtraFields.StartedWithNeow = State.UnlockState.IsEpochRevealed<NeowEpoch>();
	}

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
			Acts = runState.Acts.Select((ActModel a) => a.ToSave()).ToList(),
			Modifiers = runState.Modifiers.Select((ModifierModel m) => m.ToSerializable()).ToList(),
			DailyTime = save.DailyTime,
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
			ExtraFields = runState.ExtraFields.ToSerializable(),
			PreFinishedRoom = save.PreFinishedRoom
		};
		PacketWriter packetWriter = new PacketWriter();
		packetWriter.Write(serializableRun);
		return serializableRun;
	}

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
		this.RunStarted?.Invoke(State);
		UpdateRichPresence();
		return State;
	}

	public async Task FinalizeStartingRelics()
	{
		foreach (Player player in State.Players)
		{
			foreach (RelicModel relic in player.Relics)
			{
				await relic.AfterObtained();
			}
		}
	}

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
		if (GameMode != GameMode.Standard)
		{
			return false;
		}
		return true;
	}

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
			map = Hook.ModifyGeneratedMap(State, map, State.CurrentActIndex);
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
		NMapScreen.Instance?.SetMap(map, State.Rng.Seed, clearDrawings: true);
	}

	public Task EnterMapCoord(MapCoord coord)
	{
		if (!State.AddVisitedMapCoord(coord))
		{
			return Task.CompletedTask;
		}
		return EnterMapCoordInternal(coord, null, saveGame: true);
	}

	public async Task LoadIntoLatestMapCoord(AbstractRoom? preFinishedRoom)
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

	private Task EnterMapCoordInternal(MapCoord coord, AbstractRoom? preFinishedRoom, bool saveGame)
	{
		MapPoint point = State.Map.GetPoint(coord);
		return EnterMapPointInternal(coord.row + 1, point.PointType, coord, preFinishedRoom, saveGame);
	}

	public async Task EnterMapPointInternal(int actFloor, MapPointType pointType, MapCoord? coord, AbstractRoom? preFinishedRoom, bool saveGame)
	{
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
			AfterLocationChanged();
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

	private async Task FadeIn(bool showTransition = true)
	{
		if (!TestMode.IsOn)
		{
			await NGame.Instance.Transition.RoomFadeIn(showTransition);
		}
	}

	private void ClearScreens()
	{
		if (!TestMode.IsOn)
		{
			NOverlayStack.Instance.Clear();
			NCapstoneContainer.Instance.Close();
			NMapScreen.Instance.Close(animateOut: false);
		}
	}

	public async Task EnterMapCoordDebug(MapCoord coord, RoomType roomType, MapPointType pointType = MapPointType.Unassigned, AbstractModel? model = null, bool showTransition = true)
	{
		State.AddVisitedMapCoord(coord);
		await EnterRoomDebug(roomType, pointType, model, showTransition);
	}

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
			if (State.Map is MockActMap mockActMap)
			{
				mockActMap.MockCurrentMapPointType(pointType);
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
		while (State.CurrentRoomCount > 0)
		{
			await ExitCurrentRoom();
		}
		NRun.Instance?.GlobalUi.TopBar.RoomIcon.DebugClearMapPointTypeOverride();
	}

	private async Task<AbstractRoom> ExitCurrentRoom()
	{
		AbstractRoom currentRoom = State.PopCurrentRoom();
		await currentRoom.Exit(State);
		this.RoomExited?.Invoke();
		return currentRoom;
	}

	private async Task EnterRoomInternal(AbstractRoom room, bool isRestoringRoomStackBase = false)
	{
		bool flag = isRestoringRoomStackBase;
		bool flag2 = flag;
		bool flag3;
		if (!flag2)
		{
			if (room is CombatRoom combatRoom)
			{
				if (combatRoom.IsPreFinished)
				{
					goto IL_0065;
				}
			}
			else if (room is EventRoom { IsPreFinished: not false })
			{
				goto IL_0065;
			}
			flag3 = false;
			goto IL_006d;
		}
		goto IL_0070;
		IL_006d:
		flag2 = flag3;
		goto IL_0070;
		IL_0065:
		flag3 = true;
		goto IL_006d;
		IL_0070:
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
		if (!(room is CombatRoom))
		{
			ActionExecutor.Unpause();
		}
		NRunMusicController.Instance?.UpdateAmbience();
		this.RoomEntered?.Invoke();
	}

	public async Task EnterRoom(AbstractRoom room)
	{
		await ExitCurrentRooms();
		await EnterRoomInternal(room);
	}

	public async Task EnterRoomWithoutExitingCurrentRoom(AbstractRoom room, bool fadeToBlack)
	{
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
			State.CurrentMapPointHistoryEntry.Rooms.Add(new MapPointRoomHistoryEntry
			{
				RoomType = room.RoomType,
				ModelId = room.ModelId
			});
			await EnterRoomInternal(room);
			if (fadeToBlack)
			{
				await FadeIn();
			}
		}
	}

	public async Task EnterNextAct()
	{
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
		EventRoom eventRoom = (EventRoom)State.CurrentRoom;
		((TheArchitect)eventRoom.LocalMutableEvent).TriggerVictory();
		OnEnded(isVictory: true);
		await GuaranteeKillAllPlayers();
	}

	public async Task EnterAct(int currentActIndex, bool doTransition = true)
	{
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

	public async Task SetActInternal(int actIndex)
	{
		State.CurrentActIndex = actIndex;
		State.ClearVisitedMapCoordsDebug();
		State.Odds.UnknownMapPoint.ResetToBase();
		AfterLocationChanged();
		await PreloadManager.LoadActAssets(State.Act);
		await GenerateMap();
		NMapScreen.Instance?.SetTravelEnabled(enabled: false);
		NRunMusicController.Instance?.UpdateMusic();
		UpdateRichPresence();
	}

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

	public async Task ProceedFromTerminalRewardsScreen()
	{
		if (State.CurrentRoomCount > 1)
		{
			if (State.CurrentRoom is CombatRoom { ShouldResumeParentEventAfterCombat: not false })
			{
				await ResumePreviousRoom();
				return;
			}
			await ExitCurrentRoom();
			NMapScreen.Instance?.SetTravelEnabled(enabled: true);
			NMapScreen.Instance?.Open();
		}
		else
		{
			NMapScreen.Instance?.Open();
		}
	}

	private async Task ResumePreviousRoom()
	{
		ClearScreens();
		AbstractRoom exitedRoom = await ExitCurrentRoom();
		await State.CurrentRoom.Resume(exitedRoom, State);
		NRunMusicController.Instance?.UpdateTrack();
		await FadeIn();
	}

	private void AfterLocationChanged()
	{
		MapSelectionSynchronizer.OnRunLocationChanged(State.CurrentLocation);
		RunLocationTargetedBuffer.OnRunLocationChanged(State.CurrentLocation);
	}

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
			ActionQueueSet.Reset();
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

	private async Task GuaranteeKillAllPlayers()
	{
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
			WriteReplay(stopRecording: true);
		}
	}

	public void WriteReplay(bool stopRecording)
	{
		string profileScopedPath = SaveManager.Instance.GetProfileScopedPath("replays/latest.mcr");
		CombatReplayWriter.WriteReplay(profileScopedPath, stopRecording);
	}

	public void CleanUp(bool graceful = true)
	{
		if (State == null)
		{
			return;
		}
		IsCleaningUp = true;
		try
		{
			_runHistoryWasUploaded = false;
			NAudioManager.Instance?.StopAllLoops();
			NOverlayStack.Instance?.Clear();
			NCapstoneContainer.Instance?.CleanUp();
			NMapScreen.Instance?.CleanUp();
			NModalContainer.Instance?.Clear();
			if (graceful)
			{
				CombatManager.Instance.Reset();
			}
			CombatReplayWriter.Dispose();
			ActionQueueSynchronizer.Dispose();
			PlayerChoiceSynchronizer.Dispose();
			RewardSynchronizer.Dispose();
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

	public SerializableRun OnEnded(bool isVictory)
	{
		UpdatePlayerStatsInMapPointHistory();
		RunState state = State;
		Player me = LocalContext.GetMe(state);
		if (state.CurrentRoom is CombatRoom combatRoom)
		{
			state.CurrentMapPointHistoryEntry.Rooms.Last().TurnsTaken = combatRoom.CombatState.RoundNumber;
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
				int score2 = ScoreUtility.CalculateScore(serializableRun, isVictory);
				TaskHelper.RunSafely(DailyRunUtility.UploadScore(DailyTime.Value, score2, serializableRun.Players));
			}
			else if (NetService.Type == NetGameType.Client)
			{
				TaskHelper.RunSafely(DailyRunUtility.UploadScore(DailyTime.Value, -99999, serializableRun.Players));
			}
		}
		return serializableRun;
	}

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
		if (TestMode.IsOn)
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
		ActionQueueSet.Reset();
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

	public string? GetLocalCharacterEnergyIconPrefix()
	{
		CardPoolModel cardPoolModel = LocalContext.GetMe(State)?.Character.CardPool;
		if (cardPoolModel != null)
		{
			return EnergyIconHelper.GetPrefix(cardPoolModel);
		}
		return null;
	}

	public RunState? DebugOnlyGetState()
	{
		return State;
	}
}
