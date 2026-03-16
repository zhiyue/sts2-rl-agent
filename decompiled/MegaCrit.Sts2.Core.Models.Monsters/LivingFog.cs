using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class LivingFog : MonsterModel
{
	private int _bloatAmount = 1;

	private const string _spawnBombTrigger = "SpawnBomb";

	private const string _attackBlowSfx = "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_attack_blow";

	private const string _summonSfx = "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_summon";

	private const string _appearsSfx = "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_minion_appear";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 82, 80);

	public override int MaxInitialHp => MinInitialHp;

	private int AdvancedGasDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	private int BloatDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);

	private int SuperGasBlastDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	private int BloatAmount
	{
		get
		{
			return _bloatAmount;
		}
		set
		{
			AssertMutable();
			_bloatAmount = value;
		}
	}

	public override bool ShouldFadeAfterDeath => false;

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_die";

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("ADVANCED_GAS_MOVE", AdvancedGasMove, new SingleAttackIntent(AdvancedGasDamage), new CardDebuffIntent());
		MoveState moveState2 = new MoveState("BLOAT_MOVE", BloatMove, new SingleAttackIntent(BloatDamage), new SummonIntent());
		MoveState moveState3 = new MoveState("SUPER_GAS_BLAST_MOVE", SuperGasBlastMove, new SingleAttackIntent(SuperGasBlastDamage));
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(moveState3);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task AdvancedGasMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(AdvancedGasDamage).FromMonster(this).WithAttackerAnim("Cast", 1.25f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_attack_blow")
			.WithHitVfxNode((Creature _) => NGaseousImpactVfx.Create(CombatSide.Player, base.CombatState, new Color("#402f45")))
			.Execute(null);
		await PowerCmd.Apply<SmoggyPower>(targets, 1m, base.Creature, null);
	}

	private async Task BloatMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/living_fog/living_fog_summon");
		await CreatureCmd.TriggerAnim(base.Creature, "SpawnBomb", 0.35f);
		for (int i = 0; i < BloatAmount; i++)
		{
			string nextSlot = base.CombatState.Encounter.GetNextSlot(base.CombatState);
			if (nextSlot != "")
			{
				SfxCmd.Play("event:/sfx/enemy/enemy_attacks/living_fog/living_fog_minion_appear");
				await CreatureCmd.Add<GasBomb>(base.CombatState, nextSlot);
			}
		}
		BloatAmount = Math.Min(BloatAmount + 1, 5);
		await DamageCmd.Attack(BloatDamage).FromMonster(this).WithAttackerAnim("Attack", 0.1f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_attack_blow")
			.WithHitVfxNode((Creature _) => NGaseousImpactVfx.Create(CombatSide.Player, base.CombatState, new Color("#402f45")))
			.Execute(null);
	}

	private async Task SuperGasBlastMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SuperGasBlastDamage).FromMonster(this).WithAttackerAnim("Attack", 0.1f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_attack_blow")
			.WithHitVfxNode((Creature _) => NGaseousImpactVfx.Create(CombatSide.Player, base.CombatState, new Color("#402f45")))
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("debuff");
		AnimState animState3 = new AnimState("spawn_bomb");
		AnimState animState4 = new AnimState("attack");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		animState3.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		creatureAnimator.AddAnyState("SpawnBomb", animState3);
		return creatureAnimator;
	}
}
