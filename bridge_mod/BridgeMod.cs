// BridgeMod.cs — Entry point for the STS2 RL Bridge Mod.
//
// Uses [ModInitializer] attribute recognized by the STS2 mod loader.
// On initialization:
//   1. Applies all Harmony patches (speed patches, stability hooks)
//   2. Starts the TCP bridge server on port 9002
//
// The mod loader scans for classes with [ModInitializer] and calls the
// specified static method once at game startup.

using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace STS2BridgeMod;

/// <summary>
/// Mod entry point. The STS2 mod loader discovers this class via
/// the [ModInitializer] attribute and calls Initialize() at startup.
/// </summary>
[ModInitializer("Initialize")]
public class BridgeMod
{
    private static Harmony? _harmony;

    /// <summary>
    /// Called once by the mod loader. Sets up Harmony patches and starts
    /// the TCP bridge server for RL agent communication.
    /// </summary>
    public static void Initialize()
    {
        // Phase 1: Just verify mod loading works
        try
        {
            Logger.Log("[STS2Bridge] === Bridge Mod Loading ===");
            Logger.Log("[STS2Bridge] Phase 1: Mod entry point reached.");
        }
        catch (Exception ex)
        {
            Godot.GD.Print($"[STS2Bridge] FATAL: {ex}");
        }

        // Phase 2: Test Harmony
        try
        {
            _harmony = new Harmony("sts2.bridge.rl");
            Logger.Log("[STS2Bridge] Phase 2: Harmony instance created.");
        }
        catch (Exception ex)
        {
            Logger.Log($"[STS2Bridge] Phase 2 FAILED: {ex.Message}");
            return;
        }

        // Phase 3: TCP server (on background thread)
        try
        {
            int port = 9002;
            BridgeServer.Instance.Start(port);
            Logger.Log($"[STS2Bridge] Phase 3: TCP server started on port {port}.");
        }
        catch (Exception ex)
        {
            Logger.Log($"[STS2Bridge] Phase 3 FAILED (TCP): {ex.Message}");
        }

        // Phase 4: Harmony patches (skip for now - enable one by one later)
        Logger.Log("[STS2Bridge] Phase 4: Skipping Harmony patches for initial test.");
        Logger.Log("[STS2Bridge] === Bridge Mod Ready ===");
    }
}

/// <summary>
/// Simple logging wrapper. Uses Godot's GD.Print so output appears
/// in the game console and log file.
/// </summary>
internal static class Logger
{
    public static void Log(string message)
    {
        Godot.GD.Print(message);
    }
}
