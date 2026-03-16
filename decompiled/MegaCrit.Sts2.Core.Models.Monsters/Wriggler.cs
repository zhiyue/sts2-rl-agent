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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Wriggler : MonsterModel
{
	private const string _spawnedMoveId = "SPAWNED_MOVE";

	private const string _initMoveId = "INIT_MOVE";

	private const int _wriggleStrength = 2;

	private bool _startStunned;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 18, 17);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 22, 21);

	private int BiteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Plant;

	public bool StartStunned
	{
		get
		{
			return _startStunned;
		}
		set
		{
			AssertMutable();
			_startStunned = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("NASTY_BITE_MOVE", BiteMove, new SingleAttackIntent(BiteDamage));
		MoveState moveState2 = new MoveState("WRIGGLE_MOVE", WriggleMove, new BuffIntent(), new StatusIntent(1));
		MoveState moveState3 = new MoveState("SPAWNED_MOVE", SpawnedMove, new StunIntent());
		ConditionalBranchState conditionalBranchState = new ConditionalBranchState("INIT_MOVE");
		conditionalBranchState.AddState(moveState, () => base.Creature.SlotName == "wriggler1");
		conditionalBranchState.AddState(moveState2, () => base.Creature.SlotName == "wriggler2");
		conditionalBranchState.AddState(moveState, () => base.Creature.SlotName == "wriggler3");
		conditionalBranchState.AddState(moveState2, () => base.Creature.SlotName == "wriggler4");
		moveState3.FollowUpState = conditionalBranchState;
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState;
		list.Add(moveState3);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(conditionalBranchState);
		MonsterState initialState = (StartStunned ? ((MonsterState)moveState3) : ((MonsterState)conditionalBranchState));
		return new MonsterMoveStateMachine(list, initialState);
	}

	private Task SpawnedMove(IReadOnlyList<Creature> targets)
	{
		return Task.CompletedTask;
	}

	private async Task BiteMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(BiteDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_bite")
			.WithHitVfxNode(NWormyImpactVfx.Create)
			.Execute(null);
	}

	private async Task WriggleMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(AttackSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Attack", 0.15f);
		foreach (Creature target in targets)
		{
			NWormyImpactVfx nWormyImpactVfx = NWormyImpactVfx.Create(target);
			if (nWormyImpactVfx != null)
			{
				NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(nWormyImpactVfx);
			}
		}
		await CardPileCmd.AddToCombatAndPreview<Infection>(targets, PileType.Discard, 1, addedByPlayer: false);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
	}
}
