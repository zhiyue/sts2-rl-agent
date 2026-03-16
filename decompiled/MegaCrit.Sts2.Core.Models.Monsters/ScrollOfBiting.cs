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
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class ScrollOfBiting : MonsterModel
{
	private static readonly string[] _skinOptions = new string[2] { "skin1", "skin2" };

	private const int _chewRepeat = 2;

	private const int _buffAmt = 2;

	private int _starterMoveIdx;

	private const string _attackDoubleTrigger = "ATTACK_DOUBLE";

	public const string biteSfx = "event:/sfx/enemy/enemy_attacks/scroll_of_biting/scroll_of_biting_bite";

	public const string biteDoubleSfx = "event:/sfx/enemy/enemy_attacks/scroll_of_biting/scroll_of_biting_bite_double";

	public const string buffSfx = "event:/sfx/enemy/enemy_attacks/scroll_of_biting/scroll_of_biting_buff";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 32, 31);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 39, 38);

	private int ChompDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 14);

	private int ChewDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	public override bool HasDeathSfx => true;

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/scroll_of_biting/scroll_of_biting_die";

	public int StarterMoveIdx
	{
		get
		{
			return _starterMoveIdx;
		}
		set
		{
			AssertMutable();
			_starterMoveIdx = value;
		}
	}

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		MegaSkeleton skeleton = visuals.SpineBody.GetSkeleton();
		MegaSkin megaSkin = visuals.SpineBody.NewSkin("custom-skin");
		MegaSkeletonDataResource data = skeleton.GetData();
		megaSkin.AddSkin(data.FindSkin(MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextItem(_skinOptions)));
		skeleton.SetSkin(megaSkin);
		skeleton.SetSlotsToSetupPose();
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<PaperCutsPower>(base.Creature, 2m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("CHOMP", ChompMove, new SingleAttackIntent(ChompDamage));
		MoveState moveState2 = new MoveState("CHEW", ChewState, new MultiAttackIntent(ChewDamage, 2));
		MoveState moveState3 = new MoveState("MORE_TEETH", MoreTeethMove, new BuffIntent());
		RandomBranchState randomBranchState = new RandomBranchState("rand");
		moveState.FollowUpState = moveState3;
		moveState2.FollowUpState = randomBranchState;
		moveState3.FollowUpState = moveState2;
		randomBranchState.AddBranch(moveState, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState2, 2);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(randomBranchState);
		return (StarterMoveIdx % 3) switch
		{
			0 => new MonsterMoveStateMachine(list, moveState), 
			1 => new MonsterMoveStateMachine(list, moveState2), 
			_ => new MonsterMoveStateMachine(list, moveState3), 
		};
	}

	private async Task ChompMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ChompDamage).FromMonster(this).WithAttackerAnim("Attack", 0.2f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/scroll_of_biting/scroll_of_biting_bite")
			.WithHitFx("vfx/vfx_bite")
			.Execute(null);
	}

	private async Task ChewState(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ChewDamage).WithHitCount(2).FromMonster(this)
			.WithAttackerAnim("ATTACK_DOUBLE", 0.2f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/scroll_of_biting/scroll_of_biting_bite_double")
			.WithHitFx("vfx/vfx_bite")
			.Execute(null);
	}

	private async Task MoreTeethMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/scroll_of_biting/scroll_of_biting_buff");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.8f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("buff");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("attack_double");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState5.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		creatureAnimator.AddAnyState("ATTACK_DOUBLE", animState4);
		return creatureAnimator;
	}
}
