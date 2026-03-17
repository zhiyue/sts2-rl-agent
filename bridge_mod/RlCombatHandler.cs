// RlCombatHandler.cs -- RL-agent-driven combat handler.
//
// Replaces AutoSlay's CombatRoomHandler. Instead of applying god-mode buffs
// and playing random cards, this handler:
//   1. Waits for combat to start and the play phase
//   2. Serializes the combat state to JSON
//   3. Sends state to the Python RL agent via BridgeServer
//   4. Waits for the agent's response (play card or end turn)
//   5. Executes the action using CardCmd.AutoPlay or PlayerCmd.EndTurn
//   6. Loops until combat ends
//
// If the Python agent is not connected or times out, falls back to random play.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.AutoSlay;
using MegaCrit.Sts2.Core.AutoSlay.Handlers;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace STS2BridgeMod;

public class RlCombatHandler : IRoomHandler, IHandler
{
    private static readonly TimeSpan AgentTimeout = TimeSpan.FromSeconds(30);
    private const int MaxRlHandSlots = 10;

    public RoomType[] HandledTypes => new RoomType[]
    {
        RoomType.Monster, RoomType.Elite, RoomType.Boss
    };

    public TimeSpan Timeout => TimeSpan.FromMinutes(10);

    public async Task HandleAsync(Rng random, CancellationToken ct)
    {
        Logger.Log("[RlCombat] Waiting for combat to start");
        await WaitHelper.Until(
            () => CombatManager.Instance.IsInProgress, ct,
            AutoSlayConfig.nodeWaitTimeout, "Combat not started");

        Logger.Log("[RlCombat] Combat started");
        Player player = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState());

        int turnCount = 0;
        while (CombatManager.Instance.IsInProgress && turnCount < 200)
        {
            ct.ThrowIfCancellationRequested();
            turnCount++;

            // Wait for play phase
            await WaitHelper.Until(
                () => CombatManager.Instance.IsPlayPhase ||
                      !CombatManager.Instance.IsInProgress,
                ct, TimeSpan.FromSeconds(30), "Play phase not started");

            if (!CombatManager.Instance.IsInProgress)
                break;

            RlAutoSlayer.CurrentWatchdog?.Reset($"Combat turn {turnCount}");
            Logger.Log($"[RlCombat] Turn {turnCount}: awaiting agent decision");

            int cardsPlayed = 0;
            bool turnEnded = false;

            while (!turnEnded && cardsPlayed < 50 && CombatManager.Instance.IsPlayPhase)
            {
                ct.ThrowIfCancellationRequested();

                if (cardsPlayed > 0 && cardsPlayed % 10 == 0)
                {
                    RlAutoSlayer.CurrentWatchdog?.Reset(
                        $"Combat turn {turnCount}, played {cardsPlayed} cards");
                }

                // Serialize combat state
                string stateJson = SerializeCombatState(player);

                // Send to Python and wait for response
                string responseJson = null;
                bool clientConnected = BridgeServer.Instance.IsClientConnected;
                Logger.Log($"[RlCombat] Client connected: {clientConnected}, sending state ({stateJson.Length} bytes)");
                if (clientConnected)
                {
                    try
                    {
                        Logger.Log("[RlCombat] State sent, waiting for agent response...");
                        responseJson = await BridgeServer.Instance.SendStateAndWaitForActionAsync(
                            stateJson,
                            AgentTimeout, ct);
                        Logger.Log($"[RlCombat] Agent response: {responseJson ?? "null"}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"[RlCombat] Agent communication error: {ex.Message}");
                    }
                }

                // Parse and execute the response, or fall back to random
                if (responseJson != null)
                {
                    turnEnded = await ExecuteAgentAction(
                        responseJson, player, random, ct);
                }
                else
                {
                    Logger.Log("[RlCombat] No agent response, falling back to random");
                    turnEnded = await PlayRandomFallback(player, random, ct);
                }

                if (!turnEnded)
                    cardsPlayed++;

                await Task.Delay(100, ct);
            }

            // If we ran out of cards to play without ending turn, end it
            if (CombatManager.Instance.IsPlayPhase && CombatManager.Instance.IsInProgress && !turnEnded)
            {
                PlayerCmd.EndTurn(player, canBackOut: false);
            }
        }

        await WaitHelper.Until(
            () => !CombatManager.Instance.IsInProgress, ct,
            TimeSpan.FromSeconds(30), "Combat did not end");
        Logger.Log("[RlCombat] Combat finished");
    }

