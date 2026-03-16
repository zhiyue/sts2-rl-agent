using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Replay;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Platform.Steam;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Nodes.Debug.Multiplayer;

[ScriptPath("res://src/Core/Nodes/Debug/Multiplayer/NMultiplayerTest.cs")]
public class NMultiplayerTest : Control, IStartRunLobbyListener
{
	private struct CharacterContainer
	{
		public TextureRect characterImage;

		public Label playerName;
	}

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName AddGame = "AddGame";

		public static readonly StringName HostButtonPressed = "HostButtonPressed";

		public static readonly StringName SteamHostButtonPressed = "SteamHostButtonPressed";

		public static readonly StringName JoinButtonPressed = "JoinButtonPressed";

		public static readonly StringName ReadyButtonPressed = "ReadyButtonPressed";

		public new static readonly StringName Disconnect = "Disconnect";

		public static readonly StringName AfterMultiplayerStarted = "AfterMultiplayerStarted";

		public static readonly StringName ChooseReplayToLoad = "ChooseReplayToLoad";

		public static readonly StringName LoadReplay = "LoadReplay";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName AscensionChanged = "AscensionChanged";

		public static readonly StringName SeedChanged = "SeedChanged";

		public static readonly StringName ModifiersChanged = "ModifiersChanged";

		public static readonly StringName MaxAscensionChanged = "MaxAscensionChanged";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _ipField = "_ipField";

		public static readonly StringName _idField = "_idField";

		public static readonly StringName _readyButton = "_readyButton";

		public static readonly StringName _readyIndicator = "_readyIndicator";

		public static readonly StringName _loadingPanel = "_loadingPanel";

		public static readonly StringName _characterPaginator = "_characterPaginator";

		public static readonly StringName _game = "_game";

		public static readonly StringName _ignoreReplayModelIdHash = "_ignoreReplayModelIdHash";

		public static readonly StringName _beginningRun = "_beginningRun";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const ushort _port = 33771;

	private TextEdit _ipField;

	private TextEdit _idField;

	private Button _readyButton;

	private Control _readyIndicator;

	private Control _loadingPanel;

	private NMultiplayerTestCharacterPaginator _characterPaginator;

	private readonly List<CharacterContainer> _characterContainers = new List<CharacterContainer>();

	private NGame _game;

	private StartRunLobby? _lobby;

	private readonly SerializablePlayer _localPlayerData = new SerializablePlayer();

	private IBootstrapSettings? _settings;

	private bool _ignoreReplayModelIdHash;

	private bool _beginningRun;

