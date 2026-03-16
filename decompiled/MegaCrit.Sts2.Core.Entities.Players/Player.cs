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
using MegaCrit.Sts2.Core.Nodes.Rooms;
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

	public int MaxPotionCount => _potionSlots.Count;

	public CharacterModel Character { get; }

	public Creature Creature { get; }

	public ulong NetId { get; }

	public PlayerRngSet PlayerRng { get; private set; }

	public PlayerOddsSet PlayerOdds { get; private set; }

	public RelicGrabBag RelicGrabBag { get; }

	public UnlockState UnlockState { get; }

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

	public bool IsActiveForHooks { get; private set; }

	public PlayerCombatState? PlayerCombatState { get; private set; }

	public ExtraPlayerFields ExtraFields { get; private set; } = new ExtraPlayerFields();

	public IReadOnlyList<RelicModel> Relics => _relics;

	public IReadOnlyList<PotionModel?> PotionSlots => _potionSlots;

	public IEnumerable<PotionModel> Potions => _potionSlots.Where((PotionModel p) => p != null).OfType<PotionModel>();

	public Creature? Osty => PlayerCombatState?.GetPet<Osty>();

	public bool IsOstyAlive => Osty?.IsAlive ?? false;

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

	public int MaxAscensionWhenRunStarted { get; }

	public bool HasOpenPotionSlots => _potionSlots.Any((PotionModel p) => p == null);

	public bool CanRemovePotions { get; set; } = true;

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

	public static Player CreateForNewRun<T>(UnlockState unlockState, ulong netId) where T : CharacterModel
	{
		return CreateForNewRun(ModelDb.Character<T>(), unlockState, netId);
	}

	public static Player CreateForNewRun(CharacterModel character, UnlockState unlockState, ulong netId)
	{
		Player player = new Player(character, netId, character.StartingHp, character.StartingHp, character.MaxEnergy, character.StartingGold, 3, character.BaseOrbSlotCount, new RelicGrabBag(), unlockState);
		player.PopulateStartingInventory();
		return player;
	}

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
		PlayerRng = new PlayerRngSet((uint)((ulong)StringHelper.GetDeterministicHashCode(seed) + NetId));
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
		IsActiveForHooks = Creature.IsAlive;
	}

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

	public T? GetRelic<T>() where T : RelicModel
	{
		return Relics.FirstOrDefault((RelicModel r) => r is T) as T;
	}

	public RelicModel? GetRelicById(ModelId id)
	{
		return Relics.FirstOrDefault((RelicModel r) => r.Id == id);
	}

	public int GetPotionSlotIndex(PotionModel model)
	{
		return _potionSlots.IndexOf(model);
	}

	public PotionModel? GetPotionAtSlotIndex(int index)
	{
		if (index < 0 || index >= _potionSlots.Count)
		{
			throw new IndexOutOfRangeException($"Index {index} is not a valid potion slot index! Player has {_potionSlots.Count} potion slots");
		}
		return _potionSlots[index];
	}

	public void AddToMaxPotionCount(int maxPotionCountIncrease)
	{
		SetMaxPotionCountInternal(_potionSlots.Count + maxPotionCountIncrease);
	}

	public void SubtractFromMaxPotionCount(int maxPotionCountDecrease)
	{
		SetMaxPotionCountInternal(_potionSlots.Count - maxPotionCountDecrease);
	}

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

	public void DiscardPotionInternal(PotionModel potion, bool silent = false)
	{
		RemovePotionInternal(potion);
		if (!silent)
		{
			this.PotionDiscarded?.Invoke(potion);
		}
	}

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

	public void ResetCombatState()
	{
		PlayerCombatState = new PlayerCombatState(this);
	}

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

	public async Task ReviveBeforeCombatEnd()
	{
		if (Creature.IsDead)
		{
			await CreatureCmd.Heal(Creature, 1m);
		}
	}

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
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NRelicFlashVfx.Create(relic, target));
		}
	}

	public void OnSideSwitch()
	{
	}

	public void DeactivateHooks()
	{
		IsActiveForHooks = false;
	}

	public void ActivateHooks()
	{
		IsActiveForHooks = true;
	}
}
