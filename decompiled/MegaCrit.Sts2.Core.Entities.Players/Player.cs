using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Odds;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Entities.Players;

public class Player
{
	public const int initialMaxPotionSlotCount = 3;

	private CardPile[]? _runPiles;

	private readonly List<RelicModel> _relics = new List<RelicModel>();

	private readonly List<PotionModel?> _potionSlots = new List<PotionModel>();

	private IRunState _runState = NullRunState.Instance;

	private int _gold;

	private bool _canRemovePotions = true;

	public int MaxPotionCount => _potionSlots.Count;

	public CharacterModel Character { get; }

	public Creature Creature { get; }

	public ulong NetId { get; }

	/// <summary>
	/// Player-scoped RNG set.
	/// Note that this state is not deterministic outside of combat, events and rest sites
	/// This is initialized with a seed of 0, but is updated to a real seed when added to a <see cref="P:MegaCrit.Sts2.Core.Entities.Players.Player.RunState" />.
	/// </summary>
	public PlayerRngSet PlayerRng { get; private set; }

	/// <summary>
	/// Player-scoped odds set.
	/// Note that this state is not deterministic outside of combat, events and rest sites.
	/// This is initialized with a seed of 0, but is updated to a real seed when added to a <see cref="P:MegaCrit.Sts2.Core.Entities.Players.Player.RunState" />.
	/// </summary>
	public PlayerOddsSet PlayerOdds { get; private set; }

	/// <summary>
	/// Player-scoped relic grab bag.
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.SharedRelicGrabBag" /> for the difference between this and the shared grab bag.
	/// Note that this state is not deterministic outside of combat, events and rest sites.
	/// </summary>
	public RelicGrabBag RelicGrabBag { get; }

	/// <summary>
	/// The set of unlocks the player entered the run with.
	/// This serves several purposes:
	/// - In multiplayer, this keeps track of each player's unlocks so that we generate only cards/potions/relics that
	///   the player has unlocked.
	/// - In both singleplayer and multiplayer, unlocking epochs outside of the run does not change the unlocks in a
	///   run that you load. The unlocks are saved into the run save file.
	/// </summary>
	public UnlockState UnlockState { get; }

	/// <summary>
	/// The state of the run that the player is in.
	/// Will usually be an instance of <see cref="P:MegaCrit.Sts2.Core.Entities.Players.Player.RunState" />, but will be <see cref="T:MegaCrit.Sts2.Core.Runs.NullRunState" /> in some spots
	/// for testing, debugging, or one-off combat scenarios. You should never have to check for this though, as
	/// <see cref="T:MegaCrit.Sts2.Core.Runs.NullRunState" /> should always behave appropriately.
	/// </summary>
	public IRunState RunState
	{
		get
		{
			return _runState;
		}
		set
		{
			if (!(_runState is NullRunState))
			{
				throw new InvalidOperationException("RunState has already been set.");
			}
			_runState = value;
		}
	}

	/// <summary>
	/// This is almost equivalent to Creature.IsAlive, except for a brief period of time between when the player's HP
	/// reaches zero and when DieInternal is called.
	/// It is used to allow hooks that prevent death to run before the player is fully considered dead.
	/// This is on Player and not Creature because monsters use a different mechanism for death prevention. Since
	/// creatures are always removed from combat after they are fully dead, their powers are always iterated, and we
	/// rely on creature removal to remove their powers from Hook. Players are different - they stick around after they
	/// are fully dead, and so we rely on this flag to stop model iteration.
	/// </summary>
	public bool IsActiveForHooks { get; private set; }

	public PlayerCombatState? PlayerCombatState { get; private set; }

	public ExtraPlayerFields ExtraFields { get; private set; } = new ExtraPlayerFields();

	public IReadOnlyList<RelicModel> Relics => _relics;

	public IReadOnlyList<PotionModel?> PotionSlots => _potionSlots;

	public IEnumerable<PotionModel> Potions => _potionSlots.Where((PotionModel p) => p != null).OfType<PotionModel>();

