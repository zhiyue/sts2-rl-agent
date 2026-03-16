using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Vantom : MonsterModel
{
	private const string _vantomCustomTrackName = "vantom_progress";

	private const int _inkyLanceRepeat = 2;

	private const int _dismemberWounds = 3;

	private const int _prepareStrength = 2;

	private const string _chargeUpTrigger = "CHARGE_UP";

	private const string _buffTrigger = "BUFF";

	private const string _debuffTrigger = "DEBUFF";

	private const string _heavyAttackTrigger = "ATTACK_HEAVY";

	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/vantom/vantom_buff";

	private const string _dismemberSfx = "event:/sfx/enemy/enemy_attacks/vantom/vantom_dismember";

	private const string _extend1Sfx = "event:/sfx/enemy/enemy_attacks/vantom/vantom_extend_1";

	private const string _extend2Sfx = "event:/sfx/enemy/enemy_attacks/vantom/vantom_extend_2";

	private const string _extend3Sfx = "event:/sfx/enemy/enemy_attacks/vantom/vantom_extend_2";

	private const string _inkyLanceSfx = "event:/sfx/enemy/enemy_attacks/vantom/vantom_inky_lance";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 183, 173);

	public override int MaxInitialHp => MinInitialHp;

	private int InkBlotDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

	private int InkyLanceDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);

	private int DismemberDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 30, 27);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	public override bool ShouldDisappearFromDoom => false;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<SlipperyPower>(base.Creature, 9m, base.Creature, null);
		base.Creature.Died += AfterDeath;
	}

	private void AfterDeath(Creature _)
	{
		base.Creature.Died -= AfterDeath;
		NRunMusicController.Instance?.UpdateMusicParameter("vantom_progress", 5f);
	}

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		MegaAnimationState animationState = visuals.SpineBody.GetAnimationState();
		animationState.SetAnimation("_tracks/charge_up_1", loop: false, 1);
		animationState.AddAnimation("_tracks/charged_1", 0f, loop: true, 1);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("INK_BLOT_MOVE", InkBlotMove, new SingleAttackIntent(InkBlotDamage));
		MoveState moveState2 = new MoveState("INKY_LANCE_MOVE", InkyLanceMove, new MultiAttackIntent(InkyLanceDamage, 2));
		MoveState moveState3 = new MoveState("DISMEMBER_MOVE", DismemberMove, new SingleAttackIntent(DismemberDamage), new StatusIntent(3));
		MoveState moveState4 = new MoveState("PREPARE_MOVE", PrepareMove, new BuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task InkBlotMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(InkBlotDamage).FromMonster(this).WithAttackerAnim("Attack", 0.35f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/vantom/vantom_inky_lance")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		if (TestMode.IsOff && base.Creature.IsAlive)
		{
			await Cmd.CustomScaledWait(1f, 1f);
			NRunMusicController.Instance?.UpdateMusicParameter("vantom_progress", 1f);
			SfxCmd.Play("event:/sfx/enemy/enemy_attacks/vantom/vantom_extend_2");
			await CreatureCmd.TriggerAnim(base.Creature, "CHARGE_UP", 0.15f);
			MegaAnimationState megaAnimationState = (NCombatRoom.Instance?.GetCreatureNode(base.Creature))?.SpineController?.GetAnimationState();
			megaAnimationState?.SetAnimation("_tracks/charge_up_2", loop: false, 1);
			megaAnimationState?.AddAnimation("_tracks/charged_2", 0f, loop: true, 1);
		}
	}

	private async Task InkyLanceMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(InkyLanceDamage).WithHitCount(2).FromMonster(this)
			.WithAttackerAnim("DEBUFF", 0.4f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/vantom/vantom_inky_lance")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		if (TestMode.IsOff && base.Creature.IsAlive)
		{
			NRunMusicController.Instance?.UpdateMusicParameter("vantom_progress", 2f);
			await Cmd.CustomScaledWait(1f, 1f);
			SfxCmd.Play("event:/sfx/enemy/enemy_attacks/vantom/vantom_extend_2");
			MegaAnimationState megaAnimationState = (NCombatRoom.Instance?.GetCreatureNode(base.Creature))?.SpineController?.GetAnimationState();
			megaAnimationState?.SetAnimation("_tracks/charge_up_3", loop: false, 1);
			megaAnimationState?.AddAnimation("_tracks/charged_3", 0f, loop: true, 1);
			await CreatureCmd.TriggerAnim(base.Creature, "CHARGE_UP", 0.15f);
		}
	}

	private async Task DismemberMove(IReadOnlyList<Creature> targets)
	{
		if (TestMode.IsOff && base.Creature.IsAlive)
		{
			MegaAnimationState megaAnimationState = (NCombatRoom.Instance?.GetCreatureNode(base.Creature))?.SpineController?.GetAnimationState();
			megaAnimationState?.SetAnimation("_tracks/attack_heavy", loop: false, 1);
			megaAnimationState?.AddAnimation("_tracks/charged_0", 0f, loop: true, 1);
		}
		NRunMusicController.Instance?.UpdateMusicParameter("vantom_progress", 3f);
		await CreatureCmd.TriggerAnim(base.Creature, "ATTACK_HEAVY", 0f);
		await Cmd.Wait(0.25f);
		NCombatRoom.Instance?.RadialBlur(VfxPosition.Left);
		NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal, 180f + MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat(-10f, 10f));
		await DamageCmd.Attack(DismemberDamage).FromMonster(this).WithNoAttackerAnim()
			.WithHitFx("vfx/vfx_giant_horizontal_slash", "event:/sfx/enemy/enemy_attacks/vantom/vantom_dismember")
			.Execute(null);
		NGame.Instance.DoHitStop(ShakeStrength.Weak, ShakeDuration.Short);
		await Cmd.Wait(0.5f);
		await CardPileCmd.AddToCombatAndPreview<Wound>(targets, PileType.Discard, 3, addedByPlayer: false);
	}

	private async Task PrepareMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/vantom/vantom_buff");
		await CreatureCmd.TriggerAnim(base.Creature, "BUFF", 0.6f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
		if (TestMode.IsOff && base.Creature.IsAlive)
		{
			await Cmd.CustomScaledWait(1f, 1f);
			SfxCmd.Play("event:/sfx/enemy/enemy_attacks/vantom/vantom_extend_1");
			MegaAnimationState megaAnimationState = (NCombatRoom.Instance?.GetCreatureNode(base.Creature))?.SpineController?.GetAnimationState();
			megaAnimationState?.SetAnimation("_tracks/charge_up_1", loop: false, 1);
			megaAnimationState?.AddAnimation("_tracks/charged_1", 0f, loop: true, 1);
			await CreatureCmd.TriggerAnim(base.Creature, "CHARGE_UP", 0.25f);
			NRunMusicController.Instance?.UpdateMusicParameter("vantom_progress", 1f);
		}
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("buff");
		AnimState animState3 = new AnimState("debuff");
		AnimState animState4 = new AnimState("attack");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState6 = new AnimState("charge_up");
		AnimState animState7 = new AnimState("attack_heavy");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		animState6.NextState = animState;
		animState7.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("CHARGE_UP", animState6);
		creatureAnimator.AddAnyState("ATTACK_HEAVY", animState7);
		creatureAnimator.AddAnyState("BUFF", animState2);
		creatureAnimator.AddAnyState("Attack", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		creatureAnimator.AddAnyState("DEBUFF", animState3);
		return creatureAnimator;
	}
}
