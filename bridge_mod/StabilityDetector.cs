// StabilityDetector.cs — Detects when the game is idle and ready for commands.
//
// The game processes actions through an ActionQueueSet. Actions are queued
// and executed sequentially. We need to detect when:
//   1. The ActionQueue is empty (no actions being processed)
//   2. No animations are playing
//   3. The game is waiting for player input
//
// At that point, we serialize the state and send it to the Python client.
//
// Hooks:
//   - ActionQueueSet.ProcessQueue (Postfix) — fires after each queue cycle
//   - CombatManager.StartPlayerTurn (Postfix) — fires when player turn begins
//   - CombatManager.EndCombat (Postfix) — fires when combat ends
//   - Various non-combat entry points for map/event/shop/rest phases
//
// The StabilityDetector also implements a debounce timer to avoid sending
// multiple state updates in rapid succession during action resolution.

using HarmonyLib;
using System.Diagnostics;

namespace STS2BridgeMod;

/// <summary>
/// Central stability tracker. Hooks into the game's action pipeline to
/// detect when the game is idle (awaiting player input) and sends the
/// current state to the connected Python client.
/// </summary>
public static class StabilityDetector
{
    // Minimum milliseconds between state sends to avoid flooding
    private const int DebounceMs = 50;

    // Track when we last sent state to prevent rapid-fire updates
    private static readonly Stopwatch _lastSendTimer = Stopwatch.StartNew();

    // Flag to prevent re-entrant state sends
    private static bool _sending = false;

    /// <summary>
    /// Whether the game is currently considered "idle" (ready for input).
    /// Set by the various hooks below.
    /// </summary>
    public static bool IsIdle { get; private set; }

    /// <summary>
    /// Called when we detect the game is idle and ready for player input.
    /// Serializes the current state and sends it to the Python client.
    /// </summary>
    public static void OnGameIdle()
    {
        if (_sending) return;
        if (!BridgeServer.Instance.IsClientConnected) return;

        // Debounce: don't send more often than every DebounceMs
        if (_lastSendTimer.ElapsedMilliseconds < DebounceMs)
            return;

        try
        {
            _sending = true;
            IsIdle = true;

            string stateJson = StateSerializer.SerializeGameState();
            BridgeServer.Instance.SendState(stateJson);

            _lastSendTimer.Restart();
        }
        catch (Exception ex)
        {
            Logger.Log($"[StabilityDetector] Error in OnGameIdle: {ex.Message}");
        }
        finally
        {
            _sending = false;
        }
    }

    /// <summary>
    /// Called when an action starts executing (game is no longer idle).
    /// </summary>
    public static void OnActionStarted()
    {
        IsIdle = false;
    }
}

// =====================================================================
// Harmony Patches — Hook into game lifecycle to detect idle states
// =====================================================================

/// <summary>
/// Hook: ActionQueueSet queue processing.
///
/// ActionQueueSet.ProcessQueue() is called on every game frame to advance
/// the action queue. When the queue becomes empty, the game is idle.
///
/// ActionQueueSet is the central action pipeline:
///   - PlayCardAction, EndTurnAction, DamageAction, etc. are queued
///   - ProcessQueue() dequeues and executes them one at a time
///   - When empty → game waits for next player input
/// </summary>
[HarmonyPatch]
public static class ActionQueueIdlePatch
{
    [HarmonyTargetMethod]
    static System.Reflection.MethodBase? TargetMethod()
    {
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!asm.GetName().Name?.Contains("sts2", StringComparison.OrdinalIgnoreCase) == true)
                    continue;

