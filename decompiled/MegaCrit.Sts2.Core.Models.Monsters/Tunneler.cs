using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Tunneler : MonsterModel
{
	public const string biteMoveId = "BITE_MOVE";

	public const string stillDizzyMoveId = "DIZZY_MOVE";

	private const string _burrowedAttackTrigger = "BurrowAttack";

	public const string unburrowAttackTrigger = "UnburrowAttack";

	private const string _burrowTrigger = "Burrow";

	private const string _burrowSfx = "event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_burrow";

	private const string _hiddenBurrowAttackSfx = "event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_hidden_attack";

	private const string _unburrowSfx = "event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_unburrow_attack";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 92, 87);

	public override int MaxInitialHp => MinInitialHp;

	public override string TakeDamageSfx => "event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_hurt";

	private int BiteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 13);

	private int BlockGain => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 37, 32);

	private int BelowDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 23);

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("BITE_MOVE", BiteMove, new SingleAttackIntent(BiteDamage));
		MoveState moveState2 = new MoveState("BURROW_MOVE", BurrowMove, new BuffIntent(), new DefendIntent());
		MoveState moveState3 = new MoveState("BELOW_MOVE_1", BelowMove, new SingleAttackIntent(BelowDamage));
		MoveState moveState4 = new MoveState("DIZZY_MOVE", StillDizzyMove, new StunIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState3;
		moveState4.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task BiteMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(BiteDamage).FromMonster(this).WithAttackerAnim("Attack", 0.25f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task BurrowMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_burrow");
		await CreatureCmd.TriggerAnim(base.Creature, "Burrow", 0.25f);
		await PowerCmd.Apply<BurrowedPower>(base.Creature, 1m, base.Creature, null);
		await CreatureCmd.GainBlock(base.Creature, BlockGain, ValueProp.Move, null);
	}

	private async Task BelowMove(IReadOnlyList<Creature> targets)
	{
		if (TestMode.IsOff)
		{
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(base.Creature);
			Node2D specialNode = creatureNode.GetSpecialNode<Node2D>("Visuals/SpineBoneNode");
			if (specialNode != null)
			{
				specialNode.Position = Vector2.Right * (NCombatRoom.Instance.GetCreatureNode(targets[0]).GlobalPosition.X - creatureNode.GlobalPosition.X) * 3f;
			}
			SfxCmd.Play("event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_hidden_attack");
			await CreatureCmd.TriggerAnim(base.Creature, "BurrowAttack", 0.25f);
			await Cmd.Wait(1f);
		}
		await DamageCmd.Attack(BelowDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private Task StillDizzyMove(IReadOnlyList<Creature> targets)
	{
		return Task.CompletedTask;
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState state = new AnimState("die");
		AnimState animState2 = new AnimState("hurt");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("burrow");
		AnimState nextState = new AnimState("hidden_loop", isLooping: true);
		AnimState animState5 = new AnimState("hidden_attack");
		AnimState state2 = new AnimState("hidden_die");
		AnimState animState6 = new AnimState("unburrow_attack");
		animState4.NextState = nextState;
		animState5.NextState = nextState;
		animState3.NextState = animState;
		animState6.NextState = animState;
		animState2.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("UnburrowAttack", animState6);
		creatureAnimator.AddAnyState("Hit", animState2, () => !base.Creature.HasPower<BurrowedPower>());
		creatureAnimator.AddAnyState("Dead", state, () => !base.Creature.HasPower<BurrowedPower>());
		creatureAnimator.AddAnyState("Dead", state2, () => base.Creature.HasPower<BurrowedPower>());
		creatureAnimator.AddAnyState("Attack", animState3, () => !base.Creature.HasPower<BurrowedPower>());
		creatureAnimator.AddAnyState("BurrowAttack", animState5, () => base.Creature.HasPower<BurrowedPower>());
		creatureAnimator.AddAnyState("Burrow", animState4);
		return creatureAnimator;
	}
}
