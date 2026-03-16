using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class TheAdversaryMkThree : MonsterModel
{
	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 300, 300);

	public override int MaxInitialHp => MinInitialHp;

	private int CrashDamage => 15;

	private int FlameBeamDamage => 18;

	private int BarrageDamage => 10;

	private int BarrageRepeat => 2;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<ArtifactPower>(base.Creature, 2m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("CRASH_MOVE", CrashMove, new SingleAttackIntent(CrashDamage));
		MoveState moveState2 = new MoveState("FLAME_BEAM_MOVE", FlameBeamMove, new SingleAttackIntent(FlameBeamDamage));
		MoveState moveState3 = new MoveState("BARRAGE_MOVE", BarrageMove, new MultiAttackIntent(BarrageDamage, BarrageRepeat), new BuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task CrashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(CrashDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task FlameBeamMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(FlameBeamDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task BarrageMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(BarrageDamage).WithHitCount(BarrageRepeat).FromMonster(this)
			.WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 4m, base.Creature, null);
	}
}
