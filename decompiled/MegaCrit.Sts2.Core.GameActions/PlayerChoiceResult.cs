using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Models;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.GameActions;

public class PlayerChoiceResult
{
	private List<CardModel>? _canonicalCards;

	private List<CardModel>? _combatCards;

	private List<CardModel>? _deckCards;

	private List<CardModel>? _mutableCards;

	private List<int>? _indexes;

	private ulong? _playerId;

	public PlayerChoiceType ChoiceType { get; private init; }

	public static PlayerChoiceResult FromCanonicalCard(CardModel? canonicalCard)
	{
		canonicalCard?.AssertCanonical();
		PlayerChoiceResult obj = new PlayerChoiceResult
		{
			ChoiceType = PlayerChoiceType.CanonicalCard
		};
		List<CardModel> list;
		if (canonicalCard == null)
		{
			list = new List<CardModel>();
		}
		else
		{
			int num = 1;
			list = new List<CardModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<CardModel> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = canonicalCard;
		}
		obj._canonicalCards = list;
		return obj;
	}

	public static PlayerChoiceResult FromMutableCombatCard(CardModel? combatCard)
	{
		combatCard?.AssertMutable();
		CardPile cardPile = combatCard?.Pile;
		if (cardPile != null && !cardPile.IsCombatPile)
		{
			throw new InvalidOperationException("Card must be in a combat pile!");
		}
		PlayerChoiceResult obj = new PlayerChoiceResult
		{
			ChoiceType = PlayerChoiceType.CombatCard
		};
		List<CardModel> list;
		if (combatCard == null)
		{
			list = new List<CardModel>();
		}
		else
		{
			int num = 1;
			list = new List<CardModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<CardModel> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = combatCard;
		}
		obj._combatCards = list;
		return obj;
	}

	public static PlayerChoiceResult FromMutableDeckCard(CardModel? deckCard)
	{
		deckCard?.AssertMutable();
		if (deckCard == null)
		{
			goto IL_0025;
		}
		CardPile pile = deckCard.Pile;
		if (pile != null)
		{
			PileType type = pile.Type;
			if (type == PileType.Deck)
			{
				goto IL_0025;
			}
		}
		bool flag = true;
		goto IL_0027;
		IL_0027:
		if (flag)
		{
			throw new InvalidOperationException("Card must be in a deck!");
		}
		PlayerChoiceResult obj = new PlayerChoiceResult
		{
			ChoiceType = PlayerChoiceType.DeckCard
		};
		List<CardModel> list;
		if (deckCard == null)
		{
			list = new List<CardModel>();
		}
		else
		{
			int num = 1;
			list = new List<CardModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<CardModel> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = deckCard;
		}
		obj._deckCards = list;
		return obj;
		IL_0025:
		flag = false;
		goto IL_0027;
	}

	public static PlayerChoiceResult FromCanonicalCards(IEnumerable<CardModel> canonicalCards)
	{
		return new PlayerChoiceResult
		{
			ChoiceType = PlayerChoiceType.CanonicalCard,
			_canonicalCards = canonicalCards.ToList()
		};
	}

	public static PlayerChoiceResult FromMutableCombatCards(IEnumerable<CardModel> combatCards)
	{
		return new PlayerChoiceResult
		{
			ChoiceType = PlayerChoiceType.CombatCard,
			_combatCards = combatCards.ToList()
		};
	}

	public static PlayerChoiceResult FromMutableDeckCards(IEnumerable<CardModel> deckCards)
	{
		return new PlayerChoiceResult
		{
			ChoiceType = PlayerChoiceType.DeckCard,
			_deckCards = deckCards.ToList()
		};
	}

	public static PlayerChoiceResult FromMutableCard(CardModel? mutableCard)
	{
		mutableCard?.AssertMutable();
		PlayerChoiceResult obj = new PlayerChoiceResult
		{
			ChoiceType = PlayerChoiceType.MutableCard
		};
		List<CardModel> list;
		if (mutableCard == null)
		{
			list = new List<CardModel>();
		}
		else
		{
			int num = 1;
			list = new List<CardModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<CardModel> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = mutableCard;
		}
		obj._mutableCards = list;
		return obj;
	}

