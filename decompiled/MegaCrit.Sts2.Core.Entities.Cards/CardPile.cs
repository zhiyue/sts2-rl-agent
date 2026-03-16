using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Entities.Cards;

public class CardPile
{
	public const int maxCardsInHand = 10;

	private readonly List<CardModel> _cards = new List<CardModel>();

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

	public void RandomizeOrderInternal(Player player, Rng rng, CombatState state)
	{
		_cards.UnstableShuffle(rng);
		Hook.ModifyShuffleOrder(state, player, _cards, isInitialShuffle: true);
	}

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

	public void MoveToBottomInternal(CardModel card)
	{
		if (!Cards.Contains(card))
		{
			throw new InvalidOperationException($"Card pile does not contain {card}.");
		}
		_cards.Remove(card);
		_cards.Add(card);
	}

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
