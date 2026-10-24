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

/// <summary>
/// Options passed when creating cards for rewards.
/// Various combinations of these options will do different things, so read carefully when you are using this for a
/// new circumstance.
/// </summary>
public record CardCreationOptions
{
	/// <summary>
	/// The card pool from which rewards will be pulled.
	/// Either <see cref="P:MegaCrit.Sts2.Core.Runs.CardCreationOptions.CardPools" /> must be non-empty or <see cref="P:MegaCrit.Sts2.Core.Runs.CardCreationOptions.CustomCardPool" /> must be non-null.
	/// </summary>
	public IReadOnlyCollection<CardPoolModel> CardPools => _cardPools;

	/// <summary>
	/// The filter to apply to the card pool when generating rewards.
	/// We use this instead of <see cref="P:MegaCrit.Sts2.Core.Runs.CardCreationOptions.CustomCardPool" /> because it allows effects that modify the available card
	/// pools (like <see cref="T:MegaCrit.Sts2.Core.Models.Relics.PrismaticGem" />) to properly apply the same filter.
	/// WARNING: This cannot be serialized, so don't use this when adding extra rewards to combat rewards.
	/// </summary>
	public Func<CardModel, bool>? CardPoolFilter { get; private set; }

	/// <summary>
	/// The card pool from which rewards will be pulled.
	/// Either <see cref="P:MegaCrit.Sts2.Core.Runs.CardCreationOptions.CardPools" /> must be non-empty or <see cref="P:MegaCrit.Sts2.Core.Runs.CardCreationOptions.CustomCardPool" /> must be non-null.
	/// WARNING: This cannot be serialized, so don't use this when adding extra rewards to combat rewards.
	/// </summary>
	public IEnumerable<CardModel>? CustomCardPool { get; private set; }

	/// <summary>
	/// The source from which this reward is being generated from.
	/// Used to indicate to modification hooks whether they should apply to this reward or not. For example, some relics
	/// specify that they only apply to encounter rewards, and not to rewards from events or shops.
	/// </summary>
	public CardCreationSource Source { get; private set; }

	/// <summary>
	/// The odds to use when generating the rarity of the cards in the reward.
	/// Hooks should look at the source to determine where the cards come from, not this.
	/// </summary>
	public CardRarityOddsType RarityOdds { get; private set; }

	/// <summary>
	/// Restrictions applied when determining what can modify the generated rewards.
	/// </summary>
	public CardCreationFlags Flags { get; private set; }

	/// <summary>
	/// If set, we use this override over the default one in the factory
	/// Used in the Crystal Sphere Items so that we can pass the event rng to be used and not hit state divergences in multiplayer
	/// since we don't sync the player rng sets in that case
	/// </summary>
	public Rng? RngOverride { get; private set; }

	private readonly List<CardPoolModel> _cardPools = new List<CardPoolModel>();

	/// <summary>
	/// Constructor which takes a card pool model.
	/// </summary>
	/// <param name="cardPools">The card pools from which rewards will be generated.</param>
	/// <param name="source">The purpose for which the rewards are being generated.</param>
	/// <param name="rarityOdds">The odds to apply when rolling card rarities.</param>
	/// <param name="cardPoolFilter">A filter to apply to the card pool when generating rewards.</param>
	public CardCreationOptions(IEnumerable<CardPoolModel> cardPools, CardCreationSource source, CardRarityOddsType rarityOdds, Func<CardModel, bool>? cardPoolFilter = null)
	{
		_cardPools.AddRange(cardPools);
		Source = source;
		RarityOdds = rarityOdds;
		CardPoolFilter = cardPoolFilter;
	}

	/// <summary>
	/// Constructor which takes a custom card pool.
	/// </summary>
	/// <param name="customCardPool">The list of cards from which rewards will be generated.</param>
	/// <param name="source">The purpose for which the rewards are being generated.</param>
	/// <param name="rarityOdds">The odds to apply when rolling card rarities.</param>
	public CardCreationOptions(IEnumerable<CardModel> customCardPool, CardCreationSource source, CardRarityOddsType rarityOdds)
	{
		CustomCardPool = customCardPool;
		Source = source;
		RarityOdds = rarityOdds;
		AssertUniformOddsIfSingleRarityPool();
	}

