// RlCardBundleScreenHandler.cs -- bridge-driven card bundle screen handler.

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
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace STS2BridgeMod;

public class RlCardBundleScreenHandler : IScreenHandler, IHandler
{
    private const int AgentTimeoutSeconds = 30;
    private const int HandlerTimeoutSeconds = 30;
    private const int SelectionPreviewDelayMs = 500;
    private const int ConfirmCloseTimeoutSeconds = 10;
    private const int ActDisplayIndexOffset = 1;
    private const string ScreenLogName = "NChooseABundleSelectionScreen";
    private static readonly TimeSpan AgentTimeout = TimeSpan.FromSeconds(AgentTimeoutSeconds);

    public Type ScreenType => typeof(NChooseABundleSelectionScreen);
    public TimeSpan Timeout => TimeSpan.FromSeconds(HandlerTimeoutSeconds);

    public async Task HandleAsync(Rng random, CancellationToken ct)
    {
        AutoSlayLog.EnterScreen(ScreenLogName);
        NChooseABundleSelectionScreen screen = AutoSlayer.GetCurrentScreen<NChooseABundleSelectionScreen>();
        List<NCardBundle> bundles = UiHelper.FindAll<NCardBundle>(screen).ToList();
        if (bundles.Count == 0)
        {
            AutoSlayLog.Warn("No bundles found in bundle selection screen");
            return;
        }

        NCardBundle chosenBundle = await ChooseBundle(bundles, random, ct);
        AutoSlayLog.Action("Selecting card bundle");
        await UiHelper.Click(chosenBundle.Hitbox);
        await Task.Delay(SelectionPreviewDelayMs, ct);

        NConfirmButton confirmButton = UiHelper.FindFirst<NConfirmButton>(screen);
        if (confirmButton != null)
        {
            AutoSlayLog.Action("Confirming bundle selection");
            await UiHelper.Click(confirmButton);
            await WaitHelper.Until(
                () => !GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree(),
                ct,
                TimeSpan.FromSeconds(ConfirmCloseTimeoutSeconds),
                "Bundle selection screen did not close after confirmation");
        }
        AutoSlayLog.ExitScreen(ScreenLogName);
    }

    private static async Task<NCardBundle> ChooseBundle(
        List<NCardBundle> bundles,
        Rng random,
        CancellationToken ct)
    {
        if (!BridgeServer.Instance.IsClientConnected)
        {
            return random.NextItem(bundles);
        }

        try
        {
            RunState runState = RunManager.Instance.DebugOnlyGetState();
            string stateJson = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["type"] = NonCombatBridgeProtocol.CardBundleState,
                ["bundles"] = bundles.Select((bundle, index) => BundleOption(bundle, index)).ToList(),
                ["floor"] = runState.TotalFloor,
                ["act"] = runState.CurrentActIndex + ActDisplayIndexOffset,
            });
            string? responseJson = await BridgeServer.Instance.SendStateAndWaitForActionAsync(
                stateJson,
                AgentTimeout,
                ct);
            if (responseJson == null)
            {
                return random.NextItem(bundles);
            }

            int chosenIndex = ReadChoiceIndex(responseJson);
            if (chosenIndex >= 0 && chosenIndex < bundles.Count)
            {
                return bundles[chosenIndex];
            }
        }
        catch (Exception ex)
        {
            AutoSlayLog.Warn("[RlCardBundle] Agent error: " + ex.Message);
        }

        return random.NextItem(bundles);
    }

    private static Dictionary<string, object> BundleOption(NCardBundle bundle, int index)
    {
        return new Dictionary<string, object>
        {
            ["index"] = index,
            ["action"] = NonCombatBridgeProtocol.PickCardBundleAction,
            ["cards"] = bundle.Bundle.Select(CardData).ToList(),
            ["enabled"] = true,
        };
    }

    private static Dictionary<string, object> CardData(CardModel card)
    {
        Dictionary<string, object> data = new()
        {
            ["id"] = card.Id.Entry,
            ["type"] = card.Type.ToString(),
            ["cost"] = card.EnergyCost.Canonical,
        };
        if (card.IsUpgraded)
        {
            data["upgraded"] = true;
        }
        return data;
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
}
