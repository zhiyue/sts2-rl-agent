using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class DevotedSculptor : MonsterModel
{
	private static readonly LocString _forbiddenIncantationDialogue = new LocString("monsters", "DEVOTED_SCULPTOR.moves.FORBIDDEN_INCANTATION.banter");

	private readonly int _ritualGain = 9;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 172, 162);

	public override int MaxInitialHp => MinInitialHp;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

	private int SavageDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 12);

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("FORBIDDEN_INCANTATION_MOVE", ForbiddenIncantationMove, new BuffIntent());
		MoveState moveState2 = (MoveState)(moveState.FollowUpState = new MoveState("SAVAGE_MOVE", SavageMove, new SingleAttackIntent(SavageDamage)));
		moveState2.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task ForbiddenIncantationMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(CastSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0f);
		await Cmd.Wait(0.3f);
		VfxCmd.PlayOnCreatureCenter(base.Creature, "vfx/vfx_scream");
		TalkCmd.Play(_forbiddenIncantationDialogue, base.Creature);
		await Cmd.CustomScaledWait(0.75f, 1f);
		await PowerCmd.Apply<RitualPower>(base.Creature, _ritualGain, null, null);
	}

	private async Task SavageMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SavageDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}
}
