using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Combat;

public class CombatState : ICardScope
{
	private readonly List<Creature> _allies = new List<Creature>();

	private readonly List<Creature> _enemies = new List<Creature>();

	private uint _nextCreatureId;

	private readonly EncounterModel? _encounter;

	private readonly List<CardModel> _allCards = new List<CardModel>();

	public IRunState RunState { get; }

	public IReadOnlyList<Creature> Allies => _allies;

	public IReadOnlyList<Creature> Enemies => _enemies;

	public IReadOnlyList<Creature> Creatures => _allies.Concat(_enemies).ToList();

	public IReadOnlyList<Creature> PlayerCreatures => Creatures.Where((Creature c) => c.IsPlayer).ToList();

	public IReadOnlyList<Player> Players => PlayerCreatures.Select((Creature c) => c.Player).ToList();

	public IReadOnlyList<ModifierModel> Modifiers { get; }

	public MultiplayerScalingModel? MultiplayerScalingModel { get; private set; }

	public int RoundNumber { get; set; }

	public CombatSide CurrentSide { get; set; }

	public EncounterModel? Encounter
	{
		get
		{
			return _encounter;
		}
		private init
		{
			value?.AssertMutable();
			_encounter = value;
		}
	}

	public List<Creature> EscapedCreatures { get; private set; } = new List<Creature>();

	public IReadOnlyList<Creature> CreaturesOnCurrentSide => GetCreaturesOnSide(CurrentSide);

	public IReadOnlyList<Creature> HittableEnemies => Enemies.Where((Creature e) => e.IsHittable).ToList();

	public event Action<CombatState>? CreaturesChanged;

	public CombatState(EncounterModel? encounter = null, IRunState? runState = null, IReadOnlyList<ModifierModel>? modifiers = null, MultiplayerScalingModel? multiplayerScalingModel = null)
	{
		encounter?.AssertMutable();
		Encounter = encounter;
		RoundNumber = 1;
		CurrentSide = CombatSide.Player;
		RunState = runState ?? NullRunState.Instance;
		Modifiers = modifiers ?? Array.Empty<ModifierModel>();
		MultiplayerScalingModel = multiplayerScalingModel;
	}

	public T CreateCard<T>(Player owner) where T : CardModel
	{
		return (T)CreateCard(ModelDb.Card<T>(), owner);
	}

	public CardModel CreateCard(CardModel canonicalCard, Player owner)
	{
		CardModel cardModel = canonicalCard.ToMutable();
		AddCard(cardModel, owner);
		cardModel.AfterCreated();
		return cardModel;
	}

	public CardModel CloneCard(CardModel mutableCard)
	{
		CardModel cardModel = (CardModel)mutableCard.ClonePreservingMutability();
		AddCard(cardModel);
		return cardModel;
	}

	public void AddCard(CardModel card, Player owner)
	{
		card.Owner = owner;
		AddCard(card);
	}

	public void RemoveCard(CardModel card)
	{
		_allCards.Remove(card);
		card.Owner = null;
	}

	public bool ContainsCard(CardModel card)
	{
		return _allCards.Contains(card);
	}

	public void AddPlayer(Player player)
	{
		AttachCreature(player.Creature);
		AddCreature(player.Creature);
	}

	public Creature CreateCreature(MonsterModel monster, CombatSide side, string? slot)
	{
		monster.AssertMutable();
		monster.RunRng = RunState.Rng;
		Creature creature = new Creature(monster, side, slot);
		List<Creature> creaturesOnSide = ((side == CombatSide.Player) ? _allies : _enemies);
		if (side == CombatSide.Enemy)
		{
			creature.SetUniqueMonsterHpValue(creaturesOnSide, RunState.Rng.Niche);
			creature.ScaleMonsterHpForMultiplayer(Encounter, Players.Count, RunState.CurrentActIndex);
		}
		AttachCreature(creature);
		monster.Rng = new Rng((uint)((RunState.Rng.Seed + RunState.CurrentMapCoord?.col) ?? ((long?)RunState.CurrentMapCoord?.row) ?? (RunState.CurrentActIndex + creature.CombatId.Value)));
		return creature;
	}

	private void AttachCreature(Creature creature)
	{
		creature.CombatState = this;
		creature.CombatId = _nextCreatureId;
		_nextCreatureId++;
	}

	public void CreatureEscaped(Creature creature)
	{
		EscapedCreatures.Add(creature);
		RemoveCreature(creature);
	}

