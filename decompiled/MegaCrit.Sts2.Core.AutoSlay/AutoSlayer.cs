using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Handlers;
using MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms;
using MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace MegaCrit.Sts2.Core.AutoSlay;

public class AutoSlayer
{
	private readonly Dictionary<RoomType, IRoomHandler> _roomHandlers;

	private readonly Dictionary<Type, IScreenHandler> _screenHandlers;

	private readonly MapScreenHandler _mapHandler;

	private CancellationTokenSource? _cts;

	private Rng? _random;

	private Watchdog? _watchdog;

	private IDisposable? _cardSelectorScope;

	private static int _exitCode;

	public static bool IsActive { get; private set; }

	public static Watchdog? CurrentWatchdog { get; private set; }

	static AutoSlayer()
	{
		NonInteractiveMode.AutoSlayerCheck = () => IsActive;
	}

	public AutoSlayer()
	{
		CombatRoomHandler value = new CombatRoomHandler();
		_roomHandlers = new Dictionary<RoomType, IRoomHandler>
		{
			[RoomType.Monster] = value,
			[RoomType.Elite] = value,
			[RoomType.Boss] = value,
			[RoomType.Event] = new EventRoomHandler(),
			[RoomType.Shop] = new ShopRoomHandler(),
			[RoomType.Treasure] = new TreasureRoomHandler(),
			[RoomType.RestSite] = new RestSiteRoomHandler()
		};
		_mapHandler = new MapScreenHandler();
		_screenHandlers = new Dictionary<Type, IScreenHandler>
		{
			[typeof(NRewardsScreen)] = new RewardsScreenHandler(),
			[typeof(NCardRewardSelectionScreen)] = new CardRewardScreenHandler(),
			[typeof(NDeckUpgradeSelectScreen)] = new DeckUpgradeScreenHandler(),
			[typeof(NDeckTransformSelectScreen)] = new DeckTransformScreenHandler(),
			[typeof(NDeckEnchantSelectScreen)] = new DeckEnchantScreenHandler(),
			[typeof(NDeckCardSelectScreen)] = new DeckCardSelectScreenHandler(),
			[typeof(NSimpleCardSelectScreen)] = new SimpleCardSelectScreenHandler(),
			[typeof(NChooseACardSelectionScreen)] = new ChooseACardScreenHandler(),
			[typeof(NChooseABundleSelectionScreen)] = new ChooseABundleScreenHandler(),
			[typeof(NChooseARelicSelection)] = new ChooseARelicScreenHandler(),
			[typeof(NGameOverScreen)] = new GameOverScreenHandler(),
			[typeof(NCrystalSphereScreen)] = new CrystalSphereScreenHandler()
		};
	}

	public void Start(string seed, string? logFile = null)
	{
		if (logFile != null)
		{
			AutoSlayLog.OpenLogFile(logFile);
		}
		SentryService.SetTag("autoslay", "true");
		SentryService.SetTag("autoslay.seed", seed);
		IsActive = true;
		_cts = new CancellationTokenSource();
		Task task = RunAsync(seed, _cts.Token);
		TaskHelper.RunSafely(task);
	}

	public void Stop()
	{
		IsActive = false;
		_cts?.Cancel();
		_cts?.Dispose();
		_cts = null;
	}

	public static T GetCurrentScreen<T>() where T : Node
	{
		return (T)NOverlayStack.Instance.Peek();
	}

	private async Task RunAsync(string seed, CancellationToken ct)
	{
		AutoSlayLog.RunStarted(seed);
		try
		{
			await WaitHelper.WithTimeout((CancellationToken token) => PlayRunAsync(seed, token), AutoSlayConfig.runTimeout, ct);
			AutoSlayLog.RunCompleted(seed);
		}
		catch (Exception ex)
		{
			_exitCode = 1;
			AutoSlayLog.RunFailed(seed, ex);
			throw;
		}
		finally
		{
			IsActive = false;
			CurrentWatchdog = null;
			_watchdog = null;
			_cardSelectorScope?.Dispose();
			_cardSelectorScope = null;
			AutoSlayLog.CloseLogFile();
			QuitGame(_exitCode);
		}
	}

