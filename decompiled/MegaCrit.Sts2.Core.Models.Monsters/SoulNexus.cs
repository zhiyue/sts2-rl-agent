using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio;
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

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class SoulNexus : MonsterModel
{
	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 254, 234);

	public override int MaxInitialHp => MinInitialHp;

	private int SoulBurnDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 31, 29);

	private int MaelstromDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);

	private int MaelstromRepeat => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 4);

	private int DrainLifeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 18);

	public override bool ShouldFadeAfterDeath => false;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		visuals.SpineBody.GetAnimationState().SetAnimation("tracks/writhe", loop: true, 1);
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		base.Creature.Died += AfterDeath;
	}

	private void AfterDeath(Creature _)
	{
		base.Creature.Died -= AfterDeath;
		NCombatRoom.Instance.GetCreatureNode(base.Creature)?.SpineController.GetAnimationState().SetAnimation("tracks/empty", loop: true, 1);
	}

	public override void BeforeRemovedFromRoom()
	{
		if (!base.CombatState.RunState.IsGameOver)
		{
			NCombatRoom.Instance.GetCreatureNode(base.Creature)?.SpineController.GetAnimationState().SetAnimation("tracks/empty", loop: true, 1);
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SOUL_BURN_MOVE", SoulBurnMove, new SingleAttackIntent(SoulBurnDamage));
		MoveState moveState2 = new MoveState("MAELSTROM_MOVE", MaelstromMove, new MultiAttackIntent(MaelstromDamage, MaelstromRepeat));
		MoveState moveState3 = new MoveState("DRAIN_LIFE_MOVE", DrainLifeMove, new SingleAttackIntent(DrainLifeDamage), new DebuffIntent(strong: true));
		RandomBranchState randomBranchState = (RandomBranchState)(moveState3.FollowUpState = (moveState2.FollowUpState = (moveState.FollowUpState = new RandomBranchState("RAND"))));
		randomBranchState.AddBranch(moveState, MoveRepeatType.CannotRepeat, 1f);
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat, 1f);
		randomBranchState.AddBranch(moveState3, MoveRepeatType.CannotRepeat, 1f);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(randomBranchState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task SoulBurnMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SoulBurnDamage).FromMonster(this).WithAttackerAnim("Attack", 0.6f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task MaelstromMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(MaelstromDamage).WithHitCount(MaelstromRepeat).FromMonster(this)
			.OnlyPlayAnimOnce()
			.WithAttackerAnim("Attack", 0.2f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task DrainLifeMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(DrainLifeDamage).FromMonster(this).WithAttackerAnim("Cast", 1f)
			.WithAttackerFx(null, CastSfx)
			.Execute(null);
		await PowerCmd.Apply<VulnerablePower>(targets, 2m, base.Creature, null);
		await PowerCmd.Apply<WeakPower>(targets, 2m, base.Creature, null);
	}
}
