using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

	public CombatState? CombatState { get; set; }

	public string Name
	{
		get
		{
			if (IsMonster)
			{
				return Monster.Title.GetFormattedText();
			}
			if (RunManager.Instance.IsSinglePlayerOrFakeMultiplayer)
			{
				return Player.Character.Title.GetFormattedText();
			}
			return PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, Player.NetId);
		}
	}

	public bool IsMonster => Monster != null;

	public bool IsPlayer => Player != null;

	public bool ShowsInfiniteHp { get; set; }

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

	public bool IsPet => PetOwner != null;

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

	public async Task AfterAddedToRoom()
	{
		if (Side == CombatSide.Enemy)
		{
			await Monster.AfterAddedToRoom();
		}
	}

	public decimal DamageBlockInternal(decimal amount, ValueProp props)
	{
		decimal num = (props.HasFlag(ValueProp.Unblockable) ? 0m : Math.Min(Block, amount));
		Block -= (int)num;
		return num;
	}

	public DamageResult LoseHpInternal(decimal amount, ValueProp props)
	{
		bool flag = CurrentHp > 0 && amount >= (decimal)CurrentHp;
		int currentHp = CurrentHp;
		CurrentHp = Math.Max(CurrentHp - (int)amount, 0);
		return new DamageResult(this, props)
		{
			UnblockedDamage = currentHp - CurrentHp,
			WasTargetKilled = flag,
			OverkillDamage = (flag ? ((int)(-((decimal)currentHp - amount))) : 0)
		};
	}

	public void GainBlockInternal(decimal amount)
	{
		if (amount < 0m)
		{
			throw new ArgumentException("amount must be positive. Use LoseBlock for block loss.");
		}
		Block = Math.Min(Block + (int)amount, 999);
	}

	public void LoseBlockInternal(decimal amount)
	{
		if (amount < 0m)
		{
			throw new ArgumentException("amount must be positive. Use GainBlock for block gain.");
		}
		Block = Math.Max(Block - (int)amount, 0);
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
		MaxHp = (int)amount;
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

	public PowerModel? GetPowerById(ModelId id)
	{
		return _powers.FirstOrDefault((PowerModel p) => p.Id == id);
	}

	public int GetPowerAmount<T>() where T : PowerModel
	{
		return GetPower<T>()?.Amount ?? 0;
	}

	public void ApplyPowerInternal(PowerModel power)
	{
		if (power.Owner != this)
		{
			throw new InvalidOperationException("ONLY CALL THIS FROM PowerModel.ApplyInternal!");
		}
		if (!power.IsInstanced && _powers.Any((PowerModel p) => p.GetType() == power.GetType()))
		{
			throw new InvalidOperationException("Trying to add multiple instances of a non-instanced power to a creature.");
		}
		_powers.Add(power);
		this.PowerApplied?.Invoke(power);
	}

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

	public void RemovePowerInternal(PowerModel power)
	{
		if (power.Owner != this)
		{
			throw new InvalidOperationException("ONLY CALL THIS FROM PowerModel.RemoveInternal!");
		}
		_powers.Remove(power);
		this.PowerRemoved?.Invoke(power);
	}

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

	public void BeforeTurnStart(int roundNumber, CombatSide side)
	{
		foreach (PowerModel power in _powers)
		{
			power.AmountOnTurnStart = power.Amount;
		}
	}

	public async Task AfterTurnStart(int roundNumber, CombatSide side)
	{
		if (roundNumber > 1 || side != CombatSide.Player)
		{
			await ClearBlock();
		}
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
		return "Creature " + Name;
	}

	public double GetHpPercentRemaining()
	{
		return (double)_currentHp / (double)_maxHp;
	}

	public static decimal ScaleHpForMultiplayer(decimal hp, EncounterModel? encounter, int playerCount, int actIndex)
	{
		if (playerCount == 1)
		{
			return hp;
		}
		return hp * (decimal)playerCount * MultiplayerScalingModel.GetMultiplayerScaling(encounter, actIndex);
	}
}
