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

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class FrogKnight : MonsterModel
{
	private const string _buffTrigger = "Buff";

	private const string _lashTrigger = "Lash";

	private const string _chargeTrigger = "charge";

	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/frog_knight/frog_knight_buff";

	private const string _chargeSfx = "event:/sfx/enemy/enemy_attacks/frog_knight/frog_knight_charge";

	private const string _tongueLashSfx = "event:/sfx/enemy/enemy_attacks/frog_knight/frog_knight_tongue_lash";

	private bool _hasBeetleCharged;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 199, 191);

	public override int MaxInitialHp => MinInitialHp;

	private int StrikeDownEvilDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 23, 21);

	private int TongueLashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 13);

	private int BeetleChargeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 40, 35);

	private int PlatingAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 19, 15);

	private bool HasBeetleCharged
	{
		get
		{
			return _hasBeetleCharged;
		}
		set
		{
			AssertMutable();
			_hasBeetleCharged = value;
		}
	}

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<PlatingPower>(base.Creature, PlatingAmount, base.Creature, null);
		HasBeetleCharged = false;
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("FOR_THE_QUEEN", ForTheQueenMove, new BuffIntent());
		MoveState moveState2 = new MoveState("STRIKE_DOWN_EVIL", StrikeDownEvilMove, new SingleAttackIntent(StrikeDownEvilDamage));
		MoveState moveState3 = new MoveState("TONGUE_LASH", TongueLashMove, new SingleAttackIntent(TongueLashDamage), new DebuffIntent());
		MoveState moveState4 = new MoveState("BEETLE_CHARGE", BeetleChargeMove, new SingleAttackIntent(BeetleChargeDamage));
		ConditionalBranchState conditionalBranchState = new ConditionalBranchState("HALF_HEALTH");
		conditionalBranchState.AddState(moveState3, () => HasBeetleCharged || base.Creature.CurrentHp >= base.Creature.MaxHp / 2);
		conditionalBranchState.AddState(moveState4, () => !HasBeetleCharged && base.Creature.CurrentHp < base.Creature.MaxHp / 2);
		moveState.FollowUpState = conditionalBranchState;
		moveState2.FollowUpState = moveState;
		moveState3.FollowUpState = moveState2;
		moveState4.FollowUpState = moveState3;
		list.Add(conditionalBranchState);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState3);
	}

	private async Task ForTheQueenMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/frog_knight/frog_knight_buff");
		await CreatureCmd.TriggerAnim(base.Creature, "Buff", 0.4f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 5m, base.Creature, null);
	}

	private async Task StrikeDownEvilMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(StrikeDownEvilDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task TongueLashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(TongueLashDamage).FromMonster(this).WithAttackerAnim("Lash", 0.3f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/frog_knight/frog_knight_tongue_lash")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<FrailPower>(targets, 2m, base.Creature, null);
	}

	private async Task BeetleChargeMove(IReadOnlyList<Creature> targets)
	{
		HasBeetleCharged = true;
		await DamageCmd.Attack(BeetleChargeDamage).FromMonster(this).WithAttackerAnim("charge", 0.6f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/frog_knight/frog_knight_charge")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await Cmd.Wait(1f);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("buff");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("attack_tongue");
		AnimState animState5 = new AnimState("charge");
		AnimState animState6 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		animState6.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Buff", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Lash", animState4);
		creatureAnimator.AddAnyState("charge", animState5);
		creatureAnimator.AddAnyState("Dead", state);
		animState.AddBranch("Hit", animState6);
		animState2.AddBranch("Hit", animState6);
		animState3.AddBranch("Hit", animState6);
		animState4.AddBranch("Hit", animState6);
		animState6.AddBranch("Hit", animState6);
		return creatureAnimator;
	}
}
