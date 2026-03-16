using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Door : MonsterModel
{
	public const string initialMoveId = "DRAMATIC_OPEN_MOVE";

	private MoveState? _dramaticOpenMove;

	private MoveState _deadState;

	private Creature? _doormaker;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 165, 155);

	public override int MaxInitialHp => MinInitialHp;

	public MoveState DeadState
	{
		get
		{
			return _deadState;
		}
		private set
		{
			AssertMutable();
			_deadState = value;
		}
	}

	private int DramaticOpenDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 28, 25);

	private int EnforceDamage => 20;

	private int EnforceStrength => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	private int DoorSlamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 15);

	private int DoorSlamRepeat => 2;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.ArmorBig;

	public override bool ShouldDisappearFromDoom => false;

	public Creature Doormaker
	{
		get
		{
			return _doormaker ?? throw new InvalidOperationException();
		}
		private set
		{
			AssertMutable();
			_doormaker = value;
		}
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<DoorRevivalPower>(base.Creature, 1m, base.Creature, null);
		Doormaker monster = (Doormaker)ModelDb.Monster<Doormaker>().ToMutable();
		Doormaker = base.CombatState.CreateCreature(monster, CombatSide.Enemy, "doormaker");
	}

	public void PrepareForRevival()
	{
		if (_dramaticOpenMove != null)
		{
			DeadState.FollowUpState = _dramaticOpenMove;
		}
	}

	public void PrepareForDeath()
	{
		DeadState.FollowUpState = DeadState;
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		_dramaticOpenMove = new MoveState("DRAMATIC_OPEN_MOVE", DramaticOpenMove, new SingleAttackIntent(DramaticOpenDamage));
		MoveState dramaticOpenMove = _dramaticOpenMove;
		MoveState moveState = new MoveState("ENFORCE_MOVE", EnforceMove, new SingleAttackIntent(EnforceDamage), new BuffIntent());
		MoveState moveState2 = new MoveState("DOOR_SLAM_MOVE", DoorSlamMove, new MultiAttackIntent(DoorSlamDamage, DoorSlamRepeat));
		DeadState = new MoveState("DEAD_MOVE", DeadMove);
		DeadState.FollowUpState = DeadState;
		dramaticOpenMove.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState;
		moveState.FollowUpState = dramaticOpenMove;
		list.Add(dramaticOpenMove);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(DeadState);
		return new MonsterMoveStateMachine(list, dramaticOpenMove);
	}

	private Task DeadMove(IReadOnlyList<Creature> targets)
	{
		return Task.CompletedTask;
	}

	private async Task DramaticOpenMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(DramaticOpenDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task EnforceMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(EnforceDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, EnforceStrength, base.Creature, null);
	}

	private async Task DoorSlamMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(DoorSlamDamage).WithHitCount(DoorSlamRepeat).FromMonster(this)
			.WithAttackerAnim("Attack", 0.15f)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	public void Close()
	{
		(NCombatRoom.Instance?.GetCreatureNode(base.Creature))?.SetVisible(visible: true);
	}

	public void Open()
	{
		(NCombatRoom.Instance?.GetCreatureNode(base.Creature))?.SetVisible(visible: false);
	}
}