	/// <summary>
	/// Get this player's Osty pet Creature.
	/// If Osty is not in combat, this will return null.
	/// If Osty is dead, this will return the dead Osty Creature instance.
	///
	/// Note: If you want to check that Osty is both present in combat and alive, use <see cref="P:MegaCrit.Sts2.Core.Entities.Players.Player.IsOstyAlive" />.
	/// If you want to check that Osty is missing from combat or dead, use <see cref="P:MegaCrit.Sts2.Core.Entities.Players.Player.IsOstyMissing" />.
	/// </summary>
	public Creature? Osty => PlayerCombatState?.GetPet<Osty>();

	/// <summary>
	/// Is Osty present in combat and alive?
	/// </summary>
	public bool IsOstyAlive => Osty?.IsAlive ?? false;

	/// <summary>
	/// Is Osty missing from combat or dead?
	/// </summary>
	public bool IsOstyMissing => !IsOstyAlive;

	public int Gold
	{
		get
		{
			return _gold;
		}
		set
		{
			if (value != Gold)
			{
				_gold = value;
				this.GoldChanged?.Invoke();
			}
		}
	}

	/// <summary>
	/// The player's character's max ascension level when the run started.
	/// We need to keep track of this in order to properly show the "ascension unlocked" message when winning a run.
	/// </summary>
	public int MaxAscensionWhenRunStarted { get; }

	public bool HasOpenPotionSlots => _potionSlots.Any((PotionModel p) => p == null);

	public bool CanRemovePotions
	{
		get
		{
			return _canRemovePotions;
		}
		set
		{
			_canRemovePotions = value;
			this.CanRemovePotionsChanged?.Invoke();
		}
	}

	/// <summary>
	/// Has this player's inventory been populated?
	/// A player object is initially created unpopulated, but the various populating methods should be called before
	/// the run they're in has fully been launched.
	/// </summary>
	private bool IsInventoryPopulated
	{
		get
		{
			if (!Deck.Cards.Any() && !Relics.Any())
			{
				return Potions.Any();
			}
			return true;
		}
	}

	public CardPile Deck { get; } = new CardPile(PileType.Deck);

	public int MaxEnergy { get; set; }

	public List<ModelId> DiscoveredCards { get; set; }

	public List<ModelId> DiscoveredRelics { get; set; }

	public List<ModelId> DiscoveredPotions { get; set; }

	public List<ModelId> DiscoveredEnemies { get; set; }

	public List<string> DiscoveredEpochs { get; set; }

	public int BaseOrbSlotCount { get; set; }

	public IEnumerable<CardPile> Piles
	{
		get
		{
			if (_runPiles == null)
			{
				_runPiles = new CardPile[1] { Deck };
			}
			return (PlayerCombatState?.AllPiles ?? Array.Empty<CardPile>()).Concat(_runPiles);
		}
	}

	public event Action<RelicModel>? RelicObtained;

	public event Action<RelicModel>? RelicRemoved;

	public event Action<int>? MaxPotionCountChanged;

	public event Action<PotionModel>? PotionProcured;

	public event Action<PotionModel>? PotionDiscarded;

	public event Action<PotionModel>? UsedPotionRemoved;

	public event Action? AddPotionFailed;

	public event Action? GoldChanged;

	public event Action? CanRemovePotionsChanged;

	public bool HasEventPet()
	{
		if (!Relics.Any((RelicModel r) => r.AddsPet))
		{
			return Deck.Cards.Any((CardModel c) => c is ByrdonisEgg);
		}
		return true;
	}

