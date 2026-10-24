using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Exceptions;
using MegaCrit.Sts2.Core.Models.Modifiers;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace MegaCrit.Sts2.Core.Models;

public static class ModelDb
{
	private const int _initialCapacity = 4096;

	private static readonly Dictionary<ModelId, AbstractModel> _contentById = new Dictionary<ModelId, AbstractModel>(4096);

	private static IEnumerable<CardModel>? _allCards;

	private static IEnumerable<CardPoolModel>? _allCardPools;

	private static IEnumerable<CardPoolModel>? _allCharacterCardPools;

	private static IEnumerable<EventModel>? _allSharedEvents;

	private static IEnumerable<EventModel>? _allEvents;

	private static IEnumerable<EncounterModel>? _allEncounters;

	private static IEnumerable<PotionModel>? _allPotions;

	private static IEnumerable<PotionPoolModel>? _allPotionPools;

	private static IEnumerable<PotionPoolModel>? _allCharacterPotionPools;

	private static IEnumerable<RelicPoolModel>? _allCharacterRelicPools;

	private static IEnumerable<PotionPoolModel>? _allSharedPotionPools;

	private static IEnumerable<PowerModel>? _allPowers;

	private static IEnumerable<RelicModel>? _allRelics;

	private static List<ActModel>? _acts;

	private static List<List<ActModel>>? _actsByIndex;

	private static List<BadgeModel>? _badges;

	private static List<AchievementModel>? _achievements;

	/// <summary>
	/// Returns every single class that subclasses AbstractModel.
	/// Prefer this over using AbstractModelSubtypes.All because it also returns models in mods.
	/// </summary>
	public static Type[] AllAbstractModelSubtypes
	{
		get
		{
			List<Type> list = new List<Type>();
			list.AddRange(AbstractModelSubtypes.All);
			list.AddRange(ReflectionHelper.GetSubtypesInMods<AbstractModel>());
			return list.ToArray();
		}
	}

	/// <summary>
	/// Every Affliction defined in the game code, including mock ones for testing.
	/// </summary>
	public static IEnumerable<AfflictionModel> DebugAfflictions => from t in AllAbstractModelSubtypes
		where t.IsSubclassOf(typeof(AfflictionModel))
		select (AfflictionModel)Get(t);

	/// <summary>
	/// Every Enchantment defined in the game code, including mock ones for testing.
	/// </summary>
	public static IEnumerable<EnchantmentModel> DebugEnchantments => from t in AllAbstractModelSubtypes
		where t.IsSubclassOf(typeof(EnchantmentModel))
		select (EnchantmentModel)Get(t);

	/// <summary>
	/// Get all the cards in the game (ignores Unlocks/Epoch state).
	/// Be careful using this, it includes cards that you shouldn't be able to randomly roll for rewards.
	/// </summary>
	public static IEnumerable<CardModel> AllCards => _allCards ?? (_allCards = AllCardPools.SelectMany((CardPoolModel p) => p.AllCards).Concat(AllCharacters.SelectMany((CharacterModel c) => c.StartingDeck).Distinct()).Distinct());

	/// <summary>
	/// Get all the card pools in the game.
	/// Ignores Unlocks/Epoch state.
	/// </summary>
	public static IEnumerable<CardPoolModel> AllCardPools => _allCardPools ?? (_allCardPools = AllCharacterCardPools.Concat(AllSharedCardPools).Distinct());

	/// <summary>
	/// The card pools that are shared across all characters.
	/// WARNING: Do NOT add TestCardPool to this list, or test cards might accidentally appear in-game.
	/// </summary>
	public static IEnumerable<CardPoolModel> AllSharedCardPools => new global::_003C_003Ez__ReadOnlyArray<CardPoolModel>(new CardPoolModel[7]
	{
		CardPool<ColorlessCardPool>(),
		CardPool<CurseCardPool>(),
		CardPool<DeprecatedCardPool>(),
		CardPool<EventCardPool>(),
		CardPool<QuestCardPool>(),
		CardPool<StatusCardPool>(),
		CardPool<TokenCardPool>()
	});

	/// <summary>
	/// Get all the card pools in the game that belong to specific characters.
	/// </summary>
	public static IEnumerable<CardPoolModel> AllCharacterCardPools => _allCharacterCardPools ?? (_allCharacterCardPools = AllCharacters.Select((CharacterModel c) => c.CardPool));

