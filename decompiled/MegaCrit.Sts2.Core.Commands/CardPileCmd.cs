using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Commands;

public static class CardPileCmd
{
	public static async Task RemoveFromDeck(CardModel card, bool showPreview = true)
	{
		await RemoveFromDeck(new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(card), showPreview);
	}

	public static async Task RemoveFromDeck(IReadOnlyList<CardModel> cards, bool showPreview = true)
	{
		foreach (CardModel card in cards)
		{
			if (card.Pile.Type != PileType.Deck)
			{
				throw new InvalidOperationException("You cannot remove a card that is not in the deck.");
			}
			card.Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(card.Owner.NetId).CardsRemoved.Add(card.ToSerializable());
			await Hook.BeforeCardRemoved(card.Owner.RunState, card);
			card.RemoveFromCurrentPile();
			if (showPreview && LocalContext.IsMine(card))
			{
				NCard nCard = NCard.Create(card);
				if (nCard != null)
				{
					NRun.Instance.GlobalUi.CardPreviewContainer.AddChildSafely(nCard);
					nCard.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
					Tween tween = nCard.CreateTween();
					tween.TweenProperty(nCard, "scale", Vector2.One * 1f, 0.25).From(Vector2.Zero).SetEase(Tween.EaseType.Out)
						.SetTrans(Tween.TransitionType.Cubic);
					tween.TweenProperty(nCard, "scale:y", 0, 0.30000001192092896).SetDelay(1.5);
					tween.Parallel().TweenProperty(nCard, "scale:x", 1.5f, 0.3).SetDelay(1.5);
					tween.Parallel().TweenProperty(nCard, "modulate", Colors.Black, 0.2).SetDelay(1.5);
					tween.TweenCallback(Callable.From(nCard.QueueFreeSafely));
				}
			}
			card.RemoveFromState();
		}
	}

	public static async Task RemoveFromCombat(CardModel card, bool isBeingPlayed, bool skipVisuals = false)
	{
		await RemoveFromCombat(new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(card), isBeingPlayed, skipVisuals);
	}