	public override void _Ready()
	{
		_ipField = GetNode<TextEdit>("IpField");
		_idField = GetNode<TextEdit>("NameField");
		_characterPaginator = GetNode<NMultiplayerTestCharacterPaginator>("CharacterChooser");
		Button node = GetNode<Button>("HostButton");
		Button node2 = GetNode<Button>("SteamHostButton");
		Button node3 = GetNode<Button>("JoinButton");
		_readyButton = GetNode<Button>("ReadyButton");
		_readyIndicator = GetNode<Control>("ReadyButton/ReadyIndicator");
		Button node4 = GetNode<Button>("ReplayButton");
		Button node5 = GetNode<Button>("DeleteCloudSavesButton");
		_loadingPanel = GetNode<Control>("LoadingPanel");
		Control node6 = GetNode<Control>("Characters");
		foreach (Node child in node6.GetChildren())
		{
			_characterContainers.Add(new CharacterContainer
			{
				characterImage = child.GetNode<TextureRect>("Image"),
				playerName = child.GetNode<Label>("Name")
			});
		}
		node.Connect(BaseButton.SignalName.ButtonUp, Callable.From(HostButtonPressed));
		node2.Connect(BaseButton.SignalName.ButtonUp, Callable.From(SteamHostButtonPressed));
		node3.Connect(BaseButton.SignalName.ButtonUp, Callable.From(JoinButtonPressed));
		_readyButton.Connect(BaseButton.SignalName.ButtonUp, Callable.From(ReadyButtonPressed));
		node4.Connect(BaseButton.SignalName.ButtonUp, Callable.From(ChooseReplayToLoad));
		node5.Connect(BaseButton.SignalName.ButtonUp, Callable.From(CloudConsoleCmd.DeleteCloudSaves));
		_characterPaginator.Visible = false;
		_characterPaginator.CharacterChanged += OnCharacterChanged;
		_readyButton.Visible = false;
		_game = GetTree().Root.GetNodeOrNull<NGame>("Game");
		if (_game == null)
		{
			_game = SceneHelper.Instantiate<NGame>("game");
			_game.StartOnMainMenu = false;
			Callable.From(AddGame).CallDeferred();
		}
		Type type = BootstrapSettingsUtil.Get();
		if (type != null)
		{
			_settings = (IBootstrapSettings)Activator.CreateInstance(type);
			if (!_settings.BootstrapInMultiplayer)
			{
				_settings = null;
			}
			else
			{
				PreloadManager.Enabled = _settings.DoPreloading;
				_game.DebugSeedOverride = _settings.Seed;
			}
		}
		if (!SteamInitializer.Initialized)
		{
			SteamInitializer.Initialize(_game);
		}
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Network] = LogLevel.Debug;
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Actions] = LogLevel.VeryDebug;
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.GameSync] = LogLevel.VeryDebug;
	}

	public override void _ExitTree()
	{
		_characterPaginator.CharacterChanged -= OnCharacterChanged;
		if (!_beginningRun)
		{
			_lobby?.CleanUp(disconnectSession: true);
		}
	}

	private void AddGame()
	{
		Node root = GetTree().Root;
		root.AddChildSafely(_game);
		_game.RootSceneContainer.SetCurrentScene(this);
		TaskHelper.RunSafely(_game.Transition.FadeIn());
	}

	private void HostButtonPressed()
	{
		TaskHelper.RunSafely(StartHost(steam: false));
	}

	private void SteamHostButtonPressed()
	{
		TaskHelper.RunSafely(StartHost(steam: true));
	}

	private void JoinButtonPressed()
	{
		ulong netId = ((_idField.Text != string.Empty) ? ulong.Parse(_idField.Text) : 1000);
		string ip = ((_ipField.Text != string.Empty) ? _ipField.Text : "127.0.0.1");
		ENetClientConnectionInitializer initializer = new ENetClientConnectionInitializer(netId, ip, 33771);
		TaskHelper.RunSafely(JoinToHost(initializer));
	}

	private void ReadyButtonPressed()
	{
		_localPlayerData.Deck = _characterPaginator.Character.StartingDeck.Select((CardModel c) => c.ToMutable().ToSerializable()).ToList();
		_localPlayerData.Relics = _characterPaginator.Character.StartingRelics.Select((RelicModel r) => r.ToMutable().ToSerializable()).ToList();
		_localPlayerData.Potions = new List<SerializablePotion>();
		_localPlayerData.Rng = new SerializablePlayerRngSet();
		_localPlayerData.Odds = new SerializablePlayerOddsSet();
		_localPlayerData.RelicGrabBag = new SerializableRelicGrabBag();
		_localPlayerData.ExtraFields = new SerializableExtraPlayerFields();
		_localPlayerData.UnlockState = new SerializableUnlockState();
		_lobby.SetReady(ready: true);
		_readyIndicator.Visible = true;
	}

	public void BeginRun(string seed, List<ActModel> acts, IReadOnlyList<ModifierModel> __)
	{
		_loadingPanel.Visible = true;
		TaskHelper.RunSafely(BeginRunAsyncWrapper(seed, acts));
	}

	private async Task BeginRunAsyncWrapper(string seed, List<ActModel> acts)
	{
		try
		{
			await BeginRunAsync(seed, acts);
		}
		finally
		{
			if (_loadingPanel.IsValid())
			{
				_loadingPanel.Visible = false;
			}
		}
	}

	private async Task BeginRunAsync(string seed, List<ActModel> acts)
	{
		_beginningRun = true;
		IBootstrapSettings settings = _settings;
		if (settings != null && settings.BootstrapInMultiplayer)
		{
			using (new NetLoadingHandle(_lobby.NetService))
			{
				acts[0] = _settings.Act;
				RunState runState = RunState.CreateForNewRun(_lobby.Players.Select((LobbyPlayer p) => Player.CreateForNewRun(p.character, UnlockState.FromSerializable(p.unlockState), p.id)).ToList(), acts.Select((ActModel a) => a.ToMutable()).ToList(), _settings.Modifiers, _lobby.Ascension, seed);
				RunManager.Instance.SetUpNewMultiPlayer(runState, _lobby, _settings.SaveRunHistory);
				await PreloadManager.LoadRunAssets(runState.Players.Select((Player p) => p.Character));
				await RunManager.Instance.FinalizeStartingRelics();
				RunManager.Instance.Launch();
				_game.RootSceneContainer.SetCurrentScene(NRun.Create(runState));
				await RunManager.Instance.SetActInternal(0);
				await SaveManager.Instance.SaveRun(null);
				_lobby.CleanUp(disconnectSession: false);
				await _settings.Setup(LocalContext.GetMe(runState));
				switch (_settings.RoomType)
				{
				case RoomType.Unassigned:
					await RunManager.Instance.EnterAct(0);
					break;
				case RoomType.Treasure:
				case RoomType.Shop:
				case RoomType.RestSite:
					await RunManager.Instance.EnterRoomDebug(_settings.RoomType, MapPointType.Unassigned, null, showTransition: false);
					RunManager.Instance.ActionExecutor.Unpause();
					break;
				case RoomType.Event:
					await RunManager.Instance.EnterRoomDebug(_settings.RoomType, MapPointType.Unassigned, _settings.Event, showTransition: false);
					break;
				default:
					await RunManager.Instance.EnterRoomDebug(_settings.RoomType, MapPointType.Unassigned, _settings.RoomType.IsCombatRoom() ? _settings.Encounter.ToMutable() : null, showTransition: false);
					break;
				}
			}
		}
		else
		{
			await _game.StartNewMultiplayerRun(_lobby, shouldSave: true, acts, Array.Empty<ModifierModel>(), seed, _lobby.Ascension);
			_lobby.CleanUp(disconnectSession: false);
		}
	}

	private void Disconnect(NetError reason)
	{
		_lobby?.NetService.Disconnect(reason);
		_lobby?.CleanUp(disconnectSession: true);
		_lobby = null;
	}

	private async Task<bool> StartHost(bool steam)
	{
		Disconnect(NetError.Quit);
		NetHostGameService netService = new NetHostGameService();
		NetErrorInfo? value = ((!steam) ? netService.StartENetHost(33771, 4) : (await netService.StartSteamHost(4)));
		if (!value.HasValue)
		{
			_lobby = new StartRunLobby(GameMode.Standard, netService, this, 4);
			_lobby.AddLocalHostPlayer(new UnlockState(SaveManager.Instance.Progress), SaveManager.Instance.Progress.MaxMultiplayerAscension);
			AfterMultiplayerStarted();
			Log.Info("Successful host");
		}
		else
		{
			_lobby = null;
			Log.Info($"Failed host: {value}");
		}
		return !value.HasValue;
	}

	public async Task JoinToHost(IClientConnectionInitializer initializer)
	{
		Disconnect(NetError.Quit);
		JoinFlow joinFlow = new JoinFlow();
		try
		{
			JoinResult joinResult = await joinFlow.Begin(initializer, GetTree());
			if (joinResult.sessionState == RunSessionState.InLobby)
			{
				Log.Info("Successfully joined lobby");
				_lobby = new StartRunLobby(joinResult.gameMode, joinFlow.NetService, this, -1);
				_lobby.InitializeFromMessage(joinResult.joinResponse.Value);
				AfterMultiplayerStarted();
			}
			else if (joinResult.sessionState == RunSessionState.Running)
			{
				Log.Info("Successfully joined run in-progress. Initializing run");
				throw new NotImplementedException("Run re-joining has yet to be implemented!");
			}
		}
		catch (Exception value)
		{
			joinFlow.NetService.Disconnect(NetError.RunInProgress);
			_lobby = null;
			Log.Info($"Failed join: {value}");
		}
	}

	private void AfterMultiplayerStarted()
	{
		foreach (LobbyPlayer player in _lobby.Players)
		{
			_characterContainers[player.slotId].characterImage.Texture = player.character.IconTexture;
			_characterContainers[player.slotId].playerName.Text = PlatformUtil.GetPlayerName(_lobby.NetService.Platform, player.id);
		}
		_readyButton.Visible = true;
		_characterPaginator.Visible = true;
		_game.RemoteCursorContainer.Initialize(_lobby.InputSynchronizer, _lobby.Players.Select((LobbyPlayer p) => p.id));
		_game.ReactionContainer.InitializeNetworking(_lobby.NetService);
		OnCharacterChanged(_lobby.LocalPlayer.character);
	}

	private void ChooseReplayToLoad()
	{
		FileDialog fileDialog = new FileDialog();
		fileDialog.Filters = new string[1] { "*.mcr" };
		fileDialog.UseNativeDialog = true;
		fileDialog.Title = "Choose Replay";
		fileDialog.Access = FileDialog.AccessEnum.Filesystem;
		fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
		fileDialog.Connect(FileDialog.SignalName.FileSelected, Callable.From<string>(LoadReplay));
		fileDialog.Show();
	}

	private void LoadReplay(string path)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (FileAccessStream fileAccessStream = new FileAccessStream(path, Godot.FileAccess.ModeFlags.Read))
		{
			fileAccessStream.CopyTo(memoryStream);
		}
		PacketReader packetReader = new PacketReader();
		packetReader.Reset(memoryStream.ToArray());
		CombatReplay replay = packetReader.Read<CombatReplay>();
		TaskHelper.RunSafely(RunReplay(replay));
	}

	private async Task RunReplay(CombatReplay replay)
	{
		Log.Info($"Loaded replay. Game version: {replay.version} Commit: {replay.gitCommit} Model ID hash: {replay.modelIdHash}");
		if (replay.modelIdHash != ModelIdSerializationCache.Hash)
		{
			if (!_ignoreReplayModelIdHash)
			{
				Log.Error($"Attempting to load replay with Model ID hash {replay.modelIdHash} that does not match ours ({ModelIdSerializationCache.Hash})! The replay will mismatch. If you want to continue anyway, try running the replay again.");
				_ignoreReplayModelIdHash = true;
				return;
			}
			Log.Warn("Ignoring model ID hash mismatch in replay.");
		}
		string text = ReleaseInfoManager.Instance.ReleaseInfo?.Commit ?? GitHelper.ShortCommitId;
		if (replay.gitCommit != text)
		{
			Log.Warn($"Git commit in replay {replay.gitCommit} does not match ours ({text}). The replay has a chance of mismatching!");
		}
		RunState runState = RunState.FromSerializable(replay.serializableRun);
		RunManager.Instance.SetUpReplay(runState, replay);
		RunManager.Instance.CombatStateSynchronizer.IsDisabled = true;
		await PreloadManager.LoadRunAssets(runState.Players.Select((Player p) => p.Character));
		await PreloadManager.LoadActAssets(runState.Act);
		RunManager.Instance.Launch();
		NAudioManager.Instance?.StopMusic();
		NGame.Instance.RootSceneContainer.SetCurrentScene(NRun.Create(runState));
		await RunManager.Instance.GenerateMap();
		RunManager.Instance.ActionQueueSet.FastForwardNextActionId(replay.nextActionId);
		RunManager.Instance.ActionQueueSynchronizer.FastForwardHookId(replay.nextHookId);
		RunManager.Instance.ChecksumTracker.LoadReplayChecksums(replay.checksumData, replay.nextChecksumId);
		RunManager.Instance.PlayerChoiceSynchronizer.FastForwardChoiceIds(replay.choiceIds);
		await RunManager.Instance.LoadIntoLatestMapCoord(AbstractRoom.FromSerializable(replay.serializableRun.PreFinishedRoom, runState));
		while (RunManager.Instance.ActionExecutor.IsPaused)
		{
			await Engine.GetMainLoop().ToSignal(Engine.GetMainLoop(), SceneTree.SignalName.ProcessFrame);
		}
		foreach (CombatReplayEvent replayEvent in replay.events)
		{
			switch (replayEvent.eventType)
			{
			case CombatReplayEventType.GameAction:
			{
				while (CombatManager.Instance.EndingPlayerTurnPhaseOne || CombatManager.Instance.EndingPlayerTurnPhaseTwo)
				{
					await new SignalAwaiter(Engine.GetMainLoop(), SceneTree.SignalName.ProcessFrame, Engine.GetMainLoop());
				}
				Player player2 = runState.GetPlayer(replayEvent.playerId.Value);
				GameAction action = replayEvent.action.ToGameAction(player2);
				if (action.ActionType == GameActionType.CombatPlayPhaseOnly)
				{
					while (CombatManager.Instance.DebugOnlyGetState().CurrentSide == CombatSide.Enemy)
					{
						await new SignalAwaiter(Engine.GetMainLoop(), SceneTree.SignalName.ProcessFrame, Engine.GetMainLoop());
					}
				}
				RunManager.Instance.ActionQueueSet.EnqueueWithoutSynchronizing(action);
				if ((action is EndPlayerTurnAction || action is ReadyToBeginEnemyTurnAction) ? true : false)
				{
					await RunManager.Instance.ActionExecutor.FinishedExecutingActions();
				}
				break;
			}
			case CombatReplayEventType.HookAction:
			{
				uint value = replayEvent.hookId.Value;
				GameActionType value2 = replayEvent.gameActionType.Value;
				RunManager.Instance.ActionQueueSet.EnqueueWithoutSynchronizing(RunManager.Instance.ActionQueueSynchronizer.GetHookActionForId(value, replayEvent.playerId.Value, value2));
				break;
			}
			case CombatReplayEventType.ResumeAction:
				RunManager.Instance.ActionQueueSet.ResumeActionWithoutSynchronizing(replayEvent.actionId.Value);
				break;
			case CombatReplayEventType.PlayerChoice:
			{
				Player player = runState.GetPlayer(replayEvent.playerId.Value);
				RunManager.Instance.PlayerChoiceSynchronizer.ReceiveReplayChoice(player, replayEvent.choiceId.Value, replayEvent.playerChoiceResult.Value);
				break;
			}
			default:
				throw new InvalidEnumArgumentException();
			}
		}
	}

	private void OnCharacterChanged(CharacterModel model)
	{
		_lobby.SetLocalCharacter(model);
		_localPlayerData.CharacterId = model.Id;
		_localPlayerData.CurrentHp = model.StartingHp;
		_localPlayerData.MaxHp = model.StartingHp;
		_localPlayerData.MaxEnergy = model.MaxEnergy;
		_localPlayerData.MaxPotionSlotCount = 3;
		_localPlayerData.Gold = model.StartingGold;
	}

	public override void _Process(double delta)
	{
		_lobby?.NetService.Update();
	}

	public void PlayerConnected(LobbyPlayer player)
	{
		_characterContainers[player.slotId].characterImage.Texture = player.character.IconTexture;
		_characterContainers[player.slotId].playerName.Text = PlatformUtil.GetPlayerName(_lobby.NetService.Platform, player.id);
	}

	public void PlayerChanged(LobbyPlayer player)
	{
		_characterContainers[player.slotId].characterImage.Texture = player.character.IconTexture;
		_characterContainers[player.slotId].playerName.Text = PlatformUtil.GetPlayerName(_lobby.NetService.Platform, player.id);
	}

	public void AscensionChanged()
	{
	}

	public void SeedChanged()
	{
	}

	public void ModifiersChanged()
	{
	}

	public void MaxAscensionChanged()
	{
	}

	public void RemotePlayerDisconnected(LobbyPlayer player)
	{
		_characterContainers[player.slotId].characterImage.Texture = null;
		_characterContainers[player.slotId].playerName.Text = "?";
	}

	public void LocalPlayerDisconnected(NetErrorInfo info)
	{
		_lobby = null;
		_characterPaginator.Visible = false;
		_readyButton.Visible = false;
		_characterPaginator.SetIndex(0);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(16);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AddGame, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HostButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SteamHostButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.JoinButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ReadyButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Disconnect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "reason", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AfterMultiplayerStarted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ChooseReplayToLoad, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.LoadReplay, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AscensionChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SeedChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ModifiersChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.MaxAscensionChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddGame && args.Count == 0)
		{
			AddGame();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HostButtonPressed && args.Count == 0)
		{
			HostButtonPressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SteamHostButtonPressed && args.Count == 0)
		{
			SteamHostButtonPressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.JoinButtonPressed && args.Count == 0)
		{
			JoinButtonPressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReadyButtonPressed && args.Count == 0)
		{
			ReadyButtonPressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Disconnect && args.Count == 1)
		{
			Disconnect(VariantUtils.ConvertTo<NetError>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterMultiplayerStarted && args.Count == 0)
		{
			AfterMultiplayerStarted();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ChooseReplayToLoad && args.Count == 0)
		{
			ChooseReplayToLoad();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LoadReplay && args.Count == 1)
		{
			LoadReplay(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AscensionChanged && args.Count == 0)
		{
			AscensionChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SeedChanged && args.Count == 0)
		{
			SeedChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ModifiersChanged && args.Count == 0)
		{
			ModifiersChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MaxAscensionChanged && args.Count == 0)
		{
			MaxAscensionChanged();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.AddGame)
		{
			return true;
		}
		if (method == MethodName.HostButtonPressed)
		{
			return true;
		}
		if (method == MethodName.SteamHostButtonPressed)
		{
			return true;
		}
		if (method == MethodName.JoinButtonPressed)
		{
			return true;
		}
		if (method == MethodName.ReadyButtonPressed)
		{
			return true;
		}
		if (method == MethodName.Disconnect)
		{
			return true;
		}
		if (method == MethodName.AfterMultiplayerStarted)
		{
			return true;
		}
		if (method == MethodName.ChooseReplayToLoad)
		{
			return true;
		}
		if (method == MethodName.LoadReplay)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.AscensionChanged)
		{
			return true;
		}
		if (method == MethodName.SeedChanged)
		{
			return true;
		}
		if (method == MethodName.ModifiersChanged)
		{
			return true;
		}
		if (method == MethodName.MaxAscensionChanged)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._ipField)
		{
			_ipField = VariantUtils.ConvertTo<TextEdit>(in value);
			return true;
		}
		if (name == PropertyName._idField)
		{
			_idField = VariantUtils.ConvertTo<TextEdit>(in value);
			return true;
		}
		if (name == PropertyName._readyButton)
		{
			_readyButton = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._readyIndicator)
		{
			_readyIndicator = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._loadingPanel)
		{
			_loadingPanel = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._characterPaginator)
		{
			_characterPaginator = VariantUtils.ConvertTo<NMultiplayerTestCharacterPaginator>(in value);
			return true;
		}
		if (name == PropertyName._game)
		{
			_game = VariantUtils.ConvertTo<NGame>(in value);
			return true;
		}
		if (name == PropertyName._ignoreReplayModelIdHash)
		{
			_ignoreReplayModelIdHash = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._beginningRun)
		{
			_beginningRun = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._ipField)
		{
			value = VariantUtils.CreateFrom(in _ipField);
			return true;
		}
		if (name == PropertyName._idField)
		{
			value = VariantUtils.CreateFrom(in _idField);
			return true;
		}
		if (name == PropertyName._readyButton)
		{
			value = VariantUtils.CreateFrom(in _readyButton);
			return true;
		}
		if (name == PropertyName._readyIndicator)
		{
			value = VariantUtils.CreateFrom(in _readyIndicator);
			return true;
		}
		if (name == PropertyName._loadingPanel)
		{
			value = VariantUtils.CreateFrom(in _loadingPanel);
			return true;
		}
		if (name == PropertyName._characterPaginator)
		{
			value = VariantUtils.CreateFrom(in _characterPaginator);
			return true;
		}
		if (name == PropertyName._game)
		{
			value = VariantUtils.CreateFrom(in _game);
			return true;
		}
		if (name == PropertyName._ignoreReplayModelIdHash)
		{
			value = VariantUtils.CreateFrom(in _ignoreReplayModelIdHash);
			return true;
		}
		if (name == PropertyName._beginningRun)
		{
			value = VariantUtils.CreateFrom(in _beginningRun);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ipField, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._idField, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._readyButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._readyIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._loadingPanel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterPaginator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._game, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._ignoreReplayModelIdHash, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._beginningRun, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._ipField, Variant.From(in _ipField));
		info.AddProperty(PropertyName._idField, Variant.From(in _idField));
		info.AddProperty(PropertyName._readyButton, Variant.From(in _readyButton));
		info.AddProperty(PropertyName._readyIndicator, Variant.From(in _readyIndicator));
		info.AddProperty(PropertyName._loadingPanel, Variant.From(in _loadingPanel));
		info.AddProperty(PropertyName._characterPaginator, Variant.From(in _characterPaginator));
		info.AddProperty(PropertyName._game, Variant.From(in _game));
		info.AddProperty(PropertyName._ignoreReplayModelIdHash, Variant.From(in _ignoreReplayModelIdHash));
		info.AddProperty(PropertyName._beginningRun, Variant.From(in _beginningRun));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._ipField, out var value))
		{
			_ipField = value.As<TextEdit>();
		}
		if (info.TryGetProperty(PropertyName._idField, out var value2))
		{
			_idField = value2.As<TextEdit>();
		}
		if (info.TryGetProperty(PropertyName._readyButton, out var value3))
		{
			_readyButton = value3.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._readyIndicator, out var value4))
		{
			_readyIndicator = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._loadingPanel, out var value5))
		{
			_loadingPanel = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._characterPaginator, out var value6))
		{
			_characterPaginator = value6.As<NMultiplayerTestCharacterPaginator>();
		}
		if (info.TryGetProperty(PropertyName._game, out var value7))
		{
			_game = value7.As<NGame>();
		}
		if (info.TryGetProperty(PropertyName._ignoreReplayModelIdHash, out var value8))
		{
			_ignoreReplayModelIdHash = value8.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._beginningRun, out var value9))
		{
			_beginningRun = value9.As<bool>();
		}
	}
}