	public void RemoveCreature(Creature creature, bool unattach = true)
	{
		if (creature.CombatState == null)
		{
			return;
		}
		if (creature.CombatState != this)
		{
			throw new InvalidOperationException("Creature is in a different combat.");
		}
		if (_enemies.Contains(creature))
		{
			_enemies.Remove(creature);
		}
		else
		{
			if (!_allies.Contains(creature))
			{
				throw new InvalidOperationException($"Removed creature '{creature}' was not found.");
			}
			_allies.Remove(creature);
		}
		if (unattach)
		{
			creature.CombatState = null;
		}
		this.CreaturesChanged?.Invoke(this);
	}

	public bool ContainsCreature(Creature creature)
	{
		if (!_allies.Contains(creature))
		{
			return _enemies.Contains(creature);
		}
		return true;
	}

	public bool ContainsMonster<T>() where T : MonsterModel
	{
		return _enemies.Any((Creature c) => c.Monster is T);
	}

	public Creature? GetCreature(uint? combatId)
	{
		if (!combatId.HasValue)
		{
			return null;
		}
		return Creatures.FirstOrDefault((Creature c) => c.CombatId == combatId);
	}

	public async Task<Creature?> GetCreatureAsync(uint? combatId, double timeoutSec)
	{
		if (!combatId.HasValue)
		{
			return null;
		}
		Creature creature = GetCreature(combatId);
		if (creature != null)
		{
			return creature;
		}
		if (combatId < _nextCreatureId)
		{
			return null;
		}
		TaskCompletionSource<Creature> completionSource = new TaskCompletionSource<Creature>();
		CreaturesChanged += OnCreaturesChanged;
		Task timeoutTask = GodotTimerTask(timeoutSec);
		Task task = await Task.WhenAny(completionSource.Task, timeoutTask);
		CreaturesChanged -= OnCreaturesChanged;
		if (task == timeoutTask)
		{
			throw new InvalidOperationException($"Timed out waiting for creature with target index {combatId} to spawn!");
		}
		return await completionSource.Task;
		void OnCreaturesChanged(CombatState _)
		{
			Creature creature2 = GetCreature(combatId);
			if (creature2 != null)
			{
				completionSource.SetResult(creature2);
			}
		}
	}

	public IReadOnlyList<Creature> GetCreaturesOnSide(CombatSide side)
	{
		if (side != CombatSide.Enemy)
		{
			return Allies;
		}
		return Enemies;
	}

	public IReadOnlyList<Creature> GetOpponentsOf(Creature creature)
	{
		return GetCreaturesOnSide(creature.Side.GetOppositeSide());
	}

	public IReadOnlyList<Creature> GetTeammatesOf(Creature creature)
	{
		return GetCreaturesOnSide(creature.Side);
	}

	public Player? GetPlayer(ulong playerId)
	{
		return Players.FirstOrDefault((Player p) => p.NetId == playerId);
	}

	public IEnumerable<AbstractModel> IterateHookListeners()
	{
		List<AbstractModel> list = new List<AbstractModel>(Players.Count * 50);
		for (int i = 0; i < _allies.Count + _enemies.Count; i++)
		{
			Creature creature = ((i < _allies.Count) ? _allies[i] : _enemies[i - _allies.Count]);
			list.AddRange(creature.Powers);
			Player player = creature.Player;
			if (player == null)
			{
				list.Add(creature.Monster);
			}
			else
			{
				if (!player.IsActiveForHooks)
				{
					continue;
				}
				IReadOnlyList<RelicModel> relics = player.Relics;
				for (int j = 0; j < relics.Count; j++)
				{
					if (!relics[j].IsMelted)
					{
						list.Add(relics[j]);
					}
				}
				IReadOnlyList<PotionModel> potionSlots = player.PotionSlots;
				for (int k = 0; k < potionSlots.Count; k++)
				{
					if (potionSlots[k] != null)
					{
						list.Add(potionSlots[k]);
					}
				}
				if (player.PlayerCombatState == null)
				{
					continue;
				}
				list.AddRange(player.PlayerCombatState.OrbQueue.Orbs);
				IReadOnlyList<CardPile> allPiles = player.PlayerCombatState.AllPiles;
				for (int l = 0; l < allPiles.Count; l++)
				{
					CardPile cardPile = allPiles[l];
					IReadOnlyList<CardModel> cards = cardPile.Cards;
					for (int m = 0; m < cards.Count; m++)
					{
						CardModel cardModel = cards[m];
						list.Add(cardModel);
						if (cardModel.Affliction != null)
						{
							list.Add(cardModel.Affliction);
						}
						if (cardModel.Enchantment != null)
						{
							list.Add(cardModel.Enchantment);
						}
					}
				}
			}
		}
		for (int n = 0; n < Modifiers.Count; n++)
		{
			list.Add(Modifiers[n]);
		}
		if (MultiplayerScalingModel != null)
		{
			list.Add(MultiplayerScalingModel);
		}
		foreach (AbstractModel item in list)
		{
			if (Contains(item))
			{
				yield return item;
			}
		}
	}

