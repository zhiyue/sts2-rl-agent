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
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class LouseProgenitor : MonsterModel
{
	public const string curlTrigger = "Curl";

	private const string _uncurlTrigger = "Uncurl";

	private const string _webTrigger = "Web";

	private const string _webSfx = "event:/sfx/enemy/enemy_attacks/giant_louse/giant_louse_attack_web";

	public const string curlSfx = "event:/sfx/enemy/enemy_attacks/giant_louse/giant_louse_curl";

	private const string _uncurlSfx = "event:/sfx/enemy/enemy_attacks/giant_louse/giant_louse_uncurl";

	private bool _curled;

	private const int _webFrail = 2;

	private const int _growStrength = 5;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 138, 134);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 141, 136);

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/giant_louse/giant_louse_attack";

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/giant_louse/giant_louse_die";

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Insect;

	public bool Curled
	{
		get
		{
			return _curled;
		}
		set
		{
			AssertMutable();
			_curled = value;
		}
	}

	private int WebDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);

	private int PounceDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 14);

	private int CurlBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 18, 14);

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<CurlUpPower>(base.Creature, CurlBlock, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("WEB_CANNON_MOVE", WebMove, new SingleAttackIntent(WebDamage), new DebuffIntent());
		MoveState moveState2 = new MoveState("POUNCE_MOVE", PounceMove, new SingleAttackIntent(PounceDamage));
		MoveState moveState3 = (MoveState)(moveState.FollowUpState = new MoveState("CURL_AND_GROW_MOVE", CurlAndGrowMove, new DefendIntent(), new BuffIntent()));
		moveState3.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState;
		list.Add(moveState3);
		list.Add(moveState);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task WebMove(IReadOnlyList<Creature> targets)
	{
		if (Curled)
		{
			SfxCmd.Play("event:/sfx/enemy/enemy_attacks/giant_louse/giant_louse_uncurl");
			await CreatureCmd.TriggerAnim(base.Creature, "Uncurl", 0.9f);
			Curled = false;
		}
		await DamageCmd.Attack(WebDamage).FromMonster(this).WithAttackerAnim("Web", 0.2f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/giant_louse/giant_louse_attack_web")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<FrailPower>(targets, 2m, base.Creature, null);
	}

	private async Task CurlAndGrowMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/giant_louse/giant_louse_curl");
		await CreatureCmd.TriggerAnim(base.Creature, "Curl", 0.25f);
		await CreatureCmd.GainBlock(base.Creature, CurlBlock, ValueProp.Move, null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 5m, base.Creature, null);
		Curled = true;
	}

	private async Task PounceMove(IReadOnlyList<Creature> targets)
	{
		if (Curled)
		{
			SfxCmd.Play("event:/sfx/enemy/enemy_attacks/giant_louse/giant_louse_uncurl");
			await CreatureCmd.TriggerAnim(base.Creature, "Uncurl", 0.9f);
			Curled = false;
		}
		await DamageCmd.Attack(PounceDamage).FromMonster(this).WithAttackerAnim("Attack", 0.2f)
			.WithAttackerFx(null, AttackSfx)
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("curl");
		AnimState animState3 = new AnimState("uncurl");
		AnimState animState4 = new AnimState("curled_loop", isLooping: true);
		AnimState animState5 = new AnimState("attack");
		AnimState animState6 = new AnimState("attack_web");
		AnimState animState7 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState state2 = new AnimState("die_curled");
		animState.AddBranch("Curl", animState2);
		animState.AddBranch("Attack", animState5);
		animState.AddBranch("Web", animState6);
		animState.AddBranch("Hit", animState7);
		animState.AddBranch("Dead", state);
		animState2.NextState = animState4;
		animState4.AddBranch("Uncurl", animState3);
		animState4.AddBranch("Dead", state2);
		animState5.NextState = animState;
		animState5.AddBranch("Hit", animState7);
		animState5.AddBranch("Dead", state);
		animState6.NextState = animState;
		animState6.AddBranch("Hit", animState7);
		animState6.AddBranch("Dead", state);
		animState7.NextState = animState;
		animState7.AddBranch("Hit", animState7);
		animState7.AddBranch("Dead", state);
		animState7.AddBranch("Curl", animState2);
		animState3.NextState = animState;
		return new CreatureAnimator(animState, controller);
	}
}