	private async Task PlayRunAsync(string seed, CancellationToken ct)
	{
		await WaitHelper.Until(() => NGame.Instance != null, ct, AutoSlayConfig.gameInitTimeout, "Game instance not initialized");
		NGame.Instance.DebugSeedOverride = seed;
		SaveManager.Instance.PrefsSave.FastMode = FastModeType.Fast;
		SaveManager.Instance.SetFtuesEnabled(enabled: false);
		SaveManager.Instance.ObtainEpochOverride(EpochModel.GetId<Silent1Epoch>(), EpochState.Revealed);
		SaveManager.Instance.ObtainEpochOverride(EpochModel.GetId<Regent1Epoch>(), EpochState.Revealed);
		SaveManager.Instance.ObtainEpochOverride(EpochModel.GetId<Defect1Epoch>(), EpochState.Revealed);
		SaveManager.Instance.ObtainEpochOverride(EpochModel.GetId<Necrobinder1Epoch>(), EpochState.Revealed);
		_random = new Rng((uint)StringHelper.GetDeterministicHashCode(seed));
		_cardSelectorScope = CardSelectCmd.UseSelector(new AutoSlayCardSelector(_random));
		_watchdog = new Watchdog();
		CurrentWatchdog = _watchdog;
		_watchdog.Reset("Playing main menu");
		await PlayMainMenuAsync(ct);
		await WaitHelper.Until(() => RunManager.Instance.DebugOnlyGetState() != null, ct, AutoSlayConfig.runStateTimeout, "Run state not initialized");
		RunState runState = RunManager.Instance.DebugOnlyGetState();
		await WaitHelper.Until(() => runState.CurrentRoom != null && runState.CurrentRoom.RoomType != RoomType.Unassigned, ct, AutoSlayConfig.nodeWaitTimeout, "Room type not assigned");
		while (runState.TotalFloor < 49)
		{
			ct.ThrowIfCancellationRequested();
			RoomType roomType = runState.CurrentRoom.RoomType;
			_watchdog.Reset($"Entering {roomType} room (Act {runState.CurrentActIndex + 1}, Floor {runState.ActFloor})");
			AutoSlayLog.EnterRoom(roomType, runState.CurrentActIndex, runState.ActFloor);
			await HandleRoomAsync(roomType, ct);
			if ((uint)(roomType - 1) > 2u)
			{
				await Task.Delay(500, ct);
			}
			else
			{
				await WaitForRewardsScreenAsync(ct);
			}
			await DrainOverlayScreensAsync(ct);
			if (roomType == RoomType.RestSite)
			{
				await ClickRestSiteProceedIfNeeded(ct);
			}
			if (roomType == RoomType.Event)
			{
				await ClickEventProceedIfNeeded(ct);
			}
			if (roomType == RoomType.Boss)
			{
				_watchdog.Reset("Waiting for act transition after boss");
				RoomType postBossRoomType = RoomType.Boss;
				await WaitHelper.Until(delegate
				{
					AbstractRoom currentRoom = runState.CurrentRoom;
					if (currentRoom == null)
					{
						return false;
					}
					postBossRoomType = currentRoom.RoomType;
					return postBossRoomType != RoomType.Boss;
				}, ct, TimeSpan.FromSeconds(10L), "Act transition did not start after boss");
				AutoSlayLog.Info($"Post-boss transition: room type is now {postBossRoomType}");
				if (postBossRoomType == RoomType.Event && runState.CurrentActIndex >= runState.Acts.Count - 1)
				{
					_watchdog.Reset($"Entering {postBossRoomType} room (Act {runState.CurrentActIndex + 1}, Floor {runState.ActFloor})");
					AutoSlayLog.EnterRoom(postBossRoomType, runState.CurrentActIndex, runState.ActFloor);
					await HandleRoomAsync(postBossRoomType, ct);
					await Task.Delay(500, ct);
					await DrainOverlayScreensAsync(ct);
					_watchdog.Reset("Waiting for main menu after victory");
					await WaitForMainMenuAsync(ct);
					AutoSlayLog.Action("Victory! Run completed and returned to main menu");
					return;
				}
				await WaitHelper.Until(() => runState.VisitedMapCoords.Count == 0, ct, TimeSpan.FromSeconds(5L), "Act transition did not complete (VisitedMapCoords not cleared)");
			}
			_watchdog.Reset("Navigating map");
			await _mapHandler.HandleAsync(_random, ct);
		}
		AutoSlayLog.Action("Run completed (max floor reached). Abandoning");
		await AbandonRunAsync(ct);
	}

	private async Task HandleRoomAsync(RoomType roomType, CancellationToken ct)
	{
		if (!_roomHandlers.TryGetValue(roomType, out IRoomHandler handler))
		{
			AutoSlayLog.Warn($"No handler for room type: {roomType}");
		}
		else
		{
			await WaitHelper.WithTimeout((CancellationToken token) => handler.HandleAsync(_random, token), handler.Timeout, ct);
			AutoSlayLog.ExitRoom(roomType);
		}
	}

