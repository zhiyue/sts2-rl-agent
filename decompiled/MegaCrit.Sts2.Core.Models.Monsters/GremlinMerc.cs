using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class GremlinMerc : MonsterModel
{
	private const string _attackBuffSfx = "event:/sfx/enemy/enemy_attacks/gremlin_merc/gremlin_merc_attack_buff";

	private bool _hasSpoken;

	private const string _attackDoubleTrigger = "AttackDouble";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 51, 47);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 53, 49);

	public override bool HasDeathSfx => false;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

	private int GimmeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 8, 7);

	private int GimmeRepeat => 2;

	private int DoubleSmashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 7, 6);

	private int DoubleSmashRepeat => 2;

	public int HeheDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 9, 8);

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<SurprisePower>(base.Creature, 1m, base.Creature, null);
		foreach (Player player in base.Creature.CombatState.Players)
		{
			ThieveryPower thieveryPower = (ThieveryPower)ModelDb.Power<ThieveryPower>().ToMutable();
			thieveryPower.Target = player.Creature;
			await PowerCmd.Apply(thieveryPower, base.Creature, 20m, base.Creature, null);
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("GIMME_MOVE", GimmeMove, new MultiAttackIntent(GimmeDamage, GimmeRepeat));
		MoveState moveState2 = new MoveState("DOUBLE_SMASH_MOVE", DoubleSmashMove, new MultiAttackIntent(DoubleSmashDamage, DoubleSmashRepeat), new DebuffIntent());
		MoveState moveState3 = new MoveState("HEHE_MOVE", HeheMove, new SingleAttackIntent(HeheDamage), new BuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task GimmeMove(IReadOnlyList<Creature> targets)
	{
		if (!_hasSpoken)
		{
			_hasSpoken = true;
			LocString line = MonsterModel.L10NMonsterLookup("GREMLIN_MERC.moves.GIMME.banter");
			TalkCmd.Play(line, base.Creature);
		}
		VfxCmd.PlayOnCreatureCenters(targets, "vfx/vfx_coin_explosion_regular");
		await DamageCmd.Attack(GimmeDamage).WithHitCount(GimmeRepeat).FromMonster(this)
			.WithAttackerAnim("AttackDouble", 0.15f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		foreach (ThieveryPower powerInstance in base.Creature.GetPowerInstances<ThieveryPower>())
		{
			await powerInstance.Steal();
		}
	}

	private async Task DoubleSmashMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(AttackSfx);
		VfxCmd.PlayOnCreatureCenters(targets, "vfx/vfx_coin_explosion_regular");
		await DamageCmd.Attack(DoubleSmashDamage).WithHitCount(DoubleSmashRepeat).FromMonster(this)
			.WithAttackerAnim("AttackDouble", 0.15f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		foreach (ThieveryPower powerInstance in base.Creature.GetPowerInstances<ThieveryPower>())
		{
			await powerInstance.Steal();
		}
		await PowerCmd.Apply<WeakPower>(targets, 2m, base.Creature, null);
	}

	private async Task HeheMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(HeheDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/gremlin_merc/gremlin_merc_attack_buff")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		foreach (ThieveryPower powerInstance in base.Creature.GetPowerInstances<ThieveryPower>())
		{
			await powerInstance.Steal();
		}
		await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("attack_single");
		AnimState animState3 = new AnimState("attack_double");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Attack", animState2);
		creatureAnimator.AddAnyState("AttackDouble", animState3);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		return creatureAnimator;
	}
}
