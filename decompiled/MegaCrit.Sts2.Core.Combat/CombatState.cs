using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Combat;

/// <summary>
/// An object containing all of the state that represents the current combat.
///
/// Some state details are derived from higher-level objects. For example:
/// * All players in the combat state are derived by checking the Player property of all the creatures.
/// * All cards in the combat state are derived by checking the combat piles of all those players.
/// </summary>
public class CombatState : ICombatState, ICardScope
{
	private readonly List<Creature> _allies = new List<Creature>();

	private readonly List<Creature> _enemies = new List<Creature>();

	/// <summary>
	/// This is the ID that will be assigned to the next spawned creature's CombatId field.
	/// If we receive an action targeting a creature with an ID less than this one, then it must either be in the creature
	/// list or it had died at some point in the past.
	/// If we receive an action targeting a creature with an ID greater than or equal to this one, then it has yet to
	/// spawn (or there is some other error).
	/// </summary>
	private uint _nextCreatureId;

	private readonly EncounterModel? _encounter;

	private readonly List<Creature> _escapedCreatures = new List<Creature>();

	/// <summary>
	/// All cards that have been created within this state.
	/// This allows us to keep track of "floating" cards that have not been added to any piles (like fake cards in
	/// upgrade previews).
	/// </summary>
	private readonly List<CardModel> _allCards = new List<CardModel>();

	/// <summary>
	/// The state of the run that this combat exists in.
	/// Will be <see cref="P:MegaCrit.Sts2.Core.Combat.CombatState.RunState" /> in gameplay and <see cref="T:MegaCrit.Sts2.Core.Runs.NullRunState" /> in some test/debug scenarios.
	/// </summary>
	public IRunState RunState { get; }

	/// <summary>
	/// Get all creatures on the Allies side.
	/// </summary>
	public IReadOnlyList<Creature> Allies => _allies;

	/// <summary>
	/// Get all creatures on the Enemies side.
	/// </summary>
	public IReadOnlyList<Creature> Enemies => _enemies;

	/// <summary>
	/// Get all creatures in the combat on all sides.
	/// </summary>
	public IReadOnlyList<Creature> Creatures => _allies.Concat(_enemies).ToList();

	/// <summary>
	/// Get all the player creatures in the combat.
	/// </summary>
	public IReadOnlyList<Creature> PlayerCreatures => Creatures.Where((Creature c) => c.IsPlayer).ToList();

	/// <summary>
	/// Get all players in the combat.
	/// </summary>
	public IReadOnlyList<Player> Players => PlayerCreatures.Select((Creature c) => c.Player).ToList();

	/// <summary>
	/// List of custom modifiers applied to this combat, usually via daily or custom runs.
	/// </summary>
	public IReadOnlyList<ModifierModel> Modifiers { get; }

	/// <summary>
	/// List badge models, used when unlocking badges.
	/// </summary>
	public IReadOnlyList<BadgeModel> BadgeModels { get; }

	/// <summary>
	/// The model used to scale various things (block, power application) in multiplayer.
	/// </summary>
	public MultiplayerScalingModel? MultiplayerScalingModel { get; private set; }

	/// <summary>
	/// The round of combat that we're on in this combat.
	/// A "round" encompasses both the player's turn and the enemy's turn.
	/// This starts at 1, so it should never be 0.
	/// </summary>
	public int RoundNumber { get; set; }

	/// <summary>
	/// The side that is active in this combat.
	/// A round starts with the player side being active, then switches to the enemy side after all players have ended
	/// their turn.
	/// When the enemy turn ends, the current side changes back to the player side, and the round number is incremented.
	/// </summary>
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

	/// <summary>
	/// A list of creatures that escaped (were removed without dying) during the encounter. Used when rewards are given.
	/// </summary>
	public IReadOnlyList<Creature> EscapedCreatures => _escapedCreatures;

	/// <summary>
	/// Get all the creatures on the currently-active side.
	/// </summary>
	public IReadOnlyList<Creature> CreaturesOnCurrentSide => GetCreaturesOnSide(CurrentSide);

	/// <summary>
	/// Get all hittable enemies.
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Creatures.Creature.IsHittable" /> for a definition.
	///
	/// NOTE: We shouldn't add too many methods like this, this one is just extremely common because it's used for AOE
	/// and random attack targeting.
	/// </summary>
	public IReadOnlyList<Creature> HittableEnemies => Enemies.Where((Creature e) => e.IsHittable).ToList();

	/// <summary>
	/// Fired whenever the arrangement of creatures in the combat changes. Specifically, when:
	/// * A creature is added.
	/// * A creature is removed.
	/// * A creature's index changes.
	/// </summary>
	public event Action<ICombatState>? CreaturesChanged;

