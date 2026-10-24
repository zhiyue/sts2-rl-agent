using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Entities.Creatures;

public class Creature
{
	private int _block;

	private int _currentHp;

	private int _maxHp;

	private readonly List<PowerModel> _powers = new List<PowerModel>();

	private Player? _petOwner;

	public int Block
	{
		get
		{
			return _block;
		}
		private set
		{
			if (value < 0)
			{
				throw new ArgumentException("Block must be positive", "value");
			}
			if (_block != value)
			{
				int block = _block;
				_block = value;
				this.BlockChanged?.Invoke(block, _block);
			}
		}
	}

	public int CurrentHp
	{
		get
		{
			return _currentHp;
		}
		private set
		{
			if (value < 0)
			{
				throw new ArgumentException("Current HP must be positive", "value");
			}
			if (_currentHp != value)
			{
				int currentHp = _currentHp;
				_currentHp = value;
				this.CurrentHpChanged?.Invoke(currentHp, _currentHp);
			}
		}
	}

	public int MaxHp
	{
		get
		{
			return _maxHp;
		}
		private set
		{
			if (_maxHp != value)
			{
				int maxHp = _maxHp;
				_maxHp = value;
				this.MaxHpChanged?.Invoke(maxHp, _maxHp);
			}
		}
	}

	public int? MonsterMaxHpBeforeModification { get; private set; }

	public uint? CombatId { get; set; }

	public MonsterModel? Monster { get; }

	public Player? Player { get; }

	public ModelId ModelId
	{
		get
		{
			if (!IsPlayer)
			{
				return Monster.Id;
			}
			return Player.Character.Id;
		}
	}

	public CombatSide Side { get; }

	/// <summary>
	/// The CombatState that this creature exists in.
	/// Never null for monsters.
	/// Will be null for players when outside of combat.
	/// </summary>
	public ICombatState? CombatState { get; set; }

