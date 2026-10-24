using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Axebot : MonsterModel
{
	private const int _oneTwoRepeat = 2;

	private const string _hammerUppercutTrigger = "uppercut";

	private const string _sharpenTrigger = "sharpen";

	public const string respawnTrigger = "respawn";

	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/axebot/axebot_buff";

	private const string _spinSfx = "event:/sfx/enemy/enemy_attacks/axebot/axebot_attack_spin";

	private int? _stockOverrideAmount;

	private bool _shouldPlaySpawnAnimation;

	private int BootUpBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 10);

	private int OneTwoDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);

	private int BootUpStrGain => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	private int HammerUppercutDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 12);

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 76, 70);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 86, 78);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	public int StockAmount
	{
		get
		{
			return _stockOverrideAmount ?? 2;
		}
		set
		{
			AssertMutable();
			_stockOverrideAmount = value;
		}
	}

	public bool ShouldPlaySpawnAnimation
	{
		get
		{
			return _shouldPlaySpawnAnimation;
		}
		set
		{
			AssertMutable();
			_shouldPlaySpawnAnimation = value;
		}
	}

	public override async Task AfterAddedToRoom()
	{
		if (StockAmount > 0)
		{
			await PowerCmd.Apply<StockPower>(new ThrowingPlayerChoiceContext(), base.Creature, StockAmount, null, null);
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("BOOT_UP_MOVE", BootUpMove, new DefendIntent(), new BuffIntent());
		MoveState moveState2 = new MoveState("ONE_TWO_MOVE", OneTwoMove, new MultiAttackIntent(OneTwoDamage, 2));
		MoveState moveState3 = (MoveState)(moveState.FollowUpState = new MoveState("HAMMER_UPPERCUT_MOVE", HammerUppercutMove, new SingleAttackIntent(HammerUppercutDamage), new DebuffIntent()));
		moveState3.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		if (_stockOverrideAmount.HasValue)
		{
			return new MonsterMoveStateMachine(list, moveState);
		}
		return new MonsterMoveStateMachine(list, moveState3);
	}

	private async Task BootUpMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/axebot/axebot_buff");
		await CreatureCmd.TriggerAnim(base.Creature, "sharpen", 0.3f);
		await Cmd.Wait(0.25f);
		await CreatureCmd.GainBlock(base.Creature, BootUpBlock, ValueProp.Move, null);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), base.Creature, BootUpStrGain * (2 - StockAmount), base.Creature, null);
	}

	private async Task OneTwoMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(OneTwoDamage).WithHitCount(2).FromMonster(this)
			.WithAttackerAnim("Attack", 0.35f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.OnlyPlayAnimOnce()
			.Execute(null);
	}

	private async Task HammerUppercutMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(HammerUppercutDamage).FromMonster(this).WithAttackerAnim("uppercut", 0.8f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/axebot/axebot_attack_spin")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, 2m, base.Creature, null);
		await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, 2m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("attack");
		AnimState animState3 = new AnimState("special");
		AnimState animState4 = new AnimState("sharpen");
		AnimState animState5 = new AnimState("respawn");
		AnimState animState6 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState6.NextState = animState;
		animState5.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(_shouldPlaySpawnAnimation ? animState5 : animState, controller);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState6);
		creatureAnimator.AddAnyState("Attack", animState2);
		creatureAnimator.AddAnyState("uppercut", animState3);
		creatureAnimator.AddAnyState("sharpen", animState4);
		creatureAnimator.AddAnyState("respawn", animState5);
		return creatureAnimator;
	}
}
