using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Tunneler : MonsterModel
{
	private const string _dizzyMoveId = "DIZZY_MOVE";

	public const string biteMoveId = "BITE_MOVE";

	private const string _burrowedAttackTrigger = "BurrowAttack";

	private const string _burrowTrigger = "Burrow";

	private const string _stunTrigger = "Stun";

	private const string _wakeUpTrigger = "WakeUp";

	private const string _burrowSfx = "event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_burrow";

	private const string _hiddenBurrowAttackSfx = "event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_hidden_attack";

	private bool _isStunned;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 92, 87);

	public override int MaxInitialHp => MinInitialHp;

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_attack";

	public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_hurt";

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_die";

	private int BiteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 13);

	private int BlockGain => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 37, 32);

	private int BelowDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 23);

	public bool IsStunned
	{
		get
		{
			return _isStunned;
		}
		set
		{
			AssertMutable();
			_isStunned = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("BITE_MOVE", BiteMove, new SingleAttackIntent(BiteDamage));
		MoveState moveState2 = new MoveState("BURROW_MOVE", BurrowMove, new BuffIntent(), new DefendIntent());
		MoveState moveState3 = new MoveState("BELOW_MOVE", BelowMove, new SingleAttackIntent(BelowDamage));
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
		await PowerCmd.Apply<BurrowedPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
		await CreatureCmd.GainBlock(base.Creature, BlockGain, ValueProp.Move, null);
	}

	private async Task BelowMove(IReadOnlyList<Creature> targets)
	{
		if (TestMode.IsOff)
		{
			NCreature creatureNode = base.Creature.GetCreatureNode();
			Node2D node2D = creatureNode?.GetSpecialNode<Node2D>("Visuals/SpineBoneNode");
			if (node2D != null && creatureNode != null)
			{
				if (targets.Count > 0)
				{
					node2D.Position = Vector2.Right * (targets[0].GetCreatureNode().GlobalPosition.X - creatureNode.GlobalPosition.X) * 3f / creatureNode.Visuals.Scale;
				}
				else
				{
					node2D.Position = Vector2.Left * 400f;
				}
			}
			SfxCmd.Play("event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_hidden_attack");
			await CreatureCmd.TriggerAnim(base.Creature, "BurrowAttack", 0.25f);
			await Cmd.Wait(1f);
		}
		await DamageCmd.Attack(BelowDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	public async Task GetStunned()
	{
		IsStunned = true;
		await CreatureCmd.TriggerAnim(base.Creature, "Stun", 0.25f);
	}

	public async Task StillDizzyMove(IReadOnlyList<Creature> targets)
	{
		IsStunned = false;
		await CreatureCmd.TriggerAnim(base.Creature, "WakeUp", 0.25f);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState state = new AnimState("die");
		AnimState animState2 = new AnimState("hurt");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("stun");
		AnimState nextState = new AnimState("stunned_loop", isLooping: true);
		AnimState animState5 = new AnimState("stunned_hurt");
		AnimState animState6 = new AnimState("wake_up");
		AnimState animState7 = new AnimState("burrow");
		AnimState nextState2 = new AnimState("hidden_loop", isLooping: true);
		AnimState animState8 = new AnimState("hidden_attack");
		AnimState state2 = new AnimState("hidden_die");
		animState7.NextState = nextState2;
		animState8.NextState = nextState2;
		animState3.NextState = animState;
		animState2.NextState = animState;
		animState4.NextState = nextState;
		animState5.NextState = nextState;
		animState6.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Hit", animState2, () => !base.Creature.HasPower<BurrowedPower>() && !IsStunned);
		creatureAnimator.AddAnyState("Hit", animState5, () => !base.Creature.HasPower<BurrowedPower>() && IsStunned);
		creatureAnimator.AddAnyState("Dead", state, () => !base.Creature.HasPower<BurrowedPower>());
		creatureAnimator.AddAnyState("Dead", state2, () => base.Creature.HasPower<BurrowedPower>());
		creatureAnimator.AddAnyState("Attack", animState3, () => !base.Creature.HasPower<BurrowedPower>());
		creatureAnimator.AddAnyState("BurrowAttack", animState8, () => base.Creature.HasPower<BurrowedPower>());
		creatureAnimator.AddAnyState("Burrow", animState7);
		creatureAnimator.AddAnyState("Stun", animState4);
		creatureAnimator.AddAnyState("WakeUp", animState6);
		return creatureAnimator;
	}

	protected override bool ShouldShowMoveInBestiary(string moveStateId)
	{
		return moveStateId != "DIZZY_MOVE";
	}
}
