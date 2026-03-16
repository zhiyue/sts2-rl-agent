using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class SnappingJaxfruit : MonsterModel
{
	private const string _chargeTrigger = "Charge";

	private const string _chargedSfx = "event:/sfx/enemy/enemy_attacks/orb_plant/orb_plant_charged_loop";

	private const string _idleLoopSfx = "event:/sfx/enemy/enemy_attacks/orb_plant/orb_plant_idle_loop";

	private bool _isCharged;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 34, 31);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 36, 33);

	private int EnergyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Plant;

	private bool IsCharged
	{
		get
		{
			return _isCharged;
		}
		set
		{
			AssertMutable();
			_isCharged = value;
		}
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		SfxCmd.PlayLoop("event:/sfx/enemy/enemy_attacks/orb_plant/orb_plant_idle_loop");
	}

	public override void BeforeRemovedFromRoom()
	{
		SfxCmd.StopLoop("event:/sfx/enemy/enemy_attacks/orb_plant/orb_plant_idle_loop");
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("ENERGY_ORB_MOVE", EnergyOrb, new SingleAttackIntent(EnergyDamage), new BuffIntent());
		moveState.FollowUpState = moveState;
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	public async Task EnergyOrb(IReadOnlyList<Creature> targets)
	{
		if (TestMode.IsOff)
		{
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(base.Creature);
			Creature target = LocalContext.GetMe(base.CombatState)?.Creature;
			creatureNode.GetSpecialNode<NSnappingJaxfruitVfx>("Visuals/NSnappingJaxfruitVfx")?.SetTarget(target);
		}
		IsCharged = true;
		await DamageCmd.Attack(EnergyDamage).FromMonster(this).WithAttackerAnim("Cast", 0.25f)
			.Execute(null);
		IsCharged = false;
		await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState3 = new AnimState("charge_up");
		AnimState nextState = new AnimState("charged_loop", isLooping: true);
		AnimState animState4 = new AnimState("hurt_charged");
		AnimState animState5 = new AnimState("cast");
		animState2.NextState = animState;
		animState3.NextState = nextState;
		animState4.NextState = nextState;
		animState5.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Charge", animState3);
		creatureAnimator.AddAnyState("Cast", animState5);
		creatureAnimator.AddAnyState("Hit", animState4, () => IsCharged);
		creatureAnimator.AddAnyState("Hit", animState2, () => !IsCharged);
		return creatureAnimator;
	}
}
