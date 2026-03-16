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

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class BowlbugSilk : MonsterModel
{
	private const int _thrashRepeat = 2;

	private const string _spitSfx = "event:/sfx/enemy/enemy_attacks/workbug_silk/workbug_silk_spit";

	private const string _spineSkin = "web";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 41, 40);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 44, 43);

	private int ThrashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/workbug_silk/workbug_silk_die";

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		MegaSkeleton skeleton = visuals.SpineBody.GetSkeleton();
		skeleton.SetSkin(skeleton.GetData().FindSkin("web"));
		skeleton.SetSlotsToSetupPose();
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("TRASH_MOVE", ThrashMove, new MultiAttackIntent(ThrashDamage, 2));
		MoveState moveState2 = (MoveState)(moveState.FollowUpState = new MoveState("TOXIC_SPIT_MOVE", WebMove, new DebuffIntent()));
		moveState2.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, moveState2);
	}

	private async Task ThrashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ThrashDamage).WithHitCount(2).FromMonster(this)
			.WithAttackerAnim("Attack", 0.3f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task WebMove(IReadOnlyList<Creature> targets)
	{
		if (TestMode.IsOff)
		{
			Vector2? vector = null;
			foreach (Creature target in targets)
			{
				NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
				if (!vector.HasValue || vector.Value.X > creatureNode.GlobalPosition.X)
				{
					vector = creatureNode.GlobalPosition;
				}
			}
			NCreature creatureNode2 = NCombatRoom.Instance.GetCreatureNode(base.Creature);
			Node2D specialNode = creatureNode2.GetSpecialNode<Node2D>("Visuals/SpineBoneNode");
			if (specialNode != null)
			{
				specialNode.Position = Vector2.Right * (vector.Value.X - creatureNode2.GlobalPosition.X) * 4f;
			}
		}
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/workbug_silk/workbug_silk_spit");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.8f);
		await PowerCmd.Apply<WeakPower>(targets, 1m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("spit");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		return creatureAnimator;
	}
}