	public static PlayerChoiceResult FromMutableCards(IEnumerable<CardModel> mutableCards)
	{
		CardModel[] array = mutableCards.ToArray();
		CardModel[] array2 = array;
		foreach (CardModel cardModel in array2)
		{
			cardModel.AssertMutable();
			if (cardModel.Owner != array[0].Owner)
			{
				throw new InvalidOperationException("All cards passed to FromMutableCards must have the same owner!");
			}
		}
		return new PlayerChoiceResult
		{
			ChoiceType = PlayerChoiceType.MutableCard,
			_mutableCards = array.ToList()
		};
	}

	public static PlayerChoiceResult FromCards(IEnumerable<CardModel> cards, PlayerChoiceType choiceType)
	{
		return choiceType switch
		{
			PlayerChoiceType.CanonicalCard => new PlayerChoiceResult
			{
				ChoiceType = choiceType,
				_canonicalCards = cards.ToList()
			}, 
			PlayerChoiceType.CombatCard => new PlayerChoiceResult
			{
				ChoiceType = choiceType,
				_combatCards = cards.ToList()
			}, 
			PlayerChoiceType.DeckCard => new PlayerChoiceResult
			{
				ChoiceType = choiceType,
				_deckCards = cards.ToList()
			}, 
			PlayerChoiceType.MutableCard => new PlayerChoiceResult
			{
				ChoiceType = choiceType,
				_mutableCards = cards.ToList()
			}, 
			_ => throw new ArgumentOutOfRangeException("choiceType", choiceType, null), 
		};
	}

	public static PlayerChoiceResult FromPlayerId(ulong? playerId)
	{
		return new PlayerChoiceResult
		{
			ChoiceType = PlayerChoiceType.Player,
			_playerId = playerId
		};
	}

	public static PlayerChoiceResult FromIndex(int index)
	{
		PlayerChoiceResult obj = new PlayerChoiceResult
		{
			ChoiceType = PlayerChoiceType.Index
		};
		int num = 1;
		List<int> list = new List<int>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<int> span = CollectionsMarshal.AsSpan(list);
		int index2 = 0;
		span[index2] = index;
		obj._indexes = list;
		return obj;
	}

	public static PlayerChoiceResult FromIndexes(List<int> indexes)
	{
		return new PlayerChoiceResult
		{
			ChoiceType = PlayerChoiceType.Index,
			_indexes = indexes
		};
	}

	public CardModel? AsCanonicalCard()
	{
		if (ChoiceType != PlayerChoiceType.CanonicalCard)
		{
			throw new InvalidOperationException($"Tried to get canonical card from player choice result of type {ChoiceType}!");
		}
		return _canonicalCards.FirstOrDefault();
	}

	public IEnumerable<CardModel> AsCanonicalCards()
	{
		if (ChoiceType != PlayerChoiceType.CanonicalCard)
		{
			throw new InvalidOperationException($"Tried to get canonical cards from player choice result of type {ChoiceType}!");
		}
		return _canonicalCards;
	}

	public IEnumerable<CardModel> AsCombatCards()
	{
		if (ChoiceType != PlayerChoiceType.CombatCard)
		{
			throw new InvalidOperationException($"Tried to get combat cards from player choice result of type {ChoiceType}!");
		}
		return _combatCards;
	}

	public IEnumerable<CardModel> AsDeckCards()
	{
		if (ChoiceType != PlayerChoiceType.DeckCard)
		{
			throw new InvalidOperationException($"Tried to get deck cards from player choice result of type {ChoiceType}!");
		}
		return _deckCards;
	}

	public CardModel? AsMutableCard()
	{
		if (ChoiceType != PlayerChoiceType.MutableCard)
		{
			throw new InvalidOperationException($"Tried to get mutable cards from player choice result of type {ChoiceType}!");
		}
		return _mutableCards.FirstOrDefault();
	}

	public IEnumerable<CardModel> AsMutableCards()
	{
		if (ChoiceType != PlayerChoiceType.MutableCard)
		{
			throw new InvalidOperationException($"Tried to get mutable cards from player choice result of type {ChoiceType}!");
		}
		return _mutableCards;
	}

