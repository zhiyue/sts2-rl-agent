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
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Commands.Builders;

/// <summary>
/// A "builder" command that allows you to chain method calls to build up the property of an attack.
/// This pattern is very similar to how Godot Tweens are built up.
/// </summary>
public class AttackCommand
{
	private enum SourceType
	{
		None,
		Card,
		Monster
	}

	/// <summary>
	/// The amount of damage that this attack will deal on each hit.
	/// This will be ignored if <see cref="F:MegaCrit.Sts2.Core.Commands.Builders.AttackCommand._calculatedDamageVar" /> is set.
	/// </summary>
	private readonly decimal _damagePerHit;

	/// <summary>
	/// Dynamic var that calculates how much damage each hit does.
	/// Used for cards that do dynamic amounts of damage based on combat state, like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.PerfectedStrike" />.
	/// Settings this will cause <see cref="F:MegaCrit.Sts2.Core.Commands.Builders.AttackCommand._damagePerHit" /> to be ignored.
	/// </summary>
	private readonly CalculatedDamageVar? _calculatedDamageVar;

	/// <summary>
	/// The number of hits this attack should do.
	/// The actual number of hits may be modified by hooks.
	/// If you need to check the final number of hits, check <see cref="P:MegaCrit.Sts2.Core.Commands.Builders.AttackCommand.Results" /> after the attack has been executed.
	/// </summary>
	private int _hitCount = 1;

	/// <summary>
	/// The source of this attack.
	/// Without this, we have to glean it from some combination of Attacker and ModelSource, which can get tricky in
	/// cases like Osty attacks.
	/// </summary>
	private SourceType _sourceType;

	private ICombatState? _combatState;

	/// <summary>
	/// Set for single target attacks. Is null for AOE attacks.
	/// </summary>
	private Creature? _singleTarget;

	private bool _spawnVfxOnEachCreature;

	private bool _spawnVfxOnCreatureCenter = true;

	private bool _doesRandomTargetingAllowDuplicates = true;

	private bool _shouldPlayAnimation = true;

	private readonly List<List<DamageResult>> _results = new List<List<DamageResult>>();

	private string? _attackerAnimName;

	private float _attackerAnimDelay;

	private Creature? _visualAttacker;

	private bool _playOnEveryHit = true;

	private string? _attackerVfx;

	private string? _attackerSfx;

	private string? _tmpAttackerSfx;

	/// <summary>
	/// fast, standard
	/// </summary>
	private readonly float[] _waitBeforeHit = new float[2] { -1f, -1f };

	private readonly List<Func<Node2D?>> _customAttackerVfxNodes = new List<Func<Node2D>>();

	private readonly List<Func<Creature, Node2D?>> _customHitVfxNodes = new List<Func<Creature, Node2D>>();

	private Func<Task>? _afterAttackerAnim;

	private Func<Task>? _beforeDamage;

	/// <summary>
	/// The creature performing this attack.
	/// It's safe to assume this is non-null once the attack is being executed.
	/// </summary>
	public Creature? Attacker { get; private set; }

	/// <summary>
	/// The model that this attack comes from, such as an attack card.
	/// </summary>
	public AbstractModel? ModelSource { get; private set; }

	public CombatSide TargetSide { get; private set; }

	/// <summary>
	/// The ValueProps of the damage that this attack deals.
	/// </summary>
	public ValueProp DamageProps { get; private set; } = ValueProp.Move;

	/// <summary>
	/// Whether this attack is targeting a single creature.
	/// </summary>
	public bool IsSingleTargeted => _singleTarget != null;

	/// <summary>
	/// Whether this attack is targeting multiple creatures.
	/// </summary>
	public bool IsMultiTargeted => _combatState != null;

	/// <summary>
	/// Whether this attack is randomly targeted.
	/// </summary>
	public bool IsRandomlyTargeted { get; private set; }