	/// <summary>
	/// Get all the characters in the game (ignores Unlocks/Epoch state).
	/// </summary>
	public static IEnumerable<CharacterModel> AllCharacters => new global::_003C_003Ez__ReadOnlyArray<CharacterModel>(new CharacterModel[5]
	{
		Character<Ironclad>(),
		Character<Silent>(),
		Character<Regent>(),
		Character<Necrobinder>(),
		Character<Defect>()
	});

	/// <summary>
	/// Get all the events in the game that don't belong to a specific act.
	/// </summary>
	public static IEnumerable<EventModel> AllSharedEvents => _allSharedEvents ?? (_allSharedEvents = new global::_003C_003Ez__ReadOnlyArray<EventModel>(new EventModel[18]
	{
		Event<BrainLeech>(),
		Event<CrystalSphere>(),
		Event<DollRoom>(),
		Event<FakeMerchant>(),
		Event<PotionCourier>(),
		Event<RanwidTheElder>(),
		Event<RelicTrader>(),
		Event<RoomFullOfCheese>(),
		Event<SelfHelpBook>(),
		Event<SlipperyBridge>(),
		Event<StoneOfAllTime>(),
		Event<Symbiote>(),
		Event<TeaMaster>(),
		Event<TheFutureOfPotions>(),
		Event<TheLegendsWereTrue>(),
		Event<ThisOrThat>(),
		Event<WarHistorianRepy>(),
		Event<WelcomeToWongos>()
	}));

	/// <summary>
	/// Get all the ancients in the game.
	/// Ignores Unlocks/Epoch state.
	/// </summary>
	public static IEnumerable<AncientEventModel> AllAncients => Acts.SelectMany((ActModel a) => a.AllAncients).Concat(AllSharedAncients).Distinct();

	/// <summary>
	/// Get all the ancients in the game that don't belong to a specific act.
	/// Notably, we only have 1 right now. That's Darv.
	/// </summary>
	public static IEnumerable<AncientEventModel> AllSharedAncients => new global::_003C_003Ez__ReadOnlySingleElementList<AncientEventModel>(AncientEvent<Darv>());

	/// <summary>
	/// Get all the events in the game (ignores Unlocks/Epoch state).
	/// </summary>
	public static IEnumerable<EventModel> AllEvents => _allEvents ?? (_allEvents = Acts.SelectMany((ActModel a) => a.AllEvents).Concat(AllSharedEvents).Distinct());

	/// <summary>
	/// Returns a list of every possible Monster in the game.
	/// </summary>
	public static IEnumerable<MonsterModel> Monsters => Acts.SelectMany((ActModel act) => act.AllMonsters).Distinct();

	/// <summary>
	/// Get all the encounters in the game.
	/// </summary>
	public static IEnumerable<EncounterModel> AllEncounters => _allEncounters ?? (_allEncounters = Acts.SelectMany((ActModel a) => a.AllEncounters).Distinct());

	/// <summary>
	/// Get all the potions in the game.
	/// Be careful using this, it includes potions that you shouldn't be able to randomly roll for rewards.
	/// </summary>
	public static IEnumerable<PotionModel> AllPotions => _allPotions ?? (_allPotions = from p in AllPotionPools.SelectMany((PotionPoolModel p) => p.AllPotions).Distinct()
		orderby p.Id.Entry
		select p);

	/// <summary>
	/// Get all the potion pools in the game.
	/// </summary>
	public static IEnumerable<PotionPoolModel> AllPotionPools => _allPotionPools ?? (_allPotionPools = AllCharacterPotionPools.Concat(AllSharedPotionPools).Distinct());

	/// <summary>
	/// Get all the potion pools in the game that belong to specific characters.
	/// </summary>
	public static IEnumerable<PotionPoolModel> AllCharacterPotionPools => _allCharacterPotionPools ?? (_allCharacterPotionPools = AllCharacters.Select((CharacterModel c) => c.PotionPool));

	/// <summary>
	/// Get all the relic pools in the game that belong to specific characters.
	/// </summary>
	public static IEnumerable<RelicPoolModel> AllCharacterRelicPools => _allCharacterRelicPools ?? (_allCharacterRelicPools = AllCharacters.Select((CharacterModel c) => c.RelicPool));

