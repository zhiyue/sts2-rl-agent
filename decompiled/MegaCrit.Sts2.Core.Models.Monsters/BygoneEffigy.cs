using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class BygoneEffigy : MonsterModel
{
	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 132, 127);

	public override int MaxInitialHp => MinInitialHp;

	private int SlashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<SlowPower>(base.Creature, 1m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("INITIAL_SLEEP_MOVE", InitialSleepMove, new SleepIntent());
		MoveState moveState2 = new MoveState("WAKE_MOVE", WakeMove, new BuffIntent());
		MoveState moveState3 = new MoveState("SLEEP_MOVE", SleepMove, new SleepIntent());
		MoveState moveState4 = new MoveState("SLASHES_MOVE", SlashMove, new SingleAttackIntent(SlashDamage));
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState4;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState4;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task InitialSleepMove(IReadOnlyList<Creature> targets)
	{
		LocString line = MonsterModel.L10NMonsterLookup("BYGONE_EFFIGY.moves.SLEEP.speakLine1");
		ThinkCmd.Play(line, base.Creature);
		await Cmd.Wait(0.5f);
	}

	private Task SleepMove(IReadOnlyList<Creature> targets)
	{
		return Task.CompletedTask;
	}

	private async Task WakeMove(IReadOnlyList<Creature> targets)
	{
		if (TestMode.IsOff)
		{
			NRunMusicController.Instance.TriggerEliteSecondPhase();
		}
		await PowerCmd.Apply<StrengthPower>(base.Creature, 10m, base.Creature, null);
		LocString line = MonsterModel.L10NMonsterLookup("BYGONE_EFFIGY.moves.SLEEP.speakLine2");
		TalkCmd.Play(line, base.Creature);
		await Cmd.Wait(0.5f);
	}

	private async Task SlashMove(IReadOnlyList<Creature> targets)
	{
		if (TestMode.IsOff)
		{
			Vector2? vector = null;
			foreach (Creature target in targets)
			{
				NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
				if (!vector.HasValue || vector.Value.X > creatureNode.GlobalPosition.X)
				{
					vector = creatureNode.GlobalPosition;
				}
			}
			NCreature creatureNode2 = NCombatRoom.Instance.GetCreatureNode(base.Creature);
			Node2D specialNode = creatureNode2.GetSpecialNode<Node2D>("Visuals/SpineBoneNode");
			if (specialNode != null)
			{
				specialNode.Position = Vector2.Left * (vector.Value.X - creatureNode2.GlobalPosition.X - 300f);
			}
		}
		NCombatRoom.Instance?.RadialBlur(VfxPosition.Left);
		await DamageCmd.Attack(SlashDamage).FromMonster(this).WithAttackerAnim("Attack", 0.1f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await Cmd.Wait(0.25f);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Dead", state);
		animState.AddBranch("Hit", animState4);
		animState2.AddBranch("Hit", animState4);
		animState4.AddBranch("Hit", animState4);
		return creatureAnimator;
	}
}
