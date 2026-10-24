using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using MegaCrit.Sts2.Core.Nodes.Vfx.Backgrounds;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Crusher : MonsterModel
{
	public const string deathVfxPath = "vfx/monsters/kaiser_crab_boss_explosion";

	private const string _attackSlamSfx = "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_attack_slam";

	private const string _attackScoopSfx = "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_attack_scoop";

	private const string _attackScissorSfx = "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_attack_scissor";

	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_buff";

	private const string _slamSfx = "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_attack_slam";

	private NKaiserCrabBossBackground? _background;

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_die";

	public override IEnumerable<string> AssetPaths
	{
		get
		{
			int num = 1;
			List<string> list = new List<string>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<string> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = SceneHelper.GetScenePath("vfx/monsters/kaiser_crab_boss_explosion");
			List<string> first = list;
			return first.Concat(base.AssetPaths);
		}
	}

	public override bool ShouldFadeAfterDeath => false;

	public override bool ShouldDisappearFromDoom => false;

	public override float DeathAnimLengthOverride => 2.5f;

	private NKaiserCrabBossBackground Background
	{
		get
		{
			AssertMutable();
			if (_background == null)
			{
				Control control = NCombatRoom.Instance?.Background ?? NBestiary.Instance?.Layout ?? throw new InvalidOperationException();
				_background = control.GetNode<NKaiserCrabBossBackground>("%KaiserCrab");
			}
			return _background;
		}
	}

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 219, 209);

	public override int MaxInitialHp => MinInitialHp;

	private int ThrashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 12);

	private int EnlargingStrikeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 4);

	private int BugStingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);

	private int BugStingTimes => 2;

	private int AdaptStrengthGain => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

	private int GuardedStrikeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 12);

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("THRASH_MOVE", ThrashMove, new SingleAttackIntent(ThrashDamage));
		MoveState moveState2 = new MoveState("ENLARGING_STRIKE_MOVE", EnlargingStrikeMove, new SingleAttackIntent(EnlargingStrikeDamage));
		MoveState moveState3 = new MoveState("BUG_STING_MOVE", BugStingMove, new MultiAttackIntent(BugStingDamage, BugStingTimes), new DebuffIntent());
		MoveState moveState4 = new MoveState("ADAPT_MOVE", AdaptMove, new BuffIntent());
		MoveState moveState5 = new MoveState("GUARDED_STRIKE_MOVE", GuardedStrikeMove, new SingleAttackIntent(GuardedStrikeDamage), new DefendIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState5;
		moveState5.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		list.Add(moveState5);
		return new MonsterMoveStateMachine(list, moveState);
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<BackAttackLeftPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
		await PowerCmd.Apply<CrabRagePower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
		Background.SetVisible(visible: true);
	}

	public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
	{
		if (creature != base.Creature || delta >= 0m)
		{
			return Task.CompletedTask;
		}
		Background.PlayHurtAnim(NKaiserCrabBossBackground.ArmSide.Left);
		return Task.CompletedTask;
	}

	public override Task BeforeDeath(Creature creature)
	{
		if (creature != base.Creature)
		{
			return Task.CompletedTask;
		}
		NAudioManager.Instance.PlayOneShot(DeathSfx);
		Background.PlayArmDeathAnim(NKaiserCrabBossBackground.ArmSide.Left);
		if (CombatManager.Instance.IsOverOrEnding)
		{
			Background.PlayBodyDeathAnim();
			NRunMusicController.Instance?.UpdateMusicParameter("kaiser_crab_progress", 5f);
		}
		else
		{
			NRunMusicController.Instance?.UpdateMusicParameter("kaiser_crab_progress", 2f);
		}
		return Task.CompletedTask;
	}

	private async Task ThrashMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_attack_scoop");
		await Background.PlayAttackAnim(NKaiserCrabBossBackground.ArmSide.Left, "attack_heavy", 1f);
		await DamageCmd.Attack(ThrashDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_blunt", "event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_attack_slam")
			.Execute(null);
	}

	private async Task BugStingMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_attack_scissor");
		await Background.PlayAttackAnim(NKaiserCrabBossBackground.ArmSide.Left, "attack_double", 0.5f);
		await DamageCmd.Attack(BugStingDamage).WithHitCount(BugStingTimes).FromMonster(this)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, 2m, base.Creature, null);
		await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, 2m, base.Creature, null);
	}

	private async Task AdaptMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_buff");
		await Background.PlayAttackAnim(NKaiserCrabBossBackground.ArmSide.Left, "buff", 0.8f);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), base.Creature, AdaptStrengthGain, base.Creature, null);
	}

	private async Task EnlargingStrikeMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_attack_slam");
		await Background.PlayAttackAnim(NKaiserCrabBossBackground.ArmSide.Left, "attack_med", 0.65f);
		await DamageCmd.Attack(EnlargingStrikeDamage).FromMonster(this).WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
			.WithHitVfxSpawnedAtBase()
			.Execute(null);
	}

	private async Task GuardedStrikeMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_attack_slam");
		await Background.PlayAttackAnim(NKaiserCrabBossBackground.ArmSide.Left, "attack_med", 0.65f);
		await DamageCmd.Attack(GuardedStrikeDamage).FromMonster(this).WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
			.WithHitVfxSpawnedAtBase()
			.Execute(null);
		await CreatureCmd.GainBlock(base.Creature, 18m, ValueProp.Move, null);
	}
}
