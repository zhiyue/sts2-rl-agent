using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Commands.Builders;

public class AttackCommand
{
	private enum SourceType
	{
		None,
		Card,
		Monster
	}

	private readonly decimal _damagePerHit;

	private readonly CalculatedDamageVar? _calculatedDamageVar;

	private int _hitCount = 1;

	private SourceType _sourceType;

	private CombatState? _combatState;

	private Creature? _singleTarget;

	private bool _spawnVfxOnEachCreature;

	private bool _spawnVfxOnCreatureCenter = true;

	private bool _doesRandomTargetingAllowDuplicates = true;

	private bool _shouldPlayAnimation = true;

	private readonly List<DamageResult> _results = new List<DamageResult>();

	private string? _attackerAnimName;

	private float _attackerAnimDelay;

	private Creature? _visualAttacker;

	private bool _playOnEveryHit = true;

	private string? _attackerVfx;

	private string? _attackerSfx;

	private string? _tmpAttackerSfx;

	private readonly float[] _waitBeforeHit = new float[2] { -1f, -1f };

	private readonly List<Func<Node2D?>> _customAttackerVfxNodes = new List<Func<Node2D>>();

	private readonly List<Func<Creature, Node2D?>> _customHitVfxNodes = new List<Func<Creature, Node2D>>();

	private Func<Task>? _afterAttackerAnim;

	private Func<Task>? _beforeDamage;

	public Creature? Attacker { get; private set; }

	public AbstractModel? ModelSource { get; private set; }

	public CombatSide TargetSide { get; private set; }

	public ValueProp DamageProps { get; private set; } = ValueProp.Move;

	public bool IsSingleTargeted => _singleTarget != null;

	public bool IsMultiTargeted => _combatState != null;

	public bool IsRandomlyTargeted { get; private set; }

	public IEnumerable<DamageResult> Results => _results;

	public string? HitSfx { get; private set; }

	public string? TmpHitSfx { get; private set; }

	public string? HitVfx { get; private set; }

	private IReadOnlyList<Creature> GetPossibleTargets()
	{
		if (IsSingleTargeted)
		{
			return new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(_singleTarget);
		}
		if (IsMultiTargeted)
		{
			if (_sourceType == SourceType.Monster)
			{
				return _combatState.PlayerCreatures;
			}
			if (Attacker == null)
			{
				throw new InvalidOperationException("We require an attacker to be able to grab its opponents");
			}
			return _combatState.GetOpponentsOf(Attacker);
		}
		throw new InvalidOperationException("No targets set, a Targeting method must be called before Execute");
	}

	public AttackCommand(decimal damagePerHit)
	{
		_damagePerHit = damagePerHit;
		_calculatedDamageVar = null;
	}

	public AttackCommand(CalculatedDamageVar calculatedDamageVar)
	{
		_damagePerHit = -1m;
		_calculatedDamageVar = calculatedDamageVar;
	}

	public AttackCommand FromCard(CardModel card)
	{
		if (Attacker != null)
		{
			throw new InvalidOperationException("Attacker has already been set.");
		}
		if (ModelSource != null)
		{
			throw new InvalidOperationException("ModelSource has already been set.");
		}
		Player owner = card.Owner;
		Attacker = owner.Creature;
		_attackerAnimName = "Attack";
		_attackerAnimDelay = owner.Character.AttackAnimDelay;
		ModelSource = card;
		_sourceType = SourceType.Card;
		return this;
	}

	public AttackCommand FromOsty(Creature osty, CardModel card)
	{
		if (!(osty.Monster is Osty))
		{
			throw new ArgumentException("Creature is not Osty");
		}
		Attacker = osty;
		ModelSource = card;
		_attackerAnimName = "Attack";
		_attackerAnimDelay = 0.3f;
		_sourceType = SourceType.Card;
		return WithAttackerFx(null, "event:/sfx/characters/osty/osty_attack");
	}

	public AttackCommand FromMonster(MonsterModel monster)
	{
		if (Attacker != null)
		{
			throw new InvalidOperationException("Attacker has already been set.");
		}
		Attacker = monster.Creature;
		_attackerAnimName = "Attack";
		_sourceType = SourceType.Monster;
		return TargetingAllOpponents(monster.Creature.CombatState);
	}

	public AttackCommand Targeting(Creature target)
	{
		if (_singleTarget != null)
		{
			throw new InvalidOperationException("Targets already set.");
		}
		if (_combatState != null)
		{
			throw new InvalidOperationException("Already set to target opponents of attacker");
		}
		_singleTarget = target;
		TargetSide = target.Side;
		return this;
	}

