using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Commands;

public static class CardCmd
{
	/// <summary>
	/// Automatically play a card for free. Used for non-player-choice card playing effects.
	/// </summary>
	/// <example>
	/// Examples of where this would be used:
	/// * Havoc ("Play the top card of your Draw Pile and Exhaust it.")
	/// * Duplication Potion ("This turn, your next card is played twice.")
	/// </example>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="card">Card to autoplay.</param>
	/// <param name="target">Target for the autoplay. Will be randomized if null.</param>
	/// <param name="type">Type of autoplay. Certain checks may be bypassed for different autoplay types.</param>
	/// <param name="skipXCapture">
	/// If true, skip capturing the X value for X-cost cards and X-star cost cards. Use this when the caller has already
	/// spent energy/stars via SpendResources, which sets CapturedXValue to the energy spent and stars spent.
	/// </param>
	/// <param name="skipCardPileVisuals">Skip card pile visuals (tween to/from pile, smoke puff VFX, etc).</param>
	public static async Task AutoPlay(PlayerChoiceContext choiceContext, CardModel card, Creature? target, AutoPlayType type = AutoPlayType.Default, bool skipXCapture = false, bool skipCardPileVisuals = false)
	{
		if (CombatManager.Instance.IsOverOrEnding || card.Owner.Creature.IsDead)
		{
			return;
		}
		ICombatState combatState = card.CombatState ?? card.Owner.Creature.CombatState;
		if (card.Keywords.Contains(CardKeyword.Unplayable))
		{
			await MoveToResultPileWithoutPlaying(choiceContext, card);
			return;
		}
		if (!Hook.ShouldPlay(combatState, card, out AbstractModel preventer, type))
		{
			await MoveToResultPileWithoutPlaying(choiceContext, card);
			LocString playerDialogueLine = UnplayableReason.BlockedByHook.GetPlayerDialogueLine(preventer);
			if (playerDialogueLine != null)
			{
				card.Owner.Creature.GetVfxContainer()?.AddChildSafely(NThoughtBubbleVfx.Create(playerDialogueLine.GetFormattedText(), card.Owner.Creature, 1.0));
			}
			return;
		}
		if (card.TargetType == TargetType.AnyEnemy)
		{
			if (target == null)
			{
				target = card.Owner.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);
			}
			if (target == null)
			{
				await MoveToResultPileWithoutPlaying(choiceContext, card);
				return;
			}
		}
		if (card.TargetType == TargetType.AnyAlly)
		{
			IEnumerable<Creature> items = combatState.Allies.Where((Creature c) => c != null && c.IsAlive && c.IsPlayer && c != card.Owner.Creature);
			if (target == null)
			{
				target = card.Owner.RunState.Rng.CombatTargets.NextItem(items);
			}
			if (target == null)
			{
				await MoveToResultPileWithoutPlaying(choiceContext, card);
				return;
			}
		}
		PlayerCombatState playerCombatState = card.Owner.PlayerCombatState;
		if (card.EnergyCost.CostsX && !skipXCapture)
		{
			card.EnergyCost.CapturedXValue = playerCombatState.Energy;
		}
		if (!skipXCapture)
		{
			if (card.HasStarCostX)
			{
				card.LastStarsSpent = playerCombatState.Stars;
			}
			else
			{
				card.LastStarsSpent = Math.Max(0, card.GetStarCostWithModifiers());
			}
		}
		if (card.Pile == null)
		{
			await CardPileCmd.Add(card, PileType.Play);
		}
		if (!skipCardPileVisuals)
		{
			TaskHelper.RunSafely(card.OnEnqueuePlayVfx(target));
		}
		await Hook.BeforeCardAutoPlayed(combatState, card, target, type);
		ResourceInfo resources = new ResourceInfo
		{
			EnergySpent = 0,
			EnergyValue = card.EnergyCost.GetAmountToSpend(),
			StarsSpent = 0,
			StarValue = Math.Max(0, card.GetStarCostWithModifiers())
		};
		await card.OnPlayWrapper(choiceContext, target, isAutoPlay: true, resources, skipCardPileVisuals);
	}

	private static async Task MoveToResultPileWithoutPlaying(PlayerChoiceContext choiceContext, CardModel card)
	{
		await CardPileCmd.Add(card, PileType.Play);
		await card.MoveToResultPileWithoutPlaying(choiceContext);
	}

	/// <summary>
	/// Discard a card.
	/// WARNING: If you're discarding multiple cards at once for an effect like Concentrate or Gambler's Brew, do NOT
	/// use this method inside a loop, because the timing of the Sly effect will be incorrect. Instead, use the overload
	/// of this method that takes an IEnumerable.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="card">Card to discard.</param>
	public static async Task Discard(PlayerChoiceContext choiceContext, CardModel card)
	{
		await Discard(choiceContext, new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(card));
	}

	/// <summary>
	/// Discard multiple cards.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="cards">Cards to discard.</param>
	public static async Task Discard(PlayerChoiceContext choiceContext, IEnumerable<CardModel> cards)
	{
		await DiscardAndDraw(choiceContext, cards, 0);
	}

	/// <summary>
	/// Discard cards, then draw cards.
	/// Unlike calling <see cref="M:MegaCrit.Sts2.Core.Commands.CardCmd.Discard(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Models.CardModel)" /> followed by
	/// <see cref="M:MegaCrit.Sts2.Core.Commands.CardPileCmd.Draw(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Players.Player)" />, this will wait to trigger the discard-related hooks
	/// until after the draw is complete.
	/// Good for effects like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.CalculatedGamble" />.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="cardsToDiscard">Cards to discard.</param>
	/// <param name="cardsToDraw">Number of cards to draw.</param>
	public static async Task DiscardAndDraw(PlayerChoiceContext choiceContext, IEnumerable<CardModel> cardsToDiscard, int cardsToDraw)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}
		List<CardModel> discardCards = cardsToDiscard.ToList();
		if (discardCards.Count == 0)
		{
			return;
		}
		ICombatState combatState = discardCards[0].CombatState ?? discardCards[0].Owner.Creature.CombatState;
		List<CardModel> slyCards = new List<CardModel>();
		CardPile discardPile = PileType.Discard.GetPile(discardCards[0].Owner);
		foreach (CardModel card in discardCards)
		{
			if (card.IsSlyThisTurn)
			{
				slyCards.Add(card);
			}
			await CardPileCmd.Add(card, discardPile);
			CombatManager.Instance.History.CardDiscarded(combatState, card);
			await Hook.AfterCardDiscarded(combatState, choiceContext, card);
		}
		discardPile.InvokeContentsChanged();
		if (cardsToDraw > 0)
		{
			await CardPileCmd.Draw(choiceContext, cardsToDraw, discardCards[0].Owner);
		}
		foreach (CardModel item in slyCards)
		{
			await AutoPlay(choiceContext, item, null, AutoPlayType.SlyDiscard);
		}
	}

	/// <summary>
	/// Downgrades a card to its base form.
	/// Keeps things like enchantments and conditions.
	/// </summary>
	/// <param name="card">Card to downgrade.</param>
	public static void Downgrade(CardModel card)
	{
		if (!CombatManager.Instance.IsEnding)
		{
			CardPile pile = card.Pile;
			if (pile != null && pile.Type == PileType.Deck)
			{
				card.Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(card.Owner.NetId).DowngradedCards.Add(card.Id);
			}
			card.DowngradeInternal();
		}
	}

	/// <summary>
	/// Exhaust a card.
	/// Note: do NOT make a bulk version of this; the hooks for one exhausted card should fully run before the next
	/// card starts exhausting.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="card">Card to exhaust.</param>
	/// <param name="causedByEthereal">
	/// Was this Exhaust caused by Ethereal?
	/// This should always be false except the specific case in <see cref="T:MegaCrit.Sts2.Core.Combat.CombatManager" />.
	/// </param>
	/// <param name="skipVisuals">Skip card pile visuals (tween to/from pile, smoke puff VFX, etc).</param>
	public static async Task Exhaust(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal = false, bool skipVisuals = false)
	{
		if (!CombatManager.Instance.IsOverOrEnding)
		{
			ICombatState combatState = card.CombatState ?? card.Owner.Creature.CombatState;
			await CardPileCmd.Add(card, PileType.Exhaust, CardPilePosition.Bottom, null, skipVisuals);
			CombatManager.Instance.History.CardExhausted(combatState, card);
			await Hook.AfterCardExhausted(combatState, choiceContext, card, causedByEthereal);
		}
	}

	/// <summary>
	/// Upgrade a card.
	/// Use this for actually upgrading a card, not for previewing an upgrade, because it won't highlight value changes.
	/// </summary>
	/// <param name="card">Card to upgrade.</param>
	/// <param name="style">How the upgraded card is displayed to the player.</param>
	public static void Upgrade(CardModel card, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
	{
		Upgrade(new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(card), style);
	}

	/// <summary>
	/// Upgrade a set of cards.
	/// Use this for actually upgrading cards, not for previewing upgrades, because it won't highlight value changes.
	/// </summary>
	/// <param name="cards">Cards to upgrade.</param>
	/// <param name="style">How multiple cards are aligned if previewed together.</param>
	public static void Upgrade(IEnumerable<CardModel> cards, CardPreviewStyle style)
	{
		if (CombatManager.Instance.IsEnding)
		{
			return;
		}
		foreach (CardModel card in cards)
		{
			if (!card.IsUpgradable)
			{
				continue;
			}
			CardPile pile = card.Pile;
			if (pile != null && pile.Type == PileType.Deck)
			{
				card.Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(card.Owner.NetId).UpgradedCards.Add(card.Id);
			}
			card.UpgradeInternal();
			card.FinalizeUpgradeInternal();
			if (!LocalContext.IsMine(card))
			{
				continue;
			}
			pile = card.Pile;
			if (pile != null && pile.Type == PileType.Deck)
			{
				Control control;
				switch (style)
				{
				case CardPreviewStyle.EventLayout:
					control = NRun.Instance?.GlobalUi.EventCardPreviewContainer;
					break;
				case CardPreviewStyle.HorizontalLayout:
					control = NRun.Instance?.GlobalUi.CardPreviewContainer;
					break;
				case CardPreviewStyle.MessyLayout:
					control = NRun.Instance?.GlobalUi.MessyCardPreviewContainer;
					break;
				case CardPreviewStyle.GridLayout:
					control = NRun.Instance?.GlobalUi.GridCardPreviewContainer;
					break;
				default:
					throw new ArgumentOutOfRangeException("style", $"Unexpected {"CardPreviewStyle"} {style}!");
				case CardPreviewStyle.None:
					continue;
				}
				control?.AddChildSafely(NCardUpgradeVfx.Create(card));
			}
		}
	}

	/// <summary>
	/// Transform a card to another randomly-selected card.
	/// </summary>
	/// <param name="original">Card to transform from.</param>
	/// <param name="rng">RNG to use for random card.</param>
	/// <param name="style">How the transformed card is displayed to the player.</param>
	/// <returns>The replacement card.</returns>
	public static async Task<CardPileAddResult> TransformToRandom(CardModel original, Rng rng, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
	{
		return (await Transform(new CardTransformation(original).Yield(), rng, style)).First();
	}

	/// <summary>
	/// Transform a card into a new card of the specified type.
	/// </summary>
	/// <param name="original">Card to transform from.</param>
	/// <param name="style">How the transformed card is displayed to the player.</param>
	/// <typeparam name="T">Card type to transform to.</typeparam>
	/// <returns>The replacement card.</returns>
	public static async Task<CardPileAddResult?> TransformTo<T>(CardModel original, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout) where T : CardModel
	{
		CardModel replacement = original.CardScope.CreateCard<T>(original.Owner);
		return await Transform(original, replacement, style);
	}

	/// <summary>
	/// Transform a card to another card.
	/// </summary>
	/// <param name="original">Card to transform from.</param>
	/// <param name="replacement">Card to transform to.</param>
	/// <param name="style">How the transformed card is displayed to the player.</param>
	/// <returns>The replacement card.</returns>
	public static async Task<CardPileAddResult?> Transform(CardModel original, CardModel replacement, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
	{
		return (await Transform(new CardTransformation(original, replacement).Yield(), null, style)).FirstOrDefault();
	}

	private static int PileIndexSort((CardTransformation, CardPile, int, CardModel) value1, (CardTransformation, CardPile, int, CardModel) value2)
	{
		if (value1.Item2.Type != value2.Item2.Type)
		{
			return value1.Item2.Type.CompareTo(value2.Item2.Type);
		}
		return value1.Item3.CompareTo(value2.Item3);
	}

	/// <summary>
	/// Transforms several cards to other cards.
	/// </summary>
	/// <param name="transformations">Cards to transform.</param>
	/// <param name="rng">Random number generator to use when choosing from random options.</param>
	/// <param name="style">How the transformed cards are displayed to the player.</param>
	/// <returns>The replacement card.</returns>
	public static async Task<IEnumerable<CardPileAddResult>> Transform(IEnumerable<CardTransformation> transformations, Rng? rng, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
	{
		if (CombatManager.Instance.IsEnding)
		{
			return Array.Empty<CardPileAddResult>();
		}
		CardTransformation[] transformationsArr = transformations.ToArray();
		if (transformationsArr.Length == 0)
		{
			return Array.Empty<CardPileAddResult>();
		}
		ICombatState combatState = transformationsArr[0].Original.CombatState;
		List<(CardTransformation, CardPile, int, CardModel)> transformationsWithOriginalData = new List<(CardTransformation, CardPile, int, CardModel)>();
		CardTransformation[] array = transformationsArr;
		for (int i = 0; i < array.Length; i++)
		{
			CardTransformation item = array[i];
			item.Original.AssertMutable();
			if (!item.Original.IsTransformable)
			{
				throw new InvalidOperationException("Can't transform " + item.Original.Id.Entry + " because it's un-transformable.");
			}
			CardPile pile = item.Original.Pile;
			if (pile == null)
			{
				throw new InvalidOperationException("Can't transform " + item.Original.Id.Entry + " because it has no pile.");
			}
			int item2 = pile.Cards.IndexOf(item.Original);
			CardModel replacement = item.GetReplacement(rng);
			if (replacement == null)
			{
				throw new InvalidOperationException($"Attempting to transform un-transformable card {item.Original}!");
			}
			item.Original.RemoveFromCurrentPile();
			transformationsWithOriginalData.Add((item, pile, item2, replacement));
		}
		transformationsWithOriginalData.Sort(PileIndexSort);
		List<CardPileAddResult> results = new List<CardPileAddResult>();
		foreach (var item6 in transformationsWithOriginalData)
		{
			CardTransformation item3 = item6.Item1;
			CardPile pile2 = item6.Item2;
			int item4 = item6.Item3;
			CardModel item5 = item6.Item4;
			CardModel original = item3.Original;
			IRunState runState = original.Owner.RunState;
			CardModel replacement2 = item5;
			replacement2.AssertMutable();
			CardPileAddResult result = new CardPileAddResult
			{
				success = true,
				cardAdded = replacement2,
				modifyingModels = null
			};
			if (replacement2.Owner != original.Owner)
			{
				throw new InvalidOperationException($"Attempting to transform card {original} to {replacement2}, but the replacement has a different owner!");
			}
			if (pile2.Type == PileType.Deck)
			{
				List<AbstractModel> modifyingModels;
				CardModel cardAdded = Hook.ModifyCardBeingAddedToDeck(runState, replacement2, out modifyingModels);
				replacement2 = (result.cardAdded = cardAdded);
				result.modifyingModels = modifyingModels;
				replacement2.FloorAddedToDeck = runState.TotalFloor;
				runState.CurrentMapPointHistoryEntry?.GetEntry(original.Owner.NetId).CardsTransformed.Add(new CardTransformationHistoryEntry(original, replacement2));
			}
			PileType type = pile2.Type;
			if (type == PileType.Deck)
			{
				pile2.AddInternal(replacement2);
			}
			else
			{
				pile2.AddInternal(replacement2, item4);
				CombatManager.Instance.History.CardGenerated(combatState, replacement2, replacement2.Owner);
				await Hook.AfterCardEnteredCombat(combatState, replacement2);
			}
			await Hook.AfterCardChangedPiles(runState, combatState, replacement2, pile2.Type, null);
			pile2.InvokeCardAddFinished();
			original.AfterTransformedFrom();
			replacement2.AfterTransformedTo();
			results.Add(result);
		}
		float num = 0f;
		for (int j = 0; j < results.Count; j++)
		{
			CardModel original2 = transformationsWithOriginalData[j].Item1.Original;
			CardModel cardAdded2 = results[j].cardAdded;
			if (!LocalContext.IsMine(cardAdded2))
			{
				continue;
			}
			if (cardAdded2.Pile.Type == PileType.Hand)
			{
				if (!TestMode.IsOn)
				{
					NCardPlayQueue playQueue = NCombatRoom.Instance.Ui.PlayQueue;
					NPlayerHand hand = NCombatRoom.Instance.Ui.Hand;
					NCard nCard = NCard.FindOnTable(original2, PileType.Hand);
					if (nCard == null)
					{
						throw new InvalidOperationException($"Couldn't get hand node for original card {transformationsArr[j].Original}!");
					}
					if (playQueue.IsAncestorOf(nCard))
					{
						playQueue.RemoveCardFromQueueForCancellation(nCard, forceReturnToHand: true);
					}
					hand.TryCancelCardPlay(original2);
					NCardTransformShineVfx nCardTransformShineVfx = NCardTransformShineVfx.Create(nCard, cardAdded2, Array.Empty<RelicModel>());
					if (nCardTransformShineVfx != null)
					{
						num += nCardTransformShineVfx.GetEffectiveDuration(shortVersion: true);
						TaskHelper.RunSafely(nCardTransformShineVfx.PlayAnimation(shortVersion: true));
					}
				}
			}
			else if (style != CardPreviewStyle.None && TestMode.IsOff)
			{
				((Node)(style switch
				{
					CardPreviewStyle.EventLayout => NRun.Instance?.GlobalUi.EventCardPreviewContainer, 
					CardPreviewStyle.GridLayout => NRun.Instance?.GlobalUi.GridCardPreviewContainer, 
					CardPreviewStyle.HorizontalLayout => NCombatRoom.Instance?.Ui.CardPreviewContainer ?? NRun.Instance?.GlobalUi.CardPreviewContainer, 
					CardPreviewStyle.MessyLayout => NCombatRoom.Instance?.Ui.MessyCardPreviewContainer ?? NRun.Instance?.GlobalUi.MessyCardPreviewContainer, 
					_ => throw new ArgumentOutOfRangeException("style", $"Unexpected {"CardPreviewStyle"} {style}!"), 
				}))?.AddChildSafely(NCardTransformVfx.Create(original2, cardAdded2, results[j].modifyingModels?.OfType<RelicModel>()));
			}
		}
		await Cmd.Wait(num);
		for (int k = 0; k < results.Count; k++)
		{
			CardPileAddResult cardPileAddResult = results[k];
			if (cardPileAddResult.success && cardPileAddResult.cardAdded.Pile.Type.IsCombatPile())
			{
				await Hook.AfterCardGeneratedForCombat(cardPileAddResult.cardAdded.CombatState, cardPileAddResult.cardAdded, cardPileAddResult.cardAdded.Owner);
			}
			transformationsWithOriginalData[k].Item1.Original.RemoveFromState();
		}
		return results;
	}

	/// <summary>
	/// Apply an Enchantment to a card.
	/// </summary>
	/// <param name="card">Card to enchant.</param>
	/// <param name="amount">Amount of the enchantment to apply.</param>
	/// <typeparam name="T">Type of enchantment to apply.</typeparam>
	/// <returns>Enchantment that was applied, or null if it failed.</returns>
	public static T? Enchant<T>(CardModel card, decimal amount) where T : EnchantmentModel
	{
		return Enchant(ModelDb.Enchantment<T>().ToMutable(), card, amount) as T;
	}

	/// <summary>
	/// Apply an Enchantment to a card.
	/// Use this for actually enchanting a card, not for previewing an enchantment, because it won't highlight value
	/// changes.
	/// </summary>
	/// <param name="enchantment">Enchantment to apply.</param>
	/// <param name="card">Card to enchant.</param>
	/// <param name="amount">Amount of the enchantment to apply.</param>
	/// <returns>Enchantment that was applied, or null if it failed.</returns>
	public static EnchantmentModel? Enchant(EnchantmentModel enchantment, CardModel card, decimal amount)
	{
		enchantment.AssertMutable();
		if (!enchantment.CanEnchant(card))
		{
			throw new InvalidOperationException($"Cannot enchant {card.Id} with {enchantment.Id}.");
		}
		if (card.Enchantment == null)
		{
			card.EnchantInternal(enchantment, amount);
			enchantment.ModifyCard();
		}
		else
		{
			if (!(card.Enchantment.GetType() == enchantment.GetType()))
			{
				throw new InvalidOperationException($"Cannot enchant {card.Id} with {enchantment.Id} because it already has enchantment {card.Enchantment.Id}.");
			}
			card.Enchantment.Amount += (int)amount;
		}
		card.FinalizeUpgradeInternal();
		CardPile pile = card.Pile;
		if (pile != null && pile.Type == PileType.Deck)
		{
			card.Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(card.Owner.NetId).CardsEnchanted.Add(new CardEnchantmentHistoryEntry(card, enchantment.Id));
		}
		return card.Enchantment;
	}

	/// <summary>
	/// Clear a card's Enchantment if it has one.
	/// </summary>
	/// <param name="card">Card whose enchantment we want to clear.</param>
	public static void ClearEnchantment(CardModel card)
	{
		card.ClearEnchantmentInternal();
	}

	/// <summary>
	/// Apply Afflictions to cards and show it to the owning player if they are the local player.
	/// </summary>
	/// <param name="cards">Cards to afflict.</param>
	/// <param name="amount">Amount of the affliction to apply.</param>
	/// <param name="style">The style of preview to use.</param>
	/// <typeparam name="T">Type of affliction to apply.</typeparam>
	/// <returns>Afflictions that were applied.</returns>
	public static async Task<IEnumerable<T>> AfflictAndPreview<T>(IEnumerable<CardModel> cards, decimal amount, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout) where T : AfflictionModel
	{
		List<T> afflictions = new List<T>();
		List<CardModel> cardList = new List<CardModel>();
		foreach (CardModel card in cards)
		{
			T val = await Afflict<T>(card, amount);
			if (val != null)
			{
				afflictions.Add(val);
				cardList.Add(card);
			}
		}
		if (cardList.Count > 0 && style != CardPreviewStyle.None)
		{
			if (cardList.Any((CardModel c) => c.Owner != cardList[0].Owner))
			{
				throw new InvalidOperationException("All cards passed to AfflictAndPreview must have the same owner!");
			}
			if (LocalContext.IsMine(cardList[0]))
			{
				Preview(cardList, 1.2f, style);
				await Cmd.Wait(1.25f);
			}
		}
		return afflictions;
	}

	/// <summary>
	/// Apply an Affliction to a card.
	/// </summary>
	/// <param name="card">Card to afflict.</param>
	/// <param name="amount">Amount of the affliction to apply.</param>
	/// <typeparam name="T">Type of affliction to apply.</typeparam>
	/// <returns>Affliction that was applied, or null if it failed.</returns>
	public static async Task<T?> Afflict<T>(CardModel card, decimal amount) where T : AfflictionModel
	{
		return (await Afflict(ModelDb.Affliction<T>().ToMutable(), card, amount)) as T;
	}

	/// <summary>
	/// Apply an Affliction to a card.
	/// </summary>
	/// <param name="affliction">Affliction to apply.</param>
	/// <param name="card">Card to afflict.</param>
	/// <param name="amount">Amount of the affliction to apply.</param>
	/// <returns>Affliction that was applied, or null if it failed.</returns>
	public static Task<AfflictionModel?> Afflict(AfflictionModel affliction, CardModel card, decimal amount)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			CardPile pile = card.Pile;
			if (pile != null && pile.IsCombatPile)
			{
				return Task.FromResult<AfflictionModel>(null);
			}
		}
		affliction.AssertMutable();
		ICombatState combatState = card.CombatState ?? card.Owner.Creature.CombatState;
		if (combatState == null || !Hook.ShouldAfflict(combatState, card, affliction))
		{
			return Task.FromResult<AfflictionModel>(null);
		}
		if (!affliction.CanAfflict(card))
		{
			return Task.FromResult<AfflictionModel>(null);
		}
		if (card.Affliction == null)
		{
			card.AfflictInternal(affliction, amount);
			affliction.AfterApplied();
		}
		else
		{
			if (!(card.Affliction.GetType() == affliction.GetType()))
			{
				throw new InvalidOperationException($"Cannot afflict {card.Id} with {affliction.Id} because it already has affliction {card.Affliction.Id}.");
			}
			card.Affliction.Amount += (int)amount;
		}
		CombatManager.Instance.History.CardAfflicted(combatState, card, affliction);
		return Task.FromResult(card.Affliction);
	}

	/// <summary>
	/// Clear a card's Affliction if it has one.
	/// </summary>
	/// <param name="card">Card whose affliction we want to clear.</param>
	public static void ClearAffliction(CardModel card)
	{
		card.ClearAfflictionInternal();
	}

	/// <summary>
	/// Apply keywords to a card.
	/// </summary>
	/// <param name="card">Card to apply keyword too.</param>
	/// <param name="keywords">keywords to apply.</param>
	public static void ApplyKeyword(CardModel card, params CardKeyword[] keywords)
	{
		foreach (CardKeyword keyword in keywords)
		{
			card.AddKeyword(keyword);
		}
		NCard.FindOnTable(card)?.UpdateVisuals(card.Pile.Type, CardPreviewMode.Normal);
	}

	public static void RemoveKeyword(CardModel card, params CardKeyword[] keywords)
	{
		foreach (CardKeyword keyword in keywords)
		{
			card.RemoveKeyword(keyword);
		}
		NCard.FindOnTable(card)?.UpdateVisuals(card.Pile.Type, CardPreviewMode.Normal);
	}

	/// <summary>
	/// Apply Sly to a card for the current turn only.
	/// </summary>
	/// <param name="card">Card to apply single-turn Sly to.</param>
	public static void ApplySingleTurnSly(CardModel card)
	{
		card.GiveSingleTurnSly();
		NCard.FindOnTable(card)?.UpdateVisuals(card.Pile.Type, CardPreviewMode.Normal);
	}

	/// <summary>
	/// Creates a set of NCards that spawn in the middle of the screen, then fly to the pile they're in.
	/// Useful for when you want to preview cards that are being added to a pile.
	/// </summary>
	/// <param name="card">Card to preview.</param>
	/// <param name="time">How long the card lingers before it goes off screen</param>
	/// <param name="style">How multiple cards are aligned if previewed together.</param>
	public static TaskCompletionSource? Preview(CardModel card, float time = 1.2f, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
	{
		return PreviewInternal(card, isAddingCardsToPile: false, null, time, style);
	}

	/// <summary>
	/// Creates a set of NCards that spawn in the middle of the screen, then fly to the pile they're in.
	/// Useful for when you want to preview cards that are being added to a pile.
	/// </summary>
	/// <param name="cards">Cards to preview.</param>
	/// <param name="time">How long the card lingers before it goes off screen</param>
	/// <param name="style">How multiple cards are aligned if previewed together.</param>
	public static void Preview(IReadOnlyList<CardModel> cards, float time = 1.2f, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
	{
		if (TestMode.IsOn || CombatManager.Instance.IsEnding)
		{
			return;
		}
		foreach (CardModel card in cards)
		{
			PreviewInternal(card, isAddingCardsToPile: false, null, time, style);
		}
	}

	public static void PreviewCardPileAdd(CardPileAddResult result, float time = 1.2f, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
	{
		if (!TestMode.IsOn && !CombatManager.Instance.IsEnding && result.success && LocalContext.IsMine(result.cardAdded))
		{
			PreviewInternal(result.cardAdded, isAddingCardsToPile: true, result.modifyingModels?.OfType<RelicModel>() ?? null, time, style);
		}
	}

	public static void PreviewCardPileAdd(IReadOnlyList<CardPileAddResult> results, float time = 1.2f, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
	{
		if (TestMode.IsOn || CombatManager.Instance.IsEnding)
		{
			return;
		}
		if (results.Count > 5 && style == CardPreviewStyle.HorizontalLayout)
		{
			Log.Warn("Horizontal layout is being used with more than five cards! They will go offscreen");
		}
		foreach (CardPileAddResult result in results)
		{
			if (result.success && LocalContext.IsMine(result.cardAdded))
			{
				PreviewInternal(result.cardAdded, isAddingCardsToPile: true, result.modifyingModels?.OfType<RelicModel>() ?? null, time, style);
			}
		}
	}

	private static TaskCompletionSource? PreviewInternal(CardModel card, bool isAddingCardsToPile, IEnumerable<RelicModel>? relicsToFlash = null, float time = 1.2f, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
	{
		if (card.Pile == null)
		{
			return null;
		}
		if (TestMode.IsOn)
		{
			return null;
		}
		if (CombatManager.Instance.IsEnding)
		{
			return null;
		}
		if (!LocalContext.IsMine(card))
		{
			return null;
		}
		PileType pileType = card.Pile.Type;
		if (GetTotalCardsBeingPreviewed() > 50)
		{
			return null;
		}
		Control control;
		switch (style)
		{
		case CardPreviewStyle.HorizontalLayout:
			control = (pileType.IsCombatPile() ? NCombatRoom.Instance.Ui.CardPreviewContainer : NRun.Instance?.GlobalUi.CardPreviewContainer);
			break;
		case CardPreviewStyle.MessyLayout:
			control = (pileType.IsCombatPile() ? NCombatRoom.Instance.Ui.MessyCardPreviewContainer : NRun.Instance?.GlobalUi.MessyCardPreviewContainer);
			break;
		case CardPreviewStyle.EventLayout:
			if (pileType.IsCombatPile())
			{
				throw new InvalidOperationException();
			}
			control = NRun.Instance?.GlobalUi.EventCardPreviewContainer;
			break;
		case CardPreviewStyle.GridLayout:
			if (pileType.IsCombatPile())
			{
				throw new InvalidOperationException();
			}
			control = NRun.Instance?.GlobalUi.GridCardPreviewContainer;
			break;
		default:
			throw new ArgumentOutOfRangeException("style", $"Unexpected {"CardPreviewStyle"} {style}!");
		}
		Control control2 = control;
		if (control2 == null)
		{
			return null;
		}
		if (style == CardPreviewStyle.HorizontalLayout && control2.GetChildCount() > 5)
		{
			control2 = (pileType.IsCombatPile() ? NCombatRoom.Instance.Ui.MessyCardPreviewContainer : NRun.Instance?.GlobalUi.MessyCardPreviewContainer);
		}
		NCard node = NCard.Create(card);
		control2?.AddChildSafely(node);
		node.UpdateVisuals(pileType, CardPreviewMode.Normal);
		TaskCompletionSource source = new TaskCompletionSource();
		Tween tween = node.CreateTween();
		tween.TweenProperty(node, "scale", Vector2.One, 0.25).From(Vector2.Zero).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Cubic);
		tween.TweenCallback(Callable.From(delegate
		{
			TaskHelper.RunSafely(FlashRelics(node, relicsToFlash));
		}));
		tween.TweenCallback(Callable.From(delegate
		{
			NCardFlyVfx nCardFlyVfx = null;
			Node node2 = ((pileType != PileType.Deck) ? card.Owner.Creature.GetVfxContainer() : NRun.Instance?.GlobalUi.TopBar.TrailContainer);
			if (node2 != null)
			{
				PileType pileType2 = ((card.Pile != null) ? card.Pile.Type : pileType);
				nCardFlyVfx = NCardFlyVfx.Create(node, pileType2, isAddingCardsToPile, card.Owner.Character.TrailPath);
			}
			if (nCardFlyVfx != null && node2 != null)
			{
				node2.AddChildSafely(nCardFlyVfx);
				nCardFlyVfx.SwooshAwayCompletion.Task.ContinueWith(delegate
				{
					source.SetResult();
				});
			}
			else
			{
				node.QueueFreeSafely();
				source.SetResult();
			}
		})).SetDelay(time);
		return source;
	}

	private static int GetTotalCardsBeingPreviewed()
	{
		int num = 0;
		if (NCombatRoom.Instance != null)
		{
			num = NCombatRoom.Instance.Ui.CardPreviewContainer.GetChildCount();
			num += NCombatRoom.Instance.Ui.MessyCardPreviewContainer.GetChildCount();
		}
		if (NRun.Instance != null)
		{
			num += NRun.Instance.GlobalUi.CardPreviewContainer.GetChildCount();
			num += NRun.Instance.GlobalUi.MessyCardPreviewContainer.GetChildCount();
			num += NRun.Instance.GlobalUi.EventCardPreviewContainer.GetChildCount();
			num += NRun.Instance.GlobalUi.GridCardPreviewContainer.GetChildCount();
		}
		return num;
	}

	private static Task FlashRelics(NCard node, IEnumerable<RelicModel>? relicsToFlash)
	{
		if (relicsToFlash == null)
		{
			return Task.CompletedTask;
		}
		foreach (RelicModel item in relicsToFlash)
		{
			item.Flash();
			node.FlashRelicOnCard(item);
		}
		return Task.CompletedTask;
	}
}