	private Player(CharacterModel character, ulong netId, int currentHp, int maxHp, int maxEnergy, int gold, int potionSlotCount, int orbSlotCount, RelicGrabBag sharedRelicGrabBag, UnlockState unlockState, List<ModelId>? discoveredCards = null, List<ModelId>? discoveredEnemies = null, List<string>? discoveredEpochs = null, List<ModelId>? discoveredPotions = null, List<ModelId>? discoveredRelics = null)
	{
		RunState = NullRunState.Instance;
		Character = character;
		NetId = netId;
		Creature = new Creature(this, currentHp, maxHp);
		MaxEnergy = maxEnergy;
		Gold = gold;
		SetMaxPotionCountInternal(potionSlotCount);
		BaseOrbSlotCount = orbSlotCount;
		RelicGrabBag = sharedRelicGrabBag;
		UnlockState = unlockState;
		PlayerRng = new PlayerRngSet(0u);
		PlayerOdds = new PlayerOddsSet(PlayerRng);
		DiscoveredCards = discoveredCards ?? new List<ModelId>();
		DiscoveredEnemies = discoveredEnemies ?? new List<ModelId>();
		DiscoveredEpochs = discoveredEpochs ?? new List<string>();
		DiscoveredPotions = discoveredPotions ?? new List<ModelId>();
		DiscoveredRelics = discoveredRelics ?? new List<ModelId>();
		IsActiveForHooks = Creature.IsAlive;
		MaxAscensionWhenRunStarted = (SaveManager.Instance?.Progress.GetStatsForCharacter(Character.Id))?.MaxAscension ?? 0;
	}

	/// <summary>
	/// Create a new player for use at the start of a new run.
	/// The player's inventory will be populated with the chosen character's starting cards, relics, etc., but these
	/// models will not work properly until the player is added to a <see cref="P:MegaCrit.Sts2.Core.Entities.Players.Player.RunState" /> and/or
	/// <see cref="T:MegaCrit.Sts2.Core.Combat.CombatState" />.
	/// </summary>
	/// <param name="unlockState">The set of unlocks the player entered the run with.</param>
	/// <param name="netId">ID for uniquely identifying this player in multiplayer games.</param>
	/// <typeparam name="T">The type of the character that the player is playing as.</typeparam>
	/// <returns>A new player with an empty inventory.</returns>
	public static Player CreateForNewRun<T>(UnlockState unlockState, ulong netId) where T : CharacterModel
	{
		return CreateForNewRun(ModelDb.Character<T>(), unlockState, netId);
	}

	/// <summary>
	/// Create a new player for use at the start of a new run.
	/// The player's inventory will be populated with the chosen character's starting cards, relics, etc., but these
	/// models will not work properly until the player is added to a <see cref="P:MegaCrit.Sts2.Core.Entities.Players.Player.RunState" /> and/or
	/// <see cref="T:MegaCrit.Sts2.Core.Combat.CombatState" />.
	/// </summary>
	/// <param name="character">The character that the player is playing as.</param>
	/// <param name="unlockState">The set of unlocks the player entered the run with.</param>
	/// <param name="netId">ID for uniquely identifying this player in multiplayer games.</param>
	/// <returns>A new player with an empty inventory.</returns>
	public static Player CreateForNewRun(CharacterModel character, UnlockState unlockState, ulong netId)
	{
		Player player = new Player(character, netId, character.StartingHp, character.StartingHp, character.MaxEnergy, character.StartingGold, 3, character.BaseOrbSlotCount, new RelicGrabBag(), unlockState);
		player.PopulateStartingInventory();
		return player;
	}

	/// <summary>
	/// Load a player from a SerializablePlayer.
	/// The player's inventory will be populated with the cards, relics, etc. from the SerializablePlayer., but these
	/// models will not work properly until the player is added to a <see cref="P:MegaCrit.Sts2.Core.Entities.Players.Player.RunState" /> and/or
	/// <see cref="T:MegaCrit.Sts2.Core.Combat.CombatState" />.
	/// </summary>
	public static Player FromSerializable(SerializablePlayer save)
	{
		Player player = new Player(ModelDb.GetById<CharacterModel>(save.CharacterId), save.NetId, save.CurrentHp, save.MaxHp, save.MaxEnergy, save.Gold, save.MaxPotionSlotCount, save.BaseOrbSlotCount, MegaCrit.Sts2.Core.Runs.RelicGrabBag.FromSerializable(save.RelicGrabBag), MegaCrit.Sts2.Core.Unlocks.UnlockState.FromSerializable(save.UnlockState), save.DiscoveredCards.ToList(), save.DiscoveredEnemies.ToList(), save.DiscoveredEpochs.ToList(), save.DiscoveredPotions.ToList(), save.DiscoveredRelics.ToList());
		player.PlayerRng = PlayerRngSet.FromSerializable(save.Rng);
		player.PlayerOdds = PlayerOddsSet.FromSerializable(save.Odds, player.PlayerRng);
		player.ExtraFields = ExtraPlayerFields.FromSerializable(save.ExtraFields);
		player.LoadInventory(save);
		return player;
	}

