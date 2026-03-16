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
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class WaterfallGiant : MonsterModel
{
	private const string _waterfallGiantTrackName = "waterfall_giant_progress";

	private const int _endCombatBgmFlag = 5;

	private const int _maxIntensityBgmFlag = 2;

	private const int _increaseIntensityBgmFlag = 1;

	private const int _increaseIntensityAmbienceFlag = 1;

	private const int _maxIntensityAmbienceFlag = 3;

	private const int _endAmbienceFlag = 2;

	private int _currentPressureGunDamage;

	private int _steamEruptionDamage;

	private MoveState _aboutToBlowState;

	private bool _isAboutToBlow;

	private int _pressureBuildupIdx;

	private const int _maxPressureBuildup = 6;

	private const string _attackBuffTrigger = "AttackBuff";

	private const string _attackDebuffTrigger = "AttackDebuff";

	private const string _healTrigger = "Heal";

	private const string _eruptTrigger = "Erupt";

	private const string _attackKickSfx = "event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_attack_kick";

	private const string _attackStompSfx = "event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_attack_stomp";

	private const string _eruptionSfx = "event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_eruption";

	private const string _knockoutSfx = "event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_knockout";

	private const string _ambientSfx = "event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_ambient";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 260, 250);

	public override int MaxInitialHp => MinInitialHp;

	private int PressurizeAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 15);

	private int StompDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 15);

	private int RamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);

	private int PressureUpDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 13);

	private int BasePressureGunDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 23, 20);

	public override bool ShouldDisappearFromDoom => !base.Creature.HasPower<SteamEruptionPower>();

	private int PressureGunIncrease => 5;

	private int SiphonHeal => 15;

	private int CurrentPressureGunDamage
	{
		get
		{
			return _currentPressureGunDamage;
		}
		set
		{
			AssertMutable();
			_currentPressureGunDamage = value;
		}
	}

	private int SteamEruptionDamage
	{
		get
		{
			return _steamEruptionDamage;
		}
		set
		{
			AssertMutable();
			_steamEruptionDamage = value;
		}
	}

	private MoveState AboutToBlowState
	{
		get
		{
			return _aboutToBlowState;
		}
		set
		{
			AssertMutable();
			_aboutToBlowState = value;
		}
	}

	private bool IsAboutToBlow
	{
		get
		{
			return _isAboutToBlow;
		}
		set
		{
			AssertMutable();
			_isAboutToBlow = value;
		}
	}

	private int PressureBuildupIdx
	{
		get
		{
			return _pressureBuildupIdx;
		}
		set
		{
			AssertMutable();
			_pressureBuildupIdx = value;
		}
	}

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	public override bool HasDeathSfx => false;

	public override bool ShouldFadeAfterDeath => PressureBuildupIdx == 0;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		CurrentPressureGunDamage = BasePressureGunDamage;
		SfxCmd.PlayLoop("event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_ambient", usesLoopParam: false);
		base.Creature.Died += AfterDeath;
	}

	private void AfterDeath(Creature _)
	{
		if (!base.Creature.HasPower<SteamEruptionPower>())
		{
			base.Creature.Died -= AfterDeath;
			NRunMusicController.Instance?.UpdateMusicParameter("waterfall_giant_progress", 5f);
		}
	}

	public override void BeforeRemovedFromRoom()
	{
		StopAmbientSfx();
	}

	private void StopAmbientSfx()
	{
		SfxCmd.SetParam("event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_ambient", "waterfall_giant_sfx", 2f);
		SfxCmd.StopLoop("event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_ambient");
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("PRESSURIZE_MOVE", PressurizeMove, new BuffIntent());
		MoveState moveState2 = new MoveState("STOMP_MOVE", StompMove, new SingleAttackIntent(StompDamage), new DebuffIntent(), new BuffIntent());
		MoveState moveState3 = new MoveState("RAM_MOVE", RamMove, new SingleAttackIntent(RamDamage), new BuffIntent());
		MoveState moveState4 = new MoveState("SIPHON_MOVE", SiphonMove, new HealIntent(), new BuffIntent());
		MoveState moveState5 = new MoveState("PRESSURE_GUN_MOVE", PressureGunMove, new SingleAttackIntent(() => CurrentPressureGunDamage), new BuffIntent());
		MoveState moveState6 = new MoveState("PRESSURE_UP_MOVE", PressureUpMove, new SingleAttackIntent(PressureUpDamage), new BuffIntent());
		AboutToBlowState = new MoveState("ABOUT_TO_BLOW_MOVE", AboutToBlowMove, new StunIntent())
		{
			MustPerformOnceBeforeTransitioning = true
		};
		MoveState moveState7 = new MoveState("EXPLODE_MOVE", ExplodeMove, new DeathBlowIntent(() => SteamEruptionDamage));
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState5;
		moveState5.FollowUpState = moveState6;
		moveState6.FollowUpState = moveState2;
		AboutToBlowState.FollowUpState = moveState7;
		moveState7.FollowUpState = moveState7;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		list.Add(moveState5);
		list.Add(moveState6);
		list.Add(moveState7);
		list.Add(AboutToBlowState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task PressurizeMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_eruption");
		await CreatureCmd.TriggerAnim(base.Creature, "Heal", 0.8f);
		await PowerCmd.Apply<SteamEruptionPower>(base.Creature, PressurizeAmount, base.Creature, null);
		IncrementBuildUpAnimationTrack();
	}

	private async Task PressureUpMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(PressureUpDamage).FromMonster(this).WithAttackerAnim("AttackBuff", 0.15f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_attack_stomp")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<SteamEruptionPower>(base.Creature, 3m, base.Creature, null);
		IncrementBuildUpAnimationTrack();
	}

	private async Task StompMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(StompDamage).FromMonster(this).WithAttackerAnim("AttackDebuff", 0.3f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_attack_stomp")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<WeakPower>(targets, 1m, base.Creature, null);
		await PowerCmd.Apply<SteamEruptionPower>(base.Creature, 3m, base.Creature, null);
		IncrementBuildUpAnimationTrack();
	}

	private async Task RamMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(RamDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_attack_kick")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<SteamEruptionPower>(base.Creature, 3m, base.Creature, null);
		IncrementBuildUpAnimationTrack();
	}

	private async Task SiphonMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_eruption");
		await CreatureCmd.TriggerAnim(base.Creature, "Heal", 0.8f);
		await CreatureCmd.Heal(base.Creature, SiphonHeal * base.Creature.CombatState.Players.Count);
		await PowerCmd.Apply<SteamEruptionPower>(base.Creature, 3m, base.Creature, null);
		IncrementBuildUpAnimationTrack();
	}

	private async Task PressureGunMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(CurrentPressureGunDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_attack_kick")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		CurrentPressureGunDamage += PressureGunIncrease;
		await PowerCmd.Apply<SteamEruptionPower>(base.Creature, 3m, base.Creature, null);
		IncrementBuildUpAnimationTrack();
	}

	private async Task AboutToBlowMove(IReadOnlyList<Creature> targets)
	{
		SteamEruptionDamage = base.Creature.GetPowerAmount<SteamEruptionPower>();
		await PowerCmd.Remove<SteamEruptionPower>(base.Creature);
		PressureBuildupIdx = 6;
		IncrementBuildUpAnimationTrack();
	}

	private async Task ExplodeMove(IReadOnlyList<Creature> targets)
	{
		StopAmbientSfx();
		await DamageCmd.Attack(SteamEruptionDamage).FromMonster(this).WithAttackerAnim("Erupt", 0.1f)
			.WithAttackerFx(null, DeathSfx)
			.Execute(null);
		await CreatureCmd.Kill(base.Creature);
		NRunMusicController.Instance?.UpdateMusicParameter("waterfall_giant_progress", 5f);
	}

	public async Task TriggerAboutToBlowState()
	{
		IsAboutToBlow = true;
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_knockout");
		NRunMusicController.Instance?.UpdateMusicParameter("waterfall_giant_progress", 2f);
		await CreatureCmd.SetMaxAndCurrentHp(base.Creature, 999999999m);
		base.Creature.ShowsInfiniteHp = true;
		SetMoveImmediate(AboutToBlowState, forceTransition: true);
		SfxCmd.SetParam("event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_ambient", "waterfall_giant_sfx", 3f);
	}

	private void IncrementBuildUpAnimationTrack()
	{
		if (TestMode.IsOff)
		{
			NRunMusicController.Instance?.UpdateMusicParameter("waterfall_giant_progress", 1f);
			SfxCmd.SetParam("event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_ambient", "waterfall_giant_sfx", 1f);
			PressureBuildupIdx++;
			int value = Mathf.FloorToInt((float)PressureBuildupIdx * 0.5f);
			value = Mathf.Clamp(value, 1, 3);
			NCombatRoom.Instance.GetCreatureNode(base.Creature)?.SpineController.GetAnimationState().SetAnimation($"_tracks/buildup{value}", loop: true, 1);
		}
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("attack_buff");
		AnimState animState5 = new AnimState("attack_debuff");
		AnimState animState6 = new AnimState("heal");
		AnimState animState7 = new AnimState("hurt");
		AnimState animState8 = new AnimState("die");
		AnimState nextState = new AnimState("die_loop", isLooping: true);
		AnimState state = new AnimState("erupt");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		animState6.NextState = animState;
		animState7.NextState = animState;
		animState8.NextState = nextState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Dead", animState8, () => !IsAboutToBlow);
		creatureAnimator.AddAnyState("Hit", animState7, () => !IsAboutToBlow);
		creatureAnimator.AddAnyState("AttackBuff", animState4);
		creatureAnimator.AddAnyState("AttackDebuff", animState5);
		creatureAnimator.AddAnyState("Heal", animState6);
		creatureAnimator.AddAnyState("Erupt", state);
		return creatureAnimator;
	}
}