    /// <summary>
    /// Execute an action from the Python agent response JSON.
    /// Returns true if turn was ended, false if a card or potion was used.
    /// </summary>
    private async Task<bool> ExecuteAgentAction(
        string json, Player player, Rng random, CancellationToken ct)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            string action = root.GetProperty("action").GetString() ?? "";

            switch (action.ToLowerInvariant())
            {
                case "play":
                {
                    int cardIndex = root.GetProperty("card_index").GetInt32();
                    int targetIndex = root.TryGetProperty("target_index", out var ti)
                        ? ti.GetInt32() : -1;

                    if (cardIndex >= MaxRlHandSlots)
                    {
                        int potionSlot = cardIndex - MaxRlHandSlots;
                        Logger.Log($"[RlCombat] Using potion slot {potionSlot} -> target_index {targetIndex}");
                        await UsePotionAndWaitAsync(player, potionSlot, targetIndex, ct);
                        return false;
                    }

                    CardPile hand = PileType.Hand.GetPile(player);
                    if (cardIndex < 0 || cardIndex >= hand.Cards.Count)
                    {
                        Logger.Log($"[RlCombat] Invalid card_index {cardIndex}, hand size {hand.Cards.Count}");
                        return false;
                    }

                    CardModel card = hand.Cards[cardIndex];

                    UnplayableReason reason;
                    AbstractModel preventer;
                    if (!card.CanPlay(out reason, out preventer))
                    {
                        Logger.Log($"[RlCombat] Card {card.Id.Entry} not playable: {reason}");
                        return false;
                    }

                    Creature target = ResolveTarget(card, targetIndex);
                    if (card.TargetType == TargetType.AnyEnemy && target == null)
                    {
                        Logger.Log($"[RlCombat] Invalid target_index {targetIndex} for {card.Id.Entry}");
                        return false;
                    }
                    Logger.Log($"[RlCombat] Playing card: {card.Id.Entry} -> target_index {targetIndex}");

                    await PlayCardAndWaitAsync(player, card, target, ct);
                    return false;
                }

                case "end_turn":
                {
                    Logger.Log("[RlCombat] Agent chose to end turn");
                    PlayerCmd.EndTurn(player, canBackOut: false);
                    return true;
                }

                case "potion":
                {
                    int slot = root.GetProperty("slot").GetInt32();
                    int targetIndex = root.TryGetProperty("target_index", out var ti)
                        ? ti.GetInt32() : -1;
                    Logger.Log($"[RlCombat] Using potion slot {slot} -> target_index {targetIndex}");
                    await UsePotionAndWaitAsync(player, slot, targetIndex, ct);
                    return false;
                }

                default:
                    Logger.Log($"[RlCombat] Unknown action: {action}");
                    return false;
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[RlCombat] Error executing agent action: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Fallback: play a random playable card, then end turn.
    /// Returns true (turn ended).
    /// </summary>
    private async Task<bool> PlayRandomFallback(
        Player player, Rng random, CancellationToken ct)
    {
        CardPile hand = PileType.Hand.GetPile(player);
        UnplayableReason reason;
        AbstractModel preventer;
        List<CardModel> playable = hand.Cards
            .Where(c => c.CanPlay(out reason, out preventer))
            .ToList();

        if (playable.Count > 0)
        {
            CardModel card = random.NextItem(playable);
            Creature target = GetRandomTarget(card, random);
            Logger.Log($"[RlCombat] Random fallback: playing {card.Id.Entry}");
            await PlayCardAndWaitAsync(player, card, target, ct);
            return false;
        }
        else
        {
            Logger.Log("[RlCombat] Random fallback: no playable cards, ending turn");
            PlayerCmd.EndTurn(player, canBackOut: false);
            return true;
        }
    }

    /// <summary>
    /// Resolve a target creature from the target_index.
    /// </summary>
    private Creature? ResolveTarget(CardModel card, int targetIndex)
    {
        if (card.TargetType != TargetType.AnyEnemy)
            return null;

        CombatState combatState = card.CombatState;
        if (combatState == null)
            return null;

        List<Creature> allEnemies = combatState.Enemies.ToList();
        if (allEnemies.Count == 0)
            return null;

        if (targetIndex >= 0)
        {
            if (targetIndex >= allEnemies.Count)
                return null;
            Creature indexedEnemy = allEnemies[targetIndex];
            return indexedEnemy.IsHittable ? indexedEnemy : null;
        }

        return combatState.HittableEnemies.FirstOrDefault();
    }

    private static Creature? ResolvePotionTarget(Player player, PotionModel? potion, int targetIndex)
    {
        if (potion == null)
            return null;

        string targetType = "Self";
        try
        {
            targetType = potion.TargetType?.ToString() ?? "Self";
        }
        catch
        {
            return player.Creature;
        }

        if (targetType == "AnyEnemy")
        {
            CombatState? combatState = player.Creature?.CombatState;
            if (combatState == null)
                return null;
            List<Creature> allEnemies = combatState.Enemies.ToList();
            if (targetIndex >= 0)
            {
                if (targetIndex >= allEnemies.Count)
                    return null;
                Creature indexedEnemy = allEnemies[targetIndex];
                return indexedEnemy.IsHittable ? indexedEnemy : null;
            }
            return combatState.HittableEnemies.FirstOrDefault();
        }

        if (targetType == "Self" || targetType == "AnyPlayer")
            return player.Creature;

        return null;
    }

    private static Creature? GetRandomTarget(CardModel card, Rng random)
    {
        if (card.TargetType != TargetType.AnyEnemy)
            return null;
        CombatState combatState = card.CombatState;
        if (combatState == null)
            return null;
        List<Creature> hittable = combatState.HittableEnemies.ToList();
        if (hittable.Count == 0)
            return null;
        return random.NextItem(hittable);
    }

    private static async Task PlayCardAndWaitAsync(
        Player player, CardModel card, Creature? target, CancellationToken ct)
    {
        var playAction = new PlayCardAction(card, target);
        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(playAction);

        int energyBefore = player.PlayerCombatState?.Energy ?? -1;
        int handBefore = PileType.Hand.GetPile(player).Cards.Count;
        int waitMs = 0;
        while (waitMs < 3000)
        {
            int energyNow = player.PlayerCombatState?.Energy ?? -1;
            int handNow = PileType.Hand.GetPile(player).Cards.Count;
            if (energyNow != energyBefore || handNow != handBefore
                || !CombatManager.Instance.IsPlayPhase
                || !CombatManager.Instance.IsInProgress)
                break;
            await Task.Delay(50, ct);
            waitMs += 50;
        }
    }

    private static async Task UsePotionAndWaitAsync(
        Player player, int slot, int targetIndex, CancellationToken ct)
    {
        if (slot < 0)
            return;

        dynamic potion = null;
        try
        {
            potion = player.GetPotionAtSlotIndex(slot);
        }
        catch
        {
            return;
        }

        if (potion == null)
            return;

        Creature? target = ResolvePotionTarget(player, potion, targetIndex);
        if (potion.TargetType.ToString() == "AnyEnemy" && target == null)
            return;

        var usePotionAction = new UsePotionAction(
            potion,
            target,
            CombatManager.Instance.IsInProgress
        );
        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(usePotionAction);

        int waitMs = 0;
        while (waitMs < 3000)
        {
            dynamic potionNow = null;
            try
            {
                potionNow = player.GetPotionAtSlotIndex(slot);
            }
            catch
            {
                potionNow = null;
            }

            if (potionNow == null || !CombatManager.Instance.IsPlayPhase || !CombatManager.Instance.IsInProgress)
                break;
            await Task.Delay(50, ct);
            waitMs += 50;
        }
    }

    // ----------------------------------------------------------------
    // State serialization
    // ----------------------------------------------------------------

    private string SerializeCombatState(Player player)
    {
        try
        {
            var cm = CombatManager.Instance;
            CombatState combatState = cm.DebugOnlyGetState();
            Creature playerCreature = player.Creature;
            PlayerCombatState pcs = player.PlayerCombatState;

            Logger.Log($"[RlCombat] Serialize: cm={cm != null}, cs={combatState != null}, creature={playerCreature != null}, pcs={pcs != null}");
            if (playerCreature != null)
                Logger.Log($"[RlCombat] Player: HP={playerCreature.CurrentHp}/{playerCreature.MaxHp} Block={playerCreature.Block}");
            if (pcs != null)
                Logger.Log($"[RlCombat] Energy={pcs.Energy}/{pcs.MaxEnergy} Hand={pcs.Hand.Cards.Count} Draw={pcs.DrawPile.Cards.Count}");
            if (combatState != null)
                Logger.Log($"[RlCombat] Enemies={combatState.Enemies.Count()} Round={combatState.RoundNumber}");

            // Player info
            var playerObj = new Dictionary<string, object>
            {
                ["hp"] = playerCreature.CurrentHp,
                ["max_hp"] = playerCreature.MaxHp,
                ["block"] = playerCreature.Block,
                ["energy"] = pcs?.Energy ?? 0,
                ["max_energy"] = pcs?.MaxEnergy ?? 3,
            };

            // Player powers
            var powers = new List<Dictionary<string, object>>();
            foreach (PowerModel power in playerCreature.Powers)
            {
                powers.Add(new Dictionary<string, object>
                {
                    ["id"] = power.Id.Entry,
                    ["amount"] = power.Amount,
                });
            }
            if (powers.Count > 0)
                playerObj["powers"] = powers;

            // Hand cards
            var handCards = new List<Dictionary<string, object>>();
            if (pcs != null)
            {
                foreach (CardModel card in pcs.Hand.Cards)
                {
                    handCards.Add(SerializeCard(card));
                }
            }

            // Enemies
            var enemies = new List<Dictionary<string, object>>();
            if (combatState != null)
            {
                foreach (Creature enemy in combatState.Enemies)
                {
                    enemies.Add(SerializeEnemy(enemy));
                }
            }

            // Run state info
            RunState runState = RunManager.Instance.DebugOnlyGetState();

            List<Dictionary<string, object>> potions = SerializePotions(player);
            var state = new Dictionary<string, object>
            {
                ["type"] = "combat_action",
                ["player"] = playerObj,
                ["hand"] = handCards,
                ["enemies"] = enemies,
                ["potions"] = potions,
                ["available_actions"] = GetAvailableActions(potions),
                ["draw_pile_count"] = pcs?.DrawPile.Cards.Count ?? 0,
                ["discard_pile_count"] = pcs?.DiscardPile.Cards.Count ?? 0,
                ["exhaust_pile_count"] = pcs?.ExhaustPile.Cards.Count ?? 0,
                ["round"] = combatState?.RoundNumber ?? 0,
                ["floor"] = runState?.TotalFloor ?? 0,
                ["act"] = (runState?.CurrentActIndex ?? 0) + 1,
            };

            return JsonSerializer.Serialize(state);
        }
        catch (Exception ex)
        {
            Logger.Log($"[RlCombat] Error serializing combat state: {ex.Message}");
            return "{\"type\":\"combat_action\",\"error\":\"serialization_failed\"}";
        }
    }

    private Dictionary<string, object> SerializeCard(CardModel card)
    {
        int cost;
        try
        {
            cost = card.EnergyCost.GetWithModifiers(CostModifiers.All);
        }
        catch
        {
            cost = card.EnergyCost.Canonical;
        }

        UnplayableReason reason;
        AbstractModel preventer;
        var result = new Dictionary<string, object>
        {
            ["id"] = card.Id.Entry,
            ["cost"] = cost,
            ["type"] = card.Type.ToString(),
            ["target"] = card.TargetType.ToString(),
            ["playable"] = card.CanPlay(out reason, out preventer),
        };

        if (card.IsUpgraded)
            result["upgraded"] = true;

        return result;
    }

    private Dictionary<string, object> SerializeEnemy(Creature enemy)
    {
        var data = new Dictionary<string, object>
        {
            ["id"] = enemy.IsMonster ? enemy.Monster!.Id.Entry : "UNKNOWN",
            ["hp"] = enemy.CurrentHp,
            ["max_hp"] = enemy.MaxHp,
            ["block"] = enemy.Block,
            ["is_alive"] = enemy.IsAlive,
        };

        // Powers
        var powers = new List<Dictionary<string, object>>();
        foreach (PowerModel power in enemy.Powers)
        {
            powers.Add(new Dictionary<string, object>
            {
                ["id"] = power.Id.Entry,
                ["amount"] = power.Amount,
            });
        }
        if (powers.Count > 0)
            data["powers"] = powers;

        // Intent
        if (enemy.IsMonster && enemy.Monster != null)
        {
            try
            {
                var nextMove = enemy.Monster.NextMove;
                if (nextMove?.Intents != null && nextMove.Intents.Count > 0)
                {
                    AbstractIntent firstIntent = nextMove.Intents[0];
                    data["intent"] = firstIntent.IntentType.ToString();
                    data["intent_move_id"] = nextMove.Id;

                    if (firstIntent is AttackIntent attackIntent)
                    {
                        CombatState cs = enemy.CombatState;
                        if (cs != null)
                        {
                            try
                            {
                                data["intent_damage"] = attackIntent.GetSingleDamage(
                                    cs.PlayerCreatures, enemy);
                                data["intent_hits"] = attackIntent.Repeats > 0
                                    ? attackIntent.Repeats : 1;
                            }
                            catch { }
                        }
                    }
                }
            }
            catch
            {
                data["intent"] = "UNKNOWN";
            }
        }

        return data;
    }

    private static List<Dictionary<string, object>> SerializePotions(Player player)
    {
        var potions = new List<Dictionary<string, object>>();
        try
        {
            int slot = 0;
            foreach (dynamic potion in player.PotionSlots)
            {
                if (potion != null)
                {
                    string targetType = "Self";
                    bool canUse = true;
                    try
                    {
                        targetType = potion.TargetType?.ToString() ?? "Self";
                    }
                    catch { }

                    try
                    {
                        string usage = potion.Usage?.ToString() ?? "";
                        if (string.Equals(usage, "Automatic", StringComparison.OrdinalIgnoreCase))
                            canUse = false;
                    }
                    catch { }

                    potions.Add(new Dictionary<string, object>
                    {
                        ["slot"] = slot,
                        ["id"] = potion.Id.Entry,
                        ["usage"] = potion.Usage.ToString(),
                        ["can_use"] = canUse,
                        ["target"] = targetType,
                        ["requires_target"] = targetType == "AnyEnemy",
                        ["target_type"] = targetType,
                    });
                }
                slot++;
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[RlCombat] Error serializing potions: {ex.Message}");
        }

        return potions;
    }

    private static List<string> GetAvailableActions(IEnumerable<Dictionary<string, object>> potions)
    {
        var actions = new List<string> { "PLAY", "END_TURN" };
        if (potions.Any(p => p.TryGetValue("can_use", out object? canUse) && canUse is bool b && b))
            actions.Add("POTION");
        return actions;
    }
}
