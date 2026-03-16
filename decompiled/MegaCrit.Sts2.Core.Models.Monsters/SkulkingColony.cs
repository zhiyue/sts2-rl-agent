using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class SkulkingColony : MonsterModel
{
	private const int _smashStatusCount = 4;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 84, 79);

	public override int MaxInitialHp => MinInitialHp;

	private int SuperCrabDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);

	private int SuperCrabRepeat => 2;

	private int ZoomDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 16);

	private int SmashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 9);

	private int InertiaBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 13, 10);

	public override bool HasDeathSfx => false;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<HardenedShellPower>(base.Creature, 20m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("INERTIA_MOVE", InertiaMove, new DefendIntent(), new BuffIntent());
		MoveState moveState2 = new MoveState("ZOOM_MOVE", ZoomMove, new SingleAttackIntent(ZoomDamage));
		MoveState moveState3 = new MoveState("SUPER_CRAB_MOVE", SuperCrabMove, new MultiAttackIntent(SuperCrabDamage, SuperCrabRepeat));
		MoveState moveState4 = new MoveState("SMASH_MOVE", SmashMove, new SingleAttackIntent(SmashDamage), new StatusIntent(4));
		moveState4.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState;
		moveState.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState4);
	}

	private async Task InertiaMove(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(base.Creature, "Attack", 0.15f);
		await CreatureCmd.GainBlock(base.Creature, InertiaBlock, ValueProp.Move, null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 3m, base.Creature, null);
	}

	private async Task SuperCrabMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SuperCrabDamage).WithHitCount(SuperCrabRepeat).FromMonster(this)
			.WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task ZoomMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ZoomDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task SmashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SmashDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Discard, 4, addedByPlayer: false);
	}
}
