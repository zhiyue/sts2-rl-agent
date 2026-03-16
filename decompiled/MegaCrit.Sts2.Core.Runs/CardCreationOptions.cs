using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Runs;

public record CardCreationOptions
{
	public IReadOnlyCollection<CardPoolModel> CardPools => _cardPools;

	public Func<CardModel, bool>? CardPoolFilter { get; private set; }

	public IEnumerable<CardModel>? CustomCardPool { get; private set; }

	public CardCreationSource Source { get; private set; }

	public CardRarityOddsType RarityOdds { get; private set; }

	public CardCreationFlags Flags { get; private set; }

	public Rng? RngOverride { get; private set; }

	private readonly List<CardPoolModel> _cardPools = new List<CardPoolModel>();

	public CardCreationOptions(IEnumerable<CardPoolModel> cardPools, CardCreationSource source, CardRarityOddsType rarityOdds, Func<CardModel, bool>? cardPoolFilter = null)
	{
		_cardPools.AddRange(cardPools);
		Source = source;
		RarityOdds = rarityOdds;
		CardPoolFilter = cardPoolFilter;
	}

	public CardCreationOptions(IEnumerable<CardModel> customCardPool, CardCreationSource source, CardRarityOddsType rarityOdds)
	{
		CustomCardPool = customCardPool;
		Source = source;
		RarityOdds = rarityOdds;
		AssertUniformOddsIfSingleRarityPool();
	}

	public static CardCreationOptions ForRoom(Player player, RoomType roomType)
	{
		CardCreationSource cardCreationSource;
		switch (roomType)
		{
		case RoomType.Monster:
		case RoomType.Elite:
		case RoomType.Boss:
			cardCreationSource = CardCreationSource.Encounter;
			break;
		case RoomType.Shop:
			cardCreationSource = CardCreationSource.Shop;
			break;
		case RoomType.Event:
			throw new InvalidOperationException("ForRoom should not be used in event rooms");
		default:
			cardCreationSource = CardCreationSource.Other;
			break;
		}
		CardCreationSource source = cardCreationSource;
		return new CardCreationOptions(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(player.Character.CardPool), source, roomType switch
		{
			RoomType.Monster => CardRarityOddsType.RegularEncounter, 
			RoomType.Elite => CardRarityOddsType.EliteEncounter, 
			RoomType.Boss => CardRarityOddsType.BossEncounter, 
			RoomType.Shop => CardRarityOddsType.Shop, 
			_ => CardRarityOddsType.RegularEncounter, 
		});
	}

	public static CardCreationOptions ForNonCombatWithDefaultOdds(IEnumerable<CardPoolModel> cardPools, Func<CardModel, bool>? cardPoolFilter = null)
	{
		return new CardCreationOptions(cardPools, CardCreationSource.Other, CardRarityOddsType.RegularEncounter, cardPoolFilter).WithFlags(CardCreationFlags.NoUpgradeRoll);
	}

	public static CardCreationOptions ForNonCombatWithDefaultOdds(IEnumerable<CardModel> customCardPool)
	{
		return new CardCreationOptions(customCardPool, CardCreationSource.Other, CardRarityOddsType.RegularEncounter).WithFlags(CardCreationFlags.NoUpgradeRoll);
	}

	public static CardCreationOptions ForNonCombatWithUniformOdds(IEnumerable<CardPoolModel> cardPools, Func<CardModel, bool>? cardPoolFilter = null)
	{
		return new CardCreationOptions(cardPools, CardCreationSource.Other, CardRarityOddsType.Uniform, cardPoolFilter).WithFlags(CardCreationFlags.NoUpgradeRoll);
	}

