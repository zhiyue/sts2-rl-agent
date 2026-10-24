// RlAutoSlayer.cs -- RL-agent-driven AutoSlayer.
//
// This is a modified version of the game's AutoSlayer that replaces
// random decision handlers with RL agent handlers communicating via TCP.
// The overall game flow (main menu, room loop, screen draining, map navigation)
// is preserved from the original AutoSlayer.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay;
using MegaCrit.Sts2.Core.AutoSlay.Handlers;
using MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms;
using MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace STS2BridgeMod;

/// <summary>
/// RL-agent-driven AutoSlayer. Mirrors the structure of the game's built-in
/// AutoSlayer but replaces random decision handlers with RL agent handlers
/// that communicate with Python via BridgeServer TCP.
///
/// Combat, map navigation, card rewards, events, shops, rest sites, treasure,
/// and boss relic choices are bridge-driven. Most other screen handlers still
/// use AutoSlay helpers.
/// </summary>
public class RlAutoSlayer
{
    private const string MainMenuPath = "/root/Game/RootSceneContainer/MainMenu";
    private const string RestSiteProceedButtonPath =
        "/root/Game/RootSceneContainer/Run/RoomContainer/RestSiteRoom/ProceedButton";
    private const string EventRoomPath =
        "/root/Game/RootSceneContainer/Run/RoomContainer/EventRoom";
    private const string AbandonRunOptionsButtonPath =
        "/root/Game/RootSceneContainer/Run/GlobalUi/TopBar/RightAlignedStuff/Options";
    private const string AbandonRunButtonPath =
        "/root/Game/RootSceneContainer/Run/GlobalUi/CapstoneScreenContainer/OptionsScreen/AbandonRunButton";
    private const string AbandonRunProceedButtonPath =
        "/root/Game/RootSceneContainer/Run/GlobalUi/OverlayScreensContainer/GameOverScreen/UI/ProceedButton";
    private const string AbandonRunMenuButtonPath = "MainMenuTextButtons/AbandonRunButton";
    private const string AbandonPopupPrimaryYesButtonPath = "VerticalPopup/YesButton";
    private const string AbandonPopupFallbackYesButtonPath = "YesButton";
    private const string SingleplayerButtonPath = "MainMenuTextButtons/SingleplayerButton";
    private const string CharacterSelectScreenPath = "Submenus/CharacterSelectScreen";
    private const string StandardRunButtonPath = "Submenus/SingleplayerSubmenu/StandardButton";
    private const string CharacterButtonContainerPath = "CharSelectButtons/ButtonContainer";
    private const string CharacterConfirmButtonPath =
        "Submenus/CharacterSelectScreen/ConfirmButton";
    private const string PreferredCharacterId = "Ironclad";
    private const int FinalRunFloor = 49;
    private const int RunTimeoutMinutes = 60;
    private const int RunStateTimeoutSeconds = 60;
    private const int RoomAssignmentTimeoutSeconds = 60;
    private const int NonCombatSettleDelayMs = 500;
    private const int BossTransitionTimeoutSeconds = 10;
    private const int ActTransitionTimeoutSeconds = 5;
    private const int OverlayCloseRetryLimit = 3;
    private const int OverlayDrainSettleDelayMs = 100;
    private const int EventProceedTimeoutSeconds = 5;
    private const int RewardsScreenTimeoutSeconds = 10;
    private const int MainMenuTimeoutSeconds = 30;
    private const int AbandonPopupTimeoutSeconds = 5;
    private const int AbandonRunSettleDelayMs = 1000;
    private const int MenuClickSettleDelayMs = 500;
    private const int CharacterSelectDelayMs = 100;
    private readonly Dictionary<RoomType, IRoomHandler> _roomHandlers;
    private readonly Dictionary<Type, IScreenHandler> _screenHandlers;
    private readonly RlMapHandler _mapHandler;

    private CancellationTokenSource? _cts;
    private Rng? _random;
    private Watchdog? _watchdog;
    private IDisposable? _cardSelectorScope;
    private bool _completionSignalSent;

    public static bool IsActive { get; private set; }

    /// <summary>
    /// Public watchdog for use by handlers. Since we can't set
    /// AutoSlayer.CurrentWatchdog (private setter), we expose our own.
    /// </summary>
    public static Watchdog? CurrentWatchdog { get; private set; }

