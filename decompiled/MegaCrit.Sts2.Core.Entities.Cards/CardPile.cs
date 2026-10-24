using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Entities.Cards;

public class CardPile
{
	private readonly List<CardModel> _cards = new List<CardModel>();

	/// <summary>
	/// This is functionally a constant but is a property for modding ergonomics.
	/// </summary>
	public static int MaxCardsInHand => 10;

	public PileType Type { get; }

	public IReadOnlyList<CardModel> Cards => _cards;

	public bool IsEmpty => !Cards.Any();

	public bool IsCombatPile => Type.IsCombatPile();

	public int UpgradableCardCount => _cards.Count((CardModel card) => card.IsUpgradable);

	public event Action? ContentsChanged;

	public event Action<CardModel>? CardAdded;

	public event Action<CardModel>? CardRemoved;

	public event Action? CardAddFinished;

	public event Action? CardRemoveFinished;

	public CardPile(PileType type)
	{
		Type = type;
		base._002Ector();
	}

	public static CardPile? Get(PileType type, Player player)
	{
		return type switch
		{
			PileType.None => null, 
			PileType.Draw => player.PlayerCombatState?.DrawPile, 
			PileType.Hand => player.PlayerCombatState?.Hand, 
			PileType.Discard => player.PlayerCombatState?.DiscardPile, 
			PileType.Exhaust => player.PlayerCombatState?.ExhaustPile, 
			PileType.Play => player.PlayerCombatState?.PlayPile, 
			PileType.Deck => player.Deck, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public static IEnumerable<CardModel> GetCards(Player player, params PileType[] piles)
	{
		return piles.SelectMany((PileType p) => p.GetPile(player).Cards);
	}

	/// <summary>
	/// Randomize the order of the cards in this pile.
	/// Do NOT call this when your intention is to do a standard shuffle action.
	/// Use <see cref="M:MegaCrit.Sts2.Core.Commands.CardPileCmd.Shuffle(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Players.Player)" /> instead.
	/// This is primarily meant for randomizing the order of the draw pile when a combat first starts.
	/// </summary>
	public void RandomizeOrderInternal(Player player, Rng rng, CombatState state)
	{
		_cards.UnstableShuffle(rng);
		TestRngInjector.ConsumeInitialShuffleOverride()?.Invoke(_cards);
		Hook.ModifyShuffleOrder(state, player, _cards, isInitialShuffle: true);
	}

	/// <summary>
	/// Add a card to the bottom of this pile.
	/// </summary>
	/// <param name="card">Card to add.</param>
	/// <param name="index">Index to insert the card into. -1 will cause it to be added to the end ("bottom").</param>
	/// <param name="silent">If true, no events will be called.</param>
	/// <exception cref="T:System.InvalidOperationException">Thrown if this specific CardModel instance is already in the pile.</exception>
	public void AddInternal(CardModel card, int index = -1, bool silent = false)
	{
		card.AssertMutable();
		if (Cards.Contains(card))
		{
			throw new InvalidOperationException($"Card pile already contains {card}.");
		}
		if (index >= 0)
		{
			_cards.Insert(index, card);
		}
		else
		{
			_cards.Add(card);
		}
		if (IsCombatPile && CombatManager.Instance.IsInProgress)
		{
			CombatManager.Instance.StateTracker.Subscribe(card);
		}
		if (!silent)
		{
			this.CardAdded?.Invoke(card);
			InvokeContentsChanged();
		}
	}

	/// <summary>
	/// Remove a card from this pile.
	/// </summary>
	/// <param name="card">Card to remove.</param>
	/// <param name="silent">If set to true, no events will be sent to update the UI.</param>
	/// <exception cref="T:System.InvalidOperationException">Thrown if this specific CardModel instance is not in the pile.</exception>
	public void RemoveInternal(CardModel card, bool silent = false)
	{
		if (!Cards.Contains(card))
		{
			throw new InvalidOperationException($"Card pile does not contain {card}.");
		}
		_cards.Remove(card);
		if (IsCombatPile)
		{
			CombatManager.Instance.StateTracker.Unsubscribe(card);
		}
		if (!silent)
		{
			this.CardRemoved?.Invoke(card);
			InvokeContentsChanged();
			InvokeCardRemoveFinished();
		}
	}

	/// <summary>
	/// Move a card to the bottom of this pile.
	/// The card must already be in this pile.
	/// This does not send any events to update the UI, so you should only use this in combination with other methods
	/// that do.
	/// </summary>
	/// <param name="card">Card that we're moving to the bottom.</param>
	/// <exception cref="T:System.InvalidOperationException"></exception>
	public void MoveToBottomInternal(CardModel card)
	{
		if (!Cards.Contains(card))
		{
			throw new InvalidOperationException($"Card pile does not contain {card}.");
		}
		_cards.Remove(card);
		_cards.Add(card);
	}

	/// <summary>
	/// Move a card to the top of this pile.
	/// The card must already be in this pile.
	/// This does not send any events to update the UI, so you should only use this in combination with other methods
	/// that do.
	/// </summary>
	/// <param name="card">Card that we're moving to the top.</param>
	/// <exception cref="T:System.InvalidOperationException"></exception>
	public void MoveToTopInternal(CardModel card)
	{
		if (!Cards.Contains(card))
		{
			throw new InvalidOperationException($"Card pile does not contain {card}.");
		}
		_cards.Remove(card);
		_cards.Insert(0, card);
	}

	public void Clear(bool silent = false)
	{
		foreach (CardModel item in Cards.ToList())
		{
			RemoveInternal(item, silent);
		}
		_cards.Clear();
	}

	public void InvokeCardAddFinished()
	{
		this.CardAddFinished?.Invoke();
	}

	public void InvokeCardRemoveFinished()
	{
		this.CardRemoveFinished?.Invoke();
	}

	public void InvokeContentsChanged()
	{
		this.ContentsChanged?.Invoke();
	}
}