	public static async Task RemoveFromCombat(IEnumerable<CardModel> cards, bool isBeingPlayed, bool skipVisuals = false)
	{
		if (!cards.Any())
		{
			return;
		}
		CombatState combatState = cards.First().CombatState;
		IRunState runState = cards.First().Owner.RunState;
		List<NCard> cardNodes = new List<NCard>();
		Dictionary<CardModel, CardPile> oldPiles = new Dictionary<CardModel, CardPile>();
		CardPile value;
		foreach (CardModel card in cards)
		{
			value = card.Pile;
			if (value == null || !value.IsCombatPile)
			{
				throw new InvalidOperationException("Card must be in a combat pile for it to be removed");
			}
			if ((!isBeingPlayed || card.Type != CardType.Power) && !skipVisuals)
			{
				NCard nCard = NCard.FindOnTable(card);
				if (nCard != null)
				{
					cardNodes.Add(nCard);
				}
			}
			oldPiles.Add(card, card.Pile);
			card.RemoveFromCurrentPile();
		}
		if (cardNodes.Count != 0)
		{
			NPlayerHand hand = NCombatRoom.Instance.Ui.Hand;
			NCardPlayQueue playQueue = NCombatRoom.Instance.Ui.PlayQueue;
			Control playContainer = NCombatRoom.Instance.Ui.PlayContainer;
			Tween tween = null;
			for (int i = 0; i < cardNodes.Count; i++)
			{
				NCard node = cardNodes[i];
				Vector2 globalPosition = node.GlobalPosition;
				CardModel model = node.Model;
				CardPile cardPile = oldPiles[model];
				if (playQueue.IsAncestorOf(node))
				{
					playQueue.RemoveCardFromQueueForCancellation(node);
				}
				if (cardPile.Type == PileType.Hand && !NodeUtil.IsDescendant(playContainer, node))
				{
					hand.Remove(model);
				}
				else
				{
					node.GetParent()?.RemoveChildSafely(node);
				}
				NCombatRoom.Instance.Ui.AddChildSafely(node);
				node.GlobalPosition = globalPosition;
				if (tween == null)
				{
					tween = NCombatRoom.Instance.CreateTween();
					tween.SetParallel();
				}
				model.Pile?.InvokeCardAddFinished();
				if (cardPile.Type != PileType.Hand && cardPile.Type != PileType.Play)
				{
					AppendPileLerpTween(tween, node, PileType.Play, cardPile);
				}
				tween.Chain().TweenCallback(Callable.From(delegate
				{
					NCombatRoom.Instance.Ui.AddChildSafely(NExhaustVfx.Create(node));
				}));
				tween.Parallel().TweenProperty(node, "modulate", StsColors.exhaustGray, (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.2f : 0.3f);
			}
			if (tween != null)
			{
				tween.Play();
				if (tween.IsValid() && tween.IsRunning())
				{
					await NCombatRoom.Instance.ToSignal(tween, Tween.SignalName.Finished);
				}
			}
			foreach (NCard item in cardNodes)
			{
				item.QueueFreeSafely();
			}
		}
		foreach (KeyValuePair<CardModel, CardPile> item2 in oldPiles)
		{
			item2.Deconstruct(out var key, out value);
			CardModel oldCard = key;
			CardPile cardPile2 = value;
			await Hook.AfterCardChangedPiles(runState, combatState, oldCard, cardPile2.Type, null);
			oldCard.RemoveFromState();
		}
	}

	public static async Task<CardPileAddResult> AddGeneratedCardToCombat(CardModel card, PileType newPileType, bool addedByPlayer, CardPilePosition position = CardPilePosition.Bottom)
	{
		return (await AddGeneratedCardsToCombat(new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(card), newPileType, addedByPlayer, position))[0];
	}

	public static async Task<IReadOnlyList<CardPileAddResult>> AddGeneratedCardsToCombat(IEnumerable<CardModel> cards, PileType newPileType, bool addedByPlayer, CardPilePosition position = CardPilePosition.Bottom)
	{
		List<CardModel> list = cards.ToList();
		if (list.Count == 0)
		{
			return Array.Empty<CardPileAddResult>();
		}
		if (!CombatManager.Instance.IsInProgress)
		{
			return Array.Empty<CardPileAddResult>();
		}
		if (list.Any((CardModel c) => c.Pile != null))
		{
			throw new InvalidOperationException("You are not allowed to generate cards that already have a pile");
		}
		if (!newPileType.IsCombatPile())
		{
			throw new InvalidOperationException("You are not allowed to added generated cards to a non combat pile");
		}
		CombatState combatState = list[0].Owner.Creature.CombatState;
		List<CardPileAddResult> results = new List<CardPileAddResult>();
		foreach (CardModel card in list)
		{
			CombatManager.Instance.History.CardGenerated(combatState, card, addedByPlayer);
			List<CardPileAddResult> list2 = results;
			list2.Add(await Add(card, newPileType.GetPile(card.Owner), position));
			await Hook.AfterCardGeneratedForCombat(combatState, card, addedByPlayer);
		}
		return results;
	}

	public static async Task<CardPileAddResult> Add(CardModel card, PileType newPileType, CardPilePosition position = CardPilePosition.Bottom, AbstractModel? source = null, bool skipVisuals = false)
	{
		if (card.Owner == null)
		{
			throw new InvalidOperationException($"Attempted to add card {card} to pile, but it has no owner!");
		}
		return await Add(card, newPileType.GetPile(card.Owner), position, source, skipVisuals);
	}

	public static async Task<CardPileAddResult> Add(CardModel card, CardPile newPile, CardPilePosition position = CardPilePosition.Bottom, AbstractModel? source = null, bool skipVisuals = false)
	{
		return (await Add(new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(card), newPile, position, source, skipVisuals))[0];
	}

	public static async Task<IReadOnlyList<CardPileAddResult>> Add(IEnumerable<CardModel> cards, PileType newPileType, CardPilePosition position = CardPilePosition.Bottom, AbstractModel? source = null, bool skipVisuals = false)
	{
		if (!cards.Any())
		{
			return Array.Empty<CardPileAddResult>();
		}
		return await Add(cards, newPileType.GetPile(cards.First().Owner), position, source, skipVisuals);
	}

	public static async Task<IReadOnlyList<CardPileAddResult>> Add(IEnumerable<CardModel> cards, CardPile newPile, CardPilePosition position = CardPilePosition.Bottom, AbstractModel? source = null, bool skipVisuals = false)
	{
		if (!cards.Any())
		{
			return Array.Empty<CardPileAddResult>();
		}
		if (newPile.IsCombatPile && CombatManager.Instance.IsEnding)
		{
			return cards.Select((CardModel c) => new CardPileAddResult
			{
				cardAdded = c,
				success = false
			}).ToList();
		}
		List<CardPileAddResult> results = new List<CardPileAddResult>();
		Player owningPlayer = null;
		foreach (CardModel card5 in cards)
		{
			if (card5.Owner == null)
			{
				throw new InvalidOperationException(card5.Id.Entry + " has no owner.");
			}
			if (card5.Owner.Creature.IsDead)
			{
				CardPileAddResult item = new CardPileAddResult
				{
					success = false,
					cardAdded = card5,
					oldPile = card5.Pile,
					modifyingModels = null
				};
				results.Add(item);
				continue;
			}
			if (card5.HasBeenRemovedFromState)
			{
				throw new InvalidOperationException(card5.Id.Entry + " has already been removed from its containing state. If this is intentional, make sure to add it back to the state before adding it to a pile.");
			}
			if (newPile.Type == PileType.Deck)
			{
				if (!card5.Owner.RunState.ContainsCard(card5))
				{
					if (card5.Owner.RunState is NullRunState)
					{
						throw new InvalidOperationException("Tried to add card " + card5.Id.Entry + " to deck for an owner with a NullRunState!");
					}
					throw new InvalidOperationException(card5.Id.Entry + " must be added to a RunState before adding it to your deck.");
				}
			}
			else
			{
				CombatState combatState = card5.Owner.Creature.CombatState;
				if (combatState == null || !combatState.ContainsCard(card5))
				{
					throw new InvalidOperationException(card5.Id.Entry + " must be added to a CombatState before adding it to this pile.");
				}
			}
			if (card5.UpgradePreviewType.IsPreview())
			{
				throw new InvalidOperationException("A card preview cannot be added to a pile.");
			}
			CardPileAddResult item2 = new CardPileAddResult
			{
				success = true,
				cardAdded = card5,
				oldPile = card5.Pile,
				modifyingModels = null
			};
			results.Add(item2);
			if (owningPlayer == null)
			{
				owningPlayer = card5.Owner;
			}
			if (owningPlayer == card5.Owner)
			{
				continue;
			}
			throw new InvalidOperationException("Tried to add cards with different owners to the same pile!");
		}
		bool owningPlayerIsLocal = LocalContext.IsMe(owningPlayer);
		if (newPile.Type == PileType.Deck)
		{
			for (int i = 0; i < results.Count; i++)
			{
				CardPileAddResult result = results[i];
				if (Hook.ShouldAddToDeck(owningPlayer.RunState, result.cardAdded, out AbstractModel preventer))
				{
					IRunState runState = owningPlayer.RunState;
					runState.CurrentMapPointHistoryEntry?.GetEntry(owningPlayer.NetId).CardsGained.Add(result.cardAdded.ToSerializable());
					result.cardAdded.FloorAddedToDeck = runState.TotalFloor;
				}
				else
				{
					await preventer.AfterAddToDeckPrevented(result.cardAdded);
					result.success = false;
					results[i] = result;
				}
			}
		}
		if (newPile.IsCombatPile && !CombatManager.Instance.IsInProgress)
		{
			return results;
		}
		if (!results.Any((CardPileAddResult r) => r.success))
		{
			return results;
		}
		List<NCard> cardNodes = new List<NCard>();
		List<CardModel> cardsWithoutNodesChangingPiles = new List<CardModel>();
		for (int i = 0; i < results.Count; i++)
		{
			CardPileAddResult value = results[i];
			if (!value.success)
			{
				continue;
			}
			NCard cardNode = null;
			CardPile oldPile = value.oldPile;
			CardModel card = value.cardAdded;
			CardPile targetPile = newPile;
			int num;
			if (targetPile != null && targetPile.Type == PileType.Hand)
			{
				IReadOnlyList<CardModel> cards2 = targetPile.Cards;
				if (cards2 != null)
				{
					num = ((cards2.Count >= 10) ? 1 : 0);
					goto IL_0535;
				}
			}
			num = 0;
			goto IL_0535;
			IL_0535:
			bool isFullHandAdd = (byte)num != 0;
			if (isFullHandAdd)
			{
				targetPile = CardPile.Get(PileType.Discard, card.Owner);
			}
			int num2;
			if (!owningPlayerIsLocal && targetPile.Type != PileType.Play)
			{
				num2 = ((oldPile != null && oldPile.Type == PileType.Play) ? 1 : 0);
			}
			else
			{
				num2 = 1;
			}
			bool flag = (byte)num2 != 0;
			bool flag2;
			bool flag4;
			bool flag5;
			if (TestMode.IsOff && flag && !skipVisuals)
			{
				cardNode = NCard.FindOnTable(card);
				flag2 = cardNode == null && targetPile.Type.IsCombatPile() && (isFullHandAdd || oldPile != null || targetPile.Type == PileType.Hand);
				bool flag3 = cardNode == null;
				flag4 = flag3;
				if (flag4)
				{
					if (oldPile == null)
					{
						goto IL_064c;
					}
					switch (oldPile.Type)
					{
					case PileType.Draw:
					case PileType.Discard:
					case PileType.Exhaust:
					case PileType.Deck:
						break;
					default:
						goto IL_064c;
					}
					flag5 = true;
					goto IL_064f;
				}
				goto IL_0653;
			}
			goto IL_06dd;
			IL_064f:
			flag4 = flag5;
			goto IL_0653;
			IL_06dd:
			CardModel card2 = card;
			if (oldPile != null)
			{
				card.RemoveFromCurrentPile();
			}
			else if (targetPile.Type == PileType.Deck)
			{
				List<AbstractModel> modifyingModels;
				CardModel cardModel = Hook.ModifyCardBeingAddedToDeck(card.Owner.RunState, card, out modifyingModels);
				card2 = cardModel;
				if (modifyingModels != null && modifyingModels.Count > 0)
				{
					value.cardAdded = cardModel;
					value.modifyingModels = modifyingModels;
					results[i] = value;
				}
			}
			targetPile.AddInternal(card2, position switch
			{
				CardPilePosition.Bottom => -1, 
				CardPilePosition.Top => 0, 
				CardPilePosition.Random => card.Owner.RunState.Rng.Shuffle.NextInt(targetPile.Cards.Count + 1), 
				_ => throw new ArgumentOutOfRangeException("position", position, null), 
			});
			if (oldPile == null && targetPile.IsCombatPile)
			{
				await Hook.AfterCardEnteredCombat(card.CombatState, card);
			}
			if (isFullHandAdd && owningPlayerIsLocal)
			{
				ThinkCmd.Play(new LocString("combat_messages", "HAND_FULL"), owningPlayer.Creature, 2.0);
			}
			if (oldPile == null || oldPile.Type != PileType.Play || newPile.Type == PileType.Hand || card.IsDupe)
			{
				cardNode?.UpdateVisuals(targetPile.Type, CardPreviewMode.Normal);
			}
			continue;
			IL_0653:
			bool flag6 = flag4;
			if (flag6)
			{
				PileType type = targetPile.Type;
				flag5 = ((type == PileType.Draw || type == PileType.Discard || type == PileType.Deck) ? true : false);
				flag6 = flag5;
			}
			if (flag6)
			{
				cardsWithoutNodesChangingPiles.Add(card);
			}
			else if (flag2)
			{
				cardNode = CreateCardNodeAndUpdateVisuals(card, targetPile.Type, owningPlayerIsLocal);
			}
			if (cardNode != null)
			{
				cardNodes.Add(cardNode);
			}
			goto IL_06dd;
			IL_064c:
			flag5 = false;
			goto IL_064f;
		}
		Tween tween = null;
		if (cardNodes.Count != 0)
		{
			NPlayerHand handNode = NCombatRoom.Instance.Ui.Hand;
			_ = NCombatRoom.Instance.Ui.PlayQueue;
			_ = NCombatRoom.Instance.Ui.PlayContainer;
			tween = NCombatRoom.Instance.CreateTween().SetParallel();
			foreach (NCard cardNode2 in cardNodes)
			{
				CardModel card3 = cardNode2.Model;
				CardPile oldPile2 = results.Find((CardPileAddResult r) => r.cardAdded == card3).oldPile;
				MoveCardNodeToNewPileBeforeTween(cardNode2, card3.Pile.Type);
				bool flag7 = !owningPlayerIsLocal;
				bool flag8 = flag7;
				if (flag8)
				{
					PileType type = card3.Pile.Type;
					bool flag5 = (((uint)(type - 1) <= 2u || type == PileType.Deck) ? true : false);
					flag8 = flag5;
				}
				if (flag8)
				{
					tween.Parallel().TweenProperty(cardNode2, "position", cardNode2.Position + Vector2.Down * 25f, (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.2f : 0.3f);
					tween.Parallel().TweenProperty(cardNode2, "modulate", StsColors.exhaustGray, (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.2f : 0.3f);
					tween.Chain().TweenCallback(Callable.From(cardNode2.QueueFreeSafely));
					continue;
				}
				switch (card3.Pile.Type)
				{
				case PileType.Exhaust:
					card3.Pile.InvokeCardAddFinished();
					if (oldPile2 != null && oldPile2.Type != PileType.Hand && oldPile2.Type != PileType.Play)
					{
						AppendPileLerpTween(tween, cardNode2, PileType.Play, oldPile2);
						FastModeType fastMode = SaveManager.Instance.PrefsSave.FastMode;
						tween.Chain().TweenInterval(fastMode switch
						{
							FastModeType.Instant => 0.01f, 
							FastModeType.Fast => 0.2f, 
							_ => 0.5f, 
						});
					}
					tween.Chain().TweenCallback(Callable.From(delegate
					{
						NCombatRoom.Instance.Ui.AddChildSafely(NExhaustVfx.Create(cardNode2));
					}));
					tween.Parallel().TweenProperty(cardNode2, "modulate", StsColors.exhaustGray, (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.2f : 0.3f);
					tween.Chain().TweenCallback(Callable.From(cardNode2.QueueFreeSafely));
					break;
				case PileType.Hand:
					AppendPileLerpTween(tween, cardNode2, card3.Pile.Type, oldPile2);
					tween.Parallel().TweenCallback(Callable.From(delegate
					{
						handNode.Add(cardNode2);
					}));
					break;
				case PileType.Play:
					AppendPlayPileLerpTween(tween, cardNode2, oldPile2);
					break;
				default:
					tween.TweenCallback(Callable.From(delegate
					{
						Node node = ((card3.Pile.Type != PileType.Deck) ? NCombatRoom.Instance.CombatVfxContainer : NRun.Instance.GlobalUi.TopBar.TrailContainer);
						cardNode2.Reparent(node);
						Vector2 targetPosition = card3.Pile.Type.GetTargetPosition(cardNode2);
						NCardFlyVfx child2 = NCardFlyVfx.Create(cardNode2, targetPosition, isAddingToPile: true, card3.Owner.Character.TrailPath);
						node.AddChildSafely(child2);
					}));
					break;
				}
			}
		}
		if (cardsWithoutNodesChangingPiles.Count != 0)
		{
			foreach (CardModel card4 in cardsWithoutNodesChangingPiles)
			{
				CardPile oldPile3 = results.Find((CardPileAddResult r) => r.cardAdded == card4).oldPile;
				Node vfxContainer = ((card4.Pile.Type != PileType.Deck) ? NCombatRoom.Instance.CombatVfxContainer : NRun.Instance.GlobalUi.TopBar.TrailContainer);
				if (tween != null)
				{
					tween.TweenCallback(Callable.From(delegate
					{
						NCardFlyShuffleVfx child2 = NCardFlyShuffleVfx.Create(oldPile3, card4.Pile, card4.Owner.Character.TrailPath);
						vfxContainer.AddChildSafely(child2);
					}));
				}
				else
				{
					NCardFlyShuffleVfx child = NCardFlyShuffleVfx.Create(oldPile3, card4.Pile, card4.Owner.Character.TrailPath);
					vfxContainer.AddChildSafely(child);
				}
			}
		}
		if (tween != null)
		{
			tween.Play();
			if (tween.IsValid() && tween.IsRunning())
			{
				await NCombatRoom.Instance.ToSignal(tween, Tween.SignalName.Finished);
			}
		}
		foreach (CardPileAddResult item3 in results)
		{
			if (item3.success)
			{
				CardModel cardAdded = item3.cardAdded;
				await Hook.AfterCardChangedPiles(cardAdded.Owner.RunState, cardAdded.CombatState, cardAdded, item3.oldPile?.Type ?? PileType.None, source);
			}
		}
		return results;
	}

	public static async Task AddDuringManualCardPlay(CardModel card)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}
		CombatState combatState = card.Owner.Creature.CombatState;
		if (combatState == null || !combatState.ContainsCard(card))
		{
			throw new InvalidOperationException(card.Id.Entry + " must be added to a CombatState before playing it.");
		}
		bool owningPlayerIsLocal = LocalContext.IsMe(card.Owner);
		CardPile oldPile = card.Pile;
		NCard nCard = null;
		if (TestMode.IsOff)
		{
			nCard = NCard.FindOnTable(card);
			if (nCard == null)
			{
				nCard = CreateCardNodeAndUpdateVisuals(card, PileType.Play, owningPlayerIsLocal);
			}
		}
		card.RemoveFromCurrentPile();
		PileType.Play.GetPile(card.Owner).AddInternal(card);
		if (nCard != null)
		{
			MoveCardNodeToNewPileBeforeTween(nCard, PileType.Play);
			Tween tween = NCombatRoom.Instance.CreateTween().SetParallel();
			AppendPlayPileLerpTween(tween, nCard, oldPile);
			nCard.PlayPileTween = tween;
			tween.Play();
			if (card.Type == CardType.Power && tween.IsValid() && tween.IsRunning())
			{
				await NCombatRoom.Instance.ToSignal(tween, Tween.SignalName.Finished);
			}
		}
		await Hook.AfterCardChangedPiles(card.Owner.RunState, card.CombatState, card, oldPile?.Type ?? PileType.None, null);
	}

	private static NCard CreateCardNodeAndUpdateVisuals(CardModel card, PileType targetPileType, bool owningPlayerIsLocal)
	{
		NCard nCard = NCard.Create(card);
		NCombatRoom.Instance.Ui.AddChildSafely(nCard);
		nCard.UpdateVisuals(targetPileType, CardPreviewMode.Normal);
		if (!owningPlayerIsLocal)
		{
			nCard.Position = NCombatRoom.Instance.GetCreatureNode(card.Owner.Creature).IntentContainer.GlobalPosition;
		}
		else if (card.Pile != null)
		{
			nCard.Position = card.Pile.Type.GetTargetPosition(nCard);
		}
		else
		{
			nCard.Position = targetPileType.GetTargetPosition(nCard);
		}
		return nCard;
	}

	private static void MoveCardNodeToNewPileBeforeTween(NCard cardNode, PileType newPileType)
	{
		NPlayerHand hand = NCombatRoom.Instance.Ui.Hand;
		NCardPlayQueue playQueue = NCombatRoom.Instance.Ui.PlayQueue;
		Control playContainer = NCombatRoom.Instance.Ui.PlayContainer;
		Vector2 globalPosition = cardNode.GlobalPosition;
		CardModel model = cardNode.Model;
		if (playQueue.IsAncestorOf(cardNode))
		{
			playQueue.RemoveCardFromQueueForExecution(model);
		}
		if (hand.IsAncestorOf(cardNode))
		{
			hand.Remove(model);
		}
		else
		{
			cardNode.GetParent()?.RemoveChildSafely(cardNode);
		}
		if (newPileType == PileType.Play)
		{
			playContainer.AddChildSafely(cardNode);
			if (NCombatUi.IsDebugHidingPlayContainer)
			{
				cardNode.Visible = false;
			}
		}
		else
		{
			NCombatRoom.Instance.Ui.AddChildSafely(cardNode);
		}
		cardNode.GlobalPosition = globalPosition;
		cardNode.PlayPileTween?.FastForwardToCompletion();
	}

	private static void AppendPlayPileLerpTween(Tween tween, NCard cardNode, CardPile? oldPile)
	{
		AppendPileLerpTween(tween, cardNode, cardNode.Model.Pile.Type, oldPile);
		tween.Parallel().TweenCallback(Callable.From(delegate
		{
			NCombatRoom.Instance.Ui.AddToPlayContainer(cardNode);
		}));
	}

	private static void AppendPileLerpTween(Tween tween, NCard cardNode, PileType typePile, CardPile? oldPile)
	{
		Vector2 targetPosition = typePile.GetTargetPosition(cardNode);
		float num = SaveManager.Instance.PrefsSave.FastMode switch
		{
			FastModeType.Instant => 0.01f, 
			FastModeType.Fast => 0.1f, 
			_ => 0.25f, 
		};
		if (typePile != PileType.Hand)
		{
			tween.TweenProperty(cardNode, "position", targetPosition, num).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		}
		if (typePile == PileType.Play)
		{
			tween.TweenProperty(cardNode, "scale", Vector2.One * 0.8f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		}
		else if (oldPile == null)
		{
			tween.TweenProperty(cardNode, "scale", Vector2.One, num).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
				.From(Vector2.Zero);
		}
		else
		{
			tween.Parallel().TweenProperty(cardNode, "scale", Vector2.One, num).SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Cubic);
		}
	}

	public static async Task<CardModel?> Draw(PlayerChoiceContext choiceContext, Player player)
	{
		return (await Draw(choiceContext, 1m, player)).FirstOrDefault();
	}

	public static async Task<IEnumerable<CardModel>> Draw(PlayerChoiceContext choiceContext, decimal count, Player player, bool fromHandDraw = false)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return Array.Empty<CardModel>();
		}
		if (!Hook.ShouldDraw(player.Creature.CombatState, player, fromHandDraw, out AbstractModel modifier))
		{
			await Hook.AfterPreventingDraw(player.Creature.CombatState, modifier);
			return Array.Empty<CardModel>();
		}
		CombatState combatState = player.Creature.CombatState;
		List<CardModel> result = new List<CardModel>();
		CardPile hand = PileType.Hand.GetPile(player);
		CardPile drawPile = PileType.Draw.GetPile(player);
		int drawsRequested = ((count > 0m) ? ((int)Math.Ceiling(count)) : 0);
		if (drawsRequested == 0)
		{
			return result;
		}
		int num = Math.Max(0, 10 - hand.Cards.Count);
		if (num == 0)
		{
			CheckIfDrawIsPossibleAndShowThoughtBubbleIfNot(player);
			return result;
		}
		for (int i = 0; i < drawsRequested; i++)
		{
			if (num <= 0)
			{
				break;
			}
			if (!CheckIfDrawIsPossibleAndShowThoughtBubbleIfNot(player))
			{
				break;
			}
			await ShuffleIfNecessary(choiceContext, player);
			if (!CheckIfDrawIsPossibleAndShowThoughtBubbleIfNot(player))
			{
				break;
			}
			CardModel card = drawPile.Cards.FirstOrDefault();
			if (card == null || hand.Cards.Count >= 10)
			{
				break;
			}
			result.Add(card);
			await Add(card, hand);
			CombatManager.Instance.History.CardDrawn(combatState, card, fromHandDraw);
			await Hook.AfterCardDrawn(combatState, choiceContext, card, fromHandDraw);
			card.InvokeDrawn();
			NDebugAudioManager.Instance?.Play("card_deal.mp3", 0.25f, PitchVariance.Small);
			num = Math.Max(0, 10 - hand.Cards.Count);
		}
		return result;
	}

