using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Osty : MonsterModel
{
	public const float attackerAnimDelay = 0.3f;

	public const string pokeAnim = "attack_poke";

	public const string ostyAttackSfx = "event:/sfx/characters/osty/osty_attack";

	public static Vector2 MinOffset => new Vector2(150f, -75f);

	public static Vector2 MaxOffset => new Vector2(250f, -75f);

	public static Vector2 ScaleRange => new Vector2(1f, 2f);

	public override int MinInitialHp => 1;

	public override int MaxInitialHp => 1;

	public override string DeathSfx => "event:/sfx/characters/osty/osty_die";

	public override bool HasDeathSfx => true;

	public override bool IsHealthBarVisible => base.Creature.IsAlive;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState moveState = new MoveState("NOTHING_MOVE", (IReadOnlyList<Creature> _) => Task.CompletedTask);
		moveState.FollowUpState = moveState;
		return new MonsterMoveStateMachine(new global::_003C_003Ez__ReadOnlySingleElementList<MonsterState>(moveState), moveState);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("attack_poke");
		AnimState animState5 = new AnimState("hurt");
		AnimState animState6 = new AnimState("die");
		AnimState nextState = new AnimState("dead_loop", isLooping: true);
		AnimState animState7 = new AnimState("revive");
		animState.AddBranch("Hit", animState5);
		animState2.NextState = animState;
		animState2.AddBranch("Hit", animState5);
		animState3.NextState = animState;
		animState3.AddBranch("Hit", animState5);
		animState4.NextState = animState;
		animState4.AddBranch("Hit", animState5);
		animState5.NextState = animState;
		animState5.AddBranch("Hit", animState5);
		animState6.NextState = nextState;
		animState7.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Dead", animState6);
		creatureAnimator.AddAnyState("attack_poke", animState4);
		creatureAnimator.AddAnyState("Revive", animState7);
		return creatureAnimator;
	}

	public static bool CheckMissingWithAnim(Player owner)
	{
		NCombatRoom.Instance?.ShakeOstyIfDead(owner);
		return owner.IsOstyMissing;
	}
}
