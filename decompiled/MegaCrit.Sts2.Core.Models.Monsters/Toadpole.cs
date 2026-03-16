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
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Toadpole : MonsterModel
{
	private static readonly string[] _eyeOptions = new string[2] { "eye1", "eye2" };

	private static readonly string[] _patternOptions = new string[2] { "pattern1", "pattern2" };

	private bool _isFront;

	private const string _attackSingleTrigger = "AttackSingle";

	private const string _attackTripleTrigger = "AttackTriple";

	private const string _spinAttackSfx = "event:/sfx/enemy/enemy_attacks/toadpole/toadpole_attack_spin";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 22, 21);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 26, 25);

	public bool IsFront
	{
		get
		{
			return _isFront;
		}
		set
		{
			AssertMutable();
			_isFront = value;
		}
	}

	private int SpikeSpitDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	private int SpikeSpitRepeat => 3;

	private int WhirlDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

	private int SpikenAmount => 2;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		MegaSkeleton skeleton = visuals.SpineBody.GetSkeleton();
		MegaSkin megaSkin = visuals.SpineBody.NewSkin("custom-skin");
		MegaSkeletonDataResource data = skeleton.GetData();
		megaSkin.AddSkin(data.FindSkin(MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextItem(_eyeOptions)));
		megaSkin.AddSkin(data.FindSkin(MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextItem(_patternOptions)));
		skeleton.SetSkin(megaSkin);
		skeleton.SetSlotsToSetupPose();
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SPIKE_SPIT_MOVE", SpikeSpitMove, new MultiAttackIntent(SpikeSpitDamage, SpikeSpitRepeat));
		MoveState moveState2 = new MoveState("WHIRL_MOVE", WhirlMove, new SingleAttackIntent(WhirlDamage));
		MoveState moveState3 = new MoveState("SPIKEN_MOVE", SpikenMove, new BuffIntent());
		ConditionalBranchState conditionalBranchState = new ConditionalBranchState("INIT_MOVE");
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState;
		moveState.FollowUpState = moveState2;
		conditionalBranchState.AddState(moveState2, () => !((Toadpole)base.Creature.Monster).IsFront);
		conditionalBranchState.AddState(moveState3, () => ((Toadpole)base.Creature.Monster).IsFront);
		list.Add(conditionalBranchState);
		list.Add(moveState3);
		list.Add(moveState);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, conditionalBranchState);
	}

	private async Task SpikeSpitMove(IReadOnlyList<Creature> targets)
	{
		await PowerCmd.Apply<ThornsPower>(base.Creature, -SpikenAmount, base.Creature, null);
		await DamageCmd.Attack(SpikeSpitDamage).WithHitCount(SpikeSpitRepeat).FromMonster(this)
			.WithAttackerAnim("AttackTriple", 0.3f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/toadpole/toadpole_attack_spin")
			.WithHitFx("vfx/vfx_attack_blunt", AttackSfx)
			.Execute(null);
	}

	private async Task WhirlMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(WhirlDamage).FromMonster(this).WithAttackerAnim("AttackSingle", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt", AttackSfx)
			.Execute(null);
	}

	private async Task SpikenMove(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.2f);
		await PowerCmd.Apply<ThornsPower>(base.Creature, SpikenAmount, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("buff");
		AnimState animState3 = new AnimState("attack_single");
		AnimState animState4 = new AnimState("attack_triple");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState nextState = new AnimState("idle_loop_buffed", isLooping: true);
		AnimState animState6 = new AnimState("attack_single_buffed");
		AnimState animState7 = new AnimState("attack_triple");
		AnimState animState8 = new AnimState("hurt_buffed");
		AnimState state2 = new AnimState("die_buffed");
		animState2.NextState = nextState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		animState6.NextState = nextState;
		animState7.NextState = animState;
		animState8.NextState = nextState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("AttackSingle", animState3, () => !base.Creature.HasPower<ThornsPower>());
		creatureAnimator.AddAnyState("AttackTriple", animState4, () => !base.Creature.HasPower<ThornsPower>());
		creatureAnimator.AddAnyState("Dead", state, () => !base.Creature.HasPower<ThornsPower>());
		creatureAnimator.AddAnyState("Hit", animState5, () => !base.Creature.HasPower<ThornsPower>());
		creatureAnimator.AddAnyState("AttackSingle", animState6, () => base.Creature.HasPower<ThornsPower>());
		creatureAnimator.AddAnyState("AttackTriple", animState7, () => base.Creature.HasPower<ThornsPower>());
		creatureAnimator.AddAnyState("Dead", state2, () => base.Creature.HasPower<ThornsPower>());
		creatureAnimator.AddAnyState("Hit", animState8, () => base.Creature.HasPower<ThornsPower>());
		return creatureAnimator;
	}
}
