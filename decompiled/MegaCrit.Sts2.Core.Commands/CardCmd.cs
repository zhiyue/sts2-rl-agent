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
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Commands;

public static class CardCmd
{
	public static async Task AutoPlay(PlayerChoiceContext choiceContext, CardModel card, Creature? target, AutoPlayType type = AutoPlayType.Default, bool skipXCapture = false, bool skipCardPileVisuals = false)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}
		CombatState combatState = card.CombatState ?? card.Owner.Creature.CombatState;
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
				NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NThoughtBubbleVfx.Create(playerDialogueLine.GetFormattedText(), card.Owner.Creature, 1.0));
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
		if (!card.IsDupe)
		{
			PlayerCombatState playerCombatState = card.Owner.PlayerCombatState;
			if (card.EnergyCost.CostsX && !skipXCapture)
			{
				card.EnergyCost.CapturedXValue = playerCombatState.Energy;
			}
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

	public static async Task Discard(PlayerChoiceContext choiceContext, CardModel card)
	{
		await Discard(choiceContext, new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(card));
	}

	public static async Task Discard(PlayerChoiceContext choiceContext, IEnumerable<CardModel> cards)
	{
		await DiscardAndDraw(choiceContext, cards, 0);
	}

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
		CombatState combatState = discardCards[0].CombatState ?? discardCards[0].Owner.Creature.CombatState;
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

	public static async Task Exhaust(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal = false, bool skipVisuals = false)
	{
		if (!CombatManager.Instance.IsOverOrEnding)
		{
			CombatState combatState = card.CombatState ?? card.Owner.Creature.CombatState;
			await CardPileCmd.Add(card, PileType.Exhaust, CardPilePosition.Bottom, null, skipVisuals);
			CombatManager.Instance.History.CardExhausted(combatState, card);
			await Hook.AfterCardExhausted(combatState, choiceContext, card, causedByEthereal);
		}
	}

	public static void Upgrade(CardModel card, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
	{
		Upgrade(new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(card), style);
	}

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

	public static async Task<CardPileAddResult> TransformToRandom(CardModel original, Rng rng, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
	{
		return (await Transform(new CardTransformation(original).Yield(), rng, style)).First();
	}

	public static async Task<CardPileAddResult?> TransformTo<T>(CardModel original, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout) where T : CardModel
	{
		CardModel replacement = original.CardScope.CreateCard<T>(original.Owner);
		return await Transform(original, replacement, style);
	}

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
		CombatState combatState = transformationsArr[0].Original.CombatState;
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
				CombatManager.Instance.History.CardGenerated(combatState, replacement2, generatedByPlayer: true);
				await Hook.AfterCardEnteredCombat(combatState, replacement2);
			}
			await Hook.AfterCardChangedPiles(runState, combatState, replacement2, pile2.Type, null);
			pile2.InvokeCardAddFinished();
			original.AfterTransformedFrom();
			replacement2.AfterTransformedTo();
			results.Add(result);
		}
		List<Task> tasksToAwait = new List<Task>();
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
					tasksToAwait.Add(TaskHelper.RunSafely(NCardTransformVfx.PlayAnimOnCardInHand(nCard, cardAdded2)));
					await Cmd.Wait(0.2f);
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
		await Task.WhenAll(tasksToAwait);
		for (int j = 0; j < results.Count; j++)
		{
			CardPileAddResult cardPileAddResult = results[j];
			if (cardPileAddResult.success && cardPileAddResult.cardAdded.Pile.Type.IsCombatPile())
			{
				await Hook.AfterCardGeneratedForCombat(cardPileAddResult.cardAdded.CombatState, cardPileAddResult.cardAdded, addedByPlayer: true);
			}
			transformationsWithOriginalData[j].Item1.Original.RemoveFromState();
		}
		return results;
	}

	public static T? Enchant<T>(CardModel card, decimal amount) where T : EnchantmentModel
	{
		return Enchant(ModelDb.Enchantment<T>().ToMutable(), card, amount) as T;
	}

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
		if (card.Pile != null)
		{
			card.Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(card.Owner.NetId).CardsEnchanted.Add(new CardEnchantmentHistoryEntry(card, enchantment.Id));
		}
		return card.Enchantment;
	}

	public static void ClearEnchantment(CardModel card)
	{
		card.ClearEnchantmentInternal();
	}

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

	public static async Task<T?> Afflict<T>(CardModel card, decimal amount) where T : AfflictionModel
	{
		return (await Afflict(ModelDb.Affliction<T>().ToMutable(), card, amount)) as T;
	}

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
		CombatState combatState = card.CombatState ?? card.Owner.Creature.CombatState;
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

	public static void ClearAffliction(CardModel card)
	{
		card.ClearAfflictionInternal();
	}

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

	public static void ApplySingleTurnSly(CardModel card)
	{
		card.GiveSingleTurnSly();
		NCard.FindOnTable(card)?.UpdateVisuals(card.Pile.Type, CardPreviewMode.Normal);
	}

	public static TaskCompletionSource? Preview(CardModel card, float time = 1.2f, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
	{
		return PreviewInternal(card, isAddingCardsToPile: false, null, time, style);
	}

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
		NCard node = NCard.Create(card);
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
		control?.AddChildSafely(node);
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
			Node node2 = ((pileType != PileType.Deck) ? NCombatRoom.Instance?.CombatVfxContainer : NRun.Instance?.GlobalUi.TopBar.TrailContainer);
			if (node2 != null)
			{
				PileType pileType2 = ((card.Pile != null) ? card.Pile.Type : pileType);
				Vector2 targetPosition = pileType2.GetTargetPosition(node);
				nCardFlyVfx = NCardFlyVfx.Create(node, targetPosition, isAddingCardsToPile, card.Owner.Character.TrailPath);
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
