using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class CeremonialBeast : MonsterModel
{
	private const string _plowTrigger = "Plow";

	private const string _plowEndTrigger = "EndPlow";

	private const string _stunTrigger = "Stun";

	private const string _unStunTrigger = "Unstun";

	private const string _plowHitTrigger = "PlowHit";

	private const string _plowSfx = "event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_plow";

	private const string _plowEndSfx = "event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_plow_end";

	private const string _shrillSfx = "event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_shrill";

	private const string _stunSfx = "event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_stun";

	private bool _isStunnedByPlowRemoval;

	private bool _inMidCharge;

	private MoveState _beastCryState;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 262, 252);

	public override int MaxInitialHp => MinInitialHp;

	private int PlowAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 160, 150);

	private int PlowDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 18);

	private int PlowStrength => 2;

	private int StompDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);

	private int CrushDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);

	private int CrushStrength => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	public override bool ShouldDisappearFromDoom => false;

	public override bool ShouldFadeAfterDeath => false;

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_die";

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

	private bool IsStunnedByPlowRemoval
	{
		get
		{
			return _isStunnedByPlowRemoval;
		}
		set
		{
			AssertMutable();
			_isStunnedByPlowRemoval = value;
		}
	}

	private bool ShouldPlayRegularHurtAnim
	{
		get
		{
			if (!IsStunnedByPlowRemoval)
			{
				return !InMidCharge;
			}
			return false;
		}
	}

	private bool InMidCharge
	{
		get
		{
			return _inMidCharge;
		}
		set
		{
			AssertMutable();
			_inMidCharge = value;
		}
	}

	public MoveState BeastCryState
	{
		get
		{
			return _beastCryState;
		}
		set
		{
			AssertMutable();
			_beastCryState = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("STAMP_MOVE", StampMove, new BuffIntent());
		MoveState moveState2 = new MoveState("PLOW_MOVE", PlowMove, new SingleAttackIntent(PlowDamage), new BuffIntent());
		MoveState moveState3 = new MoveState("STUN_MOVE", StunnedMove, new StunIntent())
		{
			MustPerformOnceBeforeTransitioning = true
		};
		BeastCryState = new MoveState("BEAST_CRY_MOVE", BeastCryMove, new DebuffIntent());
		MoveState moveState4 = new MoveState("STOMP_MOVE", StompMove, new SingleAttackIntent(StompDamage));
		MoveState moveState5 = new MoveState("CRUSH_MOVE", CrushMove, new SingleAttackIntent(CrushDamage), new BuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState2;
		moveState3.FollowUpState = BeastCryState;
		BeastCryState.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState5;
		moveState5.FollowUpState = BeastCryState;
		list.Add(moveState2);
		list.Add(moveState);
		list.Add(moveState3);
		list.Add(BeastCryState);
		list.Add(moveState4);
		list.Add(moveState5);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task StampMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(AttackSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Attack", 0.6f);
		await Cmd.CustomScaledWait(0f, 0.4f);
		await PowerCmd.Apply<PlowPower>(base.Creature, PlowAmount, base.Creature, null);
	}

	private async Task PlowMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_plow");
		InMidCharge = true;
		await CreatureCmd.TriggerAnim(base.Creature, "Plow", 0f);
		await Cmd.Wait(0.5f);
		NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NHorizontalLinesVfx.Create(new Color("BFFFC880"), 1.2000000476837158, movingRightwards: false));
		await Cmd.Wait(0.5f);
		NCombatRoom.Instance?.RadialBlur(VfxPosition.Left);
		VfxCmd.PlayOnCreatureCenters(targets, "vfx/vfx_attack_blunt");
		using (IEnumerator<Creature> enumerator = targets.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				Creature current = enumerator.Current;
				NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NLineBurstVfx.Create(current));
			}
		}
		NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal, 180f + MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat(-10f, 10f));
		await DamageCmd.Attack(PlowDamage).FromMonster(this).WithNoAttackerAnim()
			.Execute(null);
		NGame.Instance?.DoHitStop(ShakeStrength.Strong, ShakeDuration.Normal);
		InMidCharge = false;
		await Cmd.Wait(0.2f);
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_plow_end");
		await CreatureCmd.TriggerAnim(base.Creature, "EndPlow", 0f);
		await Cmd.Wait(0.5f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, PlowStrength, base.Creature, null);
	}

	public async Task SetStunned()
	{
		IsStunnedByPlowRemoval = true;
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_stun");
		await CreatureCmd.TriggerAnim(base.Creature, "Stun", 0.6f);
	}

	public async Task StunnedMove(IReadOnlyList<Creature> targets)
	{
		IsStunnedByPlowRemoval = false;
		await CreatureCmd.TriggerAnim(base.Creature, "Unstun", 0.6f);
	}

	private async Task BeastCryMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_shrill");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0f);
		await Cmd.Wait(0.3f);
		VfxCmd.PlayOnCreatureCenter(base.Creature, "vfx/vfx_scream");
		await Cmd.Wait(0.75f);
		await PowerCmd.Apply<RingingPower>(targets, 1m, base.Creature, null);
	}

	private async Task StompMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(StompDamage).FromMonster(this).WithAttackerAnim("Attack", 1f)
			.WithAttackerFx(null, AttackSfx)
			.AfterAttackerAnim(delegate
			{
				NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal, 180f + MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat(-10f, 10f));
				return Task.CompletedTask;
			})
			.WithHitFx("vfx/vfx_attack_slash")
			.WithHitVfxNode((Creature _) => NSpikeSplashVfx.Create(base.Creature, VfxColor.Cyan))
			.Execute(null);
	}

	private async Task CrushMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(AttackSfx);
		await DamageCmd.Attack(CrushDamage).FromMonster(this).WithAttackerAnim("Attack", 1f)
			.WithAttackerFx(null, AttackSfx)
			.AfterAttackerAnim(delegate
			{
				NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal, 180f + MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat(-10f, 10f));
				return Task.CompletedTask;
			})
			.WithHitFx("vfx/vfx_attack_slash")
			.WithHitVfxNode((Creature _) => NSpikeSplashVfx.Create(base.Creature, VfxColor.Cyan))
			.Execute(null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, CrushStrength, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("shrill");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("plow");
		AnimState animState5 = new AnimState("plow_end");
		AnimState state = new AnimState("plow_end_die");
		AnimState animState6 = new AnimState("stun");
		AnimState animState7 = new AnimState("stun_loop", isLooping: true);
		AnimState animState8 = new AnimState("wake_up");
		AnimState animState9 = new AnimState("hurt");
		AnimState state2 = new AnimState("die");
		animState.AddBranch("Plow", animState4);
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState9.NextState = animState;
		animState4.AddBranch("EndPlow", animState5);
		animState5.NextState = animState;
		animState6.NextState = animState7;
		animState8.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Unstun", animState8);
		creatureAnimator.AddAnyState("Dead", state2, () => !InMidCharge);
		creatureAnimator.AddAnyState("Dead", state, () => InMidCharge);
		animState.AddBranch("Hit", animState9, () => ShouldPlayRegularHurtAnim);
		animState2.AddBranch("Hit", animState9, () => ShouldPlayRegularHurtAnim);
		animState9.AddBranch("Hit", animState9, () => ShouldPlayRegularHurtAnim);
		animState3.AddBranch("Hit", animState9, () => ShouldPlayRegularHurtAnim);
		animState6.AddBranch("Hit", animState9, () => ShouldPlayRegularHurtAnim);
		animState7.AddBranch("Hit", animState9, () => ShouldPlayRegularHurtAnim);
		animState8.AddBranch("Hit", animState9, () => ShouldPlayRegularHurtAnim);
		animState.AddBranch("Hit", animState6, () => IsStunnedByPlowRemoval);
		animState2.AddBranch("Hit", animState6, () => IsStunnedByPlowRemoval);
		animState9.AddBranch("Hit", animState6, () => IsStunnedByPlowRemoval);
		animState3.AddBranch("Hit", animState6, () => IsStunnedByPlowRemoval);
		animState6.AddBranch("Hit", animState6, () => IsStunnedByPlowRemoval);
		animState7.AddBranch("Hit", animState6, () => IsStunnedByPlowRemoval);
		animState8.AddBranch("Hit", animState6, () => IsStunnedByPlowRemoval);
		creatureAnimator.AddAnyState("Stun", animState6);
		creatureAnimator.AddAnyState("Plow", animState4);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("PlowHit", animState9);
		return creatureAnimator;
	}

	public override List<BestiaryMonsterMove> MonsterMoveList(NCreatureVisuals creatureVisuals)
	{
		creatureVisuals.SetUpSkin(this);
		int num = 6;
		List<BestiaryMonsterMove> list = new List<BestiaryMonsterMove>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<BestiaryMonsterMove> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = new BestiaryMonsterMove(GetBestiaryMoveName("STOMP"), BestiaryAttackAnimId, AttackSfx);
		num2++;
		span[num2] = new BestiaryMonsterMove(GetBestiaryMoveName("PLOW"), "plow", "event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_plow");
		num2++;
		span[num2] = new BestiaryMonsterMove(GetBestiaryMoveName("BEAST_CRY"), "shrill", "event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_shrill");
		num2++;
		span[num2] = new BestiaryMonsterMove("Stun", "stun", "event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_stun");
		num2++;
		span[num2] = new BestiaryMonsterMove("Hurt", "hurt", TakeDamageSfx);
		num2++;
		span[num2] = new BestiaryMonsterMove("Die", "die", DeathSfx);
		return list;
	}
}