	/// <summary>
	/// The resulting instances of damage from this attack.
	/// Each inner list is from an individual hit from this attack (so if we have a multi-attack, we have multiple lists)
	/// This will start out empty, and will be populated after <see cref="M:MegaCrit.Sts2.Core.Commands.Builders.AttackCommand.Execute(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext)" /> finishes running.
	/// </summary>
	public IEnumerable<List<DamageResult>> Results => _results;

	public string? HitSfx { get; private set; }

	public string? TmpHitSfx { get; private set; }

	public string? HitVfx { get; private set; }

	/// <summary>
	/// The creatures being targeted by this attack.
	/// For random attacks, this contains all possible targets, and for custom attacks using
	/// <see cref="T:MegaCrit.Sts2.Core.Commands.Builders.AttackContext" />, this will be empty.
	/// After the attack has been executed, you can check <see cref="P:MegaCrit.Sts2.Core.Commands.Builders.AttackCommand.Results" /> to see which targets were actually hit.
	/// </summary>
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

	/// <summary>
	/// Create a new attack command.
	/// </summary>
	/// <param name="damagePerHit">The amount of damage this attack should deal on each hit.</param>
	public AttackCommand(decimal damagePerHit)
	{
		_damagePerHit = damagePerHit;
		_calculatedDamageVar = null;
	}

	/// <summary>
	/// Create a new attack command
	/// </summary>
	/// <param name="calculatedDamageVar">
	/// Dynamic var that calculates how much damage each hit does.
	/// Used for cards that do dynamic amounts of damage based on combat state, like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.PerfectedStrike" />.
	/// </param>
	public AttackCommand(CalculatedDamageVar calculatedDamageVar)
	{
		_damagePerHit = -1m;
		_calculatedDamageVar = calculatedDamageVar;
	}

	/// <summary>
	/// Set the attack to come from the specified card.
	/// This also automatically sets the attacker as the card's owner, and the attacker animation name/delay to the card
	/// owner's defaults.
	/// </summary>
	/// <param name="card">Card that the attack came from.</param>
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

	/// <summary>
	/// Set the attack to come from the specified monster.
	/// </summary>
	/// <param name="monster">Monster that the attack came from.</param>
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

	/// <summary>
	/// Set the attack to target the specified creature.
	/// </summary>
	/// <param name="target">Creature for the attack to target.</param>
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

	/// <summary>
	/// Set the attack to target the opponents of the Attacker.
	/// Differs from TargetingAll because the list of targets is refreshed between every hit
	/// meaning that new creatures added to the combat will become valid targets
	/// </summary>
	/// <param name="combatState">Combat state that we will be pulling the opponents from</param>
	public AttackCommand TargetingAllOpponents(ICombatState combatState)
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

	/// <summary>
	/// Sets the attack to target random opponents of the attacker.
	/// A new random target will be chosen on each hit.
	/// </summary>
	/// <param name="combatState">Combat state that we will be pulling the opponents from</param>
	/// <param name="allowDuplicates">
	/// Whether the same target can be hit multiple times.
	/// In real gameplay, we pretty much always want this to be true, but we pass false sometimes for testing.
	/// </param>
	public AttackCommand TargetingRandomOpponents(ICombatState combatState, bool allowDuplicates = true)
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

	/// <summary>
	/// Set the attack to deal unpowered damage.
	///
	/// WARNING: 99% of the time, when unpowered damage is dealt, it shouldn't be considered an
	/// attack (like via <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Burn" />). However, occasionally we want to do a real attack
	/// that skips powers (like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Omnislice" />), so this is useful there.
	/// </summary>
	public AttackCommand Unpowered()
	{
		DamageProps |= ValueProp.Unpowered;
		return this;
	}