	public void InitializeSeed(string seed)
	{
		PlayerRng = new PlayerRngSet((uint)(StringHelper.GetDeterministicHashCode(seed) + _runState.GetPlayerSlotIndex(this)));
		PlayerOdds = new PlayerOddsSet(PlayerRng);
	}

	private void PopulateStartingInventory()
	{
		if (IsInventoryPopulated)
		{
			throw new InvalidOperationException("Inventory is already populated.");
		}
		if (!(RunState is NullRunState))
		{
			throw new InvalidOperationException("A player's starting inventory must be populated before being added to a run.");
		}
		PopulateStartingDeck();
		PopulateStartingRelics();
		foreach (PotionModel item in Character.StartingPotions.Select((PotionModel p) => p.ToMutable()))
		{
			AddPotionInternal(item);
		}
	}

	private void LoadInventory(SerializablePlayer save)
	{
		if (IsInventoryPopulated)
		{
			throw new InvalidOperationException("Inventory is already populated.");
		}
		if (!(RunState is NullRunState))
		{
			throw new InvalidOperationException("A player's inventory must be loaded before being added to a run.");
		}
		PopulateDeck(save.Deck.Select(CardModel.FromSerializable));
		LoadPotions(save.Potions);
		PopulateRelics(save.Relics.Select(RelicModel.FromSerializable));
	}

	public void PopulateRelicGrabBagIfNecessary(Rng rng)
	{
		if (!RelicGrabBag.IsPopulated)
		{
			RelicGrabBag.Populate(this, rng);
		}
	}

	public SerializablePlayer ToSerializable()
	{
		return new SerializablePlayer
		{
			CharacterId = Character.Id,
			CurrentHp = Creature.CurrentHp,
			MaxHp = Creature.MaxHp,
			MaxEnergy = MaxEnergy,
			MaxPotionSlotCount = MaxPotionCount,
			BaseOrbSlotCount = BaseOrbSlotCount,
			NetId = NetId,
			Gold = Gold,
			Rng = PlayerRng.ToSerializable(),
			Odds = PlayerOdds.ToSerializable(),
			RelicGrabBag = RelicGrabBag.ToSerializable(),
			Deck = Deck.Cards.Select((CardModel c) => c.ToSerializable()).ToList(),
			Relics = Relics.Select((RelicModel r) => r.ToSerializable()).ToList(),
			Potions = PotionSlots.Select((PotionModel p, int i) => p?.ToSerializable(i)).OfType<SerializablePotion>().ToList(),
			ExtraFields = ExtraFields.ToSerializable(),
			UnlockState = UnlockState.ToSerializable(),
			DiscoveredCards = DiscoveredCards.ToList(),
			DiscoveredEnemies = DiscoveredEnemies.ToList(),
			DiscoveredEpochs = DiscoveredEpochs.ToList(),
			DiscoveredPotions = DiscoveredPotions.ToList(),
			DiscoveredRelics = DiscoveredRelics.ToList()
		};
	}

