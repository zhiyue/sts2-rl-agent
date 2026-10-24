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
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class CrossbowRubyRaider : MonsterModel
{
	private const string _reloadTrigger = "Reload";

	private const string _reloadSfx = "event:/sfx/enemy/enemy_attacks/crossbow_ruby_raider/crossbow_ruby_raider_reload";

	private bool _isCrossbowReloaded;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 19, 18);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 22, 21);

	private int FireDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 14);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	private bool IsCrossbowReloaded
	{
		get
		{
			return _isCrossbowReloaded;
		}
		set
		{
			AssertMutable();
			_isCrossbowReloaded = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("FIRE_MOVE", FireMove, new SingleAttackIntent(FireDamage));
		MoveState moveState2 = (MoveState)(moveState.FollowUpState = new MoveState("RELOAD_MOVE", ReloadMove, new DefendIntent()));
		moveState2.FollowUpState = moveState;
		list.Add(moveState2);
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState2);
	}

	private async Task FireMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(FireDamage).FromMonster(this).WithAttackerAnim("Attack", 0.25f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		IsCrossbowReloaded = false;
	}

	private async Task ReloadMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/crossbow_ruby_raider/crossbow_ruby_raider_reload");
		await CreatureCmd.TriggerAnim(base.Creature, "Reload", 0.25f);
		await CreatureCmd.GainBlock(base.Creature, 3m, ValueProp.Move, null);
		IsCrossbowReloaded = true;
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState nextState = new AnimState("idle_loop", isLooping: true);
		AnimState animState = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState2 = new AnimState("attack");
		AnimState animState3 = new AnimState("hurt_empty");
		AnimState animState4 = new AnimState("idle_loop_empty", isLooping: true);
		AnimState animState5 = new AnimState("hurt_empty");
		AnimState state2 = new AnimState("die_empty");
		AnimState animState6 = new AnimState("reload");
		animState.NextState = nextState;
		animState2.NextState = animState4;
		animState3.NextState = animState4;
		animState5.NextState = animState4;
		animState6.NextState = nextState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState4, controller);
		creatureAnimator.AddAnyState("Attack", animState2);
		creatureAnimator.AddAnyState("Reload", animState6);
		creatureAnimator.AddAnyState("Hit", animState5, () => !IsCrossbowReloaded);
		creatureAnimator.AddAnyState("Hit", animState, () => IsCrossbowReloaded);
		creatureAnimator.AddAnyState("Dead", state2, () => !IsCrossbowReloaded);
		creatureAnimator.AddAnyState("Dead", state, () => IsCrossbowReloaded);
		return creatureAnimator;
	}
}
