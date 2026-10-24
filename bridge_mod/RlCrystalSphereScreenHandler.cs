// RlCrystalSphereScreenHandler.cs -- bridge-driven Crystal Sphere minigame handler.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay;
using MegaCrit.Sts2.Core.AutoSlay.Handlers;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace STS2BridgeMod;

public class RlCrystalSphereScreenHandler : IScreenHandler, IHandler
{
    private const int AgentTimeoutSeconds = 30;
    private const int HandlerTimeoutSeconds = 120;
    private const int InitialSettleDelayMs = 1000;
    private const int ClickSettleDelayMs = 500;
    private const int WaitForOutcomeTimeoutSeconds = 15;
    private const int ProceedCloseTimeoutSeconds = 10;
    private const int OverlayRemovalDelayMs = 100;
    private const int ActDisplayIndexOffset = 1;
    private const string ScreenLogName = "NCrystalSphereScreen";
    private static readonly TimeSpan AgentTimeout = TimeSpan.FromSeconds(AgentTimeoutSeconds);

    public Type ScreenType => typeof(NCrystalSphereScreen);
    public TimeSpan Timeout => TimeSpan.FromSeconds(HandlerTimeoutSeconds);

    public async Task HandleAsync(Rng random, CancellationToken ct)
    {
        AutoSlayLog.EnterScreen(ScreenLogName);
        NCrystalSphereScreen screen = AutoSlayer.GetCurrentScreen<NCrystalSphereScreen>();
        await Task.Delay(InitialSettleDelayMs, ct);

        while (GodotObject.IsInstanceValid(screen) && screen.IsVisibleInTree())
        {
            ct.ThrowIfCancellationRequested();

            IOverlayScreen overlayScreen = NOverlayStack.Instance?.Peek();
            if (overlayScreen != null && overlayScreen != screen)
            {
                AutoSlayLog.Info("Child screen appeared from Crystal Sphere, returning to drain loop");
                AutoSlayLog.ExitScreen(ScreenLogName);
                return;
            }

            NProceedButton? proceedButton = screen.GetNodeOrNull<NProceedButton>("%ProceedButton");
            List<NCrystalSphereCell> hiddenCells = HiddenCells(screen);
            CrystalSphereChoice choice = await ChooseCrystalSphereOption(hiddenCells, proceedButton, random, ct);
            if (choice.ShouldProceed)
            {
                if (proceedButton != null)
                {
                    await ClickProceed(screen, proceedButton, ct);
                }
                AutoSlayLog.ExitScreen(ScreenLogName);
                return;
            }

            if (choice.Cell == null)
            {
                await WaitForOutcome(screen, ct);
                continue;
            }

            AutoSlayLog.Info(
                $"Clicking crystal sphere cell at ({choice.Cell.Entity.X}, {choice.Cell.Entity.Y})");
            choice.Cell.EmitSignal(NClickableControl.SignalName.Released, choice.Cell);
            await Task.Delay(ClickSettleDelayMs, ct);
        }

        AutoSlayLog.ExitScreen(ScreenLogName);
    }

    private static List<NCrystalSphereCell> HiddenCells(NCrystalSphereScreen screen)
    {
        Control? cellsContainer = screen.GetNodeOrNull<Control>("%Cells");
        if (cellsContainer == null)
        {
            return new List<NCrystalSphereCell>();
        }
        return UiHelper.FindAll<NCrystalSphereCell>(cellsContainer)
            .Where(cell => cell.Visible && cell.Entity.IsHidden)
            .ToList();
    }

    private static async Task<CrystalSphereChoice> ChooseCrystalSphereOption(
        List<NCrystalSphereCell> hiddenCells,
        NProceedButton? proceedButton,
        Rng random,
        CancellationToken ct)
    {
        if (!BridgeServer.Instance.IsClientConnected)
        {
            if (proceedButton?.IsEnabled ?? false)
            {
                return CrystalSphereChoice.Proceed();
            }
            return hiddenCells.Count > 0
                ? CrystalSphereChoice.Click(random.NextItem(hiddenCells))
                : CrystalSphereChoice.Wait();
        }

        try
        {
            List<Dictionary<string, object>> options = hiddenCells
                .Select((cell, index) => CellOption(cell, index))
                .ToList();
            if (proceedButton?.IsEnabled ?? false)
            {
                options.Add(new Dictionary<string, object>
                {
                    ["index"] = options.Count,
                    ["action"] = NonCombatBridgeProtocol.ProceedAction,
                    ["enabled"] = true,
                });
            }

            if (options.Count == 0)
            {
                return CrystalSphereChoice.Wait();
            }

            RunState runState = RunManager.Instance.DebugOnlyGetState();
            string stateJson = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["type"] = NonCombatBridgeProtocol.CrystalSphereState,
                ["options"] = options,
                ["floor"] = runState.TotalFloor,
                ["act"] = runState.CurrentActIndex + ActDisplayIndexOffset,
            });
            string? responseJson = await BridgeServer.Instance.SendStateAndWaitForActionAsync(
                stateJson,
                AgentTimeout,
                ct);
            if (responseJson == null)
            {
                return hiddenCells.Count > 0
                    ? CrystalSphereChoice.Click(random.NextItem(hiddenCells))
                    : CrystalSphereChoice.Wait();
            }