	public void SyncWithSerializedPlayer(SerializablePlayer player)
	{
		if (player.NetId != NetId)
		{
			throw new InvalidOperationException($"Tried to sync player that has net ID {NetId} with SerializablePlayer that has net ID {player.NetId}!");
		}
		if (player.CharacterId != Character.Id)
		{
			throw new InvalidOperationException($"Character changed for player {NetId}! This is not allowed");
		}
		Creature.SetMaxHpInternal(player.MaxHp);
		Creature.SetCurrentHpInternal(player.CurrentHp);
		MaxEnergy = player.MaxEnergy;
		Gold = player.Gold;
		SetMaxPotionCountInternal(player.MaxPotionSlotCount);
		Deck.Clear(silent: true);
		foreach (RelicModel item in _relics.ToList())
		{
			RemoveRelicInternal(item, silent: true);
		}
		foreach (PotionModel item2 in _potionSlots.ToList())
		{
			if (item2 != null)
			{
				DiscardPotionInternal(item2, silent: true);
			}
		}
		PopulateDeck(player.Deck.Select((SerializableCard c) => RunState.LoadCard(c, this)), silent: true);
		PopulateRelics(player.Relics.Select(RelicModel.FromSerializable), silent: true);
		LoadPotions(player.Potions, silent: true);
		PlayerRng.LoadFromSerializable(player.Rng);
		PlayerOdds.LoadFromSerializable(player.Odds);
		RelicGrabBag.LoadFromSerializable(player.RelicGrabBag);
		DiscoveredCards = player.DiscoveredCards.ToList();
		DiscoveredEnemies = player.DiscoveredEnemies.ToList();
		DiscoveredEpochs = player.DiscoveredEpochs.ToList();
		DiscoveredPotions = player.DiscoveredPotions.ToList();
		DiscoveredRelics = player.DiscoveredRelics.ToList();
		ExtraFields = ExtraPlayerFields.FromSerializable(player.ExtraFields);
		IsActiveForHooks = Creature.IsAlive;
	}

	/// <summary>
	/// NEVER CALL THIS!
	/// Only RelicCmd.Obtain and save/load stuff should be calling this.
	/// </summary>
	/// <param name="relic">Relic to add.</param>
	/// <param name="index">Index at which relic should be inserted. -1 means at the end.</param>
	/// <param name="silent">If true, RelicObtained will not be called.</param>
	public void AddRelicInternal(RelicModel relic, int index = -1, bool silent = false)
	{
		relic.AssertMutable();
		relic.Owner = this;
		if (index == -1)
		{
			_relics.Add(relic);
		}
		else
		{
			_relics.Insert(index, relic);
		}
		if (relic != null && !relic.IsMelted && relic.ShouldFlashOnPlayer)
		{
			relic.Flashed += OnRelicFlashed;
		}
		if (!silent)
		{
			this.RelicObtained?.Invoke(relic);
		}
	}

	/// <summary>
	/// NEVER CALL THIS!
	/// ONLY RelicCmd.Remove should be calling this.
	/// </summary>
	/// <param name="relic">Relic to remove.</param>
	/// <param name="silent">If true, RelicRemoved will not be called.</param>
	public void RemoveRelicInternal(RelicModel relic, bool silent = false)
	{
		if (!_relics.Contains(relic))
		{
			throw new InvalidOperationException($"Player does not have relic {relic.Id}");
		}
		_relics.Remove(relic);
		relic.RemoveInternal();
		if (relic.ShouldFlashOnPlayer)
		{
			relic.Flashed -= OnRelicFlashed;
		}
		if (!silent)
		{
			this.RelicRemoved?.Invoke(relic);
		}
	}

	/// <summary>
	/// NEVER CALL THIS!
	/// ONLY RelicCmd.Melt should be calling this.
	/// </summary>
	/// <param name="relic">Relic to melt.</param>
	public void MeltRelicInternal(RelicModel relic)
	{
		if (!relic.IsWax)
		{
			throw new InvalidOperationException($"{relic.Id} is not wax.");
		}
		if (relic.IsMelted)
		{
			throw new InvalidOperationException($"{relic.Id} is already melted.");
		}
		if (!_relics.Contains(relic))
		{
			throw new InvalidOperationException($"Player does not have relic {relic.Id}");
		}
		if (relic.ShouldFlashOnPlayer)
		{
			relic.Flashed -= OnRelicFlashed;
		}
		relic.IsMelted = true;
		relic.Status = RelicStatus.Disabled;
	}

