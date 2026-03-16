using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class LeafSlimeS : MonsterModel
{
	private const int _goopAmount = 1;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 12, 11);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 16, 15);

	private int TackleDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("BUTT_MOVE", TackleMove, new SingleAttackIntent(TackleDamage));
		MoveState moveState2 = new MoveState("GOOP_MOVE", GoopMove, new StatusIntent(1));
		RandomBranchState randomBranchState = (RandomBranchState)(moveState2.FollowUpState = (moveState.FollowUpState = new RandomBranchState("RAND")));
		randomBranchState.AddBranch(moveState, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(randomBranchState);
		return new MonsterMoveStateMachine(list, randomBranchState);
	}

	private async Task TackleMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(TackleDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_slime_impact")
			.Execute(null);
	}

	private async Task GoopMove(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.5f);
		SfxCmd.Play(AttackSfx);
		VfxCmd.PlayOnCreatureCenters(targets, "vfx/vfx_slime_impact");
		await CardPileCmd.AddToCombatAndPreview<Slimed>(targets, PileType.Discard, 1, addedByPlayer: false);
	}
}
