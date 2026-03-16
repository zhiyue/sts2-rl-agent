using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Myte : MonsterModel
{
	private const int _toxicCount = 2;

	private const string _suckTrigger = "Suck";

	private const string _attackSfx = "event:/sfx/enemy/enemy_attacks/mite/mite_attack";

	private const string _castSfx = "event:/sfx/enemy/enemy_attacks/mite/mite_cast";

	private const string _suckSfx = "event:/sfx/enemy/enemy_attacks/mite/mite_suck";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 64, 61);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 69, 67);

	private int BiteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 13);

	private int SuckDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 4);

	private int SuckStrength => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/mite/mite_die";

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("TOXIC_MOVE", ToxicMove, new StatusIntent(2));
		MoveState moveState2 = new MoveState("BITE_MOVE", BiteMove, new SingleAttackIntent(BiteDamage));
		MoveState moveState3 = new MoveState("SUCK_MOVE", SuckMove, new SingleAttackIntent(SuckDamage), new BuffIntent());
		ConditionalBranchState conditionalBranchState = new ConditionalBranchState("INIT_MOVE");
		conditionalBranchState.AddState(moveState, () => base.Creature.SlotName == "first");
		conditionalBranchState.AddState(moveState3, () => base.Creature.SlotName == "second");
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, conditionalBranchState);
	}

	private async Task ToxicMove(IReadOnlyList<Creature> targets)
	{
		if (TestMode.IsOff)
		{
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(base.Creature);
			Creature target = LocalContext.GetMe(base.CombatState)?.Creature;
			creatureNode.GetSpecialNode<NMyteVfx>("%NMyteVfx")?.SetTarget(target);
		}
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/mite/mite_cast");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.6f);
		await CardPileCmd.AddToCombatAndPreview<Toxic>(targets, PileType.Hand, 2, addedByPlayer: false);
	}

	private async Task BiteMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(BiteDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/mite/mite_attack")
			.WithHitFx("vfx/vfx_bite")
			.Execute(null);
	}

	private async Task SuckMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SuckDamage).FromMonster(this).WithAttackerAnim("Suck", 0.4f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/mite/mite_suck")
			.WithHitFx("vfx/vfx_bite")
			.Execute(null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, SuckStrength, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState5 = new AnimState("suck");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Suck", animState5);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		return creatureAnimator;
	}
}
