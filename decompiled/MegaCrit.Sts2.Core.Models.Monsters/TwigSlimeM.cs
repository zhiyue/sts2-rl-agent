using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
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
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class TwigSlimeM : MonsterModel
{
	private const int _stickyAmount = 1;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 27, 26);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 29, 28);

	private int ClumpDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("CLUMP_SHOT_MOVE", ClumpShotMove, new SingleAttackIntent(ClumpDamage));
		MoveState moveState2 = new MoveState("STICKY_SHOT_MOVE", StickyShotMove, new StatusIntent(1));
		RandomBranchState randomBranchState = (RandomBranchState)(moveState2.FollowUpState = (moveState.FollowUpState = new RandomBranchState("RAND")));
		randomBranchState.AddBranch(moveState, 2);
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(randomBranchState);
		return new MonsterMoveStateMachine(list, moveState2);
	}

	private async Task ClumpShotMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ClumpDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_slime_impact")
			.Execute(null);
	}

	private async Task StickyShotMove(IReadOnlyList<Creature> targets)
	{
		if (TestMode.IsOff)
		{
			Vector2? vector = null;
			foreach (Creature target in targets)
			{
				NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
				if (creatureNode != null && (!vector.HasValue || vector.Value.X > creatureNode.GlobalPosition.X))
				{
					vector = creatureNode.VfxSpawnPosition;
				}
			}
			Node2D node2D = NCombatRoom.Instance.GetCreatureNode(base.Creature)?.GetSpecialNode<Node2D>("Visuals/SpitTarget");
			if (node2D != null)
			{
				node2D.GlobalPosition = vector.Value;
			}
		}
		SfxCmd.Play(CastSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.75f);
		VfxCmd.PlayOnCreatureCenters(targets, "vfx/vfx_slime_impact");
		await CardPileCmd.AddToCombatAndPreview<Slimed>(targets, PileType.Discard, 1, addedByPlayer: false);
	}
}
