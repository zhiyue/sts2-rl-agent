using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class CubexConstruct : MonsterModel
{
	private static readonly string[] _eyeOptions = new string[3] { "diamondeye", "circleeye", "squareeye" };

	private static readonly string[] _mossOptions = new string[3] { "moss1", "moss2", "moss3" };

	private const string _chargeTrigger = "Charge";

	private const string _attackEndTrigger = "AttackEnd";

	private const string _chargeStartAnimId = "charge_start";

	private const int _expelRepeats = 2;

	private const string _burrowSfx = "event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_burrow";

	private const string _chargedLoopSfx = "event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack";

	private bool _isBurrowed;

	private bool _isCharging;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 70, 65);

	public override int MaxInitialHp => MinInitialHp;

	private int BlastDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

	private int ExpelDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

	private bool IsBurrowed
	{
		get
		{
			return _isBurrowed;
		}
		set
		{
			AssertMutable();
			_isBurrowed = value;
		}
	}

	private bool IsCharging
	{
		get
		{
			return _isCharging;
		}
		set
		{
			AssertMutable();
			_isCharging = value;
		}
	}

	public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
	{
		MegaSkin megaSkin = spine.NewSkin("custom-skin");
		MegaSkeletonDataResource data = skeleton.GetData();
		megaSkin.AddSkin(data.FindSkin(MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextItem(_eyeOptions)));
		megaSkin.AddSkin(data.FindSkin(MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextItem(_mossOptions)));
		skeleton.SetSkin(megaSkin);
		skeleton.SetSlotsToSetupPose();
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await CreatureCmd.GainBlock(base.Creature, 13m, ValueProp.Move, null);
		await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
		base.Creature.CurrentHpChanged += OnHpChanged;
		IsBurrowed = true;
	}

	public override void BeforeRemovedFromRoom()
	{
		SfxCmd.StopLoop(base.Creature, "event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack");
		base.Creature.CurrentHpChanged -= OnHpChanged;
	}

	private void OnHpChanged(int oldHp, int newHp)
	{
		if (newHp < oldHp)
		{
			SfxCmd.SetParam("event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack", "enemy_hurt", 1f);
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("CHARGE_UP_MOVE", ChargeUpMove, new BuffIntent());
		MoveState moveState2 = new MoveState("REPEATER_BLAST_MOVE", RepeaterBlastMove, new SingleAttackIntent(BlastDamage), new BuffIntent());
		MoveState moveState3 = new MoveState("REPEATER_BLAST_MOVE_2", RepeaterBlastMove, new SingleAttackIntent(BlastDamage), new BuffIntent());
		MoveState moveState4 = new MoveState("EXPEL_MOVE", ExpelBlastMove, new MultiAttackIntent(ExpelDamage, 2));
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task ChargeUpMove(IReadOnlyList<Creature> targets)
	{
		IsBurrowed = false;
		IsCharging = true;
		SfxCmd.PlayLoop(base.Creature, "event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack", "loop", 2f);
		await CreatureCmd.TriggerAnim(base.Creature, "Charge", 0f);
		await Cmd.Wait(0.75f);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), base.Creature, 2m, base.Creature, null);
	}

	private async Task RepeaterBlastMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.SetParam("event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack", "loop", 1f);
		await Cmd.Wait(0.4f);
		await DamageCmd.Attack(BlastDamage).FromMonster(this).WithAttackerAnim("Attack", 0f)
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(null);
		SfxCmd.SetParam("event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack", "loop", 0f);
		await Cmd.Wait(0.2f);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), base.Creature, 2m, base.Creature, null);
		await CreatureCmd.TriggerAnim(base.Creature, "AttackEnd", 0f);
	}

	private async Task ExpelBlastMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.SetParam("event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack", "loop", 1f);
		await Cmd.Wait(0.4f);
		await DamageCmd.Attack(ExpelDamage).WithHitCount(2).FromMonster(this)
			.WithAttackerAnim("Attack", 0f)
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(null);
		await Cmd.Wait(0.2f);
		await CreatureCmd.TriggerAnim(base.Creature, "AttackEnd", 0f);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState nextState = new AnimState("burrowed_loop", isLooping: true)
		{
			BoundsContainer = "BurrowedBounds"
		};
		AnimState animState = new AnimState("burrow");
		AnimState animState2 = new AnimState("unburrow");
		AnimState nextState2 = new AnimState("idle_loop", isLooping: true)
		{
			BoundsContainer = "IdleBounds"
		};
		AnimState animState3 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState4 = new AnimState("hurt");
		AnimState animState5 = new AnimState("charge_start")
		{
			BoundsContainer = "ChargingBounds"
		};
		AnimState nextState3 = new AnimState("charge_loop", isLooping: true);
		AnimState state2 = new AnimState("attack_loop", isLooping: true);
		AnimState animState6 = new AnimState("attack_finish");
		animState.NextState = nextState;
		animState2.NextState = animState5;
		animState6.NextState = animState5;
		animState3.NextState = nextState2;
		animState5.NextState = nextState3;
		animState4.NextState = nextState3;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Charge", animState2);
		creatureAnimator.AddAnyState("Attack", state2);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("AttackEnd", animState6);
		creatureAnimator.AddAnyState("Hit", animState3, () => !IsBurrowed && !IsCharging);
		creatureAnimator.AddAnyState("Hit", animState4, () => !IsBurrowed && IsCharging);
		return creatureAnimator;
	}
}
