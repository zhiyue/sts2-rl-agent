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
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Seapunk : MonsterModel
{
	private const string _multiAttackTrigger = "MultiAttack";

	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/seapunk/seapunk_buff";

	private const string _kickSfx = "event:/sfx/enemy/enemy_attacks/seapunk/seapunk_kick";

	private const string _kickMultiSfx = "event:/sfx/enemy/enemy_attacks/seapunk/seapunk_kick_multi";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 47, 44);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 49, 46);

	private int SeaKickDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);

	private int SpinningKickDamage => 2;

	private int SpinningKickRepeat => 4;

	private int BubbleBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 8, 7);

	private int BubbleStr => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

	public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/seapunk/seapunk_hurt";

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SEA_KICK_MOVE", SeaKickMove, new SingleAttackIntent(SeaKickDamage));
		MoveState moveState2 = new MoveState("SPINNING_KICK_MOVE", SpinningKickMove, new MultiAttackIntent(SpinningKickDamage, SpinningKickRepeat));
		MoveState moveState3 = new MoveState("BUBBLE_BURP_MOVE", BubbleBurpMove, new BuffIntent(), new DefendIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task SeaKickMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SeaKickDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/seapunk/seapunk_kick")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task SpinningKickMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SpinningKickDamage).WithHitCount(SpinningKickRepeat).FromMonster(this)
			.WithAttackerAnim("MultiAttack", 0.15f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/seapunk/seapunk_kick_multi")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task BubbleBurpMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/seapunk/seapunk_buff");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.75f);
		await CreatureCmd.GainBlock(base.Creature, BubbleBlock, ValueProp.Move, null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, BubbleStr, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("attack_multi");
		AnimState animState3 = new AnimState("cast");
		AnimState animState4 = new AnimState("attack");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		animState2.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState3);
		creatureAnimator.AddAnyState("Attack", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		creatureAnimator.AddAnyState("MultiAttack", animState2);
		return creatureAnimator;
	}
}
