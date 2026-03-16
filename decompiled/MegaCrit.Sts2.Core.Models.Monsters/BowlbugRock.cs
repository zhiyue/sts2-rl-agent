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

public sealed class BowlbugRock : MonsterModel
{
	private const string _stunTrigger = "Stun";

	private const string _unstunTrigger = "Unstun";

	private bool _isOffBalance;

	private const string _stunSfx = "event:/sfx/enemy/enemy_attacks/workbug_rock/workbug_rock_stun";

	private const string _spineSkin = "rock";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 46, 45);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 49, 48);

	public static int HeadbuttDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 15);

	public bool IsOffBalance
	{
		get
		{
			return _isOffBalance;
		}
		set
		{
			AssertMutable();
			_isOffBalance = value;
		}
	}

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/workbug_rock/workbug_rock_die";

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/workbug_rock/workbug_rock_attack";

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Insect;

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		MegaSkeleton skeleton = visuals.SpineBody.GetSkeleton();
		skeleton.SetSkin(skeleton.GetData().FindSkin("rock"));
		skeleton.SetSlotsToSetupPose();
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<ImbalancedPower>(base.Creature, 1m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("HEADBUTT_MOVE", HeadbuttMove, new SingleAttackIntent(HeadbuttDamage));
		MoveState moveState2 = new MoveState("DIZZY_MOVE", DizzyMove, new StunIntent());
		ConditionalBranchState conditionalBranchState = (ConditionalBranchState)(moveState.FollowUpState = new ConditionalBranchState("POST_HEADBUTT"));
		moveState2.FollowUpState = moveState;
		conditionalBranchState.AddState(moveState2, () => IsOffBalance);
		conditionalBranchState.AddState(moveState, () => !IsOffBalance);
		list.Add(moveState2);
		list.Add(conditionalBranchState);
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task HeadbuttMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(HeadbuttDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		if (IsOffBalance)
		{
			SfxCmd.Play("event:/sfx/enemy/enemy_attacks/workbug_rock/workbug_rock_stun");
			await CreatureCmd.TriggerAnim(base.Creature, "Stun", 0.6f);
		}
	}

	private async Task DizzyMove(IReadOnlyList<Creature> targets)
	{
		IsOffBalance = false;
		await CreatureCmd.TriggerAnim(base.Creature, "Unstun", 0.6f);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("buff");
		AnimState animState3 = new AnimState("headbutt");
		AnimState animState4 = new AnimState("hurt");
		AnimState animState5 = new AnimState("hurt_stunned");
		AnimState state = new AnimState("die");
		AnimState animState6 = new AnimState("stun");
		AnimState nextState = new AnimState("stunned_loop", isLooping: true);
		animState2.NextState = animState;
		animState4.NextState = animState;
		animState3.NextState = animState;
		animState6.NextState = nextState;
		animState5.NextState = nextState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Hit", animState4, () => !IsOffBalance);
		creatureAnimator.AddAnyState("Hit", animState5, () => IsOffBalance);
		creatureAnimator.AddAnyState("Stun", animState6);
		creatureAnimator.AddAnyState("Unstun", animState);
		return creatureAnimator;
	}
}
