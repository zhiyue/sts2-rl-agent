using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class OwlMagistrate : MonsterModel
{
	private const int _peckAssaultRepeat = 6;

	private const string _attackPeckAnimId = "attack_peck";

	private const string _takeOffTrigger = "TakeOff";

	private const string _attackPeckSfx = "event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_attack_peck";

	private const string _attackDiveSfx = "event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_attack_dive";

	private const string _takeOffSfx = "event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_take_off";

	private const string _flyLoopSfx = "event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_fly_loop";

	private bool _isFlying;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 243, 234);

	public override int MaxInitialHp => MinInitialHp;

	private int VerdictDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 36, 33);

	private int ScrutinyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 16);

	private int PeckAssaultDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 4);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	public override string BestiaryAttackAnimId => "attack_peck";

	public override string HurtSfx
	{
		get
		{
			if (!IsFlying)
			{
				return "event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_hurt";
			}
			return "event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_hurt_flying";
		}
	}

	public override string DeathSfx
	{
		get
		{
			if (!IsFlying)
			{
				return "event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_die";
			}
			return "event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_die_flying";
		}
	}

	private bool IsFlying
	{
		get
		{
			return _isFlying;
		}
		set
		{
			AssertMutable();
			_isFlying = value;
		}
	}

	public override void BeforeRemovedFromRoom()
	{
		SfxCmd.StopLoop("event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_fly_loop");
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("MAGISTRATE_SCRUTINY", MagistrateScrutinyMove, new SingleAttackIntent(ScrutinyDamage));
		MoveState moveState2 = new MoveState("PECK_ASSAULT", PeckAssaultMove, new MultiAttackIntent(PeckAssaultDamage, 6));
		MoveState moveState3 = new MoveState("JUDICIAL_FLIGHT", JudicialFlightMove, new BuffIntent());
		MoveState moveState4 = new MoveState("VERDICT", VerdictMove, new SingleAttackIntent(VerdictDamage), new DebuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task MagistrateScrutinyMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ScrutinyDamage).FromMonster(this).WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_attack_peck")
			.WithAttackerAnim("Attack", 0.5f)
			.WithHitFx("vfx/vfx_gaze")
			.Execute(null);
	}

	private async Task PeckAssaultMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(PeckAssaultDamage).WithHitCount(6).FromMonster(this)
			.WithAttackerAnim("Attack", 0.5f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_attack_peck")
			.OnlyPlayAnimOnce()
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task JudicialFlightMove(IReadOnlyList<Creature> targets)
	{
		IsFlying = true;
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_take_off");
		await CreatureCmd.TriggerAnim(base.Creature, "TakeOff", 0f);
		await Cmd.Wait(1.25f);
		await PowerCmd.Apply<SoarPower>(base.Creature, 1m, base.Creature, null);
		SfxCmd.PlayLoop("event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_fly_loop");
	}

	private async Task VerdictMove(IReadOnlyList<Creature> targets)
	{
		IsFlying = false;
		SfxCmd.StopLoop("event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_fly_loop");
		await DamageCmd.Attack(VerdictDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
			.WithHitFx("vfx/vfx_attack_slash", "event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_attack_dive")
			.Execute(null);
		await PowerCmd.Apply<VulnerablePower>(targets, 4m, base.Creature, null);
		await PowerCmd.Remove<SoarPower>(base.Creature);
		await Cmd.Wait(1f);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("attack_peck");
		AnimState animState3 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState4 = new AnimState("take_off");
		AnimState animState5 = new AnimState("fly_loop", isLooping: true)
		{
			BoundsContainer = "FlyingBounds"
		};
		AnimState animState6 = new AnimState("attack_dive")
		{
			BoundsContainer = "IdleBounds"
		};
		AnimState animState7 = new AnimState("hurt_flying");
		AnimState state2 = new AnimState("die_flying");
		animState.AddBranch("Attack", animState2);
		animState.AddBranch("Hit", animState3);
		animState.AddBranch("Dead", state);
		animState2.NextState = animState;
		animState2.AddBranch("Hit", animState3);
		animState2.AddBranch("Dead", state);
		animState3.NextState = animState;
		animState3.AddBranch("Attack", animState2);
		animState3.AddBranch("Dead", state);
		animState3.AddBranch("Hit", animState3);
		animState4.NextState = animState5;
		animState4.AddBranch("Hit", animState7);
		animState4.AddBranch("Dead", state2);
		animState5.AddBranch("Attack", animState6);
		animState5.AddBranch("Hit", animState7);
		animState5.AddBranch("Dead", state2);
		animState7.NextState = animState5;
		animState7.AddBranch("Attack", animState6);
		animState7.AddBranch("Dead", state2);
		animState6.NextState = animState;
		animState6.AddBranch("Attack", animState2);
		animState6.AddBranch("Dead", state);
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("TakeOff", animState4);
		return creatureAnimator;
	}
}
