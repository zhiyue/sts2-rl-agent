// MainFile.cs -- Entry point for the STS2 RL Bridge Mod.
//
// Strategy: Patch NGame.IsReleaseGame() to return false, which unlocks AutoSlay.
// Then construct an AutoSlayer with our RL handlers replacing the random ones,
// and start it. The TCP server (BridgeServer) provides communication with Python.

using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;

namespace STS2BridgeMod;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "STS2BridgeMod";

    private static Harmony? _harmony;
    private static RlAutoSlayer? _autoSlayer;

    public static void Initialize()
    {
        Logger.Log("=== STS2 RL Bridge Mod Initializing ===");

        // Phase 1: Harmony patches
        try
        {
            _harmony = new Harmony(ModId);

            var patchTypes = new Type[]
            {
                typeof(IsReleaseGamePatch),
                typeof(WaitSpeedPatch),
                typeof(AnimationSpeedPatch),
            };

            int patched = 0;
            foreach (var patchType in patchTypes)
            {
                try
                {
                    _harmony.CreateClassProcessor(patchType).Patch();
                    Logger.Log($"  Patched: {patchType.Name}");
                    patched++;
                }
                catch (Exception ex)
                {
                    Logger.Log($"  SKIP: {patchType.Name} - {ex.Message}");
                }
            }
            Logger.Log($"Harmony: {patched}/{patchTypes.Length} patches applied.");
        }
        catch (Exception ex)
        {
            Logger.Log($"Harmony init failed: {ex.Message}");
        }

        // Phase 2: TCP bridge server
        try
        {
            int port = 9002;
            BridgeServer.Instance.Start(port);
            Logger.Log($"TCP server started on port {port}.");
        }
        catch (Exception ex)
        {
            Logger.Log($"TCP server failed: {ex.Message}");
        }

        // Phase 3: Launch AutoSlay with RL handlers on Godot main thread.
        TaskHelper.RunSafely(LaunchRlAutoSlayAsync());
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            try
            {
                _autoSlayer?.Stop();
            }
            catch { }
            try
            {
                BridgeServer.Instance.Stop();
            }
            catch { }
        };

        Logger.Log("=== STS2 RL Bridge Mod Ready ===");
    }

    /// <summary>
    /// Wait for the game to initialize, then start an AutoSlayer
    /// with our RL agent handlers substituted in.
    /// </summary>
    private static async Task LaunchRlAutoSlayAsync()
    {
        // Wait for NGame instance
        Logger.Log("[RlAutoSlay] Waiting for NGame.Instance...");
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await WaitHelper.Until(() => NGame.Instance != null, cts.Token,
            TimeSpan.FromSeconds(60), "NGame.Instance not available");
        Logger.Log("[RlAutoSlay] NGame.Instance available.");

        // Wait for main menu to be visible before starting
        Node root = ((SceneTree)Engine.GetMainLoop()).Root;
        await WaitHelper.Until(
            () => root.GetNodeOrNull<Control>("/root/Game/RootSceneContainer/MainMenu")?.IsVisibleInTree() ?? false,
            cts.Token, TimeSpan.FromSeconds(60), "Main menu not visible");
        Logger.Log("[RlAutoSlay] Main menu visible. Creating RL AutoSlayer...");

        // Create and start the RL-driven AutoSlayer
        _autoSlayer = new RlAutoSlayer();
        string seed = SeedHelper.GetRandomSeed();
        Logger.Log($"[RlAutoSlay] Starting RL run with seed: {seed}");
        _autoSlayer.Start(seed);
    }
}

// ---------------------------------------------------------------------------
// Harmony Patches
// ---------------------------------------------------------------------------

/// <summary>
/// Patch NGame.IsReleaseGame() to return false. This unlocks the AutoSlay
/// system and other debug features needed for automation.
/// </summary>
[HarmonyPatch(typeof(NGame), nameof(NGame.IsReleaseGame))]
public static class IsReleaseGamePatch
{
    [HarmonyPrefix]
    static bool Prefix(ref bool __result)
    {
        __result = false;
        return false; // skip original
    }
}

/// <summary>Patch Cmd.CustomScaledWait to reduce all timed delays.</summary>
[HarmonyPatch(typeof(Cmd), nameof(Cmd.CustomScaledWait))]
public static class WaitSpeedPatch
{
    public static float WaitMultiplier = 0.1f;

    [HarmonyPrefix]
    static void Prefix(ref float fastSeconds, ref float standardSeconds)
    {
        fastSeconds *= WaitMultiplier;
        standardSeconds *= WaitMultiplier;
    }
}

/// <summary>Patch MegaAnimationState.SetTimeScale to speed up Spine animations.</summary>
[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimationState),
    nameof(MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimationState.SetTimeScale))]
public static class AnimationSpeedPatch
{
    public static float AnimMultiplier = 5.0f;

    [HarmonyPrefix]
    static void Prefix(ref float timeScale)
    {
        timeScale *= AnimMultiplier;
    }
}

/// <summary>
/// Logging wrapper using GD.Print for Godot console and log file output.
/// </summary>
internal static class Logger
{
    public static void Log(string message)
    {
        GD.Print($"[STS2Bridge] {message}");
    }
}
