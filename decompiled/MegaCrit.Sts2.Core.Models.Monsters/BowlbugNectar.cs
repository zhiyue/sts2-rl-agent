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
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class BowlbugNectar : MonsterModel
{
	private const string _spitSfx = "event:/sfx/enemy/enemy_attacks/workbug_goop/workbug_goop_spit";

	private const string _spineSkin = "goop";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 36, 35);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 39, 38);

	private int ThrashDamage => 3;

	private int BuffStrengthGain => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 15);

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/workbug_goop/workbug_goop_die";

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Insect;

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		MegaSkeleton skeleton = visuals.SpineBody.GetSkeleton();
		skeleton.SetSkin(skeleton.GetData().FindSkin("goop"));
		skeleton.SetSlotsToSetupPose();
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("THRASH_MOVE", ThrashMove, new SingleAttackIntent(ThrashDamage));
		MoveState moveState2 = new MoveState("BUFF_MOVE", BuffMove, new BuffIntent());
		MoveState moveState3 = new MoveState("THRASH2_MOVE", ThrashMove, new SingleAttackIntent(ThrashDamage));
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState3;
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task ThrashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ThrashDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/workbug_goop/workbug_goop_spit")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task BuffMove(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.6f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, BuffStrengthGain, base.Creature, null);
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