	/// <summary>
	/// Get one of this player's relics.
	/// If the player has multiple of the same type of relic, just get the first one.
	/// If the player has none of this type of relic, returns null.
	/// </summary>
	/// <typeparam name="T">Type of relic to get.</typeparam>
	/// <returns>Matching relic.</returns>
	public T? GetRelic<T>() where T : RelicModel
	{
		return Relics.FirstOrDefault((RelicModel r) => r is T) as T;
	}

	public RelicModel? GetRelicById(ModelId id)
	{
		return Relics.FirstOrDefault((RelicModel r) => r.Id == id);
	}

	/// <summary>
	/// Returns the slot index of the potion in the player's belt, or -1 if it is not in the belt.
	/// </summary>
	public int GetPotionSlotIndex(PotionModel model)
	{
		return _potionSlots.IndexOf(model);
	}

	/// <summary>
	/// Returns the potion at the slot index, or throws if the index is out of range.
	/// </summary>
	public PotionModel? GetPotionAtSlotIndex(int index)
	{
		if (index < 0 || index >= _potionSlots.Count)
		{
			throw new IndexOutOfRangeException($"Index {index} is not a valid potion slot index! Player has {_potionSlots.Count} potion slots");
		}
		return _potionSlots[index];
	}

	/// <summary>
	/// Increases the maximum amount of potions the player can hold.
	/// </summary>
	/// <param name="maxPotionCountIncrease">The increased count of maximum amount of potions the player can carry.</param>
	public void AddToMaxPotionCount(int maxPotionCountIncrease)
	{
		SetMaxPotionCountInternal(_potionSlots.Count + maxPotionCountIncrease);
	}

	/// <summary>
	/// Decreases the maximum amount of potions the player can hold.
	/// </summary>
	/// <param name="maxPotionCountDecrease">The decreased count of maximum amount of potions the player can carry.</param>
	public void SubtractFromMaxPotionCount(int maxPotionCountDecrease)
	{
		SetMaxPotionCountInternal(_potionSlots.Count - maxPotionCountDecrease);
	}

	/// <summary>
	/// NEVER CALL THIS!
	/// ONLY save/load stuff should be calling this.
	/// </summary>
	/// <param name="newMaxPotionCount">The new maximum amount of potions the player can carry.</param>
	private void SetMaxPotionCountInternal(int newMaxPotionCount)
	{
		if (newMaxPotionCount > _potionSlots.Count)
		{
			for (int i = _potionSlots.Count; i < newMaxPotionCount; i++)
			{
				_potionSlots.Add(null);
			}
			this.MaxPotionCountChanged?.Invoke(MaxPotionCount);
		}
		else
		{
			if (newMaxPotionCount >= _potionSlots.Count)
			{
				return;
			}
			for (int num = _potionSlots.Count - 1; num >= newMaxPotionCount; num--)
			{
				if (_potionSlots[num] != null)
				{
					int num2 = _potionSlots.IndexOf(null);
					if (num2 < newMaxPotionCount)
					{
						_potionSlots[num2] = _potionSlots[num];
					}
					else
					{
						DiscardPotionInternal(_potionSlots[num]);
					}
				}
				_potionSlots.RemoveAt(num);
			}
			this.MaxPotionCountChanged?.Invoke(MaxPotionCount);
		}
	}

	/// <summary>
	/// NEVER CALL THIS!
	/// ONLY PotionCmd.Procure and save/load stuff should be calling this.
	/// </summary>
	/// <param name="potion">Potion to add.</param>
	/// <param name="slotIndex">Slot at which to add the potion. If -1, the potion will be added in the first available slot.</param>
	/// <param name="silent">If true, no events will be called.</param>
	public PotionProcureResult AddPotionInternal(PotionModel potion, int slotIndex = -1, bool silent = false)
	{
		potion.AssertMutable();
		PotionProcureResult potionProcureResult = new PotionProcureResult
		{
			potion = potion
		};
		if (slotIndex < 0)
		{
			slotIndex = _potionSlots.IndexOf(null);
		}
		if (slotIndex >= 0)
		{
			if (_potionSlots[slotIndex] != null)
			{
				Log.Warn($"Tried to add potion {potion} at slot index {slotIndex} which is already filled with potion {_potionSlots[slotIndex]}!");
				if (!silent)
				{
					this.AddPotionFailed?.Invoke();
				}
				potionProcureResult.success = false;
				potionProcureResult.failureReason = PotionProcureFailureReason.TooFull;
				return potionProcureResult;
			}
			potion.Owner = this;
			_potionSlots[slotIndex] = potion;
			if (!silent)
			{
				this.PotionProcured?.Invoke(potion);
			}
			potionProcureResult.success = true;
		}
		else
		{
			if (!silent)
			{
				this.AddPotionFailed?.Invoke();
			}
			potionProcureResult.success = false;
			potionProcureResult.failureReason = PotionProcureFailureReason.TooFull;
		}
		return potionProcureResult;
	}

