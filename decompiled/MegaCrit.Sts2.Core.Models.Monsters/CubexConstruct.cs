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
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class CubexConstruct : MonsterModel
{
	private static readonly string[] _eyeOptions = new string[3] { "diamondeye", "circleeye", "squareeye" };

	private static readonly string[] _mossOptions = new string[3] { "moss1", "moss2", "moss3" };

	private const string _burrowTrigger = "Burrow";

	private const string _chargeTrigger = "Charge";

	private const string _attackEndTrigger = "AttackEnd";

	private const string _chargeStartAnimId = "charge_start";

	private const int _expelRepeats = 2;

	private const string _burrowSfx = "event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_burrow";

	private const string _chargedLoopSfx = "event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 70, 65);

	public override int MaxInitialHp => MinInitialHp;

	public override string BestiaryAttackAnimId => "charge_start";

	private int BlastDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

	private int ExpelDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		MegaSkeleton skeleton = visuals.SpineBody.GetSkeleton();
		MegaSkin megaSkin = visuals.SpineBody.NewSkin("custom-skin");
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
		await PowerCmd.Apply<ArtifactPower>(base.Creature, 1m, base.Creature, null);
		base.Creature.CurrentHpChanged += OnHpChanged;
	}

	public override void BeforeRemovedFromRoom()
	{
		SfxCmd.SetParam("event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack", "loop", 2f);
		SfxCmd.StopLoop("event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack");
		base.Creature.CurrentHpChanged -= OnHpChanged;
	}

	public void OnHpChanged(int oldHp, int newHp)
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
		MoveState moveState2 = new MoveState("REPEATER_MOVE", RepeaterBlastMove, new SingleAttackIntent(BlastDamage), new BuffIntent());
		MoveState moveState3 = new MoveState("REPEATER_MOVE_2", RepeaterBlastMove, new SingleAttackIntent(BlastDamage), new BuffIntent());
		MoveState moveState4 = new MoveState("EXPEL_BLAST", ExpelBlastMove, new MultiAttackIntent(ExpelDamage, 2));
		MoveState moveState5 = new MoveState("SUBMERGE_MOVE", SubmergeMove, new DefendIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState2;
		moveState5.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		list.Add(moveState5);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task ChargeUpMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.PlayLoop("event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack", usesLoopParam: false);
		await CreatureCmd.TriggerAnim(base.Creature, "Charge", 0f);
		await Cmd.Wait(0.75f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
	}

	private async Task RepeaterBlastMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.SetParam("event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack", "loop", 1f);
		await Cmd.Wait(0.4f);
		await DamageCmd.Attack(BlastDamage).FromMonster(this).WithAttackerAnim("Attack", 0f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(null);
		SfxCmd.SetParam("event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack", "loop", 0f);
		await Cmd.Wait(0.2f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
		await CreatureCmd.TriggerAnim(base.Creature, "AttackEnd", 0f);
	}

	private async Task ExpelBlastMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.SetParam("event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack", "loop", 1f);
		await Cmd.Wait(0.4f);
		await DamageCmd.Attack(ExpelDamage).WithHitCount(2).FromMonster(this)
			.WithAttackerAnim("Attack", 0f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(null);
		await Cmd.Wait(0.2f);
		await CreatureCmd.TriggerAnim(base.Creature, "AttackEnd", 0f);
	}

	private async Task SubmergeMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_burrow");
		await CreatureCmd.TriggerAnim(base.Creature, "Burrow", 0f);
		await Cmd.Wait(1.25f);
		await CreatureCmd.GainBlock(base.Creature, 15m, ValueProp.Move, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("burrowed_loop", isLooping: true)
		{
			BoundsContainer = "BurrowedBounds"
		};
		AnimState animState2 = new AnimState("burrow");
		AnimState animState3 = new AnimState("unburrow");
		AnimState animState4 = new AnimState("idle_loop", isLooping: true)
		{
			BoundsContainer = "IdleBounds"
		};
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState6 = new AnimState("hurt");
		AnimState animState7 = new AnimState("charge_start")
		{
			BoundsContainer = "ChargingBounds"
		};
		AnimState animState8 = new AnimState("charge_loop", isLooping: true);
		AnimState animState9 = new AnimState("attack_loop", isLooping: true);
		AnimState animState10 = new AnimState("attack_finish");
		animState2.NextState = animState;
		animState.AddBranch("Charge", animState3);
		animState3.NextState = animState7;
		animState4.AddBranch("Hit", animState5);
		animState4.AddBranch("Burrow", animState2);
		animState5.NextState = animState4;
		animState5.AddBranch("Hit", animState5);
		animState5.AddBranch("Burrow", animState2);
		animState7.NextState = animState8;
		animState8.AddBranch("Hit", animState6);
		animState8.AddBranch("Attack", animState9);
		animState9.AddBranch("AttackEnd", animState10);
		animState10.NextState = animState7;
		animState10.AddBranch("Hit", animState6);
		animState6.NextState = animState8;
		animState6.AddBranch("Hit", animState6);
		animState6.AddBranch("Attack", animState9);
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState2, controller);
		creatureAnimator.AddAnyState("Dead", state);
		return creatureAnimator;
	}
}
