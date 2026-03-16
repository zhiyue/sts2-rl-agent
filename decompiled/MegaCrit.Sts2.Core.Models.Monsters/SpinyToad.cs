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

public sealed class SpinyToad : MonsterModel
{
	private const string _spikeTrigger = "Spiked";

	private const string _unSpikeTrigger = "Unspiked";

	private const string _attackHeavySfx = "event:/sfx/enemy/enemy_attacks/spiny_toad/spiny_toad_explode";

	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/spiny_toad/spiny_toad_protrude";

	private bool _isSpiny;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 121, 116);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 124, 119);

	private int LashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);

	private int ExplosionDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 25, 23);

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/spiny_toad/spiny_toad_lick";

	public bool IsSpiny
	{
		get
		{
			return _isSpiny;
		}
		set
		{
			AssertMutable();
			_isSpiny = value;
		}
	}

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/spiny_toad/spiny_toad_die";

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("PROTRUDING_SPIKES_MOVE", SpikesMove, new BuffIntent());
		MoveState moveState2 = new MoveState("SPIKE_EXPLOSION_MOVE", ExplosionMove, new SingleAttackIntent(ExplosionDamage));
		MoveState moveState3 = new MoveState("TONGUE_LASH_MOVE", LashMove, new SingleAttackIntent(LashDamage));
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState);
	}

	public override Task AfterAddedToRoom()
	{
		base.AfterAddedToRoom();
		return Task.CompletedTask;
	}

	private async Task SpikesMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/spiny_toad/spiny_toad_protrude");
		await CreatureCmd.TriggerAnim(base.Creature, "Spiked", 0.5f);
		IsSpiny = true;
		await PowerCmd.Apply<ThornsPower>(base.Creature, 5m, base.Creature, null);
	}

	private async Task ExplosionMove(IReadOnlyList<Creature> targets)
	{
		IsSpiny = false;
		await DamageCmd.Attack(ExplosionDamage).FromMonster(this).WithAttackerAnim("Unspiked", 0.7f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/spiny_toad/spiny_toad_explode")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await PowerCmd.Apply<ThornsPower>(base.Creature, -5m, base.Creature, null);
		await Cmd.Wait(1f);
	}

	private async Task LashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(LashDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState3 = new AnimState("protrude");
		AnimState animState4 = new AnimState("lick");
		AnimState animState5 = new AnimState("explode");
		AnimState animState6 = new AnimState("idle_naked_loop", isLooping: true);
		AnimState animState7 = new AnimState("hurt_naked");
		AnimState state2 = new AnimState("die_naked");
		animState6.AddBranch("Spiked", animState3);
		animState.AddBranch("Unspiked", animState5);
		animState3.NextState = animState;
		animState7.NextState = animState6;
		animState7.AddBranch("Spiked", animState3);
		animState2.NextState = animState;
		animState2.AddBranch("Unspiked", animState5);
		animState4.NextState = animState6;
		animState5.NextState = animState6;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState6, controller);
		creatureAnimator.AddAnyState("Attack", animState4);
		creatureAnimator.AddAnyState("Hit", animState7, () => !IsSpiny);
		creatureAnimator.AddAnyState("Hit", animState2, () => IsSpiny);
		creatureAnimator.AddAnyState("Dead", state2, () => !IsSpiny);
		creatureAnimator.AddAnyState("Dead", state, () => IsSpiny);
		return creatureAnimator;
	}
}
