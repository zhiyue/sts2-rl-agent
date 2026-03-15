// StateSerializer.cs — Serializes the full game state to JSON.
//
// This is the core translation layer between the game's internal C# objects
// and the JSON format that the Python RL agent expects. The JSON format
// is designed to match the observation encoding in gym_env/observation.py.
//
// Key game classes accessed (from sts2.dll):
//   - MegaCrit.Sts2.GameLogic.CombatManager         — central combat state
//   - MegaCrit.Sts2.GameLogic.CharacterState          — player/enemy HP, block, powers
//   - MegaCrit.Sts2.GameLogic.CardInstance             — cards in hand/draw/discard
//   - MegaCrit.Sts2.GameLogic.EnemyAI                  — enemy intents
//   - MegaCrit.Sts2.GameLogic.PowerInstance             — applied buffs/debuffs
//   - MegaCrit.Sts2.GameLogic.RunManager               — floor, act, gold, deck, relics
//   - MegaCrit.Sts2.GameLogic.MapManager               — map state and navigation
//
// Threading: This class is called from the Godot main thread (via
// StabilityDetector) so it's safe to access game objects directly.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace STS2BridgeMod;

/// <summary>
/// Serializes current game state into a JSON string for the Python client.
/// </summary>
public static class StateSerializer
{
    // Pre-configured JSON options for compact output
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    /// <summary>
    /// Build the full game state JSON string.
    /// Should only be called from the main thread when the game is idle.
    /// </summary>
    public static string SerializeGameState()
    {
        try
        {
            var state = new GameStateMessage
            {
                Type = "game_state",
                Phase = DetectCurrentPhase(),
                CombatState = SerializeCombatState(),
                RunState = SerializeRunState(),
                AvailableActions = GetAvailableActions(),
            };

            return JsonSerializer.Serialize(state, JsonOptions);
        }
        catch (Exception ex)
        {
            Logger.Log($"[StateSerializer] Error serializing state: {ex}");
            return JsonSerializer.Serialize(new GameStateMessage
            {
                Type = "error",
                Phase = "UNKNOWN",
            }, JsonOptions);
        }
    }

    // ----------------------------------------------------------------
    // Phase detection
    // ----------------------------------------------------------------

    /// <summary>
    /// Determine which game phase we're in. This drives what actions
    /// the Python client should consider.
    ///
    /// Game phases map to these classes/states in the game code:
    ///   - CombatManager.IsInCombat → COMBAT_PLAY / COMBAT_END_TURN
    ///   - MapManager.IsSelectingNode → MAP_SELECT
    ///   - RewardScreen.IsShowing → CARD_REWARD
    ///   - ShopManager.IsInShop → SHOP
    ///   - RestSiteManager.IsAtRestSite → REST
    ///   - EventManager.IsInEvent → EVENT
    /// </summary>
    private static string DetectCurrentPhase()
    {
        try
        {
            // Check if we're in combat
            // CombatManager is a singleton accessible via CombatManager.Instance
            var combatManager = GetCombatManager();
            if (combatManager != null && IsInCombat(combatManager))
            {
                // Determine if it's the player's turn to play cards
                bool isPlayerTurn = IsPlayerTurn(combatManager);
                return isPlayerTurn ? "COMBAT_PLAY" : "COMBAT_WAITING";
            }

            // Check map selection
            // MapManager.Instance tracks the current map state
            if (IsSelectingMapNode())
                return "MAP_SELECT";

            // Check card reward screen
            if (IsCardRewardShowing())
                return "CARD_REWARD";

            // Check shop
            if (IsInShop())
                return "SHOP";

            // Check rest site
            if (IsAtRestSite())
                return "REST";

            // Check event
            if (IsInEvent())
                return "EVENT";

            return "UNKNOWN";
        }
        catch
        {
            return "UNKNOWN";
        }
    }

    // ----------------------------------------------------------------
    // Combat state serialization
    // ----------------------------------------------------------------

