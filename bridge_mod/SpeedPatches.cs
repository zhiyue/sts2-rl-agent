// SpeedPatches.cs — Harmony patches for 5-10x game speedup.
//
// These patches accelerate animations and reduce wait times so the RL
// agent can play through combats much faster. Inspired by the
// STS2_Superfast_Mod (https://github.com/jidon333/STS2_Superfast_Mod).
//
// Patched game methods:
//
// 1. Cmd.CustomScaledWait(float fastSeconds, float standardSeconds)
//    This method is called throughout the game to add timed delays
//    between actions (e.g., between card play and damage resolution,
//    between enemy attacks). By multiplying times by 0.1, we get ~10x
//    speedup on all waits.
//
// 2. MegaAnimationState.SetTimeScale(float timeScale)
//    Controls the playback speed of Spine skeleton animations (character
//    attack anims, enemy attack anims, UI transitions). Multiplying by
//    5.0 makes all animations play 5x faster.
//
// 3. Additional optional patches for:
//    - Card draw animations
//    - Screen transition delays
//    - Damage number popup duration
//
// All patches use [HarmonyPrefix] which runs before the original method.
// They modify the parameters by reference to change the game's behavior
// without replacing the methods entirely.

using HarmonyLib;
using System.Reflection;

namespace STS2BridgeMod;

/// <summary>
/// Patch Cmd.CustomScaledWait to reduce all timed delays.
///
/// Cmd.CustomScaledWait(ref float fastSeconds, ref float standardSeconds) is
/// the primary delay mechanism in STS2's action pipeline. Every action
/// (card play, enemy attack, power trigger) uses this to wait between steps.
///
/// The game has two speed modes:
///   - standardSeconds: normal game speed
///   - fastSeconds: "fast mode" speed (toggled by player)
/// We reduce both to achieve consistent acceleration regardless of mode.
/// </summary>
[HarmonyPatch]
public static class WaitSpeedPatch
{
    /// <summary>
    /// Speed multiplier for wait times. Lower = faster.
    /// 0.1 = 10x speedup, 0.2 = 5x speedup.
    /// </summary>
    public static float WaitMultiplier = 0.1f;

    // Target method: Cmd.CustomScaledWait(float, float)
    // Cmd is in the MegaCrit.Sts2 namespace, handles coroutine-based delays.
    [HarmonyPatch(typeof(object))] // Placeholder — actual type resolved at runtime
    [HarmonyPrepare]
    static bool Prepare(MethodBase original)
    {
        // This runs during patch initialization.
        // Return true to proceed with patching.
        return true;
    }

