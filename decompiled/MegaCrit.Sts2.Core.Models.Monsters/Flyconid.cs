using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Flyconid : MonsterModel
{
	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 51, 47);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 53, 49);

	private int SmashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);

	private int SporeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("VULNERABLE_SPORES_MOVE", VulnerableSporesMove, new DebuffIntent());
		MoveState moveState2 = new MoveState("FRAIL_SPORES_MOVE", FrailSporesMove, new SingleAttackIntent(SporeDamage), new DebuffIntent());
		MoveState moveState3 = new MoveState("SMASH_MOVE", SmashMove, new SingleAttackIntent(SmashDamage));
		RandomBranchState randomBranchState = new RandomBranchState("RAND");
		RandomBranchState randomBranchState2 = new RandomBranchState("INITIAL");
		moveState.FollowUpState = randomBranchState;
		moveState2.FollowUpState = randomBranchState;
		moveState3.FollowUpState = randomBranchState;
		randomBranchState.AddBranch(moveState, 3, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState2, 2, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState3, MoveRepeatType.CannotRepeat);
		randomBranchState2.AddBranch(moveState2, 2, MoveRepeatType.CannotRepeat);
		randomBranchState2.AddBranch(moveState3, MoveRepeatType.CannotRepeat);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(randomBranchState);
		list.Add(randomBranchState2);
		return new MonsterMoveStateMachine(list, randomBranchState2);
	}

	private async Task VulnerableSporesMove(IReadOnlyList<Creature> targets)
	{
		if (TestMode.IsOff)
		{
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(base.Creature);
			NFlyconidSporesVfx node = creatureNode.Visuals.Body.GetNode<NFlyconidSporesVfx>("%VfxController");
			node.SetSporeTypeIsVulnerable(isVulnerable: true);
		}
		SfxCmd.Play(CastSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.5f);
		await PowerCmd.Apply<VulnerablePower>(targets, 2m, base.Creature, null);
	}

	private async Task FrailSporesMove(IReadOnlyList<Creature> targets)
	{
		if (TestMode.IsOff)
		{
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(base.Creature);
			NFlyconidSporesVfx node = creatureNode.Visuals.Body.GetNode<NFlyconidSporesVfx>("%VfxController");
			node.SetSporeTypeIsVulnerable(isVulnerable: false);
		}
		await DamageCmd.Attack(SporeDamage).FromMonster(this).WithAttackerAnim("Cast", 0.5f)
			.WithAttackerFx(null, CastSfx)
			.WithWaitBeforeHit(0f, 0.6f)
			.WithHitVfxNode((Creature t) => NSporeImpactVfx.Create(t, new Color("8aad7d")))
			.Execute(null);
		await PowerCmd.Apply<FrailPower>(targets, 2m, base.Creature, null);
	}

	private async Task SmashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SmashDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}
}
