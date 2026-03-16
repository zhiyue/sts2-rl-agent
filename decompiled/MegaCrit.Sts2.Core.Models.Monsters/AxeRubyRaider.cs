using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class AxeRubyRaider : MonsterModel
{
	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 21, 20);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 23, 22);

	private int SwingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);

	private int SwingBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);

	private int BigSwingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SWING_1", SwingMove, new SingleAttackIntent(SwingDamage), new DefendIntent());
		MoveState moveState2 = new MoveState("SWING_2", SwingMove, new SingleAttackIntent(SwingDamage), new DefendIntent());
		MoveState moveState3 = new MoveState("BIG_SWING", BigSwingMove, new SingleAttackIntent(BigSwingDamage));
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task SwingMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SwingDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await CreatureCmd.GainBlock(base.Creature, SwingBlock, ValueProp.Move, null);
	}

	private async Task BigSwingMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(BigSwingDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}
}
