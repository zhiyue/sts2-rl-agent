using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
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

public sealed class BowlbugEgg : MonsterModel
{
	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 23, 21);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 24, 22);

	private int BiteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

	private int ProtectBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/workbug_egg/workbug_egg_die";

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/workbug_egg/workbug_egg_attack";

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Insect;

	public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
	{
		skeleton.SetSkin(skeleton.GetData().FindSkin("cocoon"));
		skeleton.SetSlotsToSetupPose();
		Node node = (spine.BoundObject as Node)?.GetNodeOrNull("CocoonSlotNode/Cocoon");
		if (node != null)
		{
			MegaSprite megaSprite = new MegaSprite(node);
			MegaSkeleton skeleton2 = megaSprite.GetSkeleton();
			if (skeleton2 != null)
			{
				skeleton2.SetSkin(skeleton2.GetData().FindSkin("egg1"));
				megaSprite.GetAnimationState().SetAnimation("egg_idle_loop");
				skeleton2.SetSlotsToSetupPose();
			}
		}
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("BITE_MOVE", BiteMove, new SingleAttackIntent(BiteDamage), new DefendIntent());
		moveState.FollowUpState = moveState;
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task BiteMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(BiteDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await CreatureCmd.GainBlock(base.Creature, ProtectBlock, ValueProp.Move, null);
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
