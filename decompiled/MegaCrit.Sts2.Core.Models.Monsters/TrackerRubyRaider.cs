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

public sealed class TrackerRubyRaider : MonsterModel
{
	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 22, 21);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 26, 25);

	private int HoundsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 1, 1);

	private int HoundsRepeat => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("TRACK_MOVE", TrackMove, new DebuffIntent());
		MoveState moveState2 = (MoveState)(moveState.FollowUpState = new MoveState("HOUNDS_MOVE", HoundsMove, new MultiAttackIntent(HoundsDamage, HoundsRepeat)));
		moveState2.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task TrackMove(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(base.Creature, "Attack", 0.8f);
		VfxCmd.PlayOnCreatureCenters(targets, "vfx/vfx_attack_slash");
		await PowerCmd.Apply<FrailPower>(targets, 2m, base.Creature, null);
	}

	private async Task HoundsMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(HoundsDamage).WithHitCount(HoundsRepeat).FromMonster(this)
			.WithAttackerAnim("Attack", 0.5f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}
}
