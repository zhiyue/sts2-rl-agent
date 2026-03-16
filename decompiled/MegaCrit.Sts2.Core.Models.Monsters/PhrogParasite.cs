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
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class PhrogParasite : MonsterModel
{
	private const int _lashRepeat = 4;

	private const int _infestAmt = 3;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 66, 61);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 68, 64);

	private int LashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Plant;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<InfestedPower>(base.Creature, 4m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("INFECT_MOVE", InfectMove, new StatusIntent(3));
		MoveState moveState2 = new MoveState("LASH_MOVE", LashMove, new MultiAttackIntent(LashDamage, 4));
		RandomBranchState randomBranchState = new RandomBranchState("RAND");
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState;
		randomBranchState.AddBranch(moveState, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(randomBranchState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task LashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(LashDamage).WithHitCount(4).FromMonster(this)
			.WithAttackerAnim("Attack", 0.55f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, AttackSfx)
			.WithHitVfxNode(NWormyImpactVfx.Create)
			.Execute(null);
	}

	private async Task InfectMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(CastSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.75f);
		foreach (Creature target in targets)
		{
			NWormyImpactVfx nWormyImpactVfx = NWormyImpactVfx.Create(target);
			if (nWormyImpactVfx != null)
			{
				NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(nWormyImpactVfx);
			}
		}
		await CardPileCmd.AddToCombatAndPreview<Infection>(targets, PileType.Discard, 3, addedByPlayer: false);
	}
}