	/// <summary>
	/// NEVER CALL THIS!
	/// ONLY PotionModel.Discard should be calling this.
	/// </summary>
	/// <param name="potion">Potion to discard.</param>
	/// <param name="silent">If true, PotionDiscarded will not be called.</param>
	public void DiscardPotionInternal(PotionModel potion, bool silent = false)
	{
		RemovePotionInternal(potion);
		if (!silent)
		{
			this.PotionDiscarded?.Invoke(potion);
		}
	}

	/// <summary>
	/// NEVER CALL THIS!
	/// ONLY PotionModel.Remove should be calling this.
	/// </summary>
	/// <param name="potion">Used potion to remove.</param>
	public void RemoveUsedPotionInternal(PotionModel potion)
	{
		RemovePotionInternal(potion);
		this.UsedPotionRemoved?.Invoke(potion);
	}

	private void RemovePotionInternal(PotionModel potion)
	{
		int num = _potionSlots.IndexOf(potion);
		if (num < 0)
		{
			throw new InvalidOperationException($"Tried to remove potion you don't have: {potion.Id}");
		}
		_potionSlots[num] = null;
	}

	/// <summary>
	/// Populates the character's starting deck to start a new game.
	/// </summary>
	private void PopulateStartingDeck()
	{
		List<CardModel> list = new List<CardModel>();
		foreach (CardModel item in Character.StartingDeck)
		{
			CardModel cardModel = item.ToMutable();
			cardModel.FloorAddedToDeck = 1;
			list.Add(cardModel);
		}
		PopulateDeck(list);
	}

	/// <summary>
	/// Populates a player's existing deck.
	/// This can be from a multiplayer sync or from a save file.
	/// </summary>
	/// <param name="cards">Cards to load.</param>
	/// <param name="silent">
	/// Whether or not to emit events. If loading from a multiplayer sync, we don't want to emit events.
	/// </param>
	private void PopulateDeck(IEnumerable<CardModel> cards, bool silent = false)
	{
		if (Deck.Cards.Any())
		{
			throw new InvalidOperationException("Deck has already been populated.");
		}
		foreach (CardModel card in cards)
		{
			Deck.AddInternal(card, -1, silent);
		}
	}

	private void PopulateStartingRelics()
	{
		List<RelicModel> list = Character.StartingRelics.Select((RelicModel r) => r.ToMutable()).ToList();
		foreach (RelicModel item in list)
		{
			item.FloorAddedToDeck = 1;
			SaveManager.Instance.MarkRelicAsSeen(item);
		}
		PopulateRelics(list);
	}

	/// <summary>
	/// Loads a player's existing relic set.
	/// This can be from the player's starting relics, from a multiplayer sync, or from a save file.
	/// </summary>
	/// <param name="relics">Relics to load.</param>
	/// <param name="silent">
	/// Whether or not to emit events. If loading from a multiplayer sync, we don't want to emit events.
	/// </param>
	private void PopulateRelics(IEnumerable<RelicModel> relics, bool silent = false)
	{
		if (Relics.Any())
		{
			throw new InvalidOperationException("Relics have already been populated.");
		}
		foreach (RelicModel relic in relics)
		{
			AddRelicInternal(relic, -1, silent);
		}
	}