            int chosenIndex = ReadChoiceIndex(responseJson);
            if (chosenIndex >= 0 && chosenIndex < hiddenCells.Count)
            {
                return CrystalSphereChoice.Click(hiddenCells[chosenIndex]);
            }
            if (chosenIndex == hiddenCells.Count && (proceedButton?.IsEnabled ?? false))
            {
                return CrystalSphereChoice.Proceed();
            }
        }
        catch (Exception ex)
        {
            AutoSlayLog.Warn("[RlCrystalSphere] Agent error: " + ex.Message);
        }

        return hiddenCells.Count > 0
            ? CrystalSphereChoice.Click(random.NextItem(hiddenCells))
            : CrystalSphereChoice.Wait();
    }

    private static Dictionary<string, object> CellOption(NCrystalSphereCell cell, int index)
    {
        return new Dictionary<string, object>
        {
            ["index"] = index,
            ["action"] = NonCombatBridgeProtocol.DivineCellAction,
            ["x"] = cell.Entity.X,
            ["y"] = cell.Entity.Y,
            ["enabled"] = true,
        };
    }

    private static int ReadChoiceIndex(string responseJson)
    {
        using JsonDocument doc = JsonDocument.Parse(responseJson);
        JsonElement root = doc.RootElement;
        string action = root.GetProperty("action").GetString() ?? "";
        if (action == NonCombatBridgeProtocol.ChooseAction &&
            root.TryGetProperty("index", out JsonElement indexProp))
        {
            return indexProp.GetInt32();
        }
        return -1;
    }

    private static async Task WaitForOutcome(NCrystalSphereScreen screen, CancellationToken ct)
    {
        await WaitHelper.Until(delegate
        {
            if (!GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree())
            {
                return true;
            }
            NProceedButton? proceedButton = screen.GetNodeOrNull<NProceedButton>("%ProceedButton");
            if (proceedButton?.IsEnabled ?? false)
            {
                return true;
            }
            IOverlayScreen overlayScreen = NOverlayStack.Instance?.Peek();
            return overlayScreen != null && overlayScreen != screen;
        }, ct, TimeSpan.FromSeconds(WaitForOutcomeTimeoutSeconds), "Crystal Sphere did not produce proceed or reward screen");
    }

    private static async Task ClickProceed(NCrystalSphereScreen screen, NProceedButton proceedButton, CancellationToken ct)
    {
        AutoSlayLog.Action("Clicking Crystal Sphere proceed button");
        await UiHelper.Click(proceedButton);
        await WaitHelper.Until(
            () => !GodotObject.IsInstanceValid(screen)
                || !screen.IsVisibleInTree()
                || (NMapScreen.Instance?.IsVisibleInTree() ?? false),
            ct,
            TimeSpan.FromSeconds(ProceedCloseTimeoutSeconds),
            "Crystal Sphere screen did not close after clicking proceed");
        if (GodotObject.IsInstanceValid(screen) && screen.IsVisibleInTree())
        {
            NMapScreen? mapScreen = NMapScreen.Instance;
            if (mapScreen != null && mapScreen.IsVisibleInTree())
            {
                AutoSlayLog.Info("Map opened, manually removing Crystal Sphere screen from overlay stack");
                NOverlayStack.Instance?.Remove(screen);
                await Task.Delay(OverlayRemovalDelayMs, ct);
            }
        }
    }

    private readonly record struct CrystalSphereChoice(bool ShouldProceed, NCrystalSphereCell? Cell)
    {
        public static CrystalSphereChoice Proceed() => new(true, null);

        public static CrystalSphereChoice Click(NCrystalSphereCell cell) => new(false, cell);

        public static CrystalSphereChoice Wait() => new(false, null);
    }
}
