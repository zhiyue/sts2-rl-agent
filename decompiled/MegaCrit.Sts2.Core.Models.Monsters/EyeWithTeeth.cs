using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class EyeWithTeeth : MonsterModel
{
	private const int _distractAmount = 3;

	public override int MinInitialHp => 6;

	public override int MaxInitialHp => MinInitialHp;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	public override bool ShouldDisappearFromDoom => false;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<IllusionPower>(base.Creature, 1m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("DISTRACT_MOVE", DistractMove, new StatusIntent(3));
		moveState.FollowUpState = moveState;
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task DistractMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(AttackSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Attack", 0.7f);
		VfxCmd.PlayOnCreatureCenters(targets, "vfx/vfx_attack_slash");
		await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Discard, 3, addedByPlayer: false);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("attack");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Attack", animState2);
		creatureAnimator.AddAnyState("Dead", state, () => !base.CombatState.GetTeammatesOf(base.Creature).Any((Creature t) => t != null && t.IsPrimaryEnemy && t.IsAlive));
		return creatureAnimator;
	}
}