	public void SortEnemiesBySlotName()
	{
		if (Encounter != null)
		{
			_enemies.Sort((Creature a, Creature b) => Encounter.Slots.IndexOf<string>(a.SlotName) - Encounter.Slots.IndexOf<string>(b.SlotName));
		}
	}

	public void SetEnemyIndex(Creature creature, int index)
	{
		if (Encounter.Slots.Any())
		{
			throw new InvalidOperationException("Cannot modify turn order of a combat with pre-set slots");
		}
		if (!Enemies.Contains(creature))
		{
			throw new ArgumentException("Creature must be a valid enemy to change its turn order.");
		}
		_enemies.Remove(creature);
		_enemies.Insert(Math.Min(index, _enemies.Count - 1), creature);
	}

	private void AddCard(CardModel card)
	{
		card.AssertMutable();
		if (card.CombatState != null && card.CombatState != this)
		{
			throw new InvalidOperationException("Card " + card.Id.Entry + " combat state is set to a different combat.");
		}
		_allCards.Add(card);
	}

	public void AddCreature(Creature creature)
	{
		if (creature.CombatState != this)
		{
			throw new InvalidOperationException("Creature was created for a different combat.");
		}
		List<Creature> list = ((creature.Side == CombatSide.Player) ? _allies : _enemies);
		if (ContainsCreature(creature))
		{
			throw new InvalidOperationException("Creature is already in this combat, but AddCreature was called on it again.");
		}
		list.Add(creature);
		this.CreaturesChanged?.Invoke(this);
	}

	private bool Contains(AbstractModel model)
	{
		if (!(model is PowerModel powerModel))
		{
			if (!(model is RelicModel relicModel))
			{
				if (!(model is PotionModel potionModel))
				{
					if (!(model is CardModel cardModel))
					{
						if (!(model is AfflictionModel afflictionModel))
						{
							if (!(model is EnchantmentModel enchantmentModel))
							{
								if (!(model is OrbModel orbModel))
								{
									if (!(model is MonsterModel monsterModel))
									{
										if (!(model is AchievementModel))
										{
											if (!(model is ModifierModel))
											{
												if (model is MultiplayerScalingModel)
												{
													return true;
												}
												throw new ArgumentOutOfRangeException("model", model, $"Invalid model type {model.GetType()} ({model})");
											}
											return true;
										}
										return true;
									}
									return monsterModel.Creature.CombatState != null;
								}
								return !orbModel.HasBeenRemovedFromState && orbModel.Owner.IsActiveForHooks;
							}
							return enchantmentModel.HasCard && !enchantmentModel.Card.HasBeenRemovedFromState && enchantmentModel.Card.Owner.IsActiveForHooks;
						}
						return afflictionModel.HasCard && !afflictionModel.Card.HasBeenRemovedFromState && afflictionModel.Card.Owner.IsActiveForHooks;
					}
					return !cardModel.HasBeenRemovedFromState && cardModel.Owner.IsActiveForHooks;
				}
				return !potionModel.HasBeenRemovedFromState && potionModel.Owner.IsActiveForHooks;
			}
			return !relicModel.HasBeenRemovedFromState && relicModel.Owner.IsActiveForHooks;
		}
		return powerModel.Owner.CombatState != null && (powerModel.Owner.Player?.IsActiveForHooks ?? true);
	}

	private static async Task GodotTimerTask(double timeSec)
	{
		SceneTreeTimer sceneTreeTimer = ((SceneTree)Engine.GetMainLoop()).CreateTimer(timeSec);
		await sceneTreeTimer.ToSignal(sceneTreeTimer, SceneTreeTimer.SignalName.Timeout);
	}
}
