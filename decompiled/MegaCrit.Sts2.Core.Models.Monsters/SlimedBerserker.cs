using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class SlimedBerserker : MonsterModel
{
	private const int _pummelingRepeat = 4;

	private const int _leechingDrain = 3;

	private const int _vomitSlimeInDiscard = 10;

	private const string _hugTrigger = "Hug";

	private const string _vomitTrigger = "Vomit";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 276, 266);

	public override int MaxInitialHp => MinInitialHp;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

	public override bool HasDeathSfx => true;

	protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/slimed_berserker/slimed_berserker_buff";

	private string SlimeSfx => "event:/sfx/enemy/enemy_attacks/slimed_berserker/slimed_berserker_slime";

	private int PummelingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);

	private int SmotherDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 33, 30);

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("VOMIT_ICHOR_MOVE", VomitIchorMove, new StatusIntent(10));
		MoveState moveState2 = new MoveState("LEECHING_HUG_MOVE", LeechingHugMove, new DebuffIntent(), new BuffIntent());
		MoveState moveState3 = new MoveState("SMOTHER_MOVE", SmotherMove, new SingleAttackIntent(SmotherDamage));
		MoveState moveState4 = (MoveState)(moveState.FollowUpState = new MoveState("FURIOUS_PUMMELING_MOVE", FuriousPummelingMove, new MultiAttackIntent(PummelingDamage, 4)));
		moveState4.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState3);
		list.Add(moveState2);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task VomitIchorMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(SlimeSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Vomit", 0.7f);
		await CardPileCmd.AddToCombatAndPreview<Slimed>(targets, PileType.Discard, 10, addedByPlayer: false);
	}

	private async Task LeechingHugMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(CastSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Hug", 0.65f);
		await PowerCmd.Apply<WeakPower>(targets, 3m, null, null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 3m, base.Creature, null);
	}

	private async Task FuriousPummelingMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(PummelingDamage).WithHitCount(4).OnlyPlayAnimOnce()
			.FromMonster(this)
			.WithAttackerAnim("Attack", 0.2f)
			.WithAttackerFx(null, AttackSfx)
			.Execute(null);
	}

	private async Task SmotherMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SmotherDamage).FromMonster(this).WithAttackerAnim("Attack", 0.2f)
			.WithAttackerFx(null, AttackSfx)
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("hug");
		AnimState animState3 = new AnimState("vomit");
		AnimState animState4 = new AnimState("attack");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Hug", animState2);
		creatureAnimator.AddAnyState("Vomit", animState3);
		creatureAnimator.AddAnyState("Attack", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		return creatureAnimator;
	}
}
