using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class MechaKnight : MonsterModel
{
	private const int _flamethrowerCardCount = 4;

	private const int _windupBlock = 15;

	private const string _windUpTrigger = "windUp";

	private const string _flameAttackTrigger = "flamethrower";

	private const string _chargeTrigger = "charge";

	private bool _isWoundUp;

	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_buff";

	private const string _dashSfx = "event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_dash";

	private const string _flamethrowerSfx = "event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_flamethrower";

	private const string _heavyAttackSfx = "event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_heavy_attack";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 320, 300);

	public override int MaxInitialHp => MinInitialHp;

	private static int ChargeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 30, 25);

	private static int HeavyCleaveDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 40, 35);

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_die";

	public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_hurt";

	private bool IsWoundUp
	{
		get
		{
			return _isWoundUp;
		}
		set
		{
			AssertMutable();
			_isWoundUp = value;
		}
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<ArtifactPower>(base.Creature, 3m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("CHARGE_MOVE", ChargeMove, new SingleAttackIntent(ChargeDamage));
		MoveState moveState2 = new MoveState("FLAMETHROWER_MOVE", FlamethrowerMove, new StatusIntent(4));
		MoveState moveState3 = new MoveState("WINDUP_MOVE", WindupMove, new DefendIntent(), new BuffIntent());
		MoveState moveState4 = new MoveState("HEAVY_CLEAVE_MOVE", HeavyCleaveMove, new SingleAttackIntent(HeavyCleaveDamage));
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(moveState4);
		list.Add(moveState3);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task ChargeMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ChargeDamage).FromMonster(this).WithAttackerAnim("charge", 0.25f)
			.WithWaitBeforeHit(0.5f, 1f)
			.WithHitVfxNode((Creature _) => NSpikeSplashVfx.Create(base.Creature, VfxColor.Gold))
			.WithHitFx(null, "event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_dash")
			.WithHitVfxNode((Creature _) => NBigSlashImpactVfx.Create(VfxCmd.GetSideCenter(CombatSide.Player, base.CombatState).Value, 180f, new Color("#80dbff")))
			.Execute(null);
	}

	private async Task HeavyCleaveMove(IReadOnlyList<Creature> targets)
	{
		IsWoundUp = false;
		await DamageCmd.Attack(HeavyCleaveDamage).FromMonster(this).WithAttackerAnim("Attack", 0.4f)
			.WithHitFx(null, "event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_heavy_attack")
			.AfterAttackerAnim(delegate
			{
				NCombatRoom.Instance?.RadialBlur(VfxPosition.Left);
				NGame.Instance?.DoHitStop(ShakeStrength.Strong, ShakeDuration.Normal);
				return Task.CompletedTask;
			})
			.WithHitVfxNode((Creature _) => NBigSlashVfx.Create(VfxCmd.GetSideCenter(CombatSide.Player, base.CombatState).Value, facingRight: false))
			.WithHitVfxNode((Creature _) => NBigSlashImpactVfx.Create(VfxCmd.GetSideCenter(CombatSide.Player, base.CombatState).Value, 180f, new Color("#80dbff")))
			.Execute(null);
	}

	private async Task WindupMove(IReadOnlyList<Creature> targets)
	{
		IsWoundUp = true;
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_buff");
		await CreatureCmd.TriggerAnim(base.Creature, "windUp", 0.5f);
		await CreatureCmd.GainBlock(base.Creature, 15m, ValueProp.Move, null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 5m, base.Creature, null);
	}

	private async Task FlamethrowerMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_flamethrower");
		await CreatureCmd.TriggerAnim(base.Creature, "flamethrower", 1.5f);
		await CardPileCmd.AddToCombatAndPreview<Burn>(targets, PileType.Hand, 4, addedByPlayer: false);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState3 = new AnimState("attack_flame");
		AnimState animState4 = new AnimState("attack_cleave");
		AnimState animState5 = new AnimState("charge");
		AnimState animState6 = new AnimState("wind_up");
		AnimState animState7 = new AnimState("idle_loop_wound", isLooping: true);
		AnimState animState8 = new AnimState("hurt_wound");
		animState4.NextState = animState;
		animState5.NextState = animState;
		animState3.NextState = animState;
		animState2.NextState = animState;
		animState6.NextState = animState7;
		animState8.NextState = animState7;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Attack", animState4);
		creatureAnimator.AddAnyState("flamethrower", animState3);
		creatureAnimator.AddAnyState("charge", animState5);
		creatureAnimator.AddAnyState("windUp", animState6);
		animState.AddBranch("Hit", animState2, () => !IsWoundUp);
		animState3.AddBranch("Hit", animState2, () => !IsWoundUp);
		animState2.AddBranch("Hit", animState2, () => !IsWoundUp);
		animState4.AddBranch("Hit", animState2, () => !IsWoundUp);
		animState6.AddBranch("Hit", animState2, () => !IsWoundUp);
		animState7.AddBranch("Hit", animState2, () => !IsWoundUp);
		animState8.AddBranch("Hit", animState2, () => !IsWoundUp);
		animState.AddBranch("Hit", animState8, () => IsWoundUp);
		animState3.AddBranch("Hit", animState8, () => IsWoundUp);
		animState2.AddBranch("Hit", animState8, () => IsWoundUp);
		animState4.AddBranch("Hit", animState8, () => IsWoundUp);
		animState6.AddBranch("Hit", animState8, () => IsWoundUp);
		animState7.AddBranch("Hit", animState8, () => IsWoundUp);
		animState8.AddBranch("Hit", animState8, () => IsWoundUp);
		return creatureAnimator;
	}
}
