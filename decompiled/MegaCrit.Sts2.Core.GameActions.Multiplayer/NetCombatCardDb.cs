using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

public class NetCombatCardDb
{
	private struct Subscription
	{
		public CardPile pile;

		public Action action;
	}

	private uint _nextId;

	private readonly Dictionary<uint, CardModel> _idToCard = new Dictionary<uint, CardModel>();

	private readonly Dictionary<CardModel, uint> _cardToId = new Dictionary<CardModel, uint>();

	private readonly List<Subscription> _subscriptions = new List<Subscription>();

	public static NetCombatCardDb Instance { get; } = new NetCombatCardDb();

	public void StartCombat(IReadOnlyList<Player> players)
	{
		_nextId = 0u;
		_idToCard.Clear();
		_cardToId.Clear();
		foreach (Player player in players)
		{
			if (player.PlayerCombatState == null && TestMode.IsOn)
			{
				continue;
			}
			foreach (CardModel item in player.PlayerCombatState.AllPiles.SelectMany((CardPile p) => p.Cards))
			{
				IdCardIfNecessary(item);
			}
			foreach (CardPile allPile in player.PlayerCombatState.AllPiles)
			{
				Subscription subscription = default(Subscription);
				subscription.pile = allPile;
				subscription.action = delegate
				{
					OnPileContentsChanged(subscription.pile);
				};
				_subscriptions.Add(subscription);
				allPile.ContentsChanged += subscription.action;
			}
		}
		CombatManager.Instance.CombatEnded += OnCombatEnded;
	}

	private void OnCombatEnded(CombatRoom _)
	{
		foreach (Subscription subscription in _subscriptions)
		{
			subscription.pile.ContentsChanged -= subscription.action;
		}
		_subscriptions.Clear();
		CombatManager.Instance.CombatEnded -= OnCombatEnded;
	}

	public uint GetCardId(CardModel card)
	{
		if (!TryGetCardId(card, out var id))
		{
			throw new InvalidOperationException($"Card {card} could not be found in combat ID database!");
		}
		return id;
	}

	public CardModel GetCard(uint id)
	{
		if (!TryGetCard(id, out CardModel card))
		{
			throw new InvalidOperationException($"Could not map ID {id} to any card!");
		}
		return card;
	}

	public bool TryGetCardId(CardModel card, out uint id)
	{
		if (!card.IsMutable)
		{
			throw new InvalidOperationException($"Tried to get ID for canonical card {card}! Use the ModelId instead");
		}
		return _cardToId.TryGetValue(card, out id);
	}

	public bool TryGetCard(uint id, out CardModel? card)
	{
		return _idToCard.TryGetValue(id, out card);
	}

	private void OnPileContentsChanged(CardPile pile)
	{
		foreach (CardModel card in pile.Cards)
		{
			IdCardIfNecessary(card);
		}
	}

	private void IdCardIfNecessary(CardModel card)
	{
		if (!_cardToId.ContainsKey(card))
		{
			_cardToId[card] = _nextId;
			_idToCard[_nextId] = card;
			_nextId++;
		}
	}

	public uint IdCardForTesting(CardModel card)
	{
		IdCardIfNecessary(card);
		return _cardToId[card];
	}

	public void ClearCardsForTesting()
	{
		_nextId = 0u;
		_idToCard.Clear();
		_cardToId.Clear();
	}
}