	/// <summary>
	/// Set the attack to play the specified animation on the attacker.
	/// </summary>
	/// <param name="animName">Name of the animation trigger.</param>
	/// <param name="delay">Amount of time to wait for the animation to complete before proceeding.</param>
	/// <param name="visualAttacker">
	/// Optional custom creature to visually display as the attacker in this animation.
	/// Good for attacks like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.ByrdSwoop" /> where we want a pet to animate for the attack, even though the
	/// actual attack is coming from the attack's owner.
	/// If this is left null, the attack's owning creature will play the animation.
	/// </param>
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

	/// <summary>
	/// Logic to execute after the attacker animation plays. Good for one-offs.
	/// Note: If you find yourself calling this with the same logic in many different attacks, consider making a
	/// first-class builder method for the logic instead.
	/// </summary>
	public AttackCommand AfterAttackerAnim(Func<Task> afterAttackerAnim)
	{
		_afterAttackerAnim = afterAttackerAnim;
		return this;
	}

	/// <summary>
	/// Set the attack to play the specified VFX/SFX on attacker.
	/// </summary>
	/// <param name="vfx">File path of the VFX.</param>
	/// <param name="sfx">File path of the SFX.</param>
	/// <param name="tmpSfx">
	/// Temporary SFX file path. If the attack uses temporary non-FMOD SFX, pass this instead of sfx using keyword args.
	/// </param>
	public AttackCommand WithAttackerFx(string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		_attackerVfx = vfx;
		_attackerSfx = sfx;
		_tmpAttackerSfx = tmpSfx;
		return this;
	}

	/// <summary>
	/// Set the attack to add the specified custom VFX node to the combat VFX container when the owner attacks.
	/// </summary>
	public AttackCommand WithAttackerFx(Func<Node2D?> createAttackerVfx)
	{
		_customAttackerVfxNodes.Add(createAttackerVfx);
		return this;
	}

	/// <summary>
	/// Set a wait time before each hit and hit VFX/SFX.
	/// Uses the same wait time signature as <see cref="M:MegaCrit.Sts2.Core.Commands.Cmd.CustomScaledWait(System.Single,System.Single,System.Boolean,System.Threading.CancellationToken)" />.
	/// </summary>
	public AttackCommand WithWaitBeforeHit(float fastSeconds, float standardSeconds)
	{
		_waitBeforeHit[0] = fastSeconds;
		_waitBeforeHit[1] = standardSeconds;
		return this;
	}

	/// <summary>
	/// Set the attack to play the specified VFX/SFX on the target(s) when they're hit.
	/// </summary>
	/// <param name="vfx">File path of the VFX.</param>
	/// <param name="sfx">File path of the SFX.</param>
	/// <param name="tmpSfx">
	/// Temporary SFX file path. If the attack uses temporary non-FMOD SFX, pass this instead of sfx using keyword args.
	/// </param>
	public AttackCommand WithHitFx(string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		HitVfx = vfx;
		HitSfx = sfx;
		TmpHitSfx = tmpSfx;
		return this;
	}

	/// <summary>
	/// If this attack hits multiple creatures, this sets the hit VFX to spawn on each creature rather than on the side.
	/// </summary>
	public AttackCommand SpawningHitVfxOnEachCreature()
	{
		_spawnVfxOnEachCreature = true;
		return this;
	}

	/// <summary>
	/// If there is a hit VFX set, this makes it spawn at the creature's base rather than its center.
	/// </summary>
	public AttackCommand WithHitVfxSpawnedAtBase()
	{
		_spawnVfxOnCreatureCenter = false;
		return this;
	}

	/// <summary>
	/// Set the attack to add the specified custom VFX node to the combat VFX container when the target(s) are hit.
	/// </summary>
	public AttackCommand WithHitVfxNode(Func<Creature, Node2D?> createHitVfxNode)
	{
		_customHitVfxNodes.Add(createHitVfxNode);
		return this;
	}

	/// <summary>
	/// Set the attack to only play animations/VFX/SFX once, even on a multi-hit.
	/// Good for situations where a single bespoke attack animation illustrates multiple hits.
	/// </summary>
	public AttackCommand OnlyPlayAnimOnce()
	{
		_playOnEveryHit = false;
		return this;
	}