                foreach (var type in asm.GetTypes())
                {
                    if (type.Name == "ActionQueueSet")
                    {
                        // Look for ProcessQueue, Update, or Tick method
                        var method = type.GetMethod("ProcessQueue")
                            ?? type.GetMethod("Update")
                            ?? type.GetMethod("Tick");
                        if (method != null)
                        {
                            Logger.Log($"[StabilityDetector] Hooked {type.Name}.{method.Name}");
                            return method;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[StabilityDetector] Error finding ActionQueueSet: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// After the queue processes, check if it's now empty.
    /// If the queue is empty, the game is idle and waiting for input.
    /// </summary>
    [HarmonyPostfix]
    static void Postfix(object __instance)
    {
        try
        {
            // Check if the queue is empty
            // ActionQueueSet.IsEmpty or ActionQueueSet.Count == 0
            bool isEmpty = false;
            try
            {
                var isEmptyProp = __instance.GetType().GetProperty("IsEmpty");
                if (isEmptyProp != null)
                {
                    isEmpty = (bool)isEmptyProp.GetValue(__instance)!;
                }
                else
                {
                    var countProp = __instance.GetType().GetProperty("Count");
                    if (countProp != null)
                    {
                        isEmpty = (int)countProp.GetValue(__instance)! == 0;
                    }
                }
            }
            catch { }

            if (isEmpty)
            {
                StabilityDetector.OnGameIdle();
            }
            else
            {
                StabilityDetector.OnActionStarted();
            }
        }
        catch { }
    }
}

/// <summary>
/// Hook: CombatManager.StartPlayerTurn()
///
/// Called at the beginning of each player turn after cards are drawn
/// and energy is refreshed. This is a reliable signal that the game
/// is ready for the player to play cards.
/// </summary>
[HarmonyPatch]
public static class PlayerTurnStartPatch
{
    [HarmonyTargetMethod]
    static System.Reflection.MethodBase? TargetMethod()
    {
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!asm.GetName().Name?.Contains("sts2", StringComparison.OrdinalIgnoreCase) == true)
                    continue;

                foreach (var type in asm.GetTypes())
                {
                    if (type.Name == "CombatManager")
                    {
                        var method = type.GetMethod("StartPlayerTurn")
                            ?? type.GetMethod("BeginPlayerTurn")
                            ?? type.GetMethod("OnPlayerTurnStart");
                        if (method != null)
                        {
                            Logger.Log($"[StabilityDetector] Hooked {type.Name}.{method.Name}");
                            return method;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[StabilityDetector] Error finding StartPlayerTurn: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// After the player turn starts (cards drawn, energy set),
    /// signal that the game is idle and ready for card play actions.
    /// </summary>
    [HarmonyPostfix]
    static void Postfix()
    {
        // Small delay to let draw animations complete before sending state
        // The ActionQueueSet idle hook will fire shortly after
        StabilityDetector.OnGameIdle();
    }
}

/// <summary>
/// Hook: CombatManager.EndCombat() or similar.
///
/// Fires when combat ends (win or loss). We send a final state update
/// so the agent knows the combat result.
/// </summary>
[HarmonyPatch]
public static class CombatEndPatch
{
    [HarmonyTargetMethod]
    static System.Reflection.MethodBase? TargetMethod()
    {
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!asm.GetName().Name?.Contains("sts2", StringComparison.OrdinalIgnoreCase) == true)
                    continue;

                foreach (var type in asm.GetTypes())
                {
                    if (type.Name == "CombatManager")
                    {
                        var method = type.GetMethod("EndCombat")
                            ?? type.GetMethod("CombatEnded")
                            ?? type.GetMethod("OnCombatEnd");
                        if (method != null)
                        {
                            Logger.Log($"[StabilityDetector] Hooked {type.Name}.{method.Name}");
                            return method;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[StabilityDetector] Error finding EndCombat: {ex.Message}");
        }
        return null;
    }

    [HarmonyPostfix]
    static void Postfix()
    {
        Logger.Log("[StabilityDetector] Combat ended — sending final state.");
        StabilityDetector.OnGameIdle();
    }
}

/// <summary>
/// Hook: Map/Event/Shop/Rest entry points.
///
/// These fire when the player enters non-combat game phases.
/// We need to send state so the agent can make navigation decisions.
/// </summary>
[HarmonyPatch]
public static class MapEntryPatch
{
    [HarmonyTargetMethod]
    static System.Reflection.MethodBase? TargetMethod()
    {
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!asm.GetName().Name?.Contains("sts2", StringComparison.OrdinalIgnoreCase) == true)
                    continue;

                foreach (var type in asm.GetTypes())
                {
                    if (type.Name == "MapManager")
                    {
                        var method = type.GetMethod("ShowMap")
                            ?? type.GetMethod("EnterMap")
                            ?? type.GetMethod("OnMapOpen");
                        if (method != null)
                        {
                            Logger.Log($"[StabilityDetector] Hooked {type.Name}.{method.Name}");
                            return method;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[StabilityDetector] Error finding MapManager entry: {ex.Message}");
        }
        return null;
    }

    [HarmonyPostfix]
    static void Postfix()
    {
        StabilityDetector.OnGameIdle();
    }
}

/// <summary>
/// Hook: Card reward screen entry.
/// </summary>
[HarmonyPatch]
public static class CardRewardEntryPatch
{
    [HarmonyTargetMethod]
    static System.Reflection.MethodBase? TargetMethod()
    {
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!asm.GetName().Name?.Contains("sts2", StringComparison.OrdinalIgnoreCase) == true)
                    continue;

                foreach (var type in asm.GetTypes())
                {
                    if (type.Name == "RewardScreen" || type.Name == "CardRewardManager")
                    {
                        var method = type.GetMethod("Show")
                            ?? type.GetMethod("Open")
                            ?? type.GetMethod("OnShow");
                        if (method != null)
                        {
                            Logger.Log($"[StabilityDetector] Hooked {type.Name}.{method.Name} (card reward)");
                            return method;
                        }
                    }
                }
            }
        }
        catch { }
        return null;
    }

    [HarmonyPostfix]
    static void Postfix()
    {
        StabilityDetector.OnGameIdle();
    }
}

/// <summary>
/// Hook: Event entry.
/// </summary>
[HarmonyPatch]
public static class EventEntryPatch
{
    [HarmonyTargetMethod]
    static System.Reflection.MethodBase? TargetMethod()
    {
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!asm.GetName().Name?.Contains("sts2", StringComparison.OrdinalIgnoreCase) == true)
                    continue;

                foreach (var type in asm.GetTypes())
                {
                    if (type.Name == "EventManager")
                    {
                        var method = type.GetMethod("StartEvent")
                            ?? type.GetMethod("ShowEvent")
                            ?? type.GetMethod("OnEventStart");
                        if (method != null)
                        {
                            Logger.Log($"[StabilityDetector] Hooked {type.Name}.{method.Name} (event)");
                            return method;
                        }
                    }
                }
            }
        }
        catch { }
        return null;
    }

    [HarmonyPostfix]
    static void Postfix()
    {
        StabilityDetector.OnGameIdle();
    }
}
