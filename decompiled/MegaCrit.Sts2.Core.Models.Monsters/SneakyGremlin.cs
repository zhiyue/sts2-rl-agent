using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class SneakyGremlin : MonsterModel
{
	private const string _wakeUpTrigger = "WakeUpTrigger";

	private bool _isAwake;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 11, 10);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 15, 14);

	private int TackleDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Insect;

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/gremlin_merc/sneaky_gremlin_attack";

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/gremlin_merc/sneaky_gremlin_die";

	private bool IsAwake
	{
		get
		{
			return _isAwake;
		}
		set
		{
			AssertMutable();
			_isAwake = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SPAWNED_MOVE", SpawnedMove, new StunIntent());
		MoveState moveState2 = (MoveState)(moveState.FollowUpState = new MoveState("TACKLE_MOVE", TackleMove, new SingleAttackIntent(TackleDamage)));
		moveState2.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task SpawnedMove(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(base.Creature, "WakeUpTrigger", 0.8f);
		IsAwake = true;
	}

	private async Task TackleMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(TackleDamage).FromMonster(this).WithAttackerAnim("Attack", 0.1f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("awake_loop", isLooping: true);
		AnimState animState2 = new AnimState("spawn");
		AnimState animState3 = new AnimState("attack");
		AnimState nextState = new AnimState("stunned_loop", isLooping: true);
		AnimState animState4 = new AnimState("wake_up");
		AnimState animState5 = new AnimState("hurt_stunned");
		AnimState animState6 = new AnimState("hurt_awake");
		AnimState state = new AnimState("die");
		animState2.NextState = nextState;
		animState5.NextState = nextState;
		animState6.NextState = animState;
		animState4.NextState = animState;
		animState3.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState2, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("WakeUpTrigger", animState4);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState6, () => IsAwake);
		creatureAnimator.AddAnyState("Hit", animState5, () => !IsAwake);
		return creatureAnimator;
	}
}