	public AttackCommand TargetingAllOpponents(CombatState combatState)
	{
		if (_singleTarget != null)
		{
			throw new InvalidOperationException("Targets already set.");
		}
		if (_combatState != null)
		{
			throw new InvalidOperationException("Already set to target opponents of attacker");
		}
		if (Attacker == null)
		{
			throw new InvalidOperationException("We require an attacker to be able to grab its opponents");
		}
		_combatState = combatState;
		TargetSide = ((Attacker.Side == CombatSide.Enemy) ? CombatSide.Player : CombatSide.Enemy);
		return this;
	}

	public AttackCommand TargetingRandomOpponents(CombatState combatState, bool allowDuplicates = true)
	{
		if (_singleTarget != null)
		{
			throw new InvalidOperationException("Targets already set.");
		}
		if (_combatState != null)
		{
			throw new InvalidOperationException("Already set to target opponents of attacker");
		}
		if (Attacker == null)
		{
			throw new InvalidOperationException("We require an attacker to be able to grab its opponents");
		}
		_combatState = combatState;
		IsRandomlyTargeted = true;
		_doesRandomTargetingAllowDuplicates = allowDuplicates;
		return this;
	}

	public AttackCommand Unpowered()
	{
		DamageProps |= ValueProp.Unpowered;
		return this;
	}

	public AttackCommand WithAttackerAnim(string? animName, float delay, Creature? visualAttacker = null)
	{
		if (_attackerAnimName == null)
		{
			throw new InvalidOperationException("WithAttackerAnim was called before FromCard/FromMonster/FromOsty, should be called after.");
		}
		_attackerAnimName = animName;
		_attackerAnimDelay = delay;
		_visualAttacker = visualAttacker;
		return this;
	}

	public AttackCommand WithNoAttackerAnim()
	{
		_shouldPlayAnimation = false;
		return this;
	}

	public AttackCommand AfterAttackerAnim(Func<Task> afterAttackerAnim)
	{
		_afterAttackerAnim = afterAttackerAnim;
		return this;
	}

	public AttackCommand WithAttackerFx(string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		_attackerVfx = vfx;
		_attackerSfx = sfx;
		_tmpAttackerSfx = tmpSfx;
		return this;
	}

	public AttackCommand WithAttackerFx(Func<Node2D?> createAttackerVfx)
	{
		_customAttackerVfxNodes.Add(createAttackerVfx);
		return this;
	}

	public AttackCommand WithWaitBeforeHit(float fastSeconds, float standardSeconds)
	{
		_waitBeforeHit[0] = fastSeconds;
		_waitBeforeHit[1] = standardSeconds;
		return this;
	}

	public AttackCommand WithHitFx(string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		HitVfx = vfx;
		HitSfx = sfx;
		TmpHitSfx = tmpSfx;
		return this;
	}

	public AttackCommand SpawningHitVfxOnEachCreature()
	{
		_spawnVfxOnEachCreature = true;
		return this;
	}

	public AttackCommand WithHitVfxSpawnedAtBase()
	{
		_spawnVfxOnCreatureCenter = false;
		return this;
	}

	public AttackCommand WithHitVfxNode(Func<Creature, Node2D?> createHitVfxNode)
	{
		_customHitVfxNodes.Add(createHitVfxNode);
		return this;
	}

	public AttackCommand OnlyPlayAnimOnce()
	{
		_playOnEveryHit = false;
		return this;
	}

	public AttackCommand WithHitCount(int hitCount)
	{
		_hitCount = hitCount;
		return this;
	}

	public AttackCommand BeforeDamage(Func<Task> beforeDamage)
	{
		_beforeDamage = beforeDamage;
		return this;
	}

	public static async Task<AttackContext> CreateContextAsync(CombatState combatState, CardModel cardSource)
	{
		return await AttackContext.CreateAsync(combatState, cardSource);
	}