	/// <summary>
	/// Get all the potion pools in the game that are shared between all characters.
	/// </summary>
	private static IEnumerable<PotionPoolModel> AllSharedPotionPools => _allSharedPotionPools ?? (_allSharedPotionPools = new global::_003C_003Ez__ReadOnlyArray<PotionPoolModel>(new PotionPoolModel[5]
	{
		PotionPool<DeprecatedPotionPool>(),
		PotionPool<EventPotionPool>(),
		PotionPool<MockPotionPool>(),
		PotionPool<SharedPotionPool>(),
		PotionPool<TokenPotionPool>()
	}));

	public static IEnumerable<PowerModel> AllPowers => _allPowers ?? (_allPowers = from t in AllAbstractModelSubtypes
		where t.IsSubclassOf(typeof(PowerModel))
		select (PowerModel)Get(t));

	/// <summary>
	/// Get all the relics in the game (ignores Unlocks/Epoch state).
	/// Be careful using this, it includes relics that you shouldn't be able to randomly roll for rewards.
	/// </summary>
	public static IEnumerable<RelicModel> AllRelics => _allRelics ?? (_allRelics = from r in AllRelicPools.SelectMany((RelicPoolModel p) => p.AllRelics).Concat(AllCharacters.SelectMany((CharacterModel c) => c.StartingRelics)).Distinct()
		orderby r.Id.Entry
		select r);

	/// <summary>
	/// Get all the relic pools in the game.
	/// </summary>
	public static IEnumerable<RelicPoolModel> AllRelicPools => CharacterRelicPools.Concat(AllSharedRelicPools).Distinct();

	/// <summary>
	/// Get all the relic pools in the game that belong to specific characters.
	/// </summary>
	public static IEnumerable<RelicPoolModel> CharacterRelicPools => AllCharacters.Select((CharacterModel c) => c.RelicPool);

	/// <summary>
	/// Get all the relic pools in the game that are shared between all characters.
	/// </summary>
	private static IEnumerable<RelicPoolModel> AllSharedRelicPools => new global::_003C_003Ez__ReadOnlyArray<RelicPoolModel>(new RelicPoolModel[4]
	{
		RelicPool<DeprecatedRelicPool>(),
		RelicPool<EventRelicPool>(),
		RelicPool<FallbackRelicPool>(),
		RelicPool<SharedRelicPool>()
	});

	public static IEnumerable<OrbModel> Orbs => new global::_003C_003Ez__ReadOnlyArray<OrbModel>(new OrbModel[4]
	{
		Orb<LightningOrb>(),
		Orb<FrostOrb>(),
		Orb<DarkOrb>(),
		Orb<PlasmaOrb>()
	});

	/// <summary>
	/// All acts in the game.
	/// Note that callers depend on this list being sorted by act index, then by default/non-default.
	/// If you need a different ordering, feel free to change this or split into another list somewhere.
	/// </summary>
	public static IEnumerable<ActModel> Acts
	{
		get
		{
			if (_acts == null)
			{
				int num = 4;
				List<ActModel> list = new List<ActModel>(num);
				CollectionsMarshal.SetCount(list, num);
				Span<ActModel> span = CollectionsMarshal.AsSpan(list);
				int num2 = 0;
				span[num2] = Act<Overgrowth>();
				num2++;
				span[num2] = Act<Underdocks>();
				num2++;
				span[num2] = Act<Hive>();
				num2++;
				span[num2] = Act<Glory>();
				_acts = list;
			}
			return _acts;
		}
	}

	public static IReadOnlyList<IReadOnlyList<ActModel>> ActsByIndex
	{
		get
		{
			if (_actsByIndex != null)
			{
				return _actsByIndex;
			}
			_actsByIndex = new List<List<ActModel>>();
			foreach (ActModel act in Acts)
			{
				if (act.Index >= 0)
				{
					for (int i = _actsByIndex.Count; i <= act.Index; i++)
					{
						_actsByIndex.Add(new List<ActModel>());
					}
					_actsByIndex[act.Index].Add(act);
				}
			}
			return _actsByIndex;
		}
	}