    /// <summary>
    /// Serialize the full combat state. Returns null if not in combat.
    ///
    /// Accesses:
    ///   - CombatManager.Instance.Player (CharacterState)
    ///   - CombatManager.Instance.Hand (List&lt;CardInstance&gt;)
    ///   - CombatManager.Instance.DrawPile / DiscardPile / ExhaustPile
    ///   - CombatManager.Instance.Enemies (List&lt;CharacterState&gt;)
    ///   - CombatManager.Instance.CurrentEnergy / MaxEnergy
    ///   - CombatManager.Instance.TurnNumber
    /// </summary>
    private static CombatStateData? SerializeCombatState()
    {
        try
        {
            var cm = GetCombatManager();
            if (cm == null || !IsInCombat(cm))
                return null;

            // ---- Player ----
            // CharacterState holds HP, MaxHP, Block, Powers
            dynamic player = GetPlayer(cm);
            var playerData = new PlayerData
            {
                Hp = (int)player.CurrentHp,
                MaxHp = (int)player.MaxHp,
                Block = (int)player.Block,
                Energy = GetCurrentEnergy(cm),
                MaxEnergy = GetMaxEnergy(cm),
                Powers = SerializePowers(player),
            };

            // ---- Hand cards ----
            // CombatManager.Instance.Hand is a List<CardInstance>
            // Each CardInstance has: CardData (id, cost, type, target), IsUpgraded
            var hand = GetHand(cm);
            var handCards = new List<CardData>();
            if (hand != null)
            {
                foreach (dynamic card in hand)
                {
                    handCards.Add(SerializeCard(card));
                }
            }

            // ---- Pile counts and composition ----
            var drawPile = GetDrawPile(cm);
            var discardPile = GetDiscardPile(cm);
            var exhaustPile = GetExhaustPile(cm);

            int drawCount = CountPile(drawPile);
            int discardCount = CountPile(discardPile);
            int exhaustCount = CountPile(exhaustPile);

            // ---- Enemies ----
            // CombatManager.Instance.Enemies is a List<CharacterState>
            // Each enemy has HP, MaxHP, Block, Powers, and an EnemyAI
            // EnemyAI.CurrentMove has Intents (list of IntentData)
            var enemies = GetEnemies(cm);
            var enemyList = new List<EnemyData>();
            if (enemies != null)
            {
                foreach (dynamic enemy in enemies)
                {
                    enemyList.Add(SerializeEnemy(enemy));
                }
            }

            return new CombatStateData
            {
                Player = playerData,
                Hand = handCards,
                DrawPileCount = drawCount,
                DiscardPileCount = discardCount,
                ExhaustPileCount = exhaustCount,
                Enemies = enemyList,
                Round = GetTurnNumber(cm),
            };
        }
        catch (Exception ex)
        {
            Logger.Log($"[StateSerializer] Error serializing combat: {ex}");
            return null;
        }
    }

    private static CardData SerializeCard(dynamic card)
    {
        // CardInstance properties:
        //   .CardData.Id (string)  — e.g. "STRIKE_IRONCLAD"
        //   .CardData.EnergyCost (int)
        //   .CardData.CardType (enum: Attack, Skill, Power, Status, Curse)
        //   .CardData.TargetType (enum: Self, AnyEnemy, AllEnemies, None)
        //   .IsUpgraded (bool)
        //   .BaseDamage, .BaseBlock (int) — computed values
        try
        {
            return new CardData
            {
                Id = card.CardData?.Id?.ToString() ?? "UNKNOWN",
                Cost = (int)(card.EnergyCost ?? card.CardData?.EnergyCost ?? 0),
                Type = card.CardData?.CardType?.ToString() ?? "Attack",
                Target = card.CardData?.TargetType?.ToString() ?? "Self",
                Upgraded = (bool)(card.IsUpgraded ?? false),
                BaseDamage = TryGetInt(card, "BaseDamage"),
                BaseBlock = TryGetInt(card, "BaseBlock"),
            };
        }
        catch
        {
            return new CardData { Id = "UNKNOWN", Cost = 0, Type = "Attack", Target = "Self" };
        }
    }

