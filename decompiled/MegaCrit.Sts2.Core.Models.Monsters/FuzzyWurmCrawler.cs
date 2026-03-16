using System.Collections.Generic;
using System.Linq;
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

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class FuzzyWurmCrawler : MonsterModel
{
	private const string _inhaleTrigger = "Inhale";

	private bool _isPuffed;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 58, 55);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 59, 57);

	private int AcidGoopDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 4);

	private bool IsPuffed
	{
		get
		{
			return _isPuffed;
		}
		set
		{
			AssertMutable();
			_isPuffed = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("FIRST_ACID_GOOP", AcidGoop, new SingleAttackIntent(AcidGoopDamage));
		MoveState moveState2 = new MoveState("ACID_GOOP", AcidGoop, new SingleAttackIntent(AcidGoopDamage));
		MoveState moveState3 = (MoveState)(moveState.FollowUpState = new MoveState("INHALE", Inhale, new BuffIntent()));
		moveState3.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task AcidGoop(IReadOnlyList<Creature> targets)
	{
		if (TestMode.IsOff)
		{
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(base.Creature);
			Node2D specialNode = creatureNode.GetSpecialNode<Node2D>("Visuals/SpineBoneNode");
			if (specialNode != null)
			{
				specialNode.Position = Vector2.Left * (creatureNode.GlobalPosition.X - NCombatRoom.Instance.GetCreatureNode(targets.First()).GlobalPosition.X);
			}
		}
		IsPuffed = false;
		await DamageCmd.Attack(AcidGoopDamage).FromMonster(this).WithAttackerAnim("Attack", 1f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task Inhale(IReadOnlyList<Creature> targets)
	{
		IsPuffed = true;
		SfxCmd.Play(CastSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Inhale", 0.6f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 7m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("attack");
		AnimState animState3 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState4 = new AnimState("inhale");
		AnimState nextState = new AnimState("idle_loop_puffed", isLooping: true);
		AnimState animState5 = new AnimState("hurt_puffed");
		AnimState state2 = new AnimState("die_puffed");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = nextState;
		animState5.NextState = nextState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Inhale", animState4);
		creatureAnimator.AddAnyState("Attack", animState2);
		creatureAnimator.AddAnyState("Dead", state, () => !IsPuffed);
		creatureAnimator.AddAnyState("Dead", state2, () => IsPuffed);
		creatureAnimator.AddAnyState("Hit", animState3, () => !IsPuffed);
		creatureAnimator.AddAnyState("Hit", animState5, () => IsPuffed);
		return creatureAnimator;
	}
}