	/// <summary>
	/// Loads a player's existing potion set.
	/// This can be from a multiplayer sync or from a save file.
	/// </summary>
	/// <param name="serializablePotions">Cards to load.</param>
	/// <param name="silent">Whether or not to emit events. If loading from a multiplayer sync, we don't want to emit events.</param>
	private void LoadPotions(List<SerializablePotion> serializablePotions, bool silent = false)
	{
		if (Potions.Any())
		{
			throw new InvalidOperationException("Potions have already been populated.");
		}
		foreach (SerializablePotion serializablePotion in serializablePotions)
		{
			AddPotionInternal(PotionModel.FromSerializable(serializablePotion), serializablePotion.SlotIndex, silent);
		}
	}

	/// <summary>
	/// Resets the player's combat state to an empty state.
	/// This will leave the player with no cards in combat, so you should usually call <see cref="M:MegaCrit.Sts2.Core.Entities.Players.Player.PopulateCombatState(MegaCrit.Sts2.Core.Random.Rng,MegaCrit.Sts2.Core.Combat.CombatState)" />
	/// after.
	/// </summary>
	public void ResetCombatState()
	{
		PlayerCombatState = new PlayerCombatState(this);
	}

	/// <summary>
	/// Populates the player's combat state with everything they should get at the start of combat.
	/// For example, this clones all the cards from their deck into their draw pile in a random order.
	/// </summary>
	public void PopulateCombatState(Rng rng, CombatState state)
	{
		foreach (CardModel item in Deck.Cards.ToList())
		{
			CardModel cardModel = state.CloneCard(item);
			cardModel.DeckVersion = item;
			PlayerCombatState.DrawPile.AddInternal(cardModel);
		}
		PlayerCombatState.DrawPile.RandomizeOrderInternal(this, rng, state);
	}

	/// <summary>
	/// Revives the player before the combat ends, in multiplayer only. Should only trigger when combat ends with other
	/// players alive.
	/// It is very important to do this _before_ combat ends instead of after. If the player is still dead during
	/// HookBus.AfterCombatEnd, their relics will not be subscribed to the HookBus, and relics which rely on AfterCombatEnd
	/// to reset state will not be reset for the next combat.
	/// See: Centennial Puzzle, Captain's Wheel, or any other relics that use AfterCombatEnd.
	/// </summary>
	public async Task ReviveBeforeCombatEnd()
	{
		if (Creature.IsDead)
		{
			await CreatureCmd.Heal(Creature, 1m);
		}
	}

	/// <summary>
	/// Called after combat ends, giving the player the opportunity to do things like clear out their combat state and
	/// other combat teardown stuff.
	/// </summary>
	public void AfterCombatEnd()
	{
		Creature.RemoveAllPowersInternalExcept();
		PlayerCombatState?.AfterCombatEnd();
		Creature.LoseBlockInternal(Creature.Block);
	}

	private void OnRelicFlashed(RelicModel relic, IEnumerable<Creature> targets)
	{
		SfxCmd.Play(relic.FlashSfx);
		foreach (Creature target in targets)
		{
			target.GetVfxContainer()?.AddChildSafely(NRelicFlashVfx.Create(relic, target));
		}
	}

	public void OnSideSwitch()
	{
	}

	/// <summary>
	/// Called from Creature when the player reaches zero health, after all hooks that prevent death are called, as well
	/// as <see cref="M:MegaCrit.Sts2.Core.Hooks.Hook.AfterDeath(MegaCrit.Sts2.Core.Runs.IRunState,MegaCrit.Sts2.Core.Combat.ICombatState,MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Boolean,System.Single)" />.
	/// </summary>
	public void DeactivateHooks()
	{
		IsActiveForHooks = false;
	}

	/// <summary>
	/// Called from Creature when the player changes from a dead state to a non-dead state.
	/// This is _not_ called in the scenario when death is prevented, e.g. by Fairy in a Bottle.
	/// Likely only called in multiplayer scenarios - players cannot revive in singleplayer (remember that death
	/// prevention is different).
	/// </summary>
	public void ActivateHooks()
	{
		IsActiveForHooks = true;
	}
}