    public RlAutoSlayer()
    {
        // Use our RL combat handler for all combat room types
        var combatHandler = new RlCombatHandler();
        _roomHandlers = new Dictionary<RoomType, IRoomHandler>
        {
            [RoomType.Monster] = combatHandler,
            [RoomType.Elite] = combatHandler,
            [RoomType.Boss] = combatHandler,
            [RoomType.Event] = new RlEventRoomHandler(),
            [RoomType.Shop] = new RlShopRoomHandler(),
            [RoomType.Treasure] = new RlTreasureRoomHandler(),
            [RoomType.RestSite] = new RlRestSiteRoomHandler(),
        };

        _mapHandler = new RlMapHandler();

        _screenHandlers = new Dictionary<Type, IScreenHandler>
        {
            [typeof(NRewardsScreen)] = new RlRewardsScreenHandler(),
            [typeof(NCardRewardSelectionScreen)] = new RlCardRewardScreenHandler(),
            [typeof(NDeckUpgradeSelectScreen)] = new DeckUpgradeScreenHandler(),
            [typeof(NDeckTransformSelectScreen)] = new DeckTransformScreenHandler(),
            [typeof(NDeckEnchantSelectScreen)] = new DeckEnchantScreenHandler(),
            [typeof(NDeckCardSelectScreen)] = new DeckCardSelectScreenHandler(),
            [typeof(NSimpleCardSelectScreen)] = new SimpleCardSelectScreenHandler(),
            [typeof(NChooseACardSelectionScreen)] = new ChooseACardScreenHandler(),
            [typeof(NChooseABundleSelectionScreen)] = new RlCardBundleScreenHandler(),
            [typeof(NChooseARelicSelection)] = new RlChooseARelicScreenHandler(),
            [typeof(NGameOverScreen)] = new RlGameOverScreenHandler(),
            [typeof(NCrystalSphereScreen)] = new RlCrystalSphereScreenHandler(),
        };
    }

    public void Start(string seed, string? logFile = null)
    {
        if (logFile != null)
        {
            AutoSlayLog.OpenLogFile(logFile);
        }
        IsActive = true;
        SetAutoSlayerActive(true);
        _cts = new CancellationTokenSource();
        Task task = RunAsync(seed, _cts.Token);
        TaskHelper.RunSafely(task);
    }

    /// <summary>
    /// Harvest mode: play runs back to back forever. Each run failure or
    /// completion returns to the main menu and the next seeded run starts
    /// automatically, so one game session yields unlimited recorded combats
    /// (the recorder reconnects and keeps collecting on the Python side).
    /// </summary>
    public void StartContinuous(string? logFile = null)
    {
        if (logFile != null)
        {
            AutoSlayLog.OpenLogFile(logFile);
        }
        IsActive = true;
        SetAutoSlayerActive(true);
        _cts = new CancellationTokenSource();
        Task task = ContinuousRunLoopAsync(_cts.Token);
        TaskHelper.RunSafely(task);
    }

    private async Task ContinuousRunLoopAsync(CancellationToken ct)
    {
        int runIndex = 0;
        while (!ct.IsCancellationRequested)
        {
            runIndex++;
            string seed = SeedHelper.GetRandomSeed();
            Logger.Log($"[RlAutoSlayer] === Harvest run #{runIndex}, seed: {seed} ===");
            _completionSignalSent = false;
            await RunAsync(seed, ct);
            if (ct.IsCancellationRequested)
            {
                break;
            }
            await ReturnToMainMenuSafe(ct);
        }
    }

    private async Task ReturnToMainMenuSafe(CancellationToken ct)
    {
        try
        {
            Logger.Log("[RlAutoSlayer] Returning to main menu for next run");
            await NGame.Instance.ReturnToMainMenuAfterRun();
        }
        catch (Exception ex)
        {
            Logger.Log($"[RlAutoSlayer] ReturnToMainMenuAfterRun failed: {ex.Message}");
        }
        try
        {
            Node root = ((SceneTree)Engine.GetMainLoop()).Root;
            await WaitHelper.Until(
                () => root.GetNodeOrNull<Control>(MainMenuPath)?.IsVisibleInTree() ?? false,
                ct, TimeSpan.FromSeconds(MainMenuTimeoutSeconds),
                "Main menu did not appear between harvest runs");
        }
        catch (Exception ex)
        {
            Logger.Log($"[RlAutoSlayer] Main menu wait between runs failed: {ex.Message}");
        }
    }