	private async Task DrainOverlayScreensAsync(CancellationToken ct)
	{
		if (NOverlayStack.Instance == null)
		{
			await WaitHelper.Until(() => NOverlayStack.Instance != null, ct, AutoSlayConfig.nodeWaitTimeout, "Overlay stack not initialized");
		}
		HashSet<IOverlayScreen> handledScreens = new HashSet<IOverlayScreen>();
		int consecutiveFailures = 0;
		while (true)
		{
			NOverlayStack? instance = NOverlayStack.Instance;
			if (instance == null || instance.ScreenCount <= 0)
			{
				break;
			}
			ct.ThrowIfCancellationRequested();
			IOverlayScreen currentOverlay = NOverlayStack.Instance.Peek();
			if (currentOverlay == null)
			{
				break;
			}
			if (handledScreens.Contains(currentOverlay))
			{
				consecutiveFailures++;
				if (consecutiveFailures >= 3)
				{
					AutoSlayLog.Error($"Infinite loop detected: screen {currentOverlay.GetType().Name} not closing after {3} attempts");
					throw new InvalidOperationException("Screen " + currentOverlay.GetType().Name + " not closing after being handled");
				}
				AutoSlayLog.Warn($"Screen {currentOverlay.GetType().Name} still present after handling (attempt {consecutiveFailures})");
			}
			else
			{
				handledScreens.Add(currentOverlay);
				consecutiveFailures = 0;
			}
			Node node = (Node)currentOverlay;
			Type type = node.GetType();
			if (!_screenHandlers.TryGetValue(type, out IScreenHandler handler))
			{
				AutoSlayLog.Warn("No handler for screen type: " + type.Name);
				break;
			}
			_watchdog.Reset("Handling screen: " + type.Name);
			AutoSlayLog.Info("Handling screen: " + type.Name);
			await WaitHelper.WithTimeout((CancellationToken token) => handler.HandleAsync(_random, token), handler.Timeout, ct);
			if (currentOverlay is NRewardsScreen && (NMapScreen.Instance?.IsOpen ?? false))
			{
				AutoSlayLog.Info("Rewards screen handled and map is open, exiting drain loop");
				break;
			}
			await Task.Delay(100, ct);
		}
	}

	private async Task ClickRestSiteProceedIfNeeded(CancellationToken ct)
	{
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		NProceedButton nodeOrNull = root.GetNodeOrNull<NProceedButton>("/root/Game/RootSceneContainer/Run/RoomContainer/RestSiteRoom/ProceedButton");
		if (nodeOrNull != null && nodeOrNull.IsEnabled)
		{
			AutoSlayLog.Action("Clicking rest site proceed button");
			await UiHelper.Click(nodeOrNull);
		}
	}

	private async Task ClickEventProceedIfNeeded(CancellationToken ct)
	{
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		Node eventRoom = root.GetNodeOrNull("/root/Game/RootSceneContainer/Run/RoomContainer/EventRoom");
		if (eventRoom == null)
		{
			AutoSlayLog.Info("Event room not found for proceed check");
			return;
		}
		NEventOptionButton proceedOption = null;
		await WaitHelper.Until(delegate
		{
			NMapScreen? instance = NMapScreen.Instance;
			if (instance != null && instance.IsOpen)
			{
				return true;
			}
			List<NEventOptionButton> list = (from o in UiHelper.FindAll<NEventOptionButton>(eventRoom)
				where !o.Option.IsLocked && o.Option.IsProceed
				select o).ToList();
			if (list.Count > 0)
			{
				proceedOption = list[0];
				return true;
			}
			return false;
		}, ct, TimeSpan.FromSeconds(5L), "Event proceed option or map did not appear");
		if (proceedOption != null)
		{
			AutoSlayLog.Action("Clicking event proceed option");
			await UiHelper.Click(proceedOption);
		}
		else
		{
			AutoSlayLog.Info("Map already open, no proceed needed");
		}
	}

	private async Task WaitForRewardsScreenAsync(CancellationToken ct)
	{
		AutoSlayLog.Action("Waiting for rewards screen");
		await WaitHelper.Until(() => NOverlayStack.Instance?.Peek() is NRewardsScreen || (NMapScreen.Instance?.IsOpen ?? false), ct, TimeSpan.FromSeconds(10L), "Rewards screen did not appear after combat");
	}

	private async Task WaitForMainMenuAsync(CancellationToken ct)
	{
		AutoSlayLog.Action("Waiting for main menu");
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		await WaitHelper.Until(() => root.GetNodeOrNull<Control>("/root/Game/RootSceneContainer/MainMenu")?.IsVisibleInTree() ?? false, ct, TimeSpan.FromSeconds(30L), "Main menu did not appear after game over");
		AutoSlayLog.Action("Main menu appeared");
	}

