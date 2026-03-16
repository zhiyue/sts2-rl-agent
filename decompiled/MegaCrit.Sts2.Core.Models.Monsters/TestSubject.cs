using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class TestSubject : MonsterModel
{
	private const string _testSubjectCustomTrackName = "test_subject_progress";

	private const int _baseTestSubjectNum = 8;

	private const int _phase3LacerateRepeat = 3;

	private const string _growthSpurtTrigger = "GrowthSpurtTrigger";

	private const string _bigAttackTrigger = "BiteTrigger";

	private const string _multiAttackTrigger = "MultiAttackTrigger";

	private const string _deadTrigger = "DeadTrigger";

	private const string _respawnTrigger = "RespawnTrigger";

	private const string _biteSfx = "event:/sfx/enemy/enemy_attacks/test_subject/test_subject_bite";

	private const string _slashSfx = "event:/sfx/enemy/enemy_attacks/test_subject/test_subject_slash";

	private const string _knockOutSfx = "event:/sfx/enemy/enemy_attacks/test_subject/test_subject_knock_out";

	private const string _reviveTwoHeadsSfx = "event:/sfx/enemy/enemy_attacks/test_subject/test_subject_revive_two_heads";

	private const string _reviveThreeHeadsSfx = "event:/sfx/enemy/enemy_attacks/test_subject/test_subject_revive_three_heads";

	private MoveState _deadState;

	private int _respawns;

	private int _extraMultiClawCount;

	public override LocString Title
	{
		get
		{
			LocString title = base.Title;
			title.Add("Count", SaveManager.Instance.Progress.TestSubjectKills + 8);
			return title;
		}
	}

	public override int MinInitialHp => FirstFormHp;

	public override int MaxInitialHp => MinInitialHp;

	public int FirstFormHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 111, 100);

	public int SecondFormHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 212, 200);

	public int ThirdFormHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 313, 300);

	private int EnrageAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

	private int BiteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 22, 20);

	private int SkullBashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 14);

	private int PounceDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 32, 30);

	private int MultiClawDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);

	private int BaseMultiClawCount => 3;

	private int Phase3LacerateDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);

	private int BigPounceDamage => 45;

	private int BurningGrowlBurnCount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 3);

	private int BurningGrowlStrengthGain => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

	public override string DeathSfx
	{
		get
		{
			if (!base.IsMutable)
			{
				return base.DeathSfx;
			}
			if (!base.Creature.HasPower<AdaptablePower>())
			{
				return base.DeathSfx;
			}
			return "event:/sfx/enemy/enemy_attacks/test_subject/test_subject_knock_out";
		}
	}

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	private MoveState DeadState
	{
		get
		{
			return _deadState;
		}
		set
		{
			AssertMutable();
			_deadState = value;
		}
	}

	private int Respawns
	{
		get
		{
			return _respawns;
		}
		set
		{
			AssertMutable();
			_respawns = value;
		}
	}

	private int ExtraMultiClawCount
	{
		get
		{
			return _extraMultiClawCount;
		}
		set
		{
			AssertMutable();
			_extraMultiClawCount = value;
		}
	}

	private int MultiClawTotalCount => BaseMultiClawCount + ExtraMultiClawCount;

	public override bool ShouldDisappearFromDoom => Respawns >= 2;

	public async Task TriggerDeadState()
	{
		NRunMusicController.Instance?.UpdateMusicParameter("test_subject_progress", 1f);
		base.CombatState.RunState.ExtraFields.TestSubjectKills++;
		await CreatureCmd.TriggerAnim(base.Creature, "DeadTrigger", 0f);
		SetMoveImmediate(DeadState, forceTransition: true);
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<AdaptablePower>(base.Creature, 1m, base.Creature, null);
		await PowerCmd.Apply<EnragePower>(base.Creature, EnrageAmount, base.Creature, null);
		base.Creature.Died += AfterDeath;
		base.Creature.PowerApplied += AfterPowerApplied;
		base.Creature.PowerRemoved += AfterPowerRemoved;
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		DeadState = new MoveState("RESPAWN_MOVE", RespawnMove, new HealIntent(), new BuffIntent())
		{
			MustPerformOnceBeforeTransitioning = true
		};
		MoveState moveState = new MoveState("BITE_MOVE", BiteMove, new SingleAttackIntent(BiteDamage));
		MoveState moveState2 = new MoveState("SKULL_BASH_MOVE", SkullBashMove, new SingleAttackIntent(SkullBashDamage), new DebuffIntent());
		MoveState moveState3 = new MoveState("POUNCE_MOVE", PounceMove, new SingleAttackIntent(PounceDamage));
		MoveState moveState4 = new MoveState("MULTI_CLAW_MOVE", MultiClawMove, new MultiAttackIntent(MultiClawDamage, () => MultiClawTotalCount));
		MoveState moveState5 = new MoveState("PHASE3_LACERATE_MOVE", Phase3LacerateMove, new MultiAttackIntent(Phase3LacerateDamage, 3));
		MoveState moveState6 = new MoveState("BIG_POUNCE_MOVE", BigPounceMove, new SingleAttackIntent(BigPounceDamage));
		MoveState moveState7 = new MoveState("BURNING_GROWL_MOVE", BurningGrowlMove, new StatusIntent(BurningGrowlBurnCount), new BuffIntent());
		ConditionalBranchState conditionalBranchState = new ConditionalBranchState("REVIVE_BRANCH");
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState3;
		moveState5.FollowUpState = moveState6;
		moveState6.FollowUpState = moveState7;
		moveState7.FollowUpState = moveState5;
		DeadState.FollowUpState = conditionalBranchState;
		conditionalBranchState.AddState(moveState4, () => Respawns < 2);
		conditionalBranchState.AddState(moveState5, () => Respawns >= 2);
		list.Add(DeadState);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		list.Add(moveState5);
		list.Add(moveState6);
		list.Add(moveState7);
		list.Add(conditionalBranchState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private void AfterDeath(Creature _)
	{
		if (!base.Creature.HasPower<AdaptablePower>())
		{
			base.Creature.Died -= AfterDeath;
			base.Creature.PowerApplied -= AfterPowerApplied;
			base.Creature.PowerRemoved -= AfterPowerRemoved;
			NRunMusicController.Instance?.UpdateMusicParameter("test_subject_progress", 5f);
			SetColor(Colors.White);
		}
	}

	private void AfterPowerApplied(PowerModel power)
	{
		if (power is IntangiblePower)
		{
			SetColor(StsColors.halfTransparentWhite);
		}
	}

	private void AfterPowerRemoved(PowerModel power)
	{
		if (power is IntangiblePower)
		{
			SetColor(Colors.White);
		}
	}

	private async Task RespawnMove(IReadOnlyList<Creature> targets)
	{
		Respawns++;
		NRunMusicController.Instance?.UpdateMusicParameter("test_subject_progress", 2f);
		SfxCmd.Play((Respawns == 1) ? "event:/sfx/enemy/enemy_attacks/test_subject/test_subject_revive_two_heads" : "event:/sfx/enemy/enemy_attacks/test_subject/test_subject_revive_three_heads");
		await CreatureCmd.TriggerAnim(base.Creature, "RespawnTrigger", 0f);
		await Cmd.Wait(0.8f);
		NCombatRoom.Instance?.GetCreatureNode(base.Creature)?.SetDefaultScaleTo(1f + (float)Respawns * 0.1f, 0.1f);
		await Cmd.Wait(1.15f);
		base.Creature.GetPower<AdaptablePower>().DoRevive();
		switch (Respawns)
		{
		case 1:
			await Revive(SecondFormHp);
			await PowerCmd.Apply<PainfulStabsPower>(base.Creature, 1m, base.Creature, null);
			break;
		case 2:
			await Revive(ThirdFormHp);
			await PowerCmd.Apply<NemesisPower>(base.Creature, 1m, base.Creature, null);
			await PowerCmd.Remove<AdaptablePower>(base.Creature);
			await PowerCmd.Remove<PainfulStabsPower>(base.Creature);
			break;
		}
	}

	private async Task BiteMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(BiteDamage).FromMonster(this).WithAttackerAnim("BiteTrigger", 0.25f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/test_subject/test_subject_bite")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task SkullBashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SkullBashDamage).FromMonster(this).WithAttackerAnim("BiteTrigger", 0.25f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/test_subject/test_subject_bite")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<VulnerablePower>(targets, 1m, base.Creature, null);
	}

	private async Task PounceMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(PounceDamage).FromMonster(this).WithAttackerAnim("BiteTrigger", 0.25f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/test_subject/test_subject_bite")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task MultiClawMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(MultiClawDamage).WithHitCount(MultiClawTotalCount).FromMonster(this)
			.OnlyPlayAnimOnce()
			.WithAttackerAnim("MultiAttackTrigger", 0.2f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/test_subject/test_subject_slash")
			.Execute(null);
		ExtraMultiClawCount++;
	}

	private async Task Phase3LacerateMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(Phase3LacerateDamage).WithHitCount(3).FromMonster(this)
			.OnlyPlayAnimOnce()
			.WithAttackerAnim("MultiAttackTrigger", 0.2f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/test_subject/test_subject_slash")
			.Execute(null);
	}

	private async Task BigPounceMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(BigPounceDamage).FromMonster(this).WithAttackerAnim("BiteTrigger", 0.25f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/test_subject/test_subject_bite")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task BurningGrowlMove(IReadOnlyList<Creature> targets)
	{
		await CardPileCmd.AddToCombatAndPreview<Burn>(targets, PileType.Discard, BurningGrowlBurnCount, addedByPlayer: false);
		await PowerCmd.Apply<StrengthPower>(base.Creature, BurningGrowlStrengthGain, base.Creature, null);
	}

	private async Task Revive(int baseRespawnHp)
	{
		AssertMutable();
		int scaledHp = baseRespawnHp * base.Creature.CombatState.Players.Count;
		await CreatureCmd.SetMaxHp(base.Creature, scaledHp);
		await CreatureCmd.Heal(base.Creature, scaledHp);
	}

	private void SetColor(Color color)
	{
		(NCombatRoom.Instance?.GetCreatureNode(base.Creature))?.GetSpecialNode<CanvasGroup>("%CanvasGroup")?.SetSelfModulate(color);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop1", isLooping: true);
		AnimState animState2 = new AnimState("hurt1");
		AnimState animState3 = new AnimState("attack_double1");
		AnimState animState4 = new AnimState("attack_big1");
		AnimState animState5 = new AnimState("heal1");
		AnimState animState6 = new AnimState("knockout1");
		AnimState nextState = new AnimState("knocked_out_loop1", isLooping: true);
		AnimState animState7 = new AnimState("regenerate1")
		{
			BoundsContainer = "RespawnBounds1"
		};
		AnimState nextState2 = new AnimState("idle_loop2", isLooping: true);
		AnimState animState8 = new AnimState("hurt2");
		AnimState animState9 = new AnimState("attack_double2");
		AnimState animState10 = new AnimState("attack_big2");
		AnimState animState11 = new AnimState("heal2");
		AnimState animState12 = new AnimState("knockout2");
		AnimState nextState3 = new AnimState("knocked_out_loop2", isLooping: true);
		AnimState animState13 = new AnimState("regenerate2")
		{
			BoundsContainer = "RespawnBounds2"
		};
		AnimState nextState4 = new AnimState("idle_loop3", isLooping: true);
		AnimState animState14 = new AnimState("hurt3");
		AnimState animState15 = new AnimState("attack_double3");
		AnimState animState16 = new AnimState("attack_big3");
		AnimState animState17 = new AnimState("heal3");
		AnimState state = new AnimState("die");
		animState5.NextState = animState;
		animState4.NextState = animState;
		animState3.NextState = animState;
		animState2.NextState = animState;
		animState6.NextState = nextState;
		animState7.NextState = nextState2;
		animState11.NextState = nextState2;
		animState10.NextState = nextState2;
		animState9.NextState = nextState2;
		animState8.NextState = nextState2;
		animState12.NextState = nextState3;
		animState13.NextState = nextState4;
		animState17.NextState = nextState4;
		animState16.NextState = nextState4;
		animState15.NextState = nextState4;
		animState14.NextState = nextState4;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Hit", animState2, () => Respawns == 0);
		creatureAnimator.AddAnyState("BiteTrigger", animState4, () => Respawns == 0);
		creatureAnimator.AddAnyState("MultiAttackTrigger", animState3, () => Respawns == 0);
		creatureAnimator.AddAnyState("GrowthSpurtTrigger", animState5, () => Respawns == 0);
		creatureAnimator.AddAnyState("DeadTrigger", animState6, () => Respawns == 0);
		creatureAnimator.AddAnyState("RespawnTrigger", animState7, () => Respawns == 1);
		creatureAnimator.AddAnyState("Hit", animState8, () => Respawns == 1);
		creatureAnimator.AddAnyState("BiteTrigger", animState10, () => Respawns == 1);
		creatureAnimator.AddAnyState("MultiAttackTrigger", animState9, () => Respawns == 1);
		creatureAnimator.AddAnyState("GrowthSpurtTrigger", animState11, () => Respawns == 1);
		creatureAnimator.AddAnyState("DeadTrigger", animState12, () => Respawns == 1);
		creatureAnimator.AddAnyState("RespawnTrigger", animState13, () => Respawns >= 2);
		creatureAnimator.AddAnyState("Hit", animState14, () => Respawns >= 2);
		creatureAnimator.AddAnyState("BiteTrigger", animState16, () => Respawns >= 2);
		creatureAnimator.AddAnyState("MultiAttackTrigger", animState15, () => Respawns >= 2);
		creatureAnimator.AddAnyState("GrowthSpurtTrigger", animState17, () => Respawns >= 2);
		creatureAnimator.AddAnyState("Dead", state);
		return creatureAnimator;
	}
}
