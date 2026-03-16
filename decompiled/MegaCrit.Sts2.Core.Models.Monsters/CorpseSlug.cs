using System.Collections.Generic;
using System.Linq;
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
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class CorpseSlug : MonsterModel
{
	private const string _heavyAttackTrigger = "HeavyAttackTrigger";

	public const string devourStartTrigger = "DevourStartTrigger";

	public const string devourEndTrigger = "DevourEndkTrigger";

	private const string _attackLightSfx = "event:/sfx/enemy/enemy_attacks/corpse_slugs/corpse_slugs_attack_light";

	public const string ravenousSfx = "event:/sfx/enemy/enemy_attacks/corpse_slugs/corpse_slugs_ravenous";

	public const string ravenousUpSfxDouble = "event:/sfx/enemy/enemy_attacks/corpse_slugs/corpse_slugs_ravenous_up_double";

	private bool _isRavenous;

	private int _starterMoveIdx;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 27, 25);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 29, 27);

	private int WhipSlapDamage => 3;

	private int WhipSlapRepeat => 2;

	private int GlompDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	private int GoopFrailAmt => 2;

	private int RavenousStr => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/corpse_slugs/corpse_slugs_attack";

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/corpse_slugs/corpse_slugs_die";

	public bool IsRavenous
	{
		get
		{
			return _isRavenous;
		}
		set
		{
			AssertMutable();
			_isRavenous = value;
		}
	}

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

	public int StarterMoveIdx
	{
		get
		{
			return _starterMoveIdx;
		}
		set
		{
			AssertMutable();
			_starterMoveIdx = value;
		}
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<RavenousPower>(base.Creature, RavenousStr, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("WHIP_SLAP_MOVE", WhipSlapMove, new MultiAttackIntent(WhipSlapDamage, WhipSlapRepeat));
		MoveState moveState2 = new MoveState("GLOMP_MOVE", GlompMove, new SingleAttackIntent(GlompDamage));
		MoveState moveState3 = new MoveState("GOOP_MOVE", GoopMove, new DebuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, (StarterMoveIdx % 3) switch
		{
			0 => moveState, 
			1 => moveState2, 
			_ => moveState3, 
		});
	}

	private async Task WhipSlapMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(WhipSlapDamage).WithHitCount(WhipSlapRepeat).FromMonster(this)
			.WithAttackerAnim("Attack", 0.5f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/corpse_slugs/corpse_slugs_attack_light")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task GlompMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(GlompDamage).FromMonster(this).WithAttackerAnim("HeavyAttackTrigger", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task GoopMove(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(base.Creature, "Attack", 0.2f);
		await PowerCmd.Apply<FrailPower>(targets, GoopFrailAmt, base.Creature, null);
	}

	public static void EnsureCorpseSlugsStartWithDifferentMoves(IEnumerable<MonsterModel> monsters, Rng rng)
	{
		IEnumerable<CorpseSlug> enumerable = monsters.OfType<CorpseSlug>();
		int num = rng.NextInt(3);
		foreach (CorpseSlug item in enumerable)
		{
			item.StarterMoveIdx = num % 3;
			num++;
		}
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("attack");
		AnimState animState3 = new AnimState("attack_heavy");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState nextState = new AnimState("devour_loop", isLooping: true);
		AnimState animState5 = new AnimState("devour_start");
		AnimState animState6 = new AnimState("devour_end");
		AnimState animState7 = new AnimState("hurt_devouring");
		AnimState state2 = new AnimState("die_devouring");
		animState3.NextState = animState;
		animState2.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = nextState;
		animState6.NextState = animState;
		animState7.NextState = nextState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("HeavyAttackTrigger", animState3);
		creatureAnimator.AddAnyState("DevourStartTrigger", animState5, () => !_isRavenous);
		creatureAnimator.AddAnyState("DevourEndkTrigger", animState6);
		creatureAnimator.AddAnyState("Attack", animState2);
		creatureAnimator.AddAnyState("Dead", state, () => !_isRavenous);
		creatureAnimator.AddAnyState("Hit", animState4, () => !_isRavenous);
		creatureAnimator.AddAnyState("Dead", state2, () => _isRavenous);
		creatureAnimator.AddAnyState("Hit", animState7, () => _isRavenous);
		return creatureAnimator;
	}
}