    private static EnemyData SerializeEnemy(dynamic enemy)
    {
        // CharacterState (enemy):
        //   .CurrentHp, .MaxHp, .Block (int)
        //   .IsAlive (bool)
        //   .Powers (List<PowerInstance>)
        //   .EnemyAI.CurrentMove.Intents (List<IntentData>)
        //   .IntentData: .IntentType (enum), .Damage (int), .Hits (int)
        try
        {
            var data = new EnemyData
            {
                Id = enemy.Name?.ToString() ?? enemy.GetType().Name,
                Hp = (int)enemy.CurrentHp,
                MaxHp = (int)enemy.MaxHp,
                Block = (int)enemy.Block,
                IsAlive = (bool)enemy.IsAlive,
                Powers = SerializePowers(enemy),
            };

            // Extract intent from EnemyAI
            try
            {
                dynamic ai = enemy.EnemyAI;
                if (ai?.CurrentMove?.Intents != null)
                {
                    var intents = ai.CurrentMove.Intents;
                    if (intents.Count > 0)
                    {
                        dynamic intent = intents[0];
                        data.Intent = intent.IntentType?.ToString() ?? "UNKNOWN";
                        data.IntentDamage = TryGetInt(intent, "Damage");
                        data.IntentHits = TryGetInt(intent, "Hits", 1);
                    }
                }
            }
            catch
            {
                data.Intent = "UNKNOWN";
            }

            return data;
        }
        catch
        {
            return new EnemyData { Id = "UNKNOWN", Hp = 0, MaxHp = 1, IsAlive = false };
        }
    }

    private static List<PowerData> SerializePowers(dynamic character)
    {
        // CharacterState.Powers is a List<PowerInstance>
        // PowerInstance: .PowerId (enum), .Amount (int)
        var powers = new List<PowerData>();
        try
        {
            if (character.Powers != null)
            {
                foreach (dynamic power in character.Powers)
                {
                    powers.Add(new PowerData
                    {
                        Id = power.PowerId?.ToString() ?? power.Id?.ToString() ?? "UNKNOWN",
                        Amount = (int)(power.Amount ?? power.Stacks ?? 0),
                    });
                }
            }
        }
        catch { }
        return powers;
    }

    // ----------------------------------------------------------------
    // Run state serialization
    // ----------------------------------------------------------------

    /// <summary>
    /// Serialize non-combat run information (floor, act, deck, relics, etc).
    ///
    /// Accesses:
    ///   - RunManager.Instance.Floor / Act / Gold
    ///   - RunManager.Instance.Deck (List&lt;CardInstance&gt;)
    ///   - RunManager.Instance.Relics (List&lt;RelicInstance&gt;)
    ///   - RunManager.Instance.Potions (List&lt;PotionInstance&gt;)
    /// </summary>
    private static RunStateData? SerializeRunState()
    {
        try
        {
            dynamic? runManager = GetRunManager();
            if (runManager == null)
                return null;

            var runState = new RunStateData
            {
                Floor = TryGetInt(runManager, "Floor"),
                Act = TryGetInt(runManager, "Act"),
                Gold = TryGetInt(runManager, "Gold"),
            };

            // Deck
            try
            {
                runState.Deck = new List<string>();
                if (runManager.Deck != null)
                {
                    foreach (dynamic card in runManager.Deck)
                    {
                        runState.Deck.Add(card.CardData?.Id?.ToString() ?? "UNKNOWN");
                    }
                }
            }
            catch { runState.Deck = new List<string>(); }

            // Relics
            try
            {
                runState.Relics = new List<string>();
                if (runManager.Relics != null)
                {
                    foreach (dynamic relic in runManager.Relics)
                    {
                        runState.Relics.Add(relic.RelicId?.ToString() ?? relic.Id?.ToString() ?? "UNKNOWN");
                    }
                }
            }
            catch { runState.Relics = new List<string>(); }

            // Potions
            try
            {
                runState.Potions = new List<PotionData>();
                if (runManager.Potions != null)
                {
                    foreach (dynamic potion in runManager.Potions)
                    {
                        runState.Potions.Add(new PotionData
                        {
                            Id = potion.PotionId?.ToString() ?? potion.Id?.ToString() ?? "EMPTY",
                            CanUse = (bool)(potion.CanUse ?? false),
                            RequiresTarget = (bool)(potion.RequiresTarget ?? false),
                        });
                    }
                }
            }
            catch { runState.Potions = new List<PotionData>(); }

            return runState;
        }
        catch (Exception ex)
        {
            Logger.Log($"[StateSerializer] Error serializing run state: {ex.Message}");
            return null;
        }
    }

