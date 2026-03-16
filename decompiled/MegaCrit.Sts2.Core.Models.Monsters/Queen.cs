using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Queen : MonsterModel
{
	private const string _queenTrackName = "queen_progress";

	private const string _castSfx = "event:/sfx/enemy/enemy_attacks/queen/queen_cast";

	private const int _offWithYourHeadRepeat = 5;

	private bool _hasAmalgamDied;

	private Creature? _amalgam;

	private MoveState _burnBrightForMeState;

	private MoveState _enragedState;

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/queen/queen_arms_attack";

	public override IEnumerable<string> AssetPaths => base.AssetPaths.Concat(ModelDb.Monster<TorchHeadAmalgam>().AssetPaths);

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 419, 400);

	public override int MaxInitialHp => MinInitialHp;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	private int OffWithYourHeadDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	private int ExecutionDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 15);

	private bool HasAmalgamDied
	{
		get
		{
			return _hasAmalgamDied;
		}
		set
		{
			AssertMutable();
			_hasAmalgamDied = value;
		}
	}

	private Creature? Amalgam
	{
		get
		{
			return _amalgam;
		}
		set
		{
			AssertMutable();
			_amalgam = value;
		}
	}

	private MoveState BurnBrightForMeState
	{
		get
		{
			return _burnBrightForMeState;
		}
		set
		{
			AssertMutable();
			_burnBrightForMeState = value;
		}
	}

	private MoveState EnragedState
	{
		get
		{
			return _enragedState;
		}
		set
		{
			AssertMutable();
			_enragedState = value;
		}
	}

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		visuals.SpineBody.GetAnimationState().SetAnimation("tracks/writhe", loop: true, 1);
	}

	public override void BeforeRemovedFromRoom()
	{
		if (!base.CombatState.RunState.IsGameOver)
		{
			NCombatRoom.Instance.GetCreatureNode(base.Creature)?.SpineController.GetAnimationState().SetAnimation("tracks/empty", loop: true, 1);
		}
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		base.Creature.Died += AfterDeath;
		Amalgam = base.CombatState.Enemies.First((Creature c) => c.Monster is TorchHeadAmalgam);
		Amalgam.Died += AmalgamDeathResponse;
		NRunMusicController.Instance?.UpdateMusicParameter("queen_progress", 1f);
	}

	private void AfterDeath(Creature _)
	{
		base.Creature.Died -= AfterDeath;
		NRunMusicController.Instance?.UpdateMusicParameter("queen_progress", 5f);
		NCombatRoom.Instance.GetCreatureNode(base.Creature)?.SpineController.GetAnimationState().SetAnimation("tracks/empty", loop: true, 1);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("PUPPET_STRINGS_MOVE", PuppetStringsMove, new CardDebuffIntent());
		MoveState moveState2 = new MoveState("YOUR_MINE_MOVE", YoureMineMove, new DebuffIntent());
		ConditionalBranchState conditionalBranchState = new ConditionalBranchState("YOURE_MINE_NOW_BRANCH");
		BurnBrightForMeState = new MoveState("BURN_BRIGHT_FOR_ME_MOVE", BurnBrightForMeMove, new BuffIntent(), new DefendIntent());
		ConditionalBranchState conditionalBranchState2 = new ConditionalBranchState("BURN_BRIGHT_FOR_ME_BRANCH");
		MoveState moveState3 = new MoveState("OFF_WITH_YOUR_HEAD_MOVE", OffWithYourHeadMove, new MultiAttackIntent(OffWithYourHeadDamage, 5));
		MoveState moveState4 = new MoveState("EXECUTION_MOVE", ExecutionMove, new SingleAttackIntent(ExecutionDamage));
		EnragedState = new MoveState("ENRAGE_MOVE", EnrageMove, new BuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = conditionalBranchState;
		conditionalBranchState.AddState(BurnBrightForMeState, () => !HasAmalgamDied);
		conditionalBranchState.AddState(moveState3, () => HasAmalgamDied);
		BurnBrightForMeState.FollowUpState = conditionalBranchState2;
		conditionalBranchState2.AddState(BurnBrightForMeState, () => !HasAmalgamDied);
		conditionalBranchState2.AddState(moveState3, () => HasAmalgamDied);
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = EnragedState;
		EnragedState.FollowUpState = moveState3;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(BurnBrightForMeState);
		list.Add(conditionalBranchState2);
		list.Add(conditionalBranchState);
		list.Add(moveState3);
		list.Add(moveState4);
		list.Add(EnragedState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task PuppetStringsMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/queen/queen_cast");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.5f);
		await PowerCmd.Apply<ChainsOfBindingPower>(targets, 3m, base.Creature, null);
	}

	private async Task YoureMineMove(IReadOnlyList<Creature> targets)
	{
		LocString line = MonsterModel.L10NMonsterLookup("QUEEN.banter");
		TalkCmd.Play(line, base.Creature, -1.0, VfxColor.Purple);
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/queen/queen_cast");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.5f);
		await PowerCmd.Apply<FrailPower>(targets, 99m, base.Creature, null);
		await PowerCmd.Apply<WeakPower>(targets, 99m, base.Creature, null);
		await PowerCmd.Apply<VulnerablePower>(targets, 99m, base.Creature, null);
	}

	private async Task BurnBrightForMeMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/queen/queen_cast");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.8f);
		int strengthAmount = AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 1, 1);
		List<Creature> list = base.Creature.CombatState.GetTeammatesOf(base.Creature).ToList();
		foreach (Creature item in list)
		{
			if (item != base.Creature)
			{
				await PowerCmd.Apply<StrengthPower>(item, strengthAmount, base.Creature, null);
			}
		}
		await CreatureCmd.GainBlock(base.Creature, 20m, ValueProp.Move, null);
	}

	private async Task OffWithYourHeadMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(OffWithYourHeadDamage).WithHitCount(5).FromMonster(this)
			.WithAttackerAnim("Attack", 0.6f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task ExecutionMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ExecutionDamage).FromMonster(this).WithAttackerAnim("Attack", 0.6f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task EnrageMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/queen/queen_cast");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.5f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
	}

	private void AmalgamDeathResponse(Creature _)
	{
		NRunMusicController.Instance?.UpdateMusicParameter("queen_progress", 2f);
		Amalgam.Died -= AmalgamDeathResponse;
		if (!base.Creature.IsDead)
		{
			HasAmalgamDied = true;
			Amalgam = null;
			LocString line = MonsterModel.L10NMonsterLookup("QUEEN.amalgamDeathSpeakLine");
			TalkCmd.Play(line, base.Creature, -1.0, VfxColor.Purple);
			if (base.NextMove == BurnBrightForMeState)
			{
				SetMoveImmediate(EnragedState);
			}
		}
	}
}
