// ActionInjector.cs — Injects player actions into the game.
//
// All methods in this class MUST be called from the Godot main thread.
// BridgeServer dispatches actions here via Godot.Callable.From().CallDeferred().
//
// Key game classes used for action injection:
//   - CombatManager.Instance.Hand[i]  — get the CardInstance to play
//   - CombatManager.Instance.Enemies[i] — get the target CharacterState
//   - PlayCardAction — the game's internal action for playing a card
//   - EndTurnAction — the game's internal action for ending the turn
//   - ActionQueueSet.Instance.Enqueue() — queue an action for execution
//
// For non-combat actions (map, rewards, events, shop, rest):
//   - MapManager — node selection
//   - RewardScreen / CardRewardManager — card picks
//   - EventManager — event choices
//   - ShopManager — purchases
//   - RestSiteManager / CampfireManager — rest/upgrade choices

using System.Reflection;

namespace STS2BridgeMod;

/// <summary>
/// Injects actions into the game engine. All public methods must be
/// called from the Godot main thread (use CallDeferred from other threads).
/// </summary>
public static class ActionInjector
{
    /// <summary>
    /// Play a card from the hand at the given index, optionally targeting
    /// a specific enemy.
    ///
    /// Mechanism: Creates a PlayCardAction and enqueues it into the
    /// ActionQueueSet, which is the game's standard action pipeline.
    ///
    /// Game flow:
    ///   1. Get CardInstance from CombatManager.Hand[cardIndex]
    ///   2. Get target CharacterState from CombatManager.Enemies[targetIndex] (if needed)
    ///   3. Create PlayCardAction(card, target)
    ///   4. Enqueue into ActionQueueSet
    ///
    /// Alternative approach (used if PlayCardAction isn't directly accessible):
    ///   Call CombatManager.PlayCard(cardIndex, targetIndex) directly.
    /// </summary>
    public static void InjectPlayCard(int cardIndex, int targetIndex)
    {
        try
        {
            dynamic? cm = GetCombatManager();
            if (cm == null)
            {
                Logger.Log("[ActionInjector] Cannot play card: no CombatManager");
                return;
            }

            // Validate hand index
            dynamic hand = cm.Hand;
            if (cardIndex < 0 || cardIndex >= (int)hand.Count)
            {
                Logger.Log($"[ActionInjector] Invalid card index {cardIndex} (hand size: {hand.Count})");
                return;
            }

            dynamic card = hand[cardIndex];

            // Determine if we need a target
            dynamic? target = null;
            if (targetIndex >= 0)
            {
                dynamic enemies = cm.Enemies;
                if (targetIndex < (int)enemies.Count)
                {
                    target = enemies[targetIndex];
                }
                else
                {
                    Logger.Log($"[ActionInjector] Invalid target index {targetIndex}");
                    return;
                }
            }

            // Try Method 1: Call CombatManager.PlayCard directly
            // This is the simplest approach — the game handles validation internally.
            // CombatManager.PlayCard(CardInstance card, CharacterState target)
            try
            {
                if (target != null)
                    cm.PlayCard(card, target);
                else
                    cm.PlayCard(card, null);
                Logger.Log($"[ActionInjector] Played card {cardIndex} → target {targetIndex}");
                return;
            }
            catch (Exception ex)
            {
                Logger.Log($"[ActionInjector] PlayCard direct call failed: {ex.Message}");
            }

            // Try Method 2: Create PlayCardAction and enqueue it
            // PlayCardAction is the internal action object the game uses.
            try
            {
                var playCardType = FindGameType("PlayCardAction");
                if (playCardType != null)
                {
                    object action;
                    if (target != null)
                        action = Activator.CreateInstance(playCardType, card, target)!;
                    else
                        action = Activator.CreateInstance(playCardType, card)!;

                    EnqueueAction(action);
                    Logger.Log($"[ActionInjector] Enqueued PlayCardAction for card {cardIndex}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[ActionInjector] PlayCardAction creation failed: {ex.Message}");
            }

            // Try Method 3: Simulate the card click via the UI system
            // This is a last-resort approach that invokes the card's click handler.
            try
            {
                var method = card.GetType().GetMethod("OnClick") ?? card.GetType().GetMethod("Use");
                if (method != null)
                {
                    method.Invoke(card, target != null ? new object[] { target } : Array.Empty<object>());
                    Logger.Log($"[ActionInjector] Invoked card OnClick/Use for card {cardIndex}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[ActionInjector] Card click simulation failed: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[ActionInjector] Error playing card: {ex}");
        }
    }

    /// <summary>
    /// End the current turn.
    ///
    /// Mechanism: Calls CombatManager.EndTurn() or creates an EndTurnAction.
    /// This triggers:
    ///   1. Discard remaining hand
    ///   2. Execute end-of-turn effects (powers, relics)
    ///   3. Enemy turn (intents execute)
    ///   4. Start next player turn (draw, energy refill)
    /// </summary>
    public static void InjectEndTurn()
    {
        try
        {
            dynamic? cm = GetCombatManager();
            if (cm == null)
            {
                Logger.Log("[ActionInjector] Cannot end turn: no CombatManager");
                return;
            }

            // Try direct EndTurn call
            try
            {
                cm.EndTurn();
                Logger.Log("[ActionInjector] End turn executed.");
                return;
            }
            catch { }

            // Try creating and enqueuing an EndTurnAction
            try
            {
                var endTurnType = FindGameType("EndTurnAction");
                if (endTurnType != null)
                {
                    object action = Activator.CreateInstance(endTurnType)!;
                    EnqueueAction(action);
                    Logger.Log("[ActionInjector] Enqueued EndTurnAction.");
                    return;
                }
            }
            catch { }

            // Try pressing the end turn button via UI
            try
            {
                var endTurnBtn = FindGameType("EndTurnButton");
                dynamic? btn = endTurnBtn?.GetProperty("Instance")?.GetValue(null);
                if (btn != null)
                {
                    btn.OnPressed();
                    Logger.Log("[ActionInjector] End turn via button press.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[ActionInjector] Error ending turn: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[ActionInjector] Error ending turn: {ex}");
        }
    }

    /// <summary>
    /// Make a non-combat choice (map node, card reward, event option, etc).
    ///
    /// The choice index meaning depends on the current phase:
    ///   - MAP_SELECT: index into available path choices
    ///   - CARD_REWARD: index into offered cards (0..2), or skip
    ///   - EVENT: index into event options (0..N)
    ///   - SHOP: index into shop items
    ///   - REST: 0=rest(heal), 1=smith(upgrade), 2+=special options
    /// </summary>
    public static void InjectChoice(int choiceIndex)
    {
        try
        {
            string phase = GetCurrentPhase();
            Logger.Log($"[ActionInjector] Choice {choiceIndex} in phase {phase}");

            switch (phase)
            {
                case "MAP_SELECT":
                    InjectMapChoice(choiceIndex);
                    break;
                case "CARD_REWARD":
                    InjectCardRewardChoice(choiceIndex);
                    break;
                case "EVENT":
                    InjectEventChoice(choiceIndex);
                    break;
                case "SHOP":
                    InjectShopChoice(choiceIndex);
                    break;
                case "REST":
                    InjectRestChoice(choiceIndex);
                    break;
                default:
                    Logger.Log($"[ActionInjector] Cannot make choice in phase: {phase}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[ActionInjector] Error making choice: {ex}");
        }
    }

    /// <summary>
    /// Use a potion from the given slot, optionally targeting an enemy.
    ///
    /// Mechanism: Calls the potion's Use method or creates a UsePotionAction.
    /// PotionSlot maps to RunManager.Potions[slot].
    /// </summary>
    public static void InjectPotionUse(int slot, int targetIndex)
    {
        try
        {
            dynamic? rm = GetRunManager();
            if (rm == null)
            {
                Logger.Log("[ActionInjector] Cannot use potion: no RunManager");
                return;
            }

            dynamic potions = rm.Potions;
            if (slot < 0 || slot >= (int)potions.Count)
            {
                Logger.Log($"[ActionInjector] Invalid potion slot {slot}");
                return;
            }

            dynamic potion = potions[slot];

            // Determine target if needed
            dynamic? target = null;
            if (targetIndex >= 0)
            {
                dynamic? cm = GetCombatManager();
                if (cm != null)
                {
                    dynamic enemies = cm.Enemies;
                    if (targetIndex < (int)enemies.Count)
                        target = enemies[targetIndex];
                }
            }

            // Try direct use
            try
            {
                if (target != null)
                    potion.Use(target);
                else
                    potion.Use();
                Logger.Log($"[ActionInjector] Used potion slot {slot}");
                return;
            }
            catch { }

            // Try via UsePotionAction
            try
            {
                var usePotionType = FindGameType("UsePotionAction");
                if (usePotionType != null)
                {
                    object action;
                    if (target != null)
                        action = Activator.CreateInstance(usePotionType, potion, target)!;
                    else
                        action = Activator.CreateInstance(usePotionType, potion)!;
                    EnqueueAction(action);
                    Logger.Log($"[ActionInjector] Enqueued UsePotionAction for slot {slot}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[ActionInjector] Error using potion: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[ActionInjector] Error using potion: {ex}");
        }
    }

    // ----------------------------------------------------------------
    // Non-combat choice handlers
    // ----------------------------------------------------------------

    /// <summary>
    /// Select a map node to travel to.
    /// MapManager tracks available next nodes; we select by index.
    /// </summary>
    private static void InjectMapChoice(int choiceIndex)
    {
        try
        {
            var mapType = FindGameType("MapManager");
            dynamic? mm = mapType?.GetProperty("Instance")?.GetValue(null);
            if (mm == null) return;

            // MapManager.AvailableNodes or MapManager.NextNodes
            dynamic? nodes = null;
            try { nodes = mm.AvailableNodes; } catch { }
            if (nodes == null)
            {
                try { nodes = mm.NextNodes; } catch { }
            }

            if (nodes != null && choiceIndex < (int)nodes.Count)
            {
                dynamic node = nodes[choiceIndex];
                // Try SelectNode, TravelTo, or OnClick
                try { mm.SelectNode(node); return; } catch { }
                try { mm.TravelTo(node); return; } catch { }
                try { node.OnClick(); return; } catch { }
            }

            Logger.Log($"[ActionInjector] Could not select map node {choiceIndex}");
        }
        catch (Exception ex)
        {
            Logger.Log($"[ActionInjector] Map choice error: {ex.Message}");
        }
    }

    /// <summary>
    /// Pick a card from the reward screen or skip.
    /// RewardScreen.CardOptions[choiceIndex] = the card to pick.
    /// If choiceIndex equals the number of options, that means "skip".
    /// </summary>
    private static void InjectCardRewardChoice(int choiceIndex)
    {
        try
        {
            var rewardType = FindGameType("RewardScreen") ?? FindGameType("CardRewardManager");
            dynamic? rs = rewardType?.GetProperty("Instance")?.GetValue(null);
            if (rs == null) return;

            dynamic? cards = null;
            try { cards = rs.CardOptions; } catch { }
            if (cards == null)
            {
                try { cards = rs.Cards; } catch { }
            }

            if (cards != null && choiceIndex < (int)cards.Count)
            {
                dynamic card = cards[choiceIndex];
                try { rs.PickCard(card); return; } catch { }
                try { rs.SelectCard(choiceIndex); return; } catch { }
            }

            // Skip — pick no card
            try { rs.Skip(); return; } catch { }
            try { rs.Dismiss(); return; } catch { }

            Logger.Log($"[ActionInjector] Could not pick card reward {choiceIndex}");
        }
        catch (Exception ex)
        {
            Logger.Log($"[ActionInjector] Card reward choice error: {ex.Message}");
        }
    }

    /// <summary>
    /// Select an event option by index.
    /// EventManager.CurrentEvent.Options[choiceIndex].
    /// </summary>
    private static void InjectEventChoice(int choiceIndex)
    {
        try
        {
            var eventType = FindGameType("EventManager");
            dynamic? em = eventType?.GetProperty("Instance")?.GetValue(null);
            if (em == null) return;

            dynamic? evt = null;
            try { evt = em.CurrentEvent; } catch { }

            if (evt != null)
            {
                dynamic? options = null;
                try { options = evt.Options; } catch { }
                if (options == null)
                {
                    try { options = evt.Choices; } catch { }
                }

                if (options != null && choiceIndex < (int)options.Count)
                {
                    dynamic option = options[choiceIndex];
                    try { option.Choose(); return; } catch { }
                    try { option.Select(); return; } catch { }
                    try { evt.SelectOption(choiceIndex); return; } catch { }
                }
            }

            Logger.Log($"[ActionInjector] Could not select event option {choiceIndex}");
        }
        catch (Exception ex)
        {
            Logger.Log($"[ActionInjector] Event choice error: {ex.Message}");
        }
    }

    /// <summary>
    /// Buy an item from the shop by index.
    /// ShopManager.ShopItems[choiceIndex].
    /// </summary>
    private static void InjectShopChoice(int choiceIndex)
    {
        try
        {
            var shopType = FindGameType("ShopManager");
            dynamic? sm = shopType?.GetProperty("Instance")?.GetValue(null);
            if (sm == null) return;

            dynamic? items = null;
            try { items = sm.ShopItems; } catch { }
            if (items == null)
            {
                try { items = sm.Items; } catch { }
            }

            if (items != null && choiceIndex < (int)items.Count)
            {
                dynamic item = items[choiceIndex];
                try { sm.Purchase(item); return; } catch { }
                try { item.Buy(); return; } catch { }
                try { item.Purchase(); return; } catch { }
            }

            Logger.Log($"[ActionInjector] Could not buy shop item {choiceIndex}");
        }
        catch (Exception ex)
        {
            Logger.Log($"[ActionInjector] Shop choice error: {ex.Message}");
        }
    }

    /// <summary>
    /// Make a rest site choice.
    /// Index mapping: 0=Rest(heal), 1=Smith(upgrade card), 2+=special
    /// </summary>
    private static void InjectRestChoice(int choiceIndex)
    {
        try
        {
            var restType = FindGameType("RestSiteManager") ?? FindGameType("CampfireManager");
            dynamic? rm = restType?.GetProperty("Instance")?.GetValue(null);
            if (rm == null) return;

            // Try options list first
            dynamic? options = null;
            try { options = rm.Options; } catch { }
            if (options == null)
            {
                try { options = rm.Choices; } catch { }
            }

            if (options != null && choiceIndex < (int)options.Count)
            {
                dynamic option = options[choiceIndex];
                try { option.Choose(); return; } catch { }
                try { option.Select(); return; } catch { }
            }

            // Fallback: direct method calls
            switch (choiceIndex)
            {
                case 0:
                    try { rm.Rest(); return; } catch { }
                    break;
                case 1:
                    try { rm.Smith(); return; } catch { }
                    break;
            }

            Logger.Log($"[ActionInjector] Could not execute rest choice {choiceIndex}");
        }
        catch (Exception ex)
        {
            Logger.Log($"[ActionInjector] Rest choice error: {ex.Message}");
        }
    }

    // ----------------------------------------------------------------
    // Utility helpers
    // ----------------------------------------------------------------

    private static string GetCurrentPhase()
    {
        // Re-use StateSerializer's phase detection
        try
        {
            return (string)typeof(StateSerializer)
                .GetMethod("DetectCurrentPhase", BindingFlags.NonPublic | BindingFlags.Static)!
                .Invoke(null, null)!;
        }
        catch
        {
            return "UNKNOWN";
        }
    }

    /// <summary>
    /// Enqueue a game action into the ActionQueueSet.
    /// ActionQueueSet is the game's standard pipeline for executing
    /// sequential game actions.
    /// </summary>
    private static void EnqueueAction(object action)
    {
        var aqsType = FindGameType("ActionQueueSet");
        dynamic? aqs = aqsType?.GetProperty("Instance")?.GetValue(null);
        if (aqs != null)
        {
            aqs.Enqueue(action);
        }
        else
        {
            Logger.Log("[ActionInjector] Cannot find ActionQueueSet to enqueue action");
        }
    }

    private static dynamic? GetCombatManager()
    {
        try
        {
            var type = FindGameType("CombatManager");
            return type?.GetProperty("Instance")?.GetValue(null);
        }
        catch { return null; }
    }

    private static dynamic? GetRunManager()
    {
        try
        {
            var type = FindGameType("RunManager");
            return type?.GetProperty("Instance")?.GetValue(null);
        }
        catch { return null; }
    }

    private static Type? FindGameType(string shortName)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
        {
            if (!asm.GetName().Name?.Contains("sts2", StringComparison.OrdinalIgnoreCase) == true)
                continue;

            foreach (var type in asm.GetTypes())
            {
                if (type.Name == shortName)
                    return type;
            }
        }
        return null;
    }
}