    // ----------------------------------------------------------------
    // Available actions
    // ----------------------------------------------------------------

    /// <summary>
    /// Determine which action types are currently valid.
    /// The Python client uses this to know what commands it can send.
    /// </summary>
    private static List<string> GetAvailableActions()
    {
        var actions = new List<string>();
        string phase = DetectCurrentPhase();

        switch (phase)
        {
            case "COMBAT_PLAY":
                actions.Add("PLAY");
                actions.Add("END_TURN");
                // Check if any potions are usable
                if (HasUsablePotions())
                    actions.Add("POTION");
                break;
            case "MAP_SELECT":
            case "CARD_REWARD":
            case "SHOP":
            case "REST":
            case "EVENT":
                actions.Add("CHOOSE");
                break;
        }

        return actions;
    }

    // ----------------------------------------------------------------
    // Game object accessors — these wrap reflection/dynamic access
    // to game singletons. Exact property names may need adjustment
    // based on the decompiled sts2.dll version.
    // ----------------------------------------------------------------

    // CombatManager is the central singleton for combat state.
    // Access pattern: CombatManager.Instance (static property)
    private static dynamic? GetCombatManager()
    {
        try
        {
            var type = FindGameType("CombatManager");
            return type?.GetProperty("Instance")?.GetValue(null);
        }
        catch { return null; }
    }

    private static bool IsInCombat(dynamic cm)
    {
        try { return (bool)cm.IsInCombat; } catch { return false; }
    }

    private static bool IsPlayerTurn(dynamic cm)
    {
        try { return (bool)cm.IsPlayerTurn; } catch { return true; }
    }

    private static dynamic? GetPlayer(dynamic cm)
    {
        try { return cm.Player; } catch { return null; }
    }

    private static dynamic? GetHand(dynamic cm)
    {
        try { return cm.Hand; } catch { return null; }
    }

    private static dynamic? GetDrawPile(dynamic cm)
    {
        try { return cm.DrawPile; } catch { return null; }
    }

    private static dynamic? GetDiscardPile(dynamic cm)
    {
        try { return cm.DiscardPile; } catch { return null; }
    }

    private static dynamic? GetExhaustPile(dynamic cm)
    {
        try { return cm.ExhaustPile; } catch { return null; }
    }

    private static dynamic? GetEnemies(dynamic cm)
    {
        try { return cm.Enemies; } catch { return null; }
    }

    private static int GetCurrentEnergy(dynamic cm)
    {
        try { return (int)cm.CurrentEnergy; } catch { return 0; }
    }

    private static int GetMaxEnergy(dynamic cm)
    {
        try { return (int)cm.MaxEnergy; } catch { return 3; }
    }