	public IEnumerable<CardModel> AsCards(PlayerChoiceType type)
	{
		if (ChoiceType != type)
		{
			throw new InvalidOperationException($"Tried to get cards of type {type} from player choice result of type {ChoiceType}!");
		}
		return type switch
		{
			PlayerChoiceType.CanonicalCard => _canonicalCards, 
			PlayerChoiceType.CombatCard => _combatCards, 
			PlayerChoiceType.DeckCard => _deckCards, 
			PlayerChoiceType.MutableCard => _mutableCards, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public ulong? AsPlayerId()
	{
		if (ChoiceType != PlayerChoiceType.Player)
		{
			throw new InvalidOperationException($"Tried to get player ID from player choice result of type {ChoiceType}!");
		}
		return _playerId;
	}

	public int AsIndex()
	{
		if (ChoiceType != PlayerChoiceType.Index)
		{
			throw new InvalidOperationException($"Tried to get index from player choice result of type {ChoiceType}!");
		}
		return _indexes?.FirstOrDefault() ?? (-1);
	}

	public List<int> AsIndexes()
	{
		if (ChoiceType != PlayerChoiceType.Index)
		{
			throw new InvalidOperationException($"Tried to get indexes from player choice result of type {ChoiceType}!");
		}
		return _indexes;
	}

	public static PlayerChoiceResult FromNetData(Player sender, IPlayerCollection players, NetPlayerChoiceResult netData)
	{
		PlayerChoiceResult playerChoiceResult = new PlayerChoiceResult
		{
			ChoiceType = netData.type
		};
		switch (playerChoiceResult.ChoiceType)
		{
		case PlayerChoiceType.CanonicalCard:
			playerChoiceResult._canonicalCards = netData.canonicalCards;
			break;
		case PlayerChoiceType.CombatCard:
			playerChoiceResult._combatCards = netData.combatCards.Select((NetCombatCard c) => c.ToCardModel()).ToList();
			break;
		case PlayerChoiceType.DeckCard:
			playerChoiceResult._deckCards = netData.deckCards.Select((NetDeckCard c) => c.ToCardModel(sender)).ToList();
			break;
		case PlayerChoiceType.MutableCard:
			playerChoiceResult._mutableCards = netData.mutableCards.Select(CardModel.FromSerializable).ToList();
			if (!netData.mutableCardOwner.HasValue)
			{
				break;
			}
			foreach (CardModel mutableCard in playerChoiceResult._mutableCards)
			{
				mutableCard.Owner = players.GetPlayer(netData.mutableCardOwner.Value);
			}
			break;
		case PlayerChoiceType.Player:
			playerChoiceResult._playerId = netData.playerId;
			break;
		case PlayerChoiceType.Index:
			playerChoiceResult._indexes = netData.indexes;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return playerChoiceResult;
	}

	public NetPlayerChoiceResult ToNetData()
	{
		return new NetPlayerChoiceResult
		{
			type = ChoiceType,
			combatCards = _combatCards?.Select(NetCombatCard.FromModel).ToList(),
			deckCards = _deckCards?.Select(NetDeckCard.FromModel).ToList(),
			canonicalCards = _canonicalCards?.ToList(),
			mutableCards = _mutableCards?.Select((CardModel c) => c.ToSerializable()).ToList(),
			mutableCardOwner = _mutableCards?.FirstOrDefault()?.Owner.NetId,
			playerId = _playerId,
			indexes = _indexes
		};
	}

	public override string ToString()
	{
		return ChoiceType switch
		{
			PlayerChoiceType.CanonicalCard => "PlayerChoiceResult canonical " + string.Join(",", _canonicalCards ?? new List<CardModel>()), 
			PlayerChoiceType.CombatCard => "PlayerChoiceResult combat " + string.Join(",", _combatCards ?? new List<CardModel>()), 
			PlayerChoiceType.DeckCard => "PlayerChoiceResult deck " + string.Join(",", _deckCards ?? new List<CardModel>()), 
			PlayerChoiceType.MutableCard => $"{"PlayerChoiceResult"} mutable cards {string.Join(",", _mutableCards ?? new List<CardModel>())}, owner: {_mutableCards?.FirstOrDefault()?.Owner.NetId}", 
			PlayerChoiceType.Player => $"{"PlayerChoiceResult"} player ID {_playerId}", 
			PlayerChoiceType.Index => "PlayerChoiceResult indexes " + string.Join(",", _indexes ?? new List<int>()), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
