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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class LagavulinMatriarch : MonsterModel
{
	public const string slashMoveId = "SLASH_MOVE";

	private const string _sleepTrigger = "Sleep";

	public const string wakeTrigger = "Wake";

	private const string _attackHeavyTrigger = "AttackHeavy";

	private const string _attackDoubleTrigger = "AttackDouble";

	private const string _slamSfx = "event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_slam";

	private const string _castSfx = "event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_cast";

	public const string awakenSfx = "event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_awaken";

	private const string _attackStabSfx = "event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_attack_stab";

	private bool _isAwake;

	private NSleepingVfx? _sleepingVfx;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 233, 222);

	public override int MaxInitialHp => MinInitialHp;

	private int SlashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 21, 19);

	private int Slash2Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 12);

	private int Slash2Block => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 14, 12);

	private int DisembowelDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);

	private int DisembowelRepeat => 2;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.ArmorBig;

	public bool IsAwake
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

	private NSleepingVfx? SleepingVfx
	{
		get
		{
			return _sleepingVfx;
		}
		set
		{
			AssertMutable();
			_sleepingVfx = value;
		}
	}

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		visuals.SpineBody.GetAnimationState().SetAnimation("_tracks/eyes_closed_loop", loop: true, 1);
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await CreatureCmd.TriggerAnim(base.Creature, "Sleep", 0f);
		await PowerCmd.Apply<PlatingPower>(base.Creature, 12m, base.Creature, null);
		await PowerCmd.Apply<AsleepPower>(base.Creature, 3m, base.Creature, null);
		base.Creature.Died += AfterDeath;
		base.Creature.CurrentHpChanged += AfterHpChanged;
		base.Creature.CurrentHpChanged += AfterWokenUp;
		Marker2D marker2D = NCombatRoom.Instance.GetCreatureNode(base.Creature)?.GetSpecialNode<Marker2D>("%SleepVfxPos");
		if (marker2D != null)
		{
			SleepingVfx = NSleepingVfx.Create(marker2D.GlobalPosition);
			marker2D.AddChildSafely(SleepingVfx);
			SleepingVfx.Position = Vector2.Zero;
		}
	}

	private void AfterWokenUp(int _, int __)
	{
		base.Creature.CurrentHpChanged -= AfterWokenUp;
		SleepingVfx?.Stop();
		SleepingVfx = null;
	}

	private void AfterHpChanged(int _, int __)
	{
		if (base.Creature.CurrentHp <= base.Creature.MaxHp / 2)
		{
			base.Creature.CurrentHpChanged -= AfterHpChanged;
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(base.Creature);
			creatureNode?.SpineController.GetAnimationState().SetAnimation("_tracks/eyes_open", loop: false, 1);
			creatureNode?.SpineController.GetAnimationState().AddAnimation("_tracks/eyes_open_loop", 0f, loop: true, 1);
		}
	}

	private void AfterDeath(Creature _)
	{
		base.Creature.Died -= AfterDeath;
		NCombatRoom.Instance.GetCreatureNode(base.Creature)?.SpineController.GetAnimationState().SetAnimation("_tracks/eyes_dead", loop: false, 1);
		SleepingVfx?.Stop();
		SleepingVfx = null;
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SLEEP_MOVE", SleepMove, new SleepIntent());
		MoveState moveState2 = new MoveState("SLASH_MOVE", SlashMove, new SingleAttackIntent(SlashDamage));
		MoveState moveState3 = new MoveState("SLASH2_MOVE", Slash2Move, new SingleAttackIntent(Slash2Damage), new DefendIntent());
		MoveState moveState4 = new MoveState("DISEMBOWEL_MOVE", DisembowelMove, new MultiAttackIntent(DisembowelDamage, DisembowelRepeat));
		MoveState moveState5 = new MoveState("SOUL_SIPHON_MOVE", SoulSiphonMove, new DebuffIntent(), new BuffIntent());
		ConditionalBranchState conditionalBranchState = (ConditionalBranchState)(moveState.FollowUpState = new ConditionalBranchState("SLEEP_BRANCH"));
		moveState2.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState5;
		moveState5.FollowUpState = moveState2;
		conditionalBranchState.AddState(moveState, () => base.Creature.HasPower<AsleepPower>());
		conditionalBranchState.AddState(moveState2, () => !base.Creature.HasPower<AsleepPower>());
		list.Add(conditionalBranchState);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState5);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private Task SleepMove(IReadOnlyList<Creature> targets)
	{
		return Task.CompletedTask;
	}

	public async Task WakeUpMove(IReadOnlyList<Creature> _)
	{
		if (!_isAwake)
		{
			SfxCmd.Play("event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_awaken");
			await CreatureCmd.TriggerAnim(base.Creature, "Wake", 0.6f);
			SleepingVfx?.Stop();
			SleepingVfx = null;
			IsAwake = true;
		}
	}

	private async Task SlashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SlashDamage).FromMonster(this).WithAttackerAnim("AttackHeavy", 0.3f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_slam")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task Slash2Move(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(Slash2Damage).FromMonster(this).WithAttackerAnim("AttackHeavy", 0.2f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_slam")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await CreatureCmd.GainBlock(base.Creature, Slash2Block, ValueProp.Move, null);
	}

	private async Task DisembowelMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(DisembowelDamage).WithHitCount(DisembowelRepeat).FromMonster(this)
			.WithAttackerAnim("AttackDouble", 0.15f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_attack_stab")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task SoulSiphonMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_cast");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.6f);
		await PowerCmd.Apply<StrengthPower>(targets, -2m, base.Creature, null);
		await PowerCmd.Apply<DexterityPower>(targets, -2m, base.Creature, null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState state = new AnimState("sleep_loop", isLooping: true);
		AnimState animState = new AnimState("hurt_sleeping");
		AnimState animState2 = new AnimState("wake_up");
		AnimState animState3 = new AnimState("idle_loop", isLooping: true);
		AnimState animState4 = new AnimState("cast");
		AnimState animState5 = new AnimState("attack_heavy");
		AnimState animState6 = new AnimState("attack_double");
		AnimState animState7 = new AnimState("hurt");
		AnimState state2 = new AnimState("die");
		animState.NextState = animState2;
		animState2.NextState = animState3;
		animState4.NextState = animState3;
		animState5.NextState = animState3;
		animState6.NextState = animState3;
		animState7.NextState = animState3;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState3, controller);
		creatureAnimator.AddAnyState("Sleep", state);
		creatureAnimator.AddAnyState("Wake", animState2, () => !IsAwake);
		creatureAnimator.AddAnyState("Cast", animState4);
		creatureAnimator.AddAnyState("AttackHeavy", animState5);
		creatureAnimator.AddAnyState("AttackDouble", animState6);
		creatureAnimator.AddAnyState("Dead", state2);
		creatureAnimator.AddAnyState("Hit", animState7, () => IsAwake);
		creatureAnimator.AddAnyState("Hit", animState, () => !IsAwake);
		return creatureAnimator;
	}
}
