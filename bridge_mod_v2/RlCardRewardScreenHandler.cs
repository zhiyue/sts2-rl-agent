// RlCardRewardScreenHandler.cs -- RL-agent-driven card reward screen handler.
//
// This handles the NCardRewardSelectionScreen overlay. When the card selector
// (RlCardSelector) is active, this screen may not appear because CardSelectCmd
// bypasses it. But if it does appear (e.g. due to some code path not using
// Selector), this handler sends the options to Python and clicks the chosen card.
//
// Falls back to random selection if Python is disconnected or times out.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.AutoSlay.Handlers;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Random;

namespace STS2BridgeMod;

public class RlCardRewardScreenHandler : IScreenHandler, IHandler
{
    private static readonly TimeSpan AgentTimeout = TimeSpan.FromSeconds(30);

    public Type ScreenType => typeof(NCardRewardSelectionScreen);
    public TimeSpan Timeout => TimeSpan.FromSeconds(30);

    public async Task HandleAsync(Rng random, CancellationToken ct)
    {
        Logger.Log("[RlCardReward] Card reward screen appeared");
        NCardRewardSelectionScreen screen =
            (NCardRewardSelectionScreen)NOverlayStack.Instance.Peek();
        await Task.Delay(400, ct);

        List<NCardHolder> holders = UiHelper.FindAll<NCardHolder>(screen);
        if (holders.Count == 0)
        {
            Logger.Log("[RlCardReward] No card holders found");
            return;
        }

        // Build state message
        var cards = new List<Dictionary<string, object>>();
        for (int i = 0; i < holders.Count; i++)
        {
            cards.Add(new Dictionary<string, object> { ["index"] = i });
        }

        var stateMsg = new Dictionary<string, object>
        {
            ["type"] = "card_reward",
            ["cards"] = cards,
            ["can_skip"] = true,
        };

        NCardHolder chosenHolder = null;

        if (BridgeServer.Instance.IsClientConnected)
        {
            try
            {
                string stateJson = JsonSerializer.Serialize(stateMsg);
                BridgeServer.Instance.SendState(stateJson);
                string responseJson = await BridgeServer.Instance.WaitForActionAsync(
                    AgentTimeout, ct);

                if (responseJson != null)
                {
                    using var doc = JsonDocument.Parse(responseJson);
                    var root = doc.RootElement;
                    string action = root.GetProperty("action").GetString() ?? "";

                    if (action == "skip")
                    {
                        Logger.Log("[RlCardReward] Agent chose to skip");
                        // Click the skip/close button if available
                        // The screen will close via the drain loop
                        return;
                    }

                    if (action == "choose" &&
                        root.TryGetProperty("index", out var idxProp))
                    {
                        int idx = idxProp.GetInt32();
                        if (idx >= 0 && idx < holders.Count)
                        {
                            chosenHolder = holders[idx];
                            Logger.Log($"[RlCardReward] Agent chose card at index {idx}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[RlCardReward] Agent error: {ex.Message}");
            }
        }

        // Fallback to random
        if (chosenHolder == null)
        {
            Logger.Log("[RlCardReward] Falling back to random selection");
            chosenHolder = random.NextItem(holders);
        }

        chosenHolder.EmitSignal(NCardHolder.SignalName.Pressed, chosenHolder);
        await WaitHelper.Until(
            () => !GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree(),
            ct, TimeSpan.FromSeconds(10),
            "Card reward screen did not close after selection");
        Logger.Log("[RlCardReward] Card reward screen handled");
    }
}