	/// <summary>
	/// Creates a default instance of CardCreationOptions for use in generating end-of-combat rewards or shop rewards for
	/// the given room type.
	/// Do not use this in events or relics.
	/// </summary>
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

	/// <summary>
	/// Generates an instance of CardCreationOptions which can be used when generating rewards for events or relics.
	/// This uses CardRarityOddsType.RegularEncounter to generate card rarities, meaning that common cards will be far
	/// more likely to show up than rare ones.
	/// </summary>
	public static CardCreationOptions ForNonCombatWithDefaultOdds(IEnumerable<CardPoolModel> cardPools, Func<CardModel, bool>? cardPoolFilter = null)
	{
		return new CardCreationOptions(cardPools, CardCreationSource.Other, CardRarityOddsType.RegularEncounter, cardPoolFilter).WithFlags(CardCreationFlags.NoUpgradeRoll);
	}

	/// <summary>
	/// Generates an instance of CardCreationOptions which should be used when generating rewards for events or relics.
	/// This uses CardRarityOddsType.RegularEncounter to generate card rarities, meaning that common cards will be much
	/// more likely to show up than rare ones.
	/// DO NOT use this if your custom pool only contains cards of specific rarities! CardRarityOddsType.RegularEncounter
	/// rolls for rarity, and if it lands on a rarity that your pool does not contain, then it will break. In that
	/// situation, use ForEventWithUniformOdds instead.
	/// </summary>
	public static CardCreationOptions ForNonCombatWithDefaultOdds(IEnumerable<CardModel> customCardPool)
	{
		return new CardCreationOptions(customCardPool, CardCreationSource.Other, CardRarityOddsType.RegularEncounter).WithFlags(CardCreationFlags.NoUpgradeRoll);
	}

	/// <summary>
	/// Generates an instance of CardCreationOptions which can be used when generating rewards for events or relics.
	/// This uses CardRarityOddsType.Uniform to generate card rarities, meaning that all cards in the pool will have an
	/// even chance of being rolled.
	/// </summary>
	public static CardCreationOptions ForNonCombatWithUniformOdds(IEnumerable<CardPoolModel> cardPools, Func<CardModel, bool>? cardPoolFilter = null)
	{
		return new CardCreationOptions(cardPools, CardCreationSource.Other, CardRarityOddsType.Uniform, cardPoolFilter).WithFlags(CardCreationFlags.NoUpgradeRoll);
	}

	/// <summary>
	/// Get all the cards that could be created for the specified player based on this set of options.
	/// </summary>
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

	/// <summary>
	/// Sets the custom pool on this instance of CardCreationOptions.
	/// </summary>
	/// <param name="customPool">The custom pool to use when rolling rewards.</param>
	/// <param name="rarityOdds">
	/// The new rarity odds to use when rolling rewards.
	/// If there is only one rarity in the new custom pool, you should pass Uniform to this.
	/// </param>
	public CardCreationOptions WithCustomPool(IEnumerable<CardModel> customPool, CardRarityOddsType? rarityOdds = null)
	{
		CustomCardPool = customPool.ToArray();
		_cardPools.Clear();
		RarityOdds = rarityOdds ?? RarityOdds;
		AssertUniformOddsIfSingleRarityPool();
		return this;
	}

	/// <summary>
	/// Sets the card pool on this instance of CardCreationOptions.
	/// </summary>
	public CardCreationOptions WithCardPools(IEnumerable<CardPoolModel> pools, Func<CardModel, bool>? cardPoolFilter = null)
	{
		_cardPools.Clear();
		_cardPools.AddRange(pools);
		CardPoolFilter = cardPoolFilter;
		CustomCardPool = null;
		return this;
	}

	/// <summary>
	/// Sets flags on this instance of CardCreationOptions.
	/// </summary>
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