	private async Task PlayMainMenuAsync(CancellationToken ct)
	{
		AutoSlayLog.Action("Playing main menu");
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		Control mainMenu = await WaitHelper.ForNode<Control>(root, "/root/Game/RootSceneContainer/MainMenu", ct, TimeSpan.FromSeconds(30L));
		NButton node = mainMenu.GetNode<NButton>("MainMenuTextButtons/AbandonRunButton");
		if (node.Visible)
		{
			AutoSlayLog.Action("Abandoning existing run");
			await UiHelper.Click(node);
			await WaitHelper.Until(() => NModalContainer.Instance?.OpenModal != null, ct, AutoSlayConfig.nodeWaitTimeout, "Abandon run confirmation popup did not appear");
			Node node2 = (Node)NModalContainer.Instance.OpenModal;
			NButton node3 = node2.GetNode<NButton>("VerticalPopup/YesButton");
			AutoSlayLog.Action("Confirming abandon");
			await UiHelper.Click(node3);
			await WaitHelper.Until(() => NModalContainer.Instance.OpenModal == null, ct, AutoSlayConfig.nodeWaitTimeout, "Abandon run confirmation popup did not close");
		}
		NButton node4 = mainMenu.GetNode<NButton>("MainMenuTextButtons/SingleplayerButton");
		AutoSlayLog.Action("Clicking singleplayer");
		await UiHelper.Click(node4);
		Control charSelectScreen = mainMenu.GetNodeOrNull<Control>("Submenus/CharacterSelectScreen");
		NButton standardButton = mainMenu.GetNodeOrNull<NButton>("Submenus/SingleplayerSubmenu/StandardButton");
		await WaitHelper.Until(delegate
		{
			charSelectScreen = mainMenu.GetNodeOrNull<Control>("Submenus/CharacterSelectScreen");
			standardButton = mainMenu.GetNodeOrNull<NButton>("Submenus/SingleplayerSubmenu/StandardButton");
			bool flag = charSelectScreen?.Visible ?? false;
			bool flag2 = standardButton?.Visible ?? false;
			return flag || flag2;
		}, ct, AutoSlayConfig.nodeWaitTimeout, "Neither CharacterSelectScreen nor SingleplayerSubmenu became visible");
		if (standardButton?.Visible ?? false)
		{
			Control control = charSelectScreen;
			if (control == null || !control.Visible)
			{
				AutoSlayLog.Action("Clicking standard run");
				await UiHelper.Click(standardButton);
				await WaitHelper.Until(() => mainMenu.GetNodeOrNull<Control>("Submenus/CharacterSelectScreen")?.Visible ?? false, ct, AutoSlayConfig.nodeWaitTimeout, "CharacterSelectScreen did not become visible");
				charSelectScreen = mainMenu.GetNode<Control>("Submenus/CharacterSelectScreen");
				goto IL_05e6;
			}
		}
		AutoSlayLog.Action("Skipping submenu (first run)");
		goto IL_05e6;
		IL_05e6:
		Node node5 = charSelectScreen.GetNode("CharSelectButtons/ButtonContainer");
		List<NCharacterSelectButton> list = UiHelper.FindAll<NCharacterSelectButton>(node5);
		foreach (NCharacterSelectButton item in list)
		{
			item.UnlockIfPossible();
		}
		List<NCharacterSelectButton> items = list.Where((NCharacterSelectButton b) => !b.IsLocked).ToList();
		NCharacterSelectButton nCharacterSelectButton = _random.NextItem(items);
		AutoSlayLog.Action($"Selecting character: {nCharacterSelectButton.Character.Id}");
		nCharacterSelectButton.Select();
		await Task.Delay(100, ct);
		NButton button = await WaitHelper.ForNode<NButton>(mainMenu, "Submenus/CharacterSelectScreen/ConfirmButton", ct);
		AutoSlayLog.Action("Confirming character");
		await UiHelper.Click(button);
	}

	private async Task AbandonRunAsync(CancellationToken ct)
	{
		Node root = ((SceneTree)Engine.GetMainLoop()).Root;
		await Task.Delay(1000, ct);
		await UiHelper.Click(await WaitHelper.ForNode<NButton>(root, "/root/Game/RootSceneContainer/Run/GlobalUi/TopBar/RightAlignedStuff/Options", ct));
		await UiHelper.Click(await WaitHelper.ForNode<NButton>(root, "/root/Game/RootSceneContainer/Run/GlobalUi/CapstoneScreenContainer/OptionsScreen/AbandonRunButton", ct));
		await UiHelper.Click(await WaitHelper.ForNode<NButton>(root, "/root/Game/RootSceneContainer/Run/GlobalUi/OverlayScreensContainer/GameOverScreen/UI/ProceedButton", ct));
	}

	private static void QuitGame(int exitCode)
	{
		AutoSlayLog.Action($"Quitting game with exit code {exitCode}");
		NGame.Instance?.GetTree().Quit(exitCode);
	}
}
