using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class SnappingJaxfruit : MonsterModel
{
	private const string _idleLoopSfx = "event:/sfx/enemy/enemy_attacks/orb_plant/orb_plant_idle_loop";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 34, 31);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 36, 33);

	private int EnergyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Plant;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		SfxCmd.PlayLoop(base.Creature, "event:/sfx/enemy/enemy_attacks/orb_plant/orb_plant_idle_loop");
	}

	public override void BeforeRemovedFromRoom()
	{
		SfxCmd.StopLoop(base.Creature, "event:/sfx/enemy/enemy_attacks/orb_plant/orb_plant_idle_loop");
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
			NCreature creatureNode = base.Creature.GetCreatureNode();
			if (creatureNode != null)
			{
				Creature target = LocalContext.GetMe(base.CombatState)?.Creature;
				creatureNode.GetSpecialNode<NSnappingJaxfruitVfx>("Visuals/NSnappingJaxfruitVfx")?.SetTarget(target);
			}
		}
		await DamageCmd.Attack(EnergyDamage).FromMonster(this).WithAttackerAnim("Cast", 0.25f)
			.Execute(null);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), base.Creature, 2m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState3 = new AnimState("cast");
		animState2.NextState = animState;
		animState3.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Cast", animState3);
		creatureAnimator.AddAnyState("Hit", animState2);
		return creatureAnimator;
	}
}