	public CombatState(EncounterModel? encounter = null, IRunState? runState = null, IReadOnlyList<ModifierModel>? modifiers = null, IReadOnlyList<BadgeModel>? badgeModels = null, MultiplayerScalingModel? multiplayerScalingModel = null)
	{
		encounter?.AssertMutable();
		Encounter = encounter;
		RoundNumber = 1;
		CurrentSide = CombatSide.Player;
		RunState = runState ?? NullRunState.Instance;
		Modifiers = modifiers ?? Array.Empty<ModifierModel>();
		BadgeModels = badgeModels ?? Array.Empty<BadgeModel>();
		MultiplayerScalingModel = multiplayerScalingModel;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.ICardScope.CreateCard``1(MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public T CreateCard<T>(Player owner) where T : CardModel
	{
		return (T)CreateCard(ModelDb.Card<T>(), owner);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.ICardScope.CreateCard(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public CardModel CreateCard(CardModel canonicalCard, Player owner)
	{
		CardModel cardModel = canonicalCard.ToMutable();
		AddCard(cardModel, owner);
		cardModel.AfterCreated();
		return cardModel;
	}

	/// <summary>
	/// WARNING: If you're specifically intending to create a clone in combat for an effect like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.DualWield" />,
	/// you should use <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.CreateClone" /> instead.
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.ICardScope.CloneCard(MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public CardModel CloneCard(CardModel mutableCard)
	{
		CardModel cardModel = (CardModel)mutableCard.ClonePreservingMutability();
		AddCard(cardModel);
		return cardModel;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.ICardScope.AddCard(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
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

	/// <summary>
	/// Does this state contain the specified card?
	/// </summary>
	public bool ContainsCard(CardModel card)
	{
		return _allCards.Contains(card);
	}

	/// <summary>
	/// Add a player's creature on the Allies side.
	/// </summary>
	public void AddPlayer(Player player)
	{
		AttachCreature(player.Creature);
		AddCreature(player.Creature);
	}

	/// <summary>
	/// Creates a new creature from a monster in this combat state.
	/// After you create the creature, you should call AddCreature to add it to combat. This can be done immediately
	/// or after some time.
	/// </summary>
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
		_encounter?.OnCreatureSpawned(creature);
		return creature;
	}

	/// <summary>
	/// Attaches an existing creature to this combat state without adding it to the combat.
	/// This sets up the creature to be added to combat without actually putting it into the combat (i.e. it's not
	/// targeted by cards that hit all enemies, powers don't proc on it, etc).
	/// For players and almost all monsters, this is called just before adding their creature to the combat. Currently,
	/// the Doormaker is the only creature that doesn't get added to combat immediately after this is called.
	/// </summary>
	private void AttachCreature(Creature creature)
	{
		creature.CombatState = this;
		creature.CombatId = _nextCreatureId;
		_nextCreatureId++;
	}

	/// <summary>
	/// Call this to remove a creature that escaped rather than dying.
	/// </summary>
	public void CreatureEscaped(Creature creature)
	{
		_escapedCreatures.Add(creature);
		RemoveCreature(creature);
	}

	/// <summary>
	/// Removes the creature from combat.
	/// </summary>
	/// <param name="creature">The creature that will be removed.</param>
	/// <param name="unattach">If true, then the creature cannot be re-added to the combat state.</param>
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

	/// <summary>
	/// Get the creature with the specified combat ID. Null if not found.
	/// </summary>
	public Creature? GetCreature(uint? combatId)
	{
		if (!combatId.HasValue)
		{
			return null;
		}
		return Creatures.FirstOrDefault((Creature c) => c.CombatId == combatId);
	}

	/// <summary>
	/// Get the creature with the specified combat ID.
	/// If the creature doesn't exist, keep checking for a while before timing out and returning null.
	/// </summary>
	/// <param name="combatId">Combat ID of the creature to get.</param>
	/// <param name="timeoutSec">How long to wait for the creature to appear.</param>
	/// <returns>Specified Creature, or null if it doesn't exist and we've waited long enough.</returns>
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
		void OnCreaturesChanged(ICombatState _)
		{
			Creature creature2 = GetCreature(combatId);
			if (creature2 != null)
			{
				completionSource.SetResult(creature2);
			}
		}
	}

	/// <summary>
	/// Get all the creatures on the specified side.
	/// </summary>
	public IReadOnlyList<Creature> GetCreaturesOnSide(CombatSide side)
	{
		if (side != CombatSide.Enemy)
		{
			return Allies;
		}
		return Enemies;
	}

	/// <summary>
	/// Get all opponents of a creature.
	/// </summary>
	public IReadOnlyList<Creature> GetOpponentsOf(Creature creature)
	{
		return GetCreaturesOnSide(creature.Side.GetOppositeSide());
	}

	/// <summary>
	/// Get all teammates of a creature, including the creature itself.
	/// </summary>
	public IReadOnlyList<Creature> GetTeammatesOf(Creature creature)
	{
		return GetCreaturesOnSide(creature.Side);
	}

	public Player? GetPlayer(ulong playerId)
	{
		return Players.FirstOrDefault((Player p) => p.NetId == playerId);
	}

	/// <summary>
	/// Get all the models that should have combat hooks called on them.
	/// </summary>
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
		for (int num = 0; num < BadgeModels.Count; num++)
		{
			list.Add(BadgeModels[num]);
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
		foreach (AbstractModel item2 in ModHelper.IterateAllCombatStateSubscribers(this))
		{
			yield return item2;
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

	/// <summary>
	/// Adds a creature to the combat.
	/// This should almost always be called right after CreateCreature. Until this is called, creatures cannot be targeted
	/// and their powers will not be triggered. It is separate so that creatures can be removed from combat and re-added
	/// to it later. Currently, this is only used in the Doormaker combat.
	/// </summary>
	/// <param name="creature">The creature to add to the combat.</param>
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
											if (!(model is BadgeModel))
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

	public bool IsLiveCombat()
	{
		return true;
	}
}