    public void Stop()
    {
        IsActive = false;
        SetAutoSlayerActive(false);
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>
    /// Set AutoSlayer.IsActive via NonInteractiveMode.AutoSlayerCheck.
    /// The original AutoSlayer static constructor wires this up, but since
    /// we're running our own slayer, we set it directly to report our state.
    /// </summary>
    private static void SetAutoSlayerActive(bool active)
    {
        NonInteractiveMode.AutoSlayerCheck = () => active;
    }

    private async Task RunAsync(string seed, CancellationToken ct)
    {
        Logger.Log($"[RlAutoSlayer] Run started with seed: {seed}");
        try
        {
            await WaitHelper.WithTimeout(
                (CancellationToken token) => PlayRunAsync(seed, token),
                TimeSpan.FromMinutes(RunTimeoutMinutes),
                ct);
            Logger.Log($"[RlAutoSlayer] Run completed with seed: {seed}");
        }
        catch (Exception ex)
        {
            Logger.Log($"[RlAutoSlayer] Run failed: {ex}");
        }
        finally
        {
            IsActive = false;
            SetAutoSlayerActive(false);
            CurrentWatchdog = null;
            SetSharedCurrentWatchdog(null);
            _watchdog = null;
            _cardSelectorScope?.Dispose();
            _cardSelectorScope = null;
            AutoSlayLog.CloseLogFile();

            // Notify Python that the run is over
            if (!_completionSignalSent)
            {
                BridgeServer.Instance.SendState(RunCompleteState(
                    NonCombatBridgeProtocol.TerminatedResult));
                _completionSignalSent = true;
            }
        }
    }

    private async Task PlayRunAsync(string seed, CancellationToken ct)
    {
        await WaitHelper.Until(() => NGame.Instance != null, ct,
            AutoSlayConfig.gameInitTimeout, "Game instance not initialized");

        NGame.Instance.DebugSeedOverride = seed;
        SaveManager.Instance.PrefsSave.FastMode = FastModeType.Fast;
        SaveManager.Instance.SetFtuesEnabled(enabled: false);

        // Unlock all epochs
        SaveManager.Instance.ObtainEpochOverride(
            EpochModel.GetId<Silent1Epoch>(), EpochState.Revealed);
        SaveManager.Instance.ObtainEpochOverride(
            EpochModel.GetId<Regent1Epoch>(), EpochState.Revealed);
        SaveManager.Instance.ObtainEpochOverride(
            EpochModel.GetId<Defect1Epoch>(), EpochState.Revealed);
        SaveManager.Instance.ObtainEpochOverride(
            EpochModel.GetId<Necrobinder1Epoch>(), EpochState.Revealed);

        _random = new Rng((uint)StringHelper.GetDeterministicHashCode(seed));

        // Install our RL card selector for deck upgrade/transform/card selection screens
        _cardSelectorScope = CardSelectCmd.UseSelector(new RlCardSelector());

        _watchdog = new Watchdog();
        CurrentWatchdog = _watchdog;
        SetSharedCurrentWatchdog(_watchdog);
        _watchdog.Reset("Playing main menu");

        await PlayMainMenuAsync(ct);

        await WaitHelper.Until(
            () => RunManager.Instance.DebugOnlyGetState() != null, ct,
            TimeSpan.FromSeconds(RunStateTimeoutSeconds), "Run state not initialized");

        RunState runState = RunManager.Instance.DebugOnlyGetState();
        Logger.Log($"[RlAutoSlayer] RunState available. Floor: {runState.TotalFloor}");

        await WaitHelper.Until(
            () => {
                var room = runState.CurrentRoom;
                if (room != null)
                    Logger.Log($"[RlAutoSlayer] Waiting for room... type={room.RoomType}");
                return room != null && room.RoomType != RoomType.Unassigned;
            },
            ct, TimeSpan.FromSeconds(RoomAssignmentTimeoutSeconds), "Room type not assigned");

        // Main game loop
        while (runState.TotalFloor < FinalRunFloor)
        {
            ct.ThrowIfCancellationRequested();
            RoomType roomType = runState.CurrentRoom.RoomType;
            _watchdog.Reset(
                $"Entering {roomType} room (Act {runState.CurrentActIndex + 1}, Floor {runState.ActFloor})");
            Logger.Log(
                $"[RlAutoSlayer] Entering {roomType} (Act {runState.CurrentActIndex + 1}, Floor {runState.ActFloor})");

            await HandleRoomAsync(roomType, ct);

            // After combat rooms, wait for rewards screen
            if (roomType == RoomType.Monster || roomType == RoomType.Elite ||
                roomType == RoomType.Boss)
            {
                await WaitForRewardsScreenAsync(ct);
            }
            else
            {
                await Task.Delay(NonCombatSettleDelayMs, ct);
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

            // Boss room: handle act transition
            if (roomType == RoomType.Boss)
            {
                _watchdog.Reset("Waiting for act transition after boss");
                RoomType postBossRoomType = RoomType.Boss;
                await WaitHelper.Until(delegate
                {
                    AbstractRoom currentRoom = runState.CurrentRoom;
                    if (currentRoom == null) return false;
                    postBossRoomType = currentRoom.RoomType;
                    return postBossRoomType != RoomType.Boss;
                }, ct, TimeSpan.FromSeconds(BossTransitionTimeoutSeconds),
                    "Act transition did not start after boss");

                Logger.Log($"[RlAutoSlayer] Post-boss transition: room type is now {postBossRoomType}");

                if (postBossRoomType == RoomType.Event &&
                    runState.CurrentActIndex >= runState.Acts.Count - 1)
                {
                    _watchdog.Reset(
                        $"Entering {postBossRoomType} room (Act {runState.CurrentActIndex + 1}, Floor {runState.ActFloor})");
                    await HandleRoomAsync(postBossRoomType, ct);
                    await Task.Delay(NonCombatSettleDelayMs, ct);
                    await DrainOverlayScreensAsync(ct);
                    _watchdog.Reset("Waiting for main menu after victory");
                    await WaitForMainMenuAsync(ct);
                    Logger.Log("[RlAutoSlayer] Victory! Run completed and returned to main menu");

                    // Notify Python of victory
                    BridgeServer.Instance.SendState(RunCompleteState(
                        NonCombatBridgeProtocol.VictoryResult));
                    _completionSignalSent = true;
                    return;
                }

                await WaitHelper.Until(
                    () => runState.VisitedMapCoords.Count == 0, ct,
                    TimeSpan.FromSeconds(ActTransitionTimeoutSeconds),
                    "Act transition did not complete (VisitedMapCoords not cleared)");
            }

            _watchdog.Reset("Navigating map");
            await _mapHandler.HandleAsync(_random, ct);
        }

        Logger.Log("[RlAutoSlayer] Run completed (max floor reached). Abandoning");
        await AbandonRunAsync(ct);
    }

    private async Task HandleRoomAsync(RoomType roomType, CancellationToken ct)
    {
        if (!_roomHandlers.TryGetValue(roomType, out IRoomHandler handler))
        {
            Logger.Log($"[RlAutoSlayer] No handler for room type: {roomType}");
            return;
        }
        await WaitHelper.WithTimeout(
            (CancellationToken token) => handler.HandleAsync(_random, token),
            handler.Timeout, ct);
    }

    private async Task DrainOverlayScreensAsync(CancellationToken ct)
    {
        if (NOverlayStack.Instance == null)
        {
            await WaitHelper.Until(() => NOverlayStack.Instance != null, ct,
                AutoSlayConfig.nodeWaitTimeout, "Overlay stack not initialized");
        }

        HashSet<IOverlayScreen> handledScreens = new HashSet<IOverlayScreen>();
        int consecutiveFailures = 0;

        while (true)
        {
            NOverlayStack? instance = NOverlayStack.Instance;
            if (instance == null || instance.ScreenCount <= 0)
                break;

            ct.ThrowIfCancellationRequested();

            IOverlayScreen currentOverlay = NOverlayStack.Instance.Peek();
            if (currentOverlay == null)
                break;

            if (handledScreens.Contains(currentOverlay))
            {
                consecutiveFailures++;
                if (consecutiveFailures >= OverlayCloseRetryLimit)
                {
                    Logger.Log(
                        $"[RlAutoSlayer] Infinite loop: screen {currentOverlay.GetType().Name} not closing after {OverlayCloseRetryLimit} attempts");
                    throw new InvalidOperationException(
                        "Screen " + currentOverlay.GetType().Name + " not closing after being handled");
                }
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
                Logger.Log($"[RlAutoSlayer] No handler for screen type: {type.Name}");
                break;
            }

            _watchdog?.Reset("Handling screen: " + type.Name);
            Logger.Log($"[RlAutoSlayer] Handling screen: {type.Name}");
            await WaitHelper.WithTimeout(
                (CancellationToken token) => handler.HandleAsync(_random, token),
                handler.Timeout, ct);

            if (currentOverlay is NRewardsScreen &&
                (NMapScreen.Instance?.IsOpen ?? false))
            {
                break;
            }

            await Task.Delay(OverlayDrainSettleDelayMs, ct);
        }
    }

    private async Task ClickRestSiteProceedIfNeeded(CancellationToken ct)
    {
        Node root = ((SceneTree)Engine.GetMainLoop()).Root;
        NProceedButton nodeOrNull = root.GetNodeOrNull<NProceedButton>(
            RestSiteProceedButtonPath);
        if (nodeOrNull != null && nodeOrNull.IsEnabled)
        {
            Logger.Log("[RlAutoSlayer] Clicking rest site proceed button");
            await UiHelper.Click(nodeOrNull);
        }
    }

    private async Task ClickEventProceedIfNeeded(CancellationToken ct)
    {
        Node root = ((SceneTree)Engine.GetMainLoop()).Root;
        Node eventRoom = root.GetNodeOrNull(EventRoomPath);
        if (eventRoom == null)
            return;

        NEventOptionButton proceedOption = null;
        await WaitHelper.Until(delegate
        {
            NMapScreen? instance = NMapScreen.Instance;
            if (instance != null && instance.IsOpen) return true;

            List<NEventOptionButton> list = (from o in UiHelper.FindAll<NEventOptionButton>(eventRoom)
                where !o.Option.IsLocked && o.Option.IsProceed
                select o).ToList();
            if (list.Count > 0)
            {
                proceedOption = list[0];
                return true;
            }
            return false;
        }, ct, TimeSpan.FromSeconds(EventProceedTimeoutSeconds),
            "Event proceed option or map did not appear");

        if (proceedOption != null)
        {
            Logger.Log("[RlAutoSlayer] Clicking event proceed option");
            await UiHelper.Click(proceedOption);
        }
    }

    private async Task WaitForRewardsScreenAsync(CancellationToken ct)
    {
        Logger.Log("[RlAutoSlayer] Waiting for rewards screen");
        await WaitHelper.Until(
            () => NOverlayStack.Instance?.Peek() is NRewardsScreen ||
                  (NMapScreen.Instance?.IsOpen ?? false),
            ct, TimeSpan.FromSeconds(RewardsScreenTimeoutSeconds),
            "Rewards screen did not appear after combat");
    }

    private async Task WaitForMainMenuAsync(CancellationToken ct)
    {
        Logger.Log("[RlAutoSlayer] Waiting for main menu");
        Node root = ((SceneTree)Engine.GetMainLoop()).Root;
        await WaitHelper.Until(
            () => root.GetNodeOrNull<Control>(MainMenuPath)?.IsVisibleInTree() ?? false,
            ct, TimeSpan.FromSeconds(MainMenuTimeoutSeconds),
            "Main menu did not appear after game over");
    }

    private async Task PlayMainMenuAsync(CancellationToken ct)
    {
        Logger.Log("[RlAutoSlayer] Playing main menu");
        Node root = ((SceneTree)Engine.GetMainLoop()).Root;
        Control mainMenu = await WaitHelper.ForNode<Control>(
            root, MainMenuPath, ct, TimeSpan.FromSeconds(MainMenuTimeoutSeconds));

        // Abandon existing run if present (best effort)
        try
        {
            NButton abandonBtn = mainMenu.GetNodeOrNull<NButton>(
                AbandonRunMenuButtonPath);
            if (abandonBtn != null && abandonBtn.Visible)
            {
                Logger.Log("[RlAutoSlayer] Abandoning existing run");
                await UiHelper.Click(abandonBtn);
                await Task.Delay(MenuClickSettleDelayMs, ct);
                // Try to find and click Yes on the confirmation popup
                try
                {
                    await WaitHelper.Until(
                        () => NModalContainer.Instance?.OpenModal != null, ct,
                        TimeSpan.FromSeconds(AbandonPopupTimeoutSeconds), "Abandon popup");
                    Node popup = (Node)NModalContainer.Instance.OpenModal;
                    NButton yesBtn = popup.GetNodeOrNull<NButton>(AbandonPopupPrimaryYesButtonPath)
                        ?? popup.GetNodeOrNull<NButton>(AbandonPopupFallbackYesButtonPath);
                    if (yesBtn != null)
                    {
                        await UiHelper.Click(yesBtn);
                        await Task.Delay(MenuClickSettleDelayMs, ct);
                    }
                }
                catch
                {
                    Logger.Log("[RlAutoSlayer] Popup not found, trying to continue anyway");
                }
                await Task.Delay(AbandonRunSettleDelayMs, ct);
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[RlAutoSlayer] Could not abandon run: {ex.Message}, continuing...");
        }

        // Click singleplayer
        NButton spButton = mainMenu.GetNode<NButton>(
            SingleplayerButtonPath);
        Logger.Log("[RlAutoSlayer] Clicking singleplayer");
        await UiHelper.Click(spButton);

        // Navigate to character select
        Control charSelectScreen = mainMenu.GetNodeOrNull<Control>(
            CharacterSelectScreenPath);
        NButton standardButton = mainMenu.GetNodeOrNull<NButton>(
            StandardRunButtonPath);
        await WaitHelper.Until(delegate
        {
            charSelectScreen = mainMenu.GetNodeOrNull<Control>(
                CharacterSelectScreenPath);
            standardButton = mainMenu.GetNodeOrNull<NButton>(
                StandardRunButtonPath);
            bool csVisible = charSelectScreen?.Visible ?? false;
            bool sbVisible = standardButton?.Visible ?? false;
            return csVisible || sbVisible;
        }, ct, AutoSlayConfig.nodeWaitTimeout,
            "Neither CharacterSelectScreen nor SingleplayerSubmenu became visible");

        if (standardButton?.Visible ?? false)
        {
            Control csCtrl = charSelectScreen;
            if (csCtrl == null || !csCtrl.Visible)
            {
                Logger.Log("[RlAutoSlayer] Clicking standard run");
                await UiHelper.Click(standardButton);
                await WaitHelper.Until(
                    () => mainMenu.GetNodeOrNull<Control>(
                        CharacterSelectScreenPath)?.Visible ?? false,
                    ct, AutoSlayConfig.nodeWaitTimeout,
                    "CharacterSelectScreen did not become visible");
                charSelectScreen = mainMenu.GetNode<Control>(
                    CharacterSelectScreenPath);
            }
        }

        // Select Ironclad (first character) — our agent was trained on Ironclad
        Node buttonContainer = charSelectScreen.GetNode(
            CharacterButtonContainerPath);
        List<NCharacterSelectButton> buttons =
            UiHelper.FindAll<NCharacterSelectButton>(buttonContainer);
        foreach (NCharacterSelectButton btn in buttons)
        {
            btn.UnlockIfPossible();
        }
        List<NCharacterSelectButton> available =
            buttons.Where(b => !b.IsLocked).ToList();

        // Pick Ironclad (first available) instead of random
        NCharacterSelectButton selectedChar = available.FirstOrDefault(
            b => b.Character.Id.Entry.Contains(PreferredCharacterId,
                StringComparison.OrdinalIgnoreCase))
            ?? available.First();
        Logger.Log($"[RlAutoSlayer] Selecting character: {selectedChar.Character.Id}");
        selectedChar.Select();
        await Task.Delay(CharacterSelectDelayMs, ct);

        NButton confirmBtn = await WaitHelper.ForNode<NButton>(
            mainMenu, CharacterConfirmButtonPath, ct);
        Logger.Log("[RlAutoSlayer] Confirming character");
        await UiHelper.Click(confirmBtn);
    }

    private async Task AbandonRunAsync(CancellationToken ct)
    {
        Node root = ((SceneTree)Engine.GetMainLoop()).Root;
        await Task.Delay(AbandonRunSettleDelayMs, ct);
        await UiHelper.Click(await WaitHelper.ForNode<NButton>(
            root,
            AbandonRunOptionsButtonPath,
            ct));
        await UiHelper.Click(await WaitHelper.ForNode<NButton>(
            root,
            AbandonRunButtonPath,
            ct));
        await UiHelper.Click(await WaitHelper.ForNode<NButton>(
            root,
            AbandonRunProceedButtonPath,
            ct));
    }

    private static string RunCompleteState(string result)
    {
        return JsonSerializer.Serialize(new Dictionary<string, object>
        {
            [NonCombatBridgeProtocol.TypeField] = NonCombatBridgeProtocol.RunCompleteState,
            [NonCombatBridgeProtocol.ResultField] = result,
        });
    }

    private static void SetSharedCurrentWatchdog(Watchdog? watchdog)
    {
        try
        {
            PropertyInfo? property = typeof(AutoSlayer).GetProperty(
                "CurrentWatchdog",
                BindingFlags.Public | BindingFlags.Static);
            property?.SetValue(null, watchdog);
        }
        catch (Exception ex)
        {
            Logger.Log($"[RlAutoSlayer] Could not mirror watchdog: {ex.Message}");
        }
    }
}

/// <summary>
/// RL-aware GameOverScreenHandler. Same as the original but also notifies
/// the Python agent about game over.
/// </summary>
public class RlGameOverScreenHandler : IScreenHandler, IHandler
{
    private const int HandlerTimeoutMinutes = 2;
    private const int ContinueButtonTimeoutSeconds = 30;
    private const int SummaryAnimationTimeoutSeconds = 90;
    private const int WatchdogRefreshCycles = 20;

    public Type ScreenType => typeof(NGameOverScreen);
    public TimeSpan Timeout => TimeSpan.FromMinutes(HandlerTimeoutMinutes);

    public async Task HandleAsync(Rng random, CancellationToken ct)
    {
        Logger.Log("[RlGameOver] Game over screen appeared");
        NGameOverScreen screen =
            (NGameOverScreen)NOverlayStack.Instance.Peek();

        // Notify Python that the game is over
        BridgeServer.Instance.SendState(JsonSerializer.Serialize(
            new Dictionary<string, object>
            {
                [NonCombatBridgeProtocol.TypeField] = NonCombatBridgeProtocol.GameOverState,
                [NonCombatBridgeProtocol.MessageField] = NonCombatBridgeProtocol.GameOverMessage,
            }));

        NGameOverContinueButton continueButton =
            UiHelper.FindFirst<NGameOverContinueButton>(screen);
        if (continueButton == null)
        {
            Logger.Log("[RlGameOver] Continue button not found");
            return;
        }

        await WaitHelper.Until(() => continueButton.IsEnabled, ct,
            TimeSpan.FromSeconds(ContinueButtonTimeoutSeconds),
            "Continue button did not become enabled");
        await UiHelper.Click(continueButton);

        NReturnToMainMenuButton mainMenuButton = null;
        int waitCycles = 0;
        await WaitHelper.Until(delegate
        {
            if (!GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree())
                return true;
            mainMenuButton = UiHelper.FindFirst<NReturnToMainMenuButton>(screen);
            waitCycles++;
            if (waitCycles % WatchdogRefreshCycles == 0)
            {
                RlAutoSlayer.CurrentWatchdog?.Reset("Waiting for game over summary animation");
            }
            return mainMenuButton != null && mainMenuButton.Visible && mainMenuButton.IsEnabled;
        }, ct, TimeSpan.FromSeconds(SummaryAnimationTimeoutSeconds),
            "Main menu button did not become enabled");

        if (!GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree())
            return;

        await UiHelper.Click(mainMenuButton);
        await WaitHelper.Until(
            () => !GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree(),
            ct, TimeSpan.FromSeconds(ContinueButtonTimeoutSeconds),
            "Game over screen did not close");
    }
}
