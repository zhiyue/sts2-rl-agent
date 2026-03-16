using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class FakeMerchantMonster : MonsterModel
{
	private const string _spewCoinsTrigger = "spew";

	private const string _throwRelicTrigger = "throw";

	private const int _spewCoinsDamage = 2;

	private const int _spewCoinsRepeat = 8;

	private const string _attackMultiTrigger = "attack_multi";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 175, 165);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 175, 165);

	private int SwipeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 13);

	private int ThrowRelicDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

	public override string DeathSfx => "event:/sfx/npcs/reverse_merchant/reverse_merchant_die";

	public override string HurtSfx => "event:/sfx/npcs/reverse_merchant/reverse_merchant_hurt";

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SWIPE_MOVE", SwipeMove, new SingleAttackIntent(SwipeDamage));
		MoveState moveState2 = new MoveState("SPEW_COINS_MOVE", SpewCoinsMove, new MultiAttackIntent(2, 8));
		MoveState moveState3 = new MoveState("THROW_RELIC_MOVE", ThrowRelicMove, new SingleAttackIntent(ThrowRelicDamage), new DebuffIntent());
		MoveState moveState4 = new MoveState("ENRAGE_MOVE", EnrageMove, new BuffIntent());
		RandomBranchState randomBranchState = new RandomBranchState("RAND_MOVE");
		randomBranchState.AddBranch(moveState, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState3, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState4, 3, MoveRepeatType.CannotRepeat);
		moveState.FollowUpState = randomBranchState;
		moveState2.FollowUpState = randomBranchState;
		moveState4.FollowUpState = randomBranchState;
		RandomBranchState randomBranchState2 = new RandomBranchState("RAND_ATTACK_MOVE");
		randomBranchState2.AddBranch(moveState, MoveRepeatType.CannotRepeat);
		randomBranchState2.AddBranch(moveState2, MoveRepeatType.CannotRepeat);
		randomBranchState2.AddBranch(moveState3, MoveRepeatType.CannotRepeat);
		moveState3.FollowUpState = randomBranchState2;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		list.Add(randomBranchState);
		list.Add(randomBranchState2);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task SwipeMove(IReadOnlyList<Creature> targets)
	{
		await ShowDialogueForMove("SWIPE");
		await DamageCmd.Attack(SwipeDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task SpewCoinsMove(IReadOnlyList<Creature> targets)
	{
		await ShowDialogueForMove("SPEW_COINS");
		await DamageCmd.Attack(2m).FromMonster(this).WithHitCount(8)
			.WithAttackerAnim("spew", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task ThrowRelicMove(IReadOnlyList<Creature> targets)
	{
		await ShowDialogueForMove("THROW_RELIC");
		await DamageCmd.Attack(SwipeDamage).FromMonster(this).WithAttackerAnim("throw", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await PowerCmd.Apply<FrailPower>(targets, 1m, base.Creature, null);
	}

	private async Task EnrageMove(IReadOnlyList<Creature> targets)
	{
		await ShowDialogueForMove("ENRAGE");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.5f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("combat_idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("attack");
		AnimState animState3 = new AnimState("attack_multi");
		AnimState animState4 = new AnimState("attack_throw");
		AnimState animState5 = new AnimState("buff");
		AnimState animState6 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		animState6.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState6);
		creatureAnimator.AddAnyState("Attack", animState2);
		creatureAnimator.AddAnyState("spew", animState3);
		creatureAnimator.AddAnyState("throw", animState4);
		creatureAnimator.AddAnyState("Cast", animState5);
		return creatureAnimator;
	}

	private async Task ShowDialogueForMove(string moveId)
	{
		LocString locString = MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextItem(GetLinesForMove(moveId));
		if (locString != null)
		{
			TalkCmd.Play(locString, base.Creature);
			await Cmd.Wait(0.5f);
		}
	}

	private IEnumerable<LocString> GetLinesForMove(string moveId)
	{
		LocTable table = LocManager.Instance.GetTable("monsters");
		return table.GetLocStringsWithPrefix(base.Id.Entry + ".moves." + moveId + ".speakLine");
	}
}
