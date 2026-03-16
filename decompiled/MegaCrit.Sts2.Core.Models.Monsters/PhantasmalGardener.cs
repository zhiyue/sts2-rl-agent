using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class PhantasmalGardener : MonsterModel
{
	private int _enlargeTriggers;

	private const string _attackMultiTrigger = "AttackMulti";

	public const string blockStartTrigger = "BlockStart";

	public const string blockEndTrigger = "BlockEnd";

	private const string _biteSfx = "event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_attack_bite";

	private const string _lickSfx = "event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_attack_lick";

	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_buff";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 29, 28);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 33, 32);

	private int BiteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 5);

	private int LashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 7);

	private int FlailDamage => 1;

	private int FlailRepeat => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 3);

	private int EnlargeStr => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

	private int SkittishAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 7, 6);

	public int EnlargeTriggers
	{
		get
		{
			return _enlargeTriggers;
		}
		set
		{
			AssertMutable();
			_enlargeTriggers = value;
		}
	}

	public override bool ShouldFadeAfterDeath => false;

	public float CurrentScale { get; private set; } = 1f;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_die";

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		MegaSkeleton skeleton = visuals.SpineBody.GetSkeleton();
		MegaSkin megaSkin = visuals.SpineBody.NewSkin("custom-skin");
		MegaSkeletonDataResource data = skeleton.GetData();
		string slotName = base.Creature.SlotName;
		if ((slotName == "first" || slotName == "third") ? true : false)
		{
			megaSkin.AddSkin(data.FindSkin("tall"));
		}
		else
		{
			megaSkin.AddSkin(data.FindSkin("short"));
		}
		skeleton.SetSkin(megaSkin);
		skeleton.SetSlotsToSetupPose();
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<SkittishPower>(base.Creature, SkittishAmount, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("BITE_MOVE", BiteMove, new SingleAttackIntent(BiteDamage));
		MoveState moveState2 = new MoveState("LASH_MOVE", LashMove, new SingleAttackIntent(LashDamage));
		MoveState moveState3 = new MoveState("FLAIL_MOVE", FlailMove, new MultiAttackIntent(FlailDamage, FlailRepeat));
		MoveState moveState4 = new MoveState("ENLARGE_MOVE", EnlargeMove, new BuffIntent());
		ConditionalBranchState conditionalBranchState = new ConditionalBranchState("INIT_MOVE");
		conditionalBranchState.AddState(moveState3, () => base.Creature.SlotName == "first");
		conditionalBranchState.AddState(moveState, () => base.Creature.SlotName == "second");
		conditionalBranchState.AddState(moveState2, () => base.Creature.SlotName == "third");
		conditionalBranchState.AddState(moveState4, () => base.Creature.SlotName == "fourth");
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState4);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, conditionalBranchState);
	}

	private async Task BiteMove(IReadOnlyList<Creature> targets)
	{
		NCombatRoom.Instance?.GetCreatureNode(base.Creature)?.SetDefaultScaleTo(CurrentScale, 0.75f);
		await DamageCmd.Attack(BiteDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_attack_bite")
			.WithHitFx("vfx/vfx_bite")
			.Execute(null);
	}

	private async Task LashMove(IReadOnlyList<Creature> targets)
	{
		NCombatRoom.Instance?.GetCreatureNode(base.Creature)?.SetDefaultScaleTo(CurrentScale, 0.75f);
		await DamageCmd.Attack(LashDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_attack_bite")
			.WithHitFx("vfx/vfx_bite")
			.Execute(null);
	}

	private async Task FlailMove(IReadOnlyList<Creature> targets)
	{
		NCombatRoom.Instance?.GetCreatureNode(base.Creature)?.SetDefaultScaleTo(CurrentScale, 0.35f);
		await DamageCmd.Attack(FlailDamage).WithHitCount(FlailRepeat).OnlyPlayAnimOnce()
			.FromMonster(this)
			.WithAttackerAnim("AttackMulti", 0.3f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_attack_lick")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task EnlargeMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_buff");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 1.5f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, EnlargeStr, base.Creature, null);
		EnlargeTriggers++;
		CurrentScale = 1f + 0.1f * (float)Math.Log(EnlargeTriggers + 1);
		NCombatRoom.Instance?.GetCreatureNode(base.Creature)?.SetDefaultScaleTo(CurrentScale, 0.75f);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("buff");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("attack_multi");
		AnimState animState5 = new AnimState("hurt_extended");
		AnimState animState6 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState nextState = new AnimState("block_loop", isLooping: true);
		AnimState animState7 = new AnimState("block_start");
		AnimState animState8 = new AnimState("block_end");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		animState6.NextState = nextState;
		animState7.NextState = nextState;
		animState8.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("AttackMulti", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5, () => !base.Creature.GetPower<SkittishPower>().HasGainedBlockThisTurn);
		creatureAnimator.AddAnyState("Hit", animState6, () => base.Creature.GetPower<SkittishPower>().HasGainedBlockThisTurn);
		creatureAnimator.AddAnyState("BlockStart", animState7);
		creatureAnimator.AddAnyState("BlockEnd", animState8);
		return creatureAnimator;
	}
}