	public async Task<AttackCommand> Execute(PlayerChoiceContext? choiceContext)
	{
		if (Attacker == null)
		{
			throw new InvalidOperationException("No attacker set.");
		}
		if (!IsSingleTargeted && !IsMultiTargeted)
		{
			throw new InvalidOperationException("No targets set.");
		}
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return this;
		}
		CombatState combatState = Attacker.CombatState;
		await Hook.BeforeAttack(combatState, this);
		decimal attackCount = Hook.ModifyAttackHitCount(combatState, this, _hitCount);
		for (int i = 0; (decimal)i < attackCount; i++)
		{
			if (Attacker.IsDead)
			{
				break;
			}
			List<Creature> validTargets = (from c in GetPossibleTargets()
				where c.IsAlive
				select c).ToList();
			if (validTargets.Count == 0)
			{
				break;
			}
			if (_playOnEveryHit || i == 0)
			{
				if (_attackerVfx != null)
				{
					VfxCmd.PlayOnCreatureCenter(Attacker, _attackerVfx);
				}
				foreach (Func<Node2D> customAttackerVfxNode in _customAttackerVfxNodes)
				{
					NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(customAttackerVfxNode());
				}
				if (_attackerSfx != null)
				{
					SfxCmd.Play(_attackerSfx);
				}
				else if (_tmpAttackerSfx != null)
				{
					NDebugAudioManager.Instance?.Play(_tmpAttackerSfx);
				}
				if (_attackerAnimName != null && _shouldPlayAnimation)
				{
					await CreatureCmd.TriggerAnim(_visualAttacker ?? Attacker, _attackerAnimName, _attackerAnimDelay);
				}
				if (_afterAttackerAnim != null)
				{
					await _afterAttackerAnim();
				}
			}
			if (HitSfx != null)
			{
				SfxCmd.Play(HitSfx);
			}
			else if (TmpHitSfx != null)
			{
				NDebugAudioManager.Instance?.Play(TmpHitSfx);
			}
			Creature singleTarget;
			if (!IsRandomlyTargeted)
			{
				singleTarget = ((validTargets.Count != 1) ? null : validTargets[0]);
			}
			else
			{
				if (!_doesRandomTargetingAllowDuplicates)
				{
					validTargets = validTargets.Where((Creature c) => _results.All((DamageResult r) => r.Receiver != c)).ToList();
					if (validTargets.Count == 0)
					{
						throw new InvalidOperationException("No valid targets for attack with duplicates disallowed. If you're in a test, you probably need to add more enemies. If you're in real gameplay, something is wrong.");
					}
				}
				Rng combatTargets = (Attacker.Player ?? Attacker.PetOwner).RunState.Rng.CombatTargets;
				singleTarget = combatTargets.NextItem(validTargets);
			}
			if (_waitBeforeHit.Any((float w) => w > 0f))
			{
				await Cmd.CustomScaledWait(_waitBeforeHit[0], _waitBeforeHit[1]);
			}
			foreach (Func<Creature, Node2D> customHitVfxNode in _customHitVfxNodes)
			{
				if (singleTarget != null)
				{
					NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(customHitVfxNode(singleTarget));
					continue;
				}
				foreach (Creature item in validTargets)
				{
					NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(customHitVfxNode(item));
				}
			}
			if (HitVfx != null)
			{
				if (singleTarget != null)
				{
					if (_spawnVfxOnCreatureCenter)
					{
						VfxCmd.PlayOnCreatureCenter(singleTarget, HitVfx);
					}
					else
					{
						VfxCmd.PlayOnCreature(singleTarget, HitVfx);
					}
				}
				else if (_spawnVfxOnEachCreature)
				{
					if (_spawnVfxOnCreatureCenter)
					{
						VfxCmd.PlayOnCreatureCenters(validTargets, HitVfx);
					}
					else
					{
						VfxCmd.PlayOnCreatures(validTargets, HitVfx);
					}
				}
				else
				{
					VfxCmd.PlayOnSide(Attacker.Side.GetOppositeSide(), HitVfx, combatState);
				}
			}
			if (_beforeDamage != null)
			{
				await _beforeDamage();
			}
			AddResultsInternal(await CreatureCmd.Damage(amount: (_calculatedDamageVar == null) ? _damagePerHit : _calculatedDamageVar.Calculate(singleTarget), choiceContext: choiceContext ?? new BlockingPlayerChoiceContext(), targets: (singleTarget != null) ? ((IEnumerable<Creature>)new List<Creature>(1) { singleTarget }) : ((IEnumerable<Creature>)validTargets), props: DamageProps, dealer: Attacker, cardSource: ModelSource as CardModel));
		}
		CombatManager.Instance.History.CreatureAttacked(combatState, Attacker, _results.ToList());
		await Hook.AfterAttack(combatState, this);
		return this;
	}

	public void IncrementHitsInternal()
	{
		_hitCount++;
	}

	public void AddResultsInternal(IEnumerable<DamageResult> results)
	{
		_results.AddRange(results);
	}
}
