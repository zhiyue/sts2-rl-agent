using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class SlumberingBeetle : MonsterModel
{
	public const string wakeUpTrigger = "WakeUp";

	private const string _rolloutTrigger = "Rollout";

	public const string rolloutMoveId = "ROLL_OUT_MOVE";

	private const string _rollSfx = "event:/sfx/enemy/enemy_attacks/slumbering_beetle/slumbering_beetle_roll";

	public const string wakeUp = "event:/sfx/enemy/enemy_attacks/slumbering_beetle/slumbering_beetle_wake_up";

	public const string sleepLoop = "event:/sfx/enemy/enemy_attacks/slumbering_beetle/slumbering_beetle_sleep_loop";

	private bool _isAwake;

	private NSleepingVfx? _sleepingVfx;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 89, 86);

	public override int MaxInitialHp => MinInitialHp;

	private int RolloutDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);

	private int PlatingAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 18, 15);

	public bool IsAwake
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

	private NSleepingVfx? SleepingVfx
	{
		get
		{
			return _sleepingVfx;
		}
		set
		{
			AssertMutable();
			_sleepingVfx = value;
		}
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<PlatingPower>(base.Creature, PlatingAmount, base.Creature, null);
		await PowerCmd.Apply<SlumberPower>(base.Creature, 3m, base.Creature, null);
		SfxCmd.PlayLoop("event:/sfx/enemy/enemy_attacks/slumbering_beetle/slumbering_beetle_sleep_loop");
		Marker2D marker2D = NCombatRoom.Instance.GetCreatureNode(base.Creature)?.GetSpecialNode<Marker2D>("%SleepVfxPos");
		if (marker2D != null)
		{
			SleepingVfx = NSleepingVfx.Create(marker2D.GlobalPosition);
			marker2D.AddChildSafely(SleepingVfx);
			SleepingVfx.Position = Vector2.Zero;
		}
		base.Creature.Died += AfterDeath;
	}

	private void AfterDeath(Creature _)
	{
		base.Creature.Died -= AfterDeath;
		SleepingVfx?.Stop();
		SleepingVfx = null;
	}

	public async Task WakeUpMove(IReadOnlyList<Creature> _)
	{
		SfxCmd.StopLoop("event:/sfx/enemy/enemy_attacks/slumbering_beetle/slumbering_beetle_sleep_loop");
		SleepingVfx?.Stop();
		SleepingVfx = null;
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/slumbering_beetle/slumbering_beetle_wake_up");
		IsAwake = true;
		await CreatureCmd.TriggerAnim(base.Creature, "WakeUp", 0.6f);
		if (base.Creature.HasPower<PlatingPower>())
		{
			await PowerCmd.Remove(base.Creature.GetPower<PlatingPower>());
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SNORE_MOVE", SnoreMove, new SleepIntent());
		MoveState moveState2 = new MoveState("ROLL_OUT_MOVE", RolloutMove, new SingleAttackIntent(RolloutDamage), new BuffIntent());
		ConditionalBranchState conditionalBranchState = (ConditionalBranchState)(moveState.FollowUpState = new ConditionalBranchState("SNORE_NEXT"));
		conditionalBranchState.AddState(moveState, () => base.Creature.HasPower<SlumberPower>());
		conditionalBranchState.AddState(moveState2, () => !base.Creature.HasPower<SlumberPower>());
		moveState2.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(conditionalBranchState);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private Task SnoreMove(IReadOnlyList<Creature> targets)
	{
		return Task.CompletedTask;
	}

	private async Task RolloutMove(IReadOnlyList<Creature> targets)
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(base.Creature);
		if (nCreature != null)
		{
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(LocalContext.GetMe(base.CombatState).Creature);
			Node2D specialNode = nCreature.GetSpecialNode<Node2D>("Visuals/SpineBoneNode");
			if (specialNode != null)
			{
				specialNode.Position = Vector2.Left * (nCreature.GlobalPosition.X - creatureNode.GlobalPosition.X);
			}
		}
		await DamageCmd.Attack(RolloutDamage).FromMonster(this).WithAttackerAnim("Rollout", 0.5f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/slumbering_beetle/slumbering_beetle_roll")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState initialState = new AnimState("sleep_loop", isLooping: true);
		AnimState animState = new AnimState("wake_up");
		AnimState nextState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("roll");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState.NextState = nextState;
		animState3.NextState = nextState;
		animState2.NextState = nextState;
		animState4.NextState = nextState;
		animState5.NextState = nextState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(initialState, controller);
		creatureAnimator.AddAnyState("WakeUp", animState);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Rollout", animState4);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Hit", animState5, () => IsAwake);
		return creatureAnimator;
	}
}
