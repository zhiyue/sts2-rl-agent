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

public sealed class TerrorEel : MonsterModel
{
	private const string _debuffSfx = "event:/sfx/enemy/enemy_attacks/terror_eel/terror_eel_debuff";

	private const string _attackMultiSfx = "event:/sfx/enemy/enemy_attacks/terror_eel/terror_eel_attack_multi";

	private const string _thrashMoveId = "ThrashMove";

	private const int _hpNormal = 140;

	private const int _hpTough = 150;

	private MoveState _terrorState;

	private const string _attackTripleTrigger = "AttackTripleTrigger";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 150, 140);

	public override int MaxInitialHp => MinInitialHp;

	private int ShriekAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 75, 70);

	private int CrashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);

	private int ThrashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	private int ThrashRepeat => 3;

	public MoveState TerrorState
	{
		get
		{
			return _terrorState;
		}
		private set
		{
			AssertMutable();
			_terrorState = value;
		}
	}

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<ShriekPower>(base.Creature, ShriekAmount, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("CRASH_MOVE", CrashMove, new SingleAttackIntent(CrashDamage));
		MoveState moveState2 = new MoveState("ThrashMove", ThrashMove, new MultiAttackIntent(ThrashDamage, ThrashRepeat), new BuffIntent());
		MoveState moveState3 = new MoveState("STUN_MOVE", StunMove, new StunIntent());
		TerrorState = new MoveState("TERROR_MOVE", TerrorMove, new DebuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState;
		moveState3.FollowUpState = TerrorState;
		TerrorState.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(TerrorState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task CrashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(CrashDamage).FromMonster(this).WithAttackerAnim("Attack", 0.2f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task ThrashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ThrashDamage).WithHitCount(ThrashRepeat).FromMonster(this)
			.WithAttackerAnim("AttackTripleTrigger", 0.25f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/terror_eel/terror_eel_attack_multi")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await PowerCmd.Apply<VigorPower>(base.Creature, 7m, base.Creature, null);
	}

	private Task StunMove(IReadOnlyList<Creature> targets)
	{
		return Task.CompletedTask;
	}

	private async Task TerrorMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/terror_eel/terror_eel_debuff");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0f);
		await Cmd.Wait(0.7f);
		VfxCmd.PlayOnCreatureCenter(base.Creature, "vfx/vfx_scream");
		await Cmd.CustomScaledWait(0.1f, 0.3f);
		await PowerCmd.Apply<VulnerablePower>(targets, 99m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("attack_triple");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState5.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("AttackTripleTrigger", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		return creatureAnimator;
	}
}
