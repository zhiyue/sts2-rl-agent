using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using MegaCrit.Sts2.Core.Nodes.Vfx.Backgrounds;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Rocket : MonsterModel
{
	private const string _attackSlamSfx = "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_attack_slam";

	private const string _attackSnapSfx = "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_attack_snap";

	private const string _attackRegrowSfx = "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_regrow";

	private const string _buffSfxLoop = "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_buff";

	private const string _rocketSfx = "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_rocket";

	private NKaiserCrabBossBackground? _background;

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_die";

	public override bool ShouldFadeAfterDeath => false;

	public override bool ShouldDisappearFromDoom => false;

	public override float DeathAnimLengthOverride => 2.5f;

	private NKaiserCrabBossBackground Background
	{
		get
		{
			AssertMutable();
			if (_background == null)
			{
				Control control = NCombatRoom.Instance?.Background ?? NBestiary.Instance?.Layout ?? throw new InvalidOperationException();
				_background = control.GetNode<NKaiserCrabBossBackground>("%KaiserCrab");
			}
			return _background;
		}
	}

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 209, 199);

	public override int MaxInitialHp => MinInitialHp;

	private int TargetingReticleDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	private int PrecisionBeamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 18);

	private int LaserDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 35, 31);

	private int ChargeUpStrengthGain => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("TARGETING_RETICLE_MOVE", TargetingReticleMove, new SingleAttackIntent(TargetingReticleDamage));
		MoveState moveState2 = new MoveState("PRECISION_BEAM_MOVE", PrecisionBeamMove, new SingleAttackIntent(PrecisionBeamDamage));
		MoveState moveState3 = new MoveState("CHARGE_UP_MOVE", ChargeUpMove, new BuffIntent());
		MoveState moveState4 = new MoveState("LASER_MOVE", LaserMove, new SingleAttackIntent(LaserDamage));
		MoveState moveState5 = new MoveState("RECHARGE_MOVE", RechargeMove, new SleepIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState5;
		moveState5.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		list.Add(moveState5);
		return new MonsterMoveStateMachine(list, moveState);
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<SurroundedPower>(new ThrowingPlayerChoiceContext(), base.CombatState.GetOpponentsOf(base.Creature), 1m, base.Creature, null);
		await PowerCmd.Apply<BackAttackRightPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
		await PowerCmd.Apply<CrabRagePower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
		Background.SetVisible(visible: true);
	}

	public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
	{
		if (creature != base.Creature || delta >= 0m)
		{
			return Task.CompletedTask;
		}
		Background.PlayHurtAnim(NKaiserCrabBossBackground.ArmSide.Right);
		return Task.CompletedTask;
	}

	public override Task BeforeDeath(Creature creature)
	{
		if (creature != base.Creature)
		{
			return Task.CompletedTask;
		}
		NAudioManager.Instance.PlayOneShot(DeathSfx);
		Background.PlayArmDeathAnim(NKaiserCrabBossBackground.ArmSide.Right);
		if (CombatManager.Instance.IsOverOrEnding)
		{
			Background.PlayBodyDeathAnim();
			NRunMusicController.Instance?.UpdateMusicParameter("kaiser_crab_progress", 5f);
		}
		else
		{
			NRunMusicController.Instance?.UpdateMusicParameter("kaiser_crab_progress", 1f);
		}
		return Task.CompletedTask;
	}

	public override void BeforeRemovedFromRoom()
	{
		SfxCmd.StopLoop(base.Creature, "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_buff");
	}

	private async Task TargetingReticleMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_attack_snap");
		await Background.PlayAttackAnim(NKaiserCrabBossBackground.ArmSide.Right, "attack", 0.35f);
		await DamageCmd.Attack(TargetingReticleDamage).FromMonster(this).WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task PrecisionBeamMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_attack_slam");
		await Background.PlayAttackAnim(NKaiserCrabBossBackground.ArmSide.Right, "attack_med", 0.5f);
		await DamageCmd.Attack(PrecisionBeamDamage).FromMonster(this).WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
			.WithHitVfxSpawnedAtBase()
			.Execute(null);
	}

	private async Task ChargeUpMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.PlayLoop(base.Creature, "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_buff");
		await Background.PlayRightSideChargeUpAnim(0.7f);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), base.Creature, ChargeUpStrengthGain, base.Creature, null);
	}

	private async Task LaserMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.StopLoop(base.Creature, "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_buff");
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_rocket");
		await Background.PlayRightSideHeavy(0.5f);
		await DamageCmd.Attack(LaserDamage).FromMonster(this).WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task RechargeMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_regrow");
		await Background.PlayRightRecharge(0.5f);
	}
}
