// RlCardSelector.cs -- RL-agent-driven card selector.
//
// Implements ICardSelector (MegaCrit.Sts2.Core.TestSupport) to intercept
// all card selection prompts in the game (card rewards, deck upgrades,
// deck transforms, deck enchants, hand selections, etc.).
//
// When CardSelectCmd has a Selector set, it bypasses the UI screens and
// calls these methods directly. This is the same mechanism AutoSlay uses
// with AutoSlayCardSelector, but we replace random with RL agent decisions.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;

namespace STS2BridgeMod;

public class RlCardSelector : ICardSelector
{
    private static readonly TimeSpan AgentTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Called for deck upgrade, deck transform, deck enchant, hand selection,
    /// and various other card selection prompts.
    /// </summary>
    public async Task<IEnumerable<CardModel>> GetSelectedCards(
        IEnumerable<CardModel> options, int minSelect, int maxSelect)
    {
        List<CardModel> cardList = options.ToList();
        if (cardList.Count == 0)
            return Array.Empty<CardModel>();

        // If there's only one option and we must select at least one, auto-select
        if (cardList.Count <= minSelect)
            return cardList;

        // Build the state message
        var cards = new List<Dictionary<string, object>>();
        for (int i = 0; i < cardList.Count; i++)
        {
            CardModel card = cardList[i];
            var cardData = new Dictionary<string, object>
            {
                ["index"] = i,
                ["id"] = card.Id.Entry,
                ["type"] = card.Type.ToString(),
            };
            if (card.IsUpgraded)
                cardData["upgraded"] = true;
            cards.Add(cardData);
        }

        var stateMsg = new Dictionary<string, object>
        {
            ["type"] = "card_select",
            ["cards"] = cards,
            ["min_select"] = minSelect,
            ["max_select"] = maxSelect,
        };

        // Try to get decision from Python agent
        if (BridgeServer.Instance.IsClientConnected)
        {
            try
            {
                string stateJson = JsonSerializer.Serialize(stateMsg);
                BridgeServer.Instance.SendState(stateJson);

                using var cts = new CancellationTokenSource(AgentTimeout);
                string responseJson = await BridgeServer.Instance.WaitForActionAsync(
                    AgentTimeout, cts.Token);

                if (responseJson != null)
                {
                    return ParseCardSelectResponse(responseJson, cardList, minSelect, maxSelect);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[RlCardSelector] Agent error: {ex.Message}");
            }
        }

        // Fallback: select random cards
        Logger.Log("[RlCardSelector] Falling back to random selection");
        return FallbackSelect(cardList, minSelect, maxSelect);
    }

    /// <summary>
    /// Called specifically for card reward screens (post-combat card picks).
    /// </summary>
    public CardModel? GetSelectedCardReward(
        IReadOnlyList<CardCreationResult> options,
        IReadOnlyList<CardRewardAlternative> alternatives)
    {
        if (options.Count == 0)
            return null;

        // Build the state message
        var cards = new List<Dictionary<string, object>>();
        for (int i = 0; i < options.Count; i++)
        {
            CardModel card = options[i].Card;
            var cardData = new Dictionary<string, object>
            {
                ["index"] = i,
                ["id"] = card.Id.Entry,
                ["type"] = card.Type.ToString(),
                ["cost"] = card.EnergyCost.Canonical,
            };
            if (card.IsUpgraded)
                cardData["upgraded"] = true;
            cards.Add(cardData);
        }

        var stateMsg = new Dictionary<string, object>
        {
            ["type"] = "card_reward",
            ["cards"] = cards,
            ["can_skip"] = true,
        };

        // This method is synchronous, so we use a blocking wait
        if (BridgeServer.Instance.IsClientConnected)
        {
            try
            {
                string stateJson = JsonSerializer.Serialize(stateMsg);
                BridgeServer.Instance.SendState(stateJson);

                using var cts = new CancellationTokenSource(AgentTimeout);
                // We must block here since the interface method is not async
                string responseJson = BridgeServer.Instance.WaitForActionAsync(
                    AgentTimeout, cts.Token).GetAwaiter().GetResult();

                if (responseJson != null)
                {
                    using var doc = JsonDocument.Parse(responseJson);
                    var root = doc.RootElement;
                    string action = root.GetProperty("action").GetString() ?? "";

                    if (action == "skip")
                    {
                        Logger.Log("[RlCardSelector] Agent chose to skip card reward");
                        return null;
                    }

                    if (action == "choose" &&
                        root.TryGetProperty("index", out var idxProp))
                    {
                        int idx = idxProp.GetInt32();
                        if (idx >= 0 && idx < options.Count)
                        {
                            Logger.Log(
                                $"[RlCardSelector] Agent chose card reward: {options[idx].Card.Id.Entry}");
                            return options[idx].Card;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[RlCardSelector] Agent error for card reward: {ex.Message}");
            }
        }

        // Fallback: pick the first card
        Logger.Log("[RlCardSelector] Falling back: picking first card reward");
        return options[0].Card;
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private IEnumerable<CardModel> ParseCardSelectResponse(
        string json, List<CardModel> cardList, int minSelect, int maxSelect)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            string action = root.GetProperty("action").GetString() ?? "";

            if (action == "skip")
            {
                if (minSelect <= 0)
                {
                    Logger.Log("[RlCardSelector] Agent chose to skip");
                    return Array.Empty<CardModel>();
                }
                // Can't skip if min is > 0, fall through
            }

            if (action == "choose")
            {
                // Single index
                if (root.TryGetProperty("index", out var idxProp))
                {
                    int idx = idxProp.GetInt32();
                    if (idx >= 0 && idx < cardList.Count)
                    {
                        Logger.Log($"[RlCardSelector] Agent chose card: {cardList[idx].Id.Entry}");
                        return new[] { cardList[idx] };
                    }
                }

                // Multiple indexes
                if (root.TryGetProperty("indexes", out var idxsProp) &&
                    idxsProp.ValueKind == JsonValueKind.Array)
                {
                    var selected = new List<CardModel>();
                    foreach (var elem in idxsProp.EnumerateArray())
                    {
                        int idx = elem.GetInt32();
                        if (idx >= 0 && idx < cardList.Count)
                        {
                            selected.Add(cardList[idx]);
                        }
                    }
                    if (selected.Count >= minSelect && selected.Count <= maxSelect)
                    {
                        Logger.Log(
                            $"[RlCardSelector] Agent chose {selected.Count} cards");
                        return selected;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[RlCardSelector] Error parsing response: {ex.Message}");
        }

        Logger.Log("[RlCardSelector] Invalid response, falling back to random");
        return FallbackSelect(cardList, minSelect, maxSelect);
    }

    private static IEnumerable<CardModel> FallbackSelect(
        List<CardModel> cards, int minSelect, int maxSelect)
    {
        int count = Math.Min(maxSelect, cards.Count);
        if (count < minSelect)
            count = Math.Min(minSelect, cards.Count);
        return cards.Take(count);
    }
}
