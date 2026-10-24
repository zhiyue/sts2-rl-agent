using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class HauntedShip : MonsterModel
{
	private const string _attackTripleTrigger = "AttackTriple";

	protected override bool HasPhobiaSpineSkin => true;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 67, 63);

	public override int MaxInitialHp => MinInitialHp;

	private int HauntDazed => 5;

	private int SwipeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 13);

	private int StompDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);

	private int StompRepeat => 3;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.ArmorBig;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SWIPE_MOVE", SwipeMove, new SingleAttackIntent(SwipeDamage));
		MoveState moveState2 = new MoveState("STOMP_MOVE", StompMove, new MultiAttackIntent(StompDamage, StompRepeat));
		MoveState moveState3 = new MoveState("HAUNT_MOVE", HauntMove, new DebuffIntent(), new StatusIntent(HauntDazed));
		moveState3.FollowUpState = moveState;
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState3);
	}

	private async Task SwipeMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SwipeDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task StompMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(StompDamage).WithHitCount(StompRepeat).FromMonster(this)
			.WithAttackerAnim("AttackTriple", 0.15f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task HauntMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(CastSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0f);
		await Cmd.Wait(0.6f);
		VfxCmd.PlayOnCreatureCenter(base.Creature, "vfx/vfx_spooky_scream");
		await Cmd.CustomScaledWait(0.2f, 0.5f);
		await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, 3m, base.Creature, null);
		await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Discard, HauntDazed, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("debuff");
		AnimState animState3 = new AnimState("attack_triple");
		AnimState animState4 = new AnimState("attack");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState4.NextState = animState;
		animState3.NextState = animState;
		animState5.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		creatureAnimator.AddAnyState("AttackTriple", animState3);
		return creatureAnimator;
	}
}
