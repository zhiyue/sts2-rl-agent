using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class GasBomb : MonsterModel
{
	private const string _explodeTrigger = "ExplodeTrigger";

	private bool _hasExploded;

	private const string _explodeSfx = "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_explode";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 12, 10);

	public override int MaxInitialHp => MinInitialHp;

	private int ExplodeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	private bool HasExploded
	{
		get
		{
			return _hasExploded;
		}
		set
		{
			AssertMutable();
			_hasExploded = value;
		}
	}

	public override bool ShouldFadeAfterDeath => false;

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_minion_die";

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<MinionPower>(base.Creature, 1m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("EXPLODE_MOVE", ExplodeMove, new DeathBlowIntent(() => ExplodeDamage));
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task ExplodeMove(IReadOnlyList<Creature> targets)
	{
		HasExploded = true;
		await DamageCmd.Attack(ExplodeDamage).FromMonster(this).WithAttackerAnim("ExplodeTrigger", 0.1f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/living_fog/living_fog_explode")
			.WithHitVfxNode((Creature _) => NGaseousImpactVfx.Create(CombatSide.Player, base.CombatState, new Color("#402f45")))
			.Execute(null);
		await CreatureCmd.Kill(base.Creature);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("spawn");
		AnimState state = new AnimState("explode");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state2 = new AnimState("die");
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState2.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState2, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Dead", state2, () => !HasExploded);
		creatureAnimator.AddAnyState("Hit", animState4);
		creatureAnimator.AddAnyState("ExplodeTrigger", state);
		return creatureAnimator;
	}
}