    private static int GetTurnNumber(dynamic cm)
    {
        try { return (int)cm.TurnNumber; } catch { return 0; }
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

    private static bool IsSelectingMapNode()
    {
        try
        {
            var type = FindGameType("MapManager");
            dynamic? mm = type?.GetProperty("Instance")?.GetValue(null);
            return mm != null && (bool)mm.IsSelectingNode;
        }
        catch { return false; }
    }

    private static bool IsCardRewardShowing()
    {
        try
        {
            var type = FindGameType("RewardScreen") ?? FindGameType("CardRewardManager");
            dynamic? rs = type?.GetProperty("Instance")?.GetValue(null);
            return rs != null && (bool)(rs.IsShowing ?? rs.IsActive ?? false);
        }
        catch { return false; }
    }

    private static bool IsInShop()
    {
        try
        {
            var type = FindGameType("ShopManager");
            dynamic? sm = type?.GetProperty("Instance")?.GetValue(null);
            return sm != null && (bool)(sm.IsInShop ?? sm.IsActive ?? false);
        }
        catch { return false; }
    }

    private static bool IsAtRestSite()
    {
        try
        {
            var type = FindGameType("RestSiteManager") ?? FindGameType("CampfireManager");
            dynamic? rm = type?.GetProperty("Instance")?.GetValue(null);
            return rm != null && (bool)(rm.IsAtRestSite ?? rm.IsActive ?? false);
        }
        catch { return false; }
    }

    private static bool IsInEvent()
    {
        try
        {
            var type = FindGameType("EventManager");
            dynamic? em = type?.GetProperty("Instance")?.GetValue(null);
            return em != null && (bool)(em.IsInEvent ?? em.IsActive ?? false);
        }
        catch { return false; }
    }

    private static bool HasUsablePotions()
    {
        try
        {
            dynamic? rm = GetRunManager();
            if (rm?.Potions == null) return false;
            foreach (dynamic p in rm.Potions)
            {
                if ((bool)(p.CanUse ?? false)) return true;
            }
        }
        catch { }
        return false;
    }

    // ----------------------------------------------------------------
    // Utility helpers
    // ----------------------------------------------------------------

    /// <summary>
    /// Find a game type by short name. Searches the sts2 assembly for
    /// types matching common namespace patterns.
    /// </summary>
    private static Type? FindGameType(string shortName)
    {
        // Cache could be added here for performance if needed
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

    private static int CountPile(dynamic? pile)
    {
        try { return pile == null ? 0 : (int)pile.Count; } catch { return 0; }
    }

    private static int TryGetInt(dynamic obj, string property, int fallback = 0)
    {
        try
        {
            var val = obj.GetType().GetProperty(property)?.GetValue(obj);
            return val != null ? (int)val : fallback;
        }
        catch { return fallback; }
    }

    // ----------------------------------------------------------------
    // JSON data model classes
    // ----------------------------------------------------------------

    public class GameStateMessage
    {
        public string Type { get; set; } = "game_state";
        public string Phase { get; set; } = "UNKNOWN";
        public CombatStateData? CombatState { get; set; }
        public RunStateData? RunState { get; set; }
        public List<string>? AvailableActions { get; set; }
    }

    public class CombatStateData
    {
        public PlayerData? Player { get; set; }
        public List<CardData>? Hand { get; set; }
        public int DrawPileCount { get; set; }
        public int DiscardPileCount { get; set; }
        public int ExhaustPileCount { get; set; }
        public List<EnemyData>? Enemies { get; set; }
        public int Round { get; set; }
    }

    public class PlayerData
    {
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Block { get; set; }
        public int Energy { get; set; }
        public int MaxEnergy { get; set; }
        public List<PowerData>? Powers { get; set; }
    }

    public class CardData
    {
        public string Id { get; set; } = "";
        public int Cost { get; set; }
        public string Type { get; set; } = "";
        public string Target { get; set; } = "";
        public bool Upgraded { get; set; }
        public int BaseDamage { get; set; }
        public int BaseBlock { get; set; }
    }

    public class EnemyData
    {
        public string Id { get; set; } = "";
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Block { get; set; }
        public bool IsAlive { get; set; }
        public string Intent { get; set; } = "UNKNOWN";
        public int IntentDamage { get; set; }
        public int IntentHits { get; set; } = 1;
        public List<PowerData>? Powers { get; set; }
    }

    public class PowerData
    {
        public string Id { get; set; } = "";
        public int Amount { get; set; }
    }

    public class RunStateData
    {
        public int Floor { get; set; }
        public int Act { get; set; }
        public int Gold { get; set; }
        public List<string>? Deck { get; set; }
        public List<string>? Relics { get; set; }
        public List<PotionData>? Potions { get; set; }
    }

    public class PotionData
    {
        public string Id { get; set; } = "";
        public bool CanUse { get; set; }
        public bool RequiresTarget { get; set; }
    }
}