	public static IReadOnlyList<BadgeModel> BadgeModels
	{
		get
		{
			if (_badges == null)
			{
				_badges = new List<BadgeModel>();
				Type[] allAbstractModelSubtypes = AllAbstractModelSubtypes;
				foreach (Type type in allAbstractModelSubtypes)
				{
					if (type.IsSubclassOf(typeof(BadgeModel)))
					{
						_badges.Add((BadgeModel)Get(type));
					}
				}
			}
			return _badges;
		}
	}

	public static IReadOnlyList<AchievementModel> Achievements
	{
		get
		{
			if (_achievements == null)
			{
				_achievements = new List<AchievementModel>();
				Type[] allAbstractModelSubtypes = AllAbstractModelSubtypes;
				foreach (Type type in allAbstractModelSubtypes)
				{
					if (type.IsSubclassOf(typeof(AchievementModel)))
					{
						_achievements.Add((AchievementModel)Get(type));
					}
				}
			}
			return _achievements;
		}
	}

	public static IReadOnlyList<ModifierModel> GoodModifiers => new global::_003C_003Ez__ReadOnlyArray<ModifierModel>(new ModifierModel[9]
	{
		Modifier<Draft>(),
		Modifier<SealedDeck>(),
		Modifier<Hoarder>(),
		Modifier<Specialized>(),
		Modifier<Insanity>(),
		Modifier<AllStar>(),
		Modifier<Flight>(),
		Modifier<Vintage>(),
		Modifier<CharacterCards>()
	});

	public static IReadOnlyList<ModifierModel> BadModifiers => new global::_003C_003Ez__ReadOnlyArray<ModifierModel>(new ModifierModel[7]
	{
		Modifier<DeadlyEvents>(),
		Modifier<CursedRun>(),
		Modifier<BigGameHunter>(),
		Modifier<Midas>(),
		Modifier<Murderous>(),
		Modifier<NightTerrors>(),
		Modifier<Terminal>()
	});

	public static IReadOnlyList<IReadOnlySet<ModifierModel>> MutuallyExclusiveModifiers => new global::_003C_003Ez__ReadOnlySingleElementList<IReadOnlySet<ModifierModel>>(new HashSet<ModifierModel>
	{
		Modifier<SealedDeck>(),
		Modifier<Draft>(),
		Modifier<Insanity>()
	});

	/// <summary>
	/// Initializes the ModelDb.
	/// Note that the alternative is to initialize in a static initializer, but if we do this we don't control the time
	/// at which it gets initialized, and it usually happens at a bad time during gameplay.
	/// </summary>
	public static void Init()
	{
		Type[] allAbstractModelSubtypes = AllAbstractModelSubtypes;
		foreach (Type type in allAbstractModelSubtypes)
		{
			ModelId id = GetId(type);
			AbstractModel value = (AbstractModel)Activator.CreateInstance(type);
			_contentById[id] = value;
		}
	}

