// RlMapHandler.cs -- RL-agent-driven map navigation handler.
//
// Replaces AutoSlay's MapScreenHandler. Instead of picking the first child
// of the current map point, this handler:
//   1. Enumerates available map nodes (the reachable next nodes)
//   2. Sends them to Python with their types (Monster, Elite, Shop, etc.)
//   3. Waits for the agent to choose a node index
//   4. Clicks the chosen node
//
// Falls back to random selection if Python is disconnected or times out.

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
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace STS2BridgeMod;

public class RlMapHandler : IScreenHandler, IHandler
{
    private static readonly TimeSpan AgentTimeout = TimeSpan.FromSeconds(30);

    private TaskCompletionSource? _roomEnteredTcs;

    public Type ScreenType => typeof(NMapScreen);
    public TimeSpan Timeout => TimeSpan.FromSeconds(60);

    public async Task HandleAsync(Rng random, CancellationToken ct)
    {
        Logger.Log("[RlMap] Handling map screen");
        Node root = ((SceneTree)Engine.GetMainLoop()).Root;
        NRun runNode = root.GetNode<NRun>("/root/Game/RootSceneContainer/Run");

        await WaitHelper.Until(
            () => runNode.GlobalUi.MapScreen.IsVisibleInTree(), ct,
            AutoSlayConfig.mapScreenTimeout, "Map screen not visible");

        List<NMapPoint> allPoints = UiHelper.FindAll<NMapPoint>(runNode.GlobalUi.MapScreen);
        RunState runState = RunManager.Instance.DebugOnlyGetState();

        // Determine available next nodes
        List<NMapPoint> availableNodes;
        if (runState.VisitedMapCoords.Count == 0)
        {
            // First room selection: all nodes in row 0
            availableNodes = allPoints
                .Where(mp => mp.Point.coord.row == 0)
                .ToList();
        }
        else
        {
            // Get the children of the last visited node
            IReadOnlyList<MapCoord> visited = runState.VisitedMapCoords;
            MapCoord lastCoord = visited[visited.Count - 1];
            NMapPoint lastNode = allPoints.First(
                mp => mp.Point.coord.Equals(lastCoord));
            HashSet<MapCoord> childCoords = new HashSet<MapCoord>(
                lastNode.Point.Children.Select(c => c.coord));
            availableNodes = allPoints
                .Where(mp => childCoords.Contains(mp.Point.coord))
                .ToList();
        }

        if (availableNodes.Count == 0)
        {
            Logger.Log("[RlMap] No available nodes found!");
            return;
        }

        // Build the state message for Python
        var nodes = new List<Dictionary<string, object>>();
        for (int i = 0; i < availableNodes.Count; i++)
        {
            NMapPoint mp = availableNodes[i];
            nodes.Add(new Dictionary<string, object>
            {
                ["index"] = i,
                ["type"] = mp.Point.PointType.ToString(),
                ["row"] = mp.Point.coord.row,
                ["col"] = mp.Point.coord.col,
            });
        }

        var stateMsg = new Dictionary<string, object>
        {
            ["type"] = "map_select",
            ["nodes"] = nodes,
            ["floor"] = runState.TotalFloor,
            ["act"] = runState.CurrentActIndex + 1,
        };

        NMapPoint chosenNode;

        if (BridgeServer.Instance.IsClientConnected)
        {
            try
            {
                string stateJson = JsonSerializer.Serialize(stateMsg);
                string responseJson = await BridgeServer.Instance.SendStateAndWaitForActionAsync(
                    stateJson,
                    AgentTimeout, ct);

                if (responseJson != null)
                {
                    using var doc = JsonDocument.Parse(responseJson);
                    var rRoot = doc.RootElement;
                    int chosenIndex = rRoot.GetProperty("index").GetInt32();

                    if (chosenIndex >= 0 && chosenIndex < availableNodes.Count)
                    {
                        chosenNode = availableNodes[chosenIndex];
                        Logger.Log(
                            $"[RlMap] Agent chose node {chosenIndex}: {chosenNode.Point.PointType} at ({chosenNode.Point.coord.row},{chosenNode.Point.coord.col})");
                    }
                    else
                    {
                        Logger.Log($"[RlMap] Invalid index {chosenIndex}, falling back to random");
                        chosenNode = random.NextItem(availableNodes);
                    }
                }
                else
                {
                    Logger.Log("[RlMap] No response from agent, falling back to random");
                    chosenNode = random.NextItem(availableNodes);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[RlMap] Agent error: {ex.Message}, falling back to random");
                chosenNode = random.NextItem(availableNodes);
            }
        }
        else
        {
            Logger.Log("[RlMap] No agent connected, selecting random node");
            chosenNode = random.NextItem(availableNodes);
        }

        // Wait for the node to be enabled and click it
        await WaitHelper.Until(() => chosenNode.IsEnabled, ct,
            TimeSpan.FromSeconds(10), "Map point not enabled");

        _roomEnteredTcs = new TaskCompletionSource();
        RunManager.Instance.RoomEntered += OnRoomEntered;
        try
        {
            await UiHelper.Click(chosenNode);
            await WaitHelper.ForTask(_roomEnteredTcs.Task, ct,
                AutoSlayConfig.mapScreenTimeout, "Room not entered after map click");
        }
        finally
        {
            RunManager.Instance.RoomEntered -= OnRoomEntered;
            _roomEnteredTcs = null;
        }

        Logger.Log("[RlMap] Map navigation complete");
    }

    private void OnRoomEntered()
    {
        _roomEnteredTcs?.TrySetResult();
    }
}