	/// <summary>
	/// Set the number of hits this attack should do.
	/// </summary>
	/// <param name="hitCount">The number of times this attack should hit.</param>
	public AttackCommand WithHitCount(int hitCount)
	{
		_hitCount = hitCount;
		return this;
	}

	/// <summary>
	/// Logic to execute before each instance of damage is dealt. Good for one-offs.
	/// Note: If you find yourself calling this with the same logic in many different attacks, consider making a
	/// first-class builder method for the logic instead.
	/// </summary>
	public AttackCommand BeforeDamage(Func<Task> beforeDamage)
	{
		_beforeDamage = beforeDamage;
		return this;
	}

	/// <summary>
	/// Creates an attack context for grouping multiple damage calls as a single attack.
	/// Use with `await using` to automatically call BeforeAttack and AfterAttack hooks.
	/// </summary>
	/// <param name="combatState">The current combat state</param>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="cardSource">The card that is the source of this attack</param>
	/// <returns>An AttackContext ready for use with await using</returns>
	public static async Task<AttackContext> CreateContextAsync(ICombatState combatState, PlayerChoiceContext choiceContext, CardModel cardSource)
	{
		return await AttackContext.CreateAsync(combatState, choiceContext, cardSource);
	}

	/// <summary>
	/// Execute this attack.
	/// If you forget to set some required values (like attacker or target), this will error.
	/// </summary>
	/// <param name="choiceContext">
	/// The context that is signalled in the event of a player choice. If null, a
	/// <see cref="T:MegaCrit.Sts2.Core.GameActions.Multiplayer.BlockingPlayerChoiceContext" /> is used. This should only be done when a monster is attacking.
	/// </param>
	public async Task<AttackCommand> Execute(PlayerChoiceContext? choiceContext)
	{
		ICombatState combatState = Attacker?.CombatState;
		if (Attacker == null)
		{
			throw new InvalidOperationException("No attacker set.");
		}
		if (CombatManager.Instance.IsOverOrEnding && (combatState == null || combatState.IsLiveCombat()))
		{
			return this;
		}
		if (combatState == null)
		{
			throw new InvalidOperationException("No combat state even though combat is not over.");
		}
		if (Attacker.IsDead)
		{
			return this;
		}
		if (!IsSingleTargeted && !IsMultiTargeted)
		{
			throw new InvalidOperationException("No targets set.");
		}
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
			if (validTargets.Count == 0 && combatState.IsLiveCombat())
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
					Attacker.GetVfxContainer()?.AddChildSafely(customAttackerVfxNode());
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
					validTargets = validTargets.Where((Creature c) => _results.SelectMany((List<DamageResult> r) => r).All((DamageResult r) => r.Receiver != c)).ToList();
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
					singleTarget.GetVfxContainer()?.AddChildSafely(customHitVfxNode(singleTarget));
					continue;
				}
				foreach (Creature item in validTargets)
				{
					item.GetVfxContainer()?.AddChildSafely(customHitVfxNode(item));
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
		CombatManager.Instance.History.CreatureAttacked(combatState, Attacker, _results.SelectMany((List<DamageResult> r) => r).ToList());
		await Hook.AfterAttack(combatState, choiceContext ?? new BlockingPlayerChoiceContext(), this);
		return this;
	}

	/// <summary>
	/// NEVER CALL THIS FROM MODELS, only in system code where you know exactly what you're doing.
	/// Increment the number of hits this attack should do.
	/// </summary>
	public void IncrementHitsInternal()
	{
		_hitCount++;
	}

	/// <summary>
	/// NEVER CALL THIS FROM MODELS, only in system code where you know exactly what you're doing.
	/// Add a set of DamageResults to the list of results for this attack.
	/// </summary>
	public void AddResultsInternal(IEnumerable<DamageResult> results)
	{
		_results.Add(results.ToList());
	}
}