	public static async Task Shuffle(PlayerChoiceContext choiceContext, Player player)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}
		CardPile drawPile = PileType.Draw.GetPile(player);
		List<CardModel> list = PileType.Discard.GetPile(player).Cards.ToList();
		float timeBetweenCardAdds = Mathf.Min(0.045f, 0.8f / (float)list.Count);
		float randomTimeBetweenCardAdds = 1.11f * timeBetweenCardAdds;
		HashSet<CardModel> drawPileCards = drawPile.Cards.ToHashSet();
		foreach (CardModel item in drawPileCards)
		{
			drawPile.RemoveInternal(item, silent: true);
			list.Add(item);
		}
		list.StableShuffle(player.RunState.Rng.Shuffle);
		Hook.ModifyShuffleOrder(player.Creature.CombatState, player, list, isInitialShuffle: false);
		if (CombatManager.Instance.DebugForcedTopCardOnNextShuffle != null)
		{
			if (!list.Remove(CombatManager.Instance.DebugForcedTopCardOnNextShuffle))
			{
				throw new InvalidOperationException("Could not find card " + CombatManager.Instance.DebugForcedTopCardOnNextShuffle.Id.Entry + " in discard pile.");
			}
			list.Insert(0, CombatManager.Instance.DebugForcedTopCardOnNextShuffle);
			CombatManager.Instance.DebugClearForcedTopCardOnNextShuffle();
		}
		float waitTimeAccumulator = 0f;
		foreach (CardModel item2 in list)
		{
			if (!drawPileCards.Contains(item2))
			{
				await Add(item2, drawPile);
				float num = timeBetweenCardAdds + Rng.Chaotic.NextFloat((0f - randomTimeBetweenCardAdds) * 0.5f, randomTimeBetweenCardAdds * 0.5f);
				waitTimeAccumulator += num;
				if ((double)waitTimeAccumulator >= ((SceneTree)Engine.GetMainLoop()).Root.GetProcessDeltaTime())
				{
					await Cmd.Wait(num);
					waitTimeAccumulator = 0f;
				}
			}
			else
			{
				drawPile.AddInternal(item2, -1, silent: true);
			}
		}
		await Cmd.CustomScaledWait(0.2f, 0.5f);
		await Hook.AfterShuffle(player.Creature.CombatState, choiceContext, player);
	}

	public static async Task AutoPlayFromDrawPile(PlayerChoiceContext choiceContext, Player player, int count, CardPilePosition position, bool forceExhaust)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}
		List<CardModel> cards = new List<CardModel>(count);
		CardPile drawPile = PileType.Draw.GetPile(player);
		for (int i = 0; i < count; i++)
		{
			await ShuffleIfNecessary(choiceContext, player);
			CardModel cardModel = position switch
			{
				CardPilePosition.Bottom => drawPile.Cards.LastOrDefault(), 
				CardPilePosition.Top => drawPile.Cards.FirstOrDefault(), 
				CardPilePosition.Random => player.RunState.Rng.CombatCardSelection.NextItem(drawPile.Cards), 
				_ => throw new ArgumentOutOfRangeException("position", position, null), 
			};
			if (cardModel == null)
			{
				break;
			}
			cards.Add(cardModel);
			await Add(cardModel, PileType.Play);
		}
		foreach (CardModel item in cards)
		{
			item.ExhaustOnNextPlay = forceExhaust;
			await CardCmd.AutoPlay(choiceContext, item, null);
		}
	}

	public static async Task ShuffleIfNecessary(PlayerChoiceContext choiceContext, Player player)
	{
		CardPile pile = PileType.Draw.GetPile(player);
		CardPile pile2 = PileType.Discard.GetPile(player);
		if (!pile.Cards.Any() && pile2.Cards.Any())
		{
			await ShuffleFtueCheck();
			await Shuffle(choiceContext, player);
		}
	}

	private static async Task ShuffleFtueCheck()
	{
		if (!SaveManager.Instance.SeenFtue("shuffle_ftue") && NModalContainer.Instance != null)
		{
			NShuffleFtue nShuffleFtue = NShuffleFtue.Create();
			NModalContainer.Instance.Add(nShuffleFtue);
			SaveManager.Instance.MarkFtueAsComplete("shuffle_ftue");
			await nShuffleFtue.WaitForPlayerToConfirm();
		}
	}

	public static async Task AddToCombatAndPreview<T>(IEnumerable<Creature> targets, PileType pileType, int count, bool addedByPlayer, CardPilePosition position = CardPilePosition.Bottom) where T : CardModel
	{
		foreach (Creature target in targets)
		{
			await AddToCombatAndPreview<T>(target, pileType, count, addedByPlayer, position);
		}
	}

	public static async Task AddToCombatAndPreview<T>(Creature target, PileType pileType, int count, bool addedByPlayer, CardPilePosition position = CardPilePosition.Bottom) where T : CardModel
	{
		Player player = target.Player ?? target.PetOwner;
		if (player.Creature.IsDead)
		{
			return;
		}
		CardPileAddResult[] statusCards = new CardPileAddResult[count];
		for (int i = 0; i < count; i++)
		{
			CardModel card = target.CombatState.CreateCard<T>(player);
			CardPileAddResult[] array = statusCards;
			int num = i;
			array[num] = await AddGeneratedCardToCombat(card, pileType, addedByPlayer, position);
		}
		if (LocalContext.IsMe(player))
		{
			if (pileType == PileType.Hand)
			{
				await Cmd.Wait(0.1f);
				return;
			}
			CardPreviewStyle style = ((statusCards.Length <= 5) ? CardPreviewStyle.HorizontalLayout : CardPreviewStyle.MessyLayout);
			CardCmd.PreviewCardPileAdd(statusCards, 1.2f, style);
			await Cmd.Wait(1f);
		}
	}

	public static async Task AddCurseToDeck<T>(Player owner) where T : CardModel
	{
		await AddCursesToDeck(new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(ModelDb.Card<T>()), owner);
	}

	public static async Task AddCursesToDeck(IEnumerable<CardModel> curses, Player owner)
	{
		List<CardPileAddResult> results = new List<CardPileAddResult>();
		foreach (CardModel curse in curses)
		{
			if (curse.Type != CardType.Curse)
			{
				throw new ArgumentException(curse.Id.Entry + " is not a curse");
			}
			CardModel card = owner.RunState.CreateCard(curse, owner);
			results.Add(await Add(card, PileType.Deck));
		}
		CardCmd.PreviewCardPileAdd(results, 2f);
	}

	private static bool CheckIfDrawIsPossibleAndShowThoughtBubbleIfNot(Player player)
	{
		if (PileType.Draw.GetPile(player).Cards.Count + PileType.Discard.GetPile(player).Cards.Count == 0)
		{
			ThinkCmd.Play(new LocString("combat_messages", "NO_DRAW"), player.Creature, 2.0);
			return false;
		}
		if (PileType.Hand.GetPile(player).Cards.Count >= 10)
		{
			ThinkCmd.Play(new LocString("combat_messages", "HAND_FULL"), player.Creature, 2.0);
			return false;
		}
		return true;
	}
}