	/// <summary>
	/// Use this when displaying the name to the player.
	/// </summary>
	public string Name
	{
		get
		{
			if (IsMonster)
			{
				return Monster.Title.GetFormattedText();
			}
			if (RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
			{
				return Player.Character.Title.GetFormattedText();
			}
			return PlatformUtil.GetPlayerNameRaw(RunManager.Instance.NetService.Platform, Player.NetId);
		}
	}

	/// <summary>
	/// Use this when logging the name of the creature.
	/// It avoids logging player names directly.
	/// </summary>
	public string LogName
	{
		get
		{
			if (IsMonster)
			{
				return Monster.Title.GetFormattedText();
			}
			if (RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
			{
				return Player.Character.Title.GetFormattedText();
			}
			return "PlayerId " + Player.NetId;
		}
	}

	public bool IsMonster => Monster != null;

	public bool IsPlayer => Player != null;

	/// <summary>
	/// How should this creature's health bar be displayed?
	/// This is mostly for visuals, although a few gameplay effects (like <see cref="T:MegaCrit.Sts2.Core.Models.Powers.HellraiserPower" />) check it to see
	/// if the creature can currently take damage.
	/// </summary>
	public HpDisplay HpDisplay { get; set; }

	/// <summary>
	/// The player that owns this pet.
	/// Returns null if this creature is not a pet.
	/// </summary>
	public Player? PetOwner
	{
		get
		{
			return _petOwner;
		}
		set
		{
			if (_petOwner != null)
			{
				throw new InvalidOperationException($"Pet {this} already has an owner.");
			}
			_petOwner = value;
		}
	}

	/// <summary>
	/// Is this creature a pet?
	/// </summary>
	public bool IsPet => PetOwner != null;

	/// <summary>
	/// Get all the pets that belong to this creature.
	/// </summary>
	public IReadOnlyList<Creature> Pets => Player?.PlayerCombatState?.Pets ?? Array.Empty<Creature>();

	public bool IsAlive => CurrentHp > 0;

	public bool IsDead => !IsAlive;

	public string? SlotName { get; set; }

	public IEnumerable<IHoverTip> HoverTips
	{
		get
		{
			if (!CombatManager.Instance.IsInProgress)
			{
				return Array.Empty<IHoverTip>();
			}
			List<IHoverTip> list = new List<IHoverTip>();
			if (IsMonster)
			{
				foreach (AbstractIntent intent in Monster.NextMove.Intents)
				{
					if (intent.HasIntentTip)
					{
						list.Add(intent.GetHoverTip(CombatState.Allies, this));
					}
				}
			}
			foreach (PowerModel power in _powers)
			{
				IEnumerable<IHoverTip> hoverTips = power.HoverTips;
				foreach (IHoverTip item in hoverTips)
				{
					list.MegaTryAddingTip(item);
				}
			}
			return list;
		}
	}

	public bool IsEnemy => Side == CombatSide.Enemy;

	/// <summary>
	/// Is this creature a <b>primary enemy</b>?
	/// A <b>primary enemy</b> can stay alive all alone.
	/// A <b>secondary enemy</b> will automatically die unless there's also a living primary enemy.
	/// Most enemies are primary enemies. Enemies with powers like <see cref="T:MegaCrit.Sts2.Core.Models.Powers.MinionPower" /> or <see cref="T:MegaCrit.Sts2.Core.Models.Powers.IllusionPower" /> are
	/// secondary.
	/// </summary>
	public bool IsPrimaryEnemy
	{
		get
		{
			if (Side != CombatSide.Enemy)
			{
				return false;
			}
			return !IsSecondaryEnemy;
		}
	}

	/// <summary>
	/// Is this creature a <b>secondary enemy</b>?
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Creatures.Creature.IsPrimaryEnemy" /> for detailed definitions.
	/// </summary>
	public bool IsSecondaryEnemy
	{
		get
		{
			if (Side != CombatSide.Enemy)
			{
				return false;
			}
			return Powers.Any((PowerModel p) => p.OwnerIsSecondaryEnemy);
		}
	}

	/// <summary>
	/// Is this creature <b>hittable</b>?
	/// A <b>hittable</b> creature is alive and can be "hit" by effects.
	/// This is different from <b>targetable</b>; an un-targetable creature can still be hit by AOE effects.
	/// </summary>
	public bool IsHittable
	{
		get
		{
			if (IsDead)
			{
				return false;
			}
			if (!Hook.ShouldAllowHitting(CombatState, this))
			{
				return false;
			}
			return true;
		}
	}

	/// <summary>
	/// Can this creature have powers applied to it?
	/// A creature can have powers applied to it if it's in combat and can be "hit" by effects.
	/// This is different from <b>targetable</b>; an un-targetable creature can have powers applied to it.
	/// It's also different from <b>hittable</b>; a creature is not hittable if it's dead, but dead creatures can still
	/// have powers applied to them.
	/// </summary>
	public bool CanReceivePowers
	{
		get
		{
			if (CombatState == null)
			{
				return false;
			}
			if (!Hook.ShouldAllowHitting(CombatState, this))
			{
				return false;
			}
			return true;
		}
	}

	public bool IsStunned => Monster?.NextMove.Id == "STUNNED";

	public IReadOnlyList<PowerModel> Powers => _powers;

	public event Action<int, int>? BlockChanged;

	public event Action<int, int>? CurrentHpChanged;

	public event Action<int, int>? MaxHpChanged;

	public event Action<PowerModel>? PowerApplied;

	public event Action<PowerModel, int, bool>? PowerIncreased;

	public event Action<PowerModel, bool>? PowerDecreased;

	public event Action<PowerModel>? PowerRemoved;

	public event Action<Creature>? Died;

	public event Action<Creature>? Revived;

	public Creature(MonsterModel monster, CombatSide side, string? slotName)
	{
		monster.AssertMutable();
		int minInitialHp = monster.MinInitialHp;
		int maxInitialHp = monster.MaxInitialHp;
		if (minInitialHp > maxInitialHp)
		{
			throw new InvalidOperationException($"{monster.Id.Entry} has min HP {minInitialHp} greater than its max {maxInitialHp}!");
		}
		Monster = monster;
		Monster.Creature = this;
		SlotName = slotName;
		_maxHp = maxInitialHp;
		_currentHp = maxInitialHp;
		Side = side;
	}

	public Creature(Player player, int currentHp, int maxHp)
	{
		Player = player;
		_currentHp = currentHp;
		_maxHp = maxHp;
		Side = CombatSide.Player;
	}

	public void SetUniqueMonsterHpValue(IReadOnlyList<Creature> creaturesOnSide, Rng rng)
	{
		if (Monster == null)
		{
			throw new InvalidOperationException("Can't set unique monster HP value for a player.");
		}
		int minInitialHp = Monster.MinInitialHp;
		int num = Monster.MaxInitialHp + 1;
		HashSet<int> hashSet = Enumerable.Range(minInitialHp, num - minInitialHp).ToHashSet();
		hashSet.ExceptWith(from e in creaturesOnSide.Except(new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(this))
			select e.MaxHp);
		MonsterMaxHpBeforeModification = (_currentHp = (_maxHp = ((hashSet.Count <= 0) ? rng.NextInt(minInitialHp, num) : rng.NextItem(hashSet))));
	}

	public void ScaleMonsterHpForMultiplayer(EncounterModel? encounter, int playerCount, int actIndex)
	{
		if (playerCount != 1)
		{
			SetMaxHpInternal(ScaleHpForMultiplayer(MaxHp, encounter, playerCount, actIndex));
			SetCurrentHpInternal(MaxHp);
		}
	}

	public NCreatureVisuals? CreateVisuals()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (Player != null)
		{
			return Player.Character.CreateVisuals();
		}
		if (Monster != null)
		{
			return Monster.CreateVisuals();
		}
		throw new InvalidOperationException("Creature and Monster should never both be null.");
	}

	/// <summary>
	/// Called when the creature is first added to combat.
	/// </summary>
	public async Task AfterAddedToRoom()
	{
		if (Side == CombatSide.Enemy)
		{
			await Monster.AfterAddedToRoom();
		}
	}

	/// <summary>
	/// DO NOT CALL UNLESS YOU KNOW EXACTLY WHAT YOU'RE DOING.
	/// Hooks and everything are all done in CreatureCmd.Damage, so there needs to be a really good reason to want to
	/// avoid them.
	/// </summary>
	/// <param name="amount">Amount of damage to be dealt to this creature's block.</param>
	/// <param name="props">Damage props</param>
	/// <returns>How much damage was blocked.</returns>
	public decimal DamageBlockInternal(decimal amount, ValueProp props)
	{
		decimal num = (props.HasFlag(ValueProp.Unblockable) ? 0m : Math.Min(Block, amount));
		Block -= (int)num;
		return num;
	}

	/// <summary>
	/// DO NOT CALL UNLESS YOU KNOW EXACTLY WHAT YOU'RE DOING.
	/// Hooks and everything are all done in CreatureCmd.Damage, so there needs to be a really good reason to want to
	/// avoid them.
	/// </summary>
	/// <param name="amount">Amount of damage to be dealt to this creature's HP.</param>
	/// <param name="props">Value props for the damage.</param>
	/// <returns>Result of the damage.</returns>
	public DamageResult LoseHpInternal(decimal amount, ValueProp props)
	{
		bool flag = CurrentHp > 0 && amount >= (decimal)CurrentHp;
		int currentHp = CurrentHp;
		int num = (int)Math.Min(amount, 999999999m);
		CurrentHp = Math.Max(CurrentHp - num, 0);
		return new DamageResult(this, props)
		{
			UnblockedDamage = currentHp - CurrentHp,
			WasTargetKilled = flag,
			OverkillDamage = (flag ? Math.Max(num - currentHp, 0) : 0)
		};
	}

	public void GainBlockInternal(decimal amount)
	{
		if (amount < 0m)
		{
			throw new ArgumentException("amount must be positive. Use LoseBlock for block loss.");
		}
		Block = (int)Math.Min((decimal)Block + amount, 999999999m);
	}

	public void LoseBlockInternal(decimal amount)
	{
		if (amount < 0m)
		{
			throw new ArgumentException("amount must be positive. Use GainBlock for block gain.");
		}
		Block = (int)Math.Max((decimal)Block - amount, 0m);
	}

	public void HealInternal(decimal amount)
	{
		bool isDead = IsDead;
		SetCurrentHpInternal((decimal)CurrentHp + amount);
		if (isDead && !IsDead)
		{
			Player?.ActivateHooks();
			this.Revived?.Invoke(this);
		}
	}

	public void SetCurrentHpInternal(decimal amount)
	{
		CurrentHp = (int)Math.Min(amount, MaxHp);
	}

	public void SetMaxHpInternal(decimal amount)
	{
		if (amount < 0m)
		{
			throw new ArgumentException("amount must be non-negative.");
		}
		MaxHp = Math.Min((int)amount, 999999999);
		CurrentHp = Math.Min(CurrentHp, MaxHp);
	}

	public void Reset()
	{
		RemoveAllPowersInternalExcept();
		Block = 0;
	}

	public void InvokeDiedEvent()
	{
		this.Died?.Invoke(this);
	}

	/// <summary>
	/// Stun this creature.
	/// You should probably be calling
	/// <see cref="M:MegaCrit.Sts2.Core.Commands.CreatureCmd.Stun(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Func{System.Collections.Generic.IReadOnlyList{MegaCrit.Sts2.Core.Entities.Creatures.Creature},System.Threading.Tasks.Task},System.String)" /> instead.
	/// </summary>
	/// <param name="stunMove">Logic for the move that the stunned creature will "perform".</param>
	/// <param name="nextMoveId">
	/// ID of the move that the creature should perform the turn after they perform stunMove.
	/// If null or empty, this will default to the last move they performed.
	/// </param>
	public void StunInternal(Func<IReadOnlyList<Creature>, Task> stunMove, string? nextMoveId)
	{
		if (Monster == null)
		{
			throw new InvalidOperationException("Can't stun a player.");
		}
		if (CombatState != null && !IsDead)
		{
			if (string.IsNullOrEmpty(nextMoveId))
			{
				List<MonsterState> stateLog = Monster.MoveStateMachine.StateLog;
				nextMoveId = stateLog.Last().Id;
			}
			MoveState state = new MoveState("STUNNED", stunMove, new StunIntent())
			{
				FollowUpStateId = nextMoveId,
				MustPerformOnceBeforeTransitioning = true
			};
			Monster.SetMoveImmediate(state);
		}
	}

	public void PrepareForNextTurn(IEnumerable<Creature> targets, bool rollNewMove = true)
	{
		Creature[] targets2 = targets.ToArray();
		if (rollNewMove)
		{
			Monster.RollMove(targets2);
		}
		NCombatRoom.Instance?.GetCreatureNode(this)?.RefreshIntents();
	}

	public bool HasPower<T>() where T : PowerModel
	{
		return _powers.Any((PowerModel p) => p is T);
	}

	public bool HasPower(ModelId id)
	{
		return _powers.Any((PowerModel p) => p.Id == id);
	}

	public T? GetPower<T>() where T : PowerModel
	{
		return _powers.FirstOrDefault((PowerModel p) => p is T) as T;
	}

	public PowerModel? GetPower(ModelId id)
	{
		return _powers.FirstOrDefault((PowerModel p) => p.Id == id);
	}

	public IEnumerable<T> GetPowerInstances<T>() where T : PowerModel
	{
		return _powers.OfType<T>();
	}

	public IEnumerable<PowerModel> GetPowerInstances(ModelId id)
	{
		return _powers.Where((PowerModel p) => p.Id == id);
	}

	public PowerModel? GetPowerById(ModelId id)
	{
		return _powers.FirstOrDefault((PowerModel p) => p.Id == id);
	}

	public int GetPowerAmount<T>() where T : PowerModel
	{
		return GetPower<T>()?.Amount ?? 0;
	}

	/// <summary>
	/// NEVER CALL THIS!
	/// ONLY PowerModel.ApplyInternal should be calling this.
	/// </summary>
	/// <param name="power">Power to apply.</param>
	public void ApplyPowerInternal(PowerModel power)
	{
		if (power.Owner != this)
		{
			throw new InvalidOperationException("ONLY CALL THIS FROM PowerModel.ApplyInternal!");
		}
		if (power.InstanceType == PowerInstanceType.None && _powers.Any((PowerModel p) => p.GetType() == power.GetType()))
		{
			throw new InvalidOperationException("Trying to add multiple instances of a non-instanced power to a creature.");
		}
		_powers.Add(power);
		this.PowerApplied?.Invoke(power);
	}

	/// <summary>
	/// NEVER CALL THIS!
	/// ONLY PowerModel.Amount should be calling this.
	/// </summary>
	/// <param name="power">Power to modify.</param>
	/// <param name="change">How much the power has changed.</param>
	/// <param name="silent">Whether or not VFX should be displayed for this power.</param>
	public void InvokePowerModified(PowerModel power, int change, bool silent)
	{
		if (change > 0)
		{
			this.PowerIncreased?.Invoke(power, change, silent);
		}
		else if (power.StackType.Equals(PowerStackType.Counter) && power.AllowNegative && change < 0)
		{
			this.PowerIncreased?.Invoke(power, change, silent);
		}
		else
		{
			this.PowerDecreased?.Invoke(power, silent);
		}
	}

	/// <summary>
	/// NEVER CALL THIS!
	/// ONLY PowerModel.RemoveInternal should be calling this.
	/// </summary>
	/// <param name="power">Power to remove.</param>
	public void RemovePowerInternal(PowerModel power)
	{
		if (power.Owner != this)
		{
			throw new InvalidOperationException("ONLY CALL THIS FROM PowerModel.RemoveInternal!");
		}
		_powers.Remove(power);
		this.PowerRemoved?.Invoke(power);
	}

	/// <summary>
	/// NEVER CALL THIS UNLESS YOU KNOW WHAT YOU'RE DOING!
	/// This skips the AfterRemoved call for powers.
	/// </summary>
	/// <param name="except">Powers that should not be removed.</param>
	public IEnumerable<PowerModel> RemoveAllPowersInternalExcept(IEnumerable<PowerModel>? except = null)
	{
		List<PowerModel> list = _powers.Except(except ?? Array.Empty<PowerModel>()).ToList();
		foreach (PowerModel item in list)
		{
			item.RemoveInternal();
		}
		return list;
	}

	public IEnumerable<PowerModel> RemoveAllPowersAfterDeath()
	{
		return RemoveAllPowersInternalExcept(_powers.Where((PowerModel p) => !p.ShouldPowerBeRemovedAfterOwnerDeath() || !Hook.ShouldPowerBeRemovedOnDeath(p)));
	}

	public void BeforeTurnStart(CombatSide side)
	{
		foreach (PowerModel power in _powers)
		{
			power.AmountOnTurnStart = power.Amount;
		}
	}

	public async Task AfterTurnStart(CombatSide side)
	{
		if (side == CombatSide.Player)
		{
			Player? player = Player;
			if (player != null && player.PlayerCombatState?.TurnNumber == 1)
			{
				return;
			}
		}
		await ClearBlock();
	}

	public void OnSideSwitch()
	{
		if (IsPlayer)
		{
			Player.OnSideSwitch();
		}
		else
		{
			Monster.OnSideSwitch();
		}
	}

	public async Task TakeTurn()
	{
		if (!IsMonster || Side != CombatSide.Enemy)
		{
			throw new InvalidOperationException("Only enemy monsters can take automated turns.");
		}
		if (!Monster.SpawnedThisTurn)
		{
			await Monster.PerformMove();
		}
	}

	private async Task ClearBlock()
	{
		if (Hook.ShouldClearBlock(CombatState, this, out AbstractModel preventer))
		{
			Block = 0;
		}
		else
		{
			await Hook.AfterPreventingBlockClear(CombatState, preventer, this);
		}
	}

	public override string ToString()
	{
		return "Creature " + LogName;
	}

	/// <summary>
	/// Helper function to get the percent hp of a creature (0 - 1).
	/// </summary>
	/// <returns></returns>
	public double GetHpPercentRemaining()
	{
		return (double)_currentHp / (double)_maxHp;
	}

	public static decimal ScaleHpForMultiplayer(decimal hp, EncounterModel? encounter, int playerCount, int actIndex)
	{
		if (playerCount <= 1)
		{
			return hp;
		}
		return hp * (decimal)playerCount * MultiplayerScalingModel.GetMultiplayerScaling(encounter, actIndex);
	}

	public NCreature? GetCreatureNode()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		object obj = NCombatRoom.Instance?.GetCreatureNode(this);
		if (obj == null)
		{
			NBestiary? instance = NBestiary.Instance;
			if (instance == null)
			{
				return null;
			}
			obj = instance.GetCreatureNode(this);
		}
		return (NCreature?)obj;
	}

	public Control? GetVfxContainer()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (NCombatRoom.Instance?.GetCreatureNode(this) != null)
		{
			return NCombatRoom.Instance.CombatVfxContainer;
		}
		if (NBestiary.Instance?.GetCreatureNode(this) != null)
		{
			return NBestiary.Instance.VfxContainer;
		}
		return null;
	}

	public Control? GetBackVfxContainer()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (NCombatRoom.Instance?.GetCreatureNode(this) != null)
		{
			return NCombatRoom.Instance.BackCombatVfxContainer;
		}
		if (NBestiary.Instance?.GetCreatureNode(this) != null)
		{
			return NBestiary.Instance.BackVfxContainer;
		}
		return null;
	}
}