	public IEnumerable<CardModel> GetPossibleCards(Player player)
	{
		IEnumerable<CardModel> enumerable = ((CardPools.Count <= 0) ? CustomCardPool : (from c in CardPools.SelectMany((CardPoolModel p) => p.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint))
			where CardPoolFilter == null || CardPoolFilter(c)
			select c));
		if (enumerable == null)
		{
			throw new InvalidOperationException("Tried to get possible cards from CardCreationOptions but neither the pool nor custom pool were set!");
		}
		return enumerable;
	}

	public CardCreationOptions WithCustomPool(IEnumerable<CardModel> customPool, CardRarityOddsType? rarityOdds = null)
	{
		CustomCardPool = customPool.ToArray();
		_cardPools.Clear();
		RarityOdds = rarityOdds ?? RarityOdds;
		AssertUniformOddsIfSingleRarityPool();
		return this;
	}

	public CardCreationOptions WithCardPools(IEnumerable<CardPoolModel> pools, Func<CardModel, bool>? cardPoolFilter = null)
	{
		_cardPools.Clear();
		_cardPools.AddRange(pools);
		CardPoolFilter = cardPoolFilter;
		CustomCardPool = null;
		return this;
	}

	public CardCreationOptions WithFlags(CardCreationFlags flag)
	{
		Flags |= flag;
		return this;
	}

	public CardCreationOptions WithRngOverride(Rng rng)
	{
		RngOverride = rng;
		return this;
	}

	private void AssertUniformOddsIfSingleRarityPool()
	{
		if (CustomCardPool != null && RarityOdds != CardRarityOddsType.Uniform)
		{
			CardModel first = CustomCardPool.FirstOrDefault();
			if (first != null && CustomCardPool.All((CardModel c) => c.Rarity == first.Rarity))
			{
				throw new InvalidOperationException($"You have passed a custom card pool with only one rarity to {"CardCreationOptions"} and a rarity odds of {RarityOdds}! This is invalid - card pools with only one rarity must use Uniform rarity odds.");
			}
		}
	}

	public CardRarity? TryGetSingleRarityInPool()
	{
		if (CustomCardPool != null)
		{
			CardModel first = CustomCardPool.FirstOrDefault();
			if (first != null && CustomCardPool.All((CardModel c) => c.Rarity == first.Rarity))
			{
				return first.Rarity;
			}
		}
		else
		{
			List<CardModel> source = CardPools.SelectMany((CardPoolModel c) => c.AllCards).ToList();
			if (CardPoolFilter != null)
			{
				source = source.Where((CardModel c) => CardPoolFilter(c)).ToList();
			}
			CardModel first2 = source.FirstOrDefault();
			if (first2 != null && source.All((CardModel c) => c.Rarity == first2.Rarity))
			{
				return first2.Rarity;
			}
		}
		return null;
	}

	[CompilerGenerated]
	protected virtual bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("CardPools = ");
		builder.Append(CardPools);
		builder.Append(", CardPoolFilter = ");
		builder.Append(CardPoolFilter);
		builder.Append(", CustomCardPool = ");
		builder.Append(CustomCardPool);
		builder.Append(", Source = ");
		builder.Append(Source.ToString());
		builder.Append(", RarityOdds = ");
		builder.Append(RarityOdds.ToString());
		builder.Append(", Flags = ");
		builder.Append(Flags.ToString());
		builder.Append(", RngOverride = ");
		builder.Append(RngOverride);
		return true;
	}

	[CompilerGenerated]
	public override int GetHashCode()
	{
		return ((((((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<List<CardPoolModel>>.Default.GetHashCode(_cardPools)) * -1521134295 + EqualityComparer<Func<CardModel, bool>>.Default.GetHashCode(CardPoolFilter)) * -1521134295 + EqualityComparer<IEnumerable<CardModel>>.Default.GetHashCode(CustomCardPool)) * -1521134295 + EqualityComparer<CardCreationSource>.Default.GetHashCode(Source)) * -1521134295 + EqualityComparer<CardRarityOddsType>.Default.GetHashCode(RarityOdds)) * -1521134295 + EqualityComparer<CardCreationFlags>.Default.GetHashCode(Flags)) * -1521134295 + EqualityComparer<Rng>.Default.GetHashCode(RngOverride);
	}

	[CompilerGenerated]
	public virtual bool Equals(CardCreationOptions? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<List<CardPoolModel>>.Default.Equals(_cardPools, other._cardPools) && EqualityComparer<Func<CardModel, bool>>.Default.Equals(CardPoolFilter, other.CardPoolFilter) && EqualityComparer<IEnumerable<CardModel>>.Default.Equals(CustomCardPool, other.CustomCardPool) && EqualityComparer<CardCreationSource>.Default.Equals(Source, other.Source) && EqualityComparer<CardRarityOddsType>.Default.Equals(RarityOdds, other.RarityOdds) && EqualityComparer<CardCreationFlags>.Default.Equals(Flags, other.Flags))
			{
				return EqualityComparer<Rng>.Default.Equals(RngOverride, other.RngOverride);
			}
			return false;
		}
		return true;
	}

	[CompilerGenerated]
	protected CardCreationOptions(CardCreationOptions original)
	{
		_cardPools = original._cardPools;
		CardPoolFilter = original.CardPoolFilter;
		CustomCardPool = original.CustomCardPool;
		Source = original.Source;
		RarityOdds = original.RarityOdds;
		Flags = original.Flags;
		RngOverride = original.RngOverride;
	}
}