    /// <summary>
    /// Dynamically find and patch Cmd.CustomScaledWait.
    /// Using TargetMethod() because the Cmd type might not be directly
    /// accessible at compile time through the typeof() operator.
    /// </summary>
    [HarmonyTargetMethod]
    static System.Reflection.MethodBase? TargetMethod()
    {
        try
        {
            // Search for the Cmd class in the sts2 assembly
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!asm.GetName().Name?.Contains("sts2", StringComparison.OrdinalIgnoreCase) == true)
                    continue;

                foreach (var type in asm.GetTypes())
                {
                    if (type.Name == "Cmd")
                    {
                        var method = type.GetMethod("CustomScaledWait",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                            null,
                            new[] { typeof(float), typeof(float) },
                            null);
                        if (method != null)
                        {
                            Logger.Log($"[SpeedPatches] Found Cmd.CustomScaledWait: {method}");
                            return method;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[SpeedPatches] Error finding CustomScaledWait: {ex.Message}");
        }
        Logger.Log("[SpeedPatches] WARNING: Could not find Cmd.CustomScaledWait");
        return null;
    }

    /// <summary>
    /// Prefix patch: modify wait time parameters before the method executes.
    /// Both fastSeconds and standardSeconds are reduced by WaitMultiplier.
    /// </summary>
    [HarmonyPrefix]
    static void Prefix(ref float fastSeconds, ref float standardSeconds)
    {
        fastSeconds *= WaitMultiplier;
        standardSeconds *= WaitMultiplier;
    }
}

/// <summary>
/// Patch MegaAnimationState.SetTimeScale to speed up Spine animations.
///
/// MegaAnimationState wraps the Spine runtime's AnimationState and controls
/// playback speed for all character/enemy skeleton animations. By multiplying
/// the timeScale, animations complete faster, making combat visuals zip by.
/// </summary>
[HarmonyPatch]
public static class AnimationSpeedPatch
{
    /// <summary>
    /// Animation speed multiplier. Higher = faster.
    /// 5.0 = 5x animation speed.
    /// </summary>
    public static float AnimSpeedMultiplier = 5.0f;

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
                    if (type.Name == "MegaAnimationState")
                    {
                        var method = type.GetMethod("SetTimeScale",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                            null,
                            new[] { typeof(float) },
                            null);
                        if (method != null)
                        {
                            Logger.Log($"[SpeedPatches] Found MegaAnimationState.SetTimeScale: {method}");
                            return method;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[SpeedPatches] Error finding SetTimeScale: {ex.Message}");
        }
        Logger.Log("[SpeedPatches] WARNING: Could not find MegaAnimationState.SetTimeScale");
        return null;
    }

    /// <summary>
    /// Prefix patch: multiply the animation timeScale before it's applied.
    /// </summary>
    [HarmonyPrefix]
    static void Prefix(ref float timeScale)
    {
        timeScale *= AnimSpeedMultiplier;
    }
}

/// <summary>
/// Patch card movement/draw animations for faster card drawing.
///
/// CardMoveHelper.MoveCard or similar methods control the tween duration
/// for cards moving from draw pile to hand, hand to discard, etc.
/// </summary>
[HarmonyPatch]
public static class CardAnimSpeedPatch
{
    public static float CardAnimMultiplier = 0.15f;

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
                    // Look for card movement/transition timing methods
                    if (type.Name == "CardMoveHelper" || type.Name == "CardAnimHelper")
                    {
                        // Try MoveCard, AnimateCardDraw, or similar
                        foreach (var method in type.GetMethods())
                        {
                            if (method.Name.Contains("Duration") || method.Name.Contains("Move"))
                            {
                                var parameters = method.GetParameters();
                                if (parameters.Any(p => p.ParameterType == typeof(float)))
                                {
                                    Logger.Log($"[SpeedPatches] Found card anim method: {type.Name}.{method.Name}");
                                    return method;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[SpeedPatches] Error finding card anim method: {ex.Message}");
        }
        // Not critical if not found — the main speed patches handle most delays
        return null;
    }

    [HarmonyPrefix]
    static void Prefix(ref float duration)
    {
        duration *= CardAnimMultiplier;
    }
}

/// <summary>
/// Static utility to allow runtime adjustment of speed multipliers.
/// The Python client can send a SET_SPEED action to adjust these.
/// </summary>
public static class SpeedConfig
{
    /// <summary>
    /// Set the overall speed multiplier. Affects all speed patches.
    /// 1.0 = normal speed, 10.0 = 10x faster.
    /// </summary>
    public static void SetSpeed(float multiplier)
    {
        if (multiplier < 0.1f) multiplier = 0.1f;
        if (multiplier > 100f) multiplier = 100f;

        WaitSpeedPatch.WaitMultiplier = 1.0f / multiplier;
        AnimationSpeedPatch.AnimSpeedMultiplier = multiplier;
        CardAnimSpeedPatch.CardAnimMultiplier = 1.0f / multiplier;

        Logger.Log($"[SpeedConfig] Speed set to {multiplier}x " +
                   $"(wait={WaitSpeedPatch.WaitMultiplier}, " +
                   $"anim={AnimationSpeedPatch.AnimSpeedMultiplier}, " +
                   $"card={CardAnimSpeedPatch.CardAnimMultiplier})");
    }
}
