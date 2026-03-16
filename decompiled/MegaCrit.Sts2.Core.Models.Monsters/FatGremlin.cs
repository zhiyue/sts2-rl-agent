using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class FatGremlin : MonsterModel
{
	private const string _fleeTrigger = "FleeTrigger";

	private const string _wakeUpTrigger = "WakeUpTrigger";

	private const string _escapeSfx = "event:/sfx/enemy/enemy_attacks/gremlin_merc/fat_gremlin_escape";

	private bool _isAwake;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 14, 13);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 18, 17);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Insect;

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/gremlin_merc/fat_gremlin_die";

	private bool IsAwake
	{
		get
		{
			return _isAwake;
		}
		set
		{
			AssertMutable();
			_isAwake = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SPAWNED_MOVE", SpawnedMove, new StunIntent());
		MoveState moveState2 = (MoveState)(moveState.FollowUpState = new MoveState("FLEE_MOVE", FleeMove, new EscapeIntent()));
		moveState2.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task SpawnedMove(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(base.Creature, "WakeUpTrigger", 0.8f);
		IsAwake = true;
	}

	private async Task FleeMove(IReadOnlyList<Creature> targets)
	{
		LocString line = MonsterModel.L10NMonsterLookup("FAT_GREMLIN.moves.FLEE.banter");
		TalkCmd.Play(line, base.Creature, 1.25);
		NCombatRoom.Instance?.GetCreatureNode(base.Creature)?.ToggleIsInteractable(on: false);
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/gremlin_merc/fat_gremlin_escape");
		await CreatureCmd.TriggerAnim(base.Creature, "FleeTrigger", 0f);
		await Cmd.Wait(1.25f);
		await CreatureCmd.Escape(base.Creature);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("awake_loop", isLooping: true);
		AnimState animState2 = new AnimState("spawn");
		AnimState state = new AnimState("flee");
		AnimState nextState = new AnimState("stunned_loop", isLooping: true);
		AnimState animState3 = new AnimState("wake_up");
		AnimState animState4 = new AnimState("hurt_stunned");
		AnimState animState5 = new AnimState("hurt_awake");
		AnimState state2 = new AnimState("die");
		animState2.NextState = nextState;
		animState4.NextState = nextState;
		animState5.NextState = animState;
		animState3.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState2, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("FleeTrigger", state);
		creatureAnimator.AddAnyState("WakeUpTrigger", animState3);
		creatureAnimator.AddAnyState("Dead", state2);
		creatureAnimator.AddAnyState("Hit", animState5, () => IsAwake);
		creatureAnimator.AddAnyState("Hit", animState4, () => !IsAwake);
		return creatureAnimator;
	}
}