	/// <summary>
	/// Injects a model into the ModelDb. Should only be used in tests and mods.
	/// </summary>
	/// <param name="type">The type to inject.</param>
	public static void Inject([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
	{
		if (!Contains(type))
		{
			ModelId id = GetId(type);
			AbstractModel value = (AbstractModel)Activator.CreateInstance(type);
			_contentById[id] = value;
		}
	}

	/// <summary>
	/// Removes a model from the ModelDb. Should only be used in tests and mods.
	/// </summary>
	/// <param name="type">The type to remove.</param>
	public static void Remove(Type type)
	{
		ModelId id = GetId(type);
		_contentById.Remove(id);
	}

	/// <summary>
	/// Assigns IDs to all canonical models in the model database.
	/// This must happen after Init (i.e. the AbstractModel constructors); otherwise, there is a circular dependency
	/// between the static constructor and the ModelIdSerializationCache.
	/// </summary>
	public static void InitIds()
	{
		foreach (KeyValuePair<ModelId, AbstractModel> item in _contentById)
		{
			item.Value.InitId(item.Key);
		}
	}

	/// <summary>
	/// Precomputes a bunch of model data to speed up operations later.
	/// </summary>
	public static void Preload()
	{
		_ = AllCards;
		_ = AllCharacterCardPools;
		_ = AllSharedEvents;
		_ = AllEvents;
		_ = AllRelics;
		_ = AllPotions;
		_ = AllEncounters;
		_ = Achievements;
		foreach (CardModel allCard in AllCards)
		{
			_ = allCard.Pool;
			_ = allCard.AllPortraitPaths;
		}
		foreach (RelicModel allRelic in AllRelics)
		{
			_ = allRelic.IconPath;
		}
		foreach (PowerModel allPower in AllPowers)
		{
			_ = allPower.IconPath;
			_ = allPower.ResolvedBigIconPath;
		}
	}

	public static ModelId GetId<T>() where T : AbstractModel
	{
		return GetId(typeof(T));
	}

	public static ModelId GetId(Type type)
	{
		return new ModelId(GetCategory(type), GetEntry(type));
	}

	public static Type GetCategoryType(Type type)
	{
		Type type2 = type;
		while (type2.BaseType != typeof(AbstractModel))
		{
			type2 = type2.BaseType;
		}
		return type2;
	}

	public static string GetCategory(Type type)
	{
		return ModelId.SlugifyCategory(GetCategoryType(type).Name);
	}

	public static string GetEntry(Type type)
	{
		return StringHelper.Slugify(type.Name);
	}

	public static T? GetByIdOrNull<T>(ModelId id) where T : AbstractModel
	{
		if (_contentById.TryGetValue(id, out AbstractModel value))
		{
			return (T)value;
		}
		return null;
	}

	public static T GetById<T>(ModelId id) where T : AbstractModel
	{
		T byIdOrNull = GetByIdOrNull<T>(id);
		return byIdOrNull ?? throw new ModelNotFoundException(id);
	}

	public static bool Contains(Type type)
	{
		return _contentById.ContainsKey(GetId(type));
	}

	private static T Get<T>() where T : AbstractModel
	{
		return (T)_contentById[GetId<T>()];
	}

	private static AbstractModel Get(Type type)
	{
		if (!type.IsSubclassOf(typeof(AbstractModel)))
		{
			throw new InvalidOperationException();
		}
		ModelId id = GetId(type);
		if (_contentById.TryGetValue(id, out AbstractModel value))
		{
			return value;
		}
		throw new ModelNotFoundException(id);
	}

	public static T Affliction<T>() where T : AfflictionModel
	{
		return Get<T>();
	}

	public static T Enchantment<T>() where T : EnchantmentModel
	{
		return Get<T>();
	}

	public static T Card<T>() where T : CardModel
	{
		return Get<T>();
	}

	public static T CardPool<T>() where T : CardPoolModel
	{
		return Get<T>();
	}

	public static T Character<T>() where T : CharacterModel
	{
		return Get<T>();
	}

	public static T Event<T>() where T : EventModel
	{
		return Get<T>();
	}

	public static T AncientEvent<T>() where T : AncientEventModel
	{
		return Get<T>();
	}

	public static T Monster<T>() where T : MonsterModel
	{
		return Get<T>();
	}

	public static T Encounter<T>() where T : EncounterModel
	{
		return Get<T>();
	}

	public static T Potion<T>() where T : PotionModel
	{
		return Get<T>();
	}

	public static T PotionPool<T>() where T : PotionPoolModel
	{
		return Get<T>();
	}

	public static T Power<T>() where T : PowerModel
	{
		return Get<T>();
	}

	public static PowerModel DebugPower(Type type)
	{
		return (PowerModel)Get(type);
	}

	public static T Relic<T>() where T : RelicModel
	{
		return Get<T>();
	}

	public static T RelicPool<T>() where T : RelicPoolModel
	{
		return Get<T>();
	}

	public static T Orb<T>() where T : OrbModel
	{
		return Get<T>();
	}

	public static OrbModel? DebugOrb(Type type)
	{
		try
		{
			return (OrbModel)Get(type);
		}
		catch
		{
			return null;
		}
	}

	public static T Act<T>() where T : ActModel
	{
		return Get<T>();
	}

	public static T Singleton<T>() where T : SingletonModel
	{
		return Get<T>();
	}

	public static T Badge<T>() where T : BadgeModel
	{
		return Get<T>();
	}

	public static T Achievement<T>() where T : AchievementModel
	{
		return Get<T>();
	}

	public static T Modifier<T>() where T : ModifierModel
	{
		return Get<T>();
	}
}
