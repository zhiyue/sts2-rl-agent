using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class SoulFysh : MonsterModel
{
	private const string _intangibleSfx = "event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_intangible";

	private const string _beckonSfx = "event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_beckon";

	private const string _waveSfx = "event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_wave";

	private const string _reappearSfx = "event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_reappear";

	private const string _attackBeckonTrigger = "AttackBeckon";

	private const string _intangibleStartTrigger = "IntangibleStart";

	private const string _attackDebuffTrigger = "AttackDebuffTrigger";

	private bool _isInvisible;

	public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_hurt";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 221, 211);

	public override int MaxInitialHp => MinInitialHp;

	private int DeGasDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 16);

	private int ScreamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);

	private int GazeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

	private int BeckonMoveAmount => 2;

	private int GazeMoveAmount => 1;

	private int ScreamMoveAmount => 3;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	public bool IsInvisible
	{
		get
		{
			return _isInvisible;
		}
		set
		{
			AssertMutable();
			_isInvisible = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("BECKON_MOVE", BeckonMove, new StatusIntent(BeckonMoveAmount));
		MoveState moveState2 = new MoveState("DE_GAS_MOVE", DeGasMove, new SingleAttackIntent(DeGasDamage));
		MoveState moveState3 = new MoveState("GAZE_MOVE", GazeMove, new SingleAttackIntent(GazeDamage), new StatusIntent(GazeMoveAmount));
		MoveState moveState4 = new MoveState("FADE_MOVE", FadeMove, new BuffIntent());
		MoveState moveState5 = new MoveState("SCREAM_MOVE", ScreamMove, new SingleAttackIntent(ScreamDamage), new DebuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState5;
		moveState5.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState5);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task BeckonMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_beckon");
		await CreatureCmd.TriggerAnim(base.Creature, "AttackBeckon", 0f);
		await Cmd.Wait(0.3f);
		VfxCmd.PlayOnCreatureCenter(base.Creature, "vfx/vfx_spooky_scream");
		await Cmd.CustomScaledWait(0f, 0.3f);
		foreach (Creature target in targets)
		{
			Player player = target.Player ?? target.PetOwner;
			CardPileAddResult[] statusCards = new CardPileAddResult[BeckonMoveAmount];
			CardModel card = base.CombatState.CreateCard<Beckon>(player);
			CardPileAddResult[] array = statusCards;
			array[0] = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, addedByPlayer: false, CardPilePosition.Random);
			CardModel card2 = base.CombatState.CreateCard<Beckon>(player);
			array = statusCards;
			array[1] = await CardPileCmd.AddGeneratedCardToCombat(card2, PileType.Discard, addedByPlayer: false);
			if (LocalContext.IsMe(player))
			{
				CardCmd.PreviewCardPileAdd(statusCards);
				await Cmd.Wait(1f);
			}
		}
	}

	private async Task GazeMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(GazeDamage).FromMonster(this).WithAttackerAnim("AttackBeckon", 0.6f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_beckon")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		foreach (Creature target in targets)
		{
			Player player = target.Player ?? target.PetOwner;
			CardPileAddResult[] statusCards = new CardPileAddResult[1];
			CardModel card = base.CombatState.CreateCard<Beckon>(player);
			CardPileAddResult[] array = statusCards;
			array[0] = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, addedByPlayer: false);
			if (LocalContext.IsMe(player))
			{
				CardCmd.PreviewCardPileAdd(statusCards);
				await Cmd.Wait(1f);
			}
		}
	}

	private async Task DeGasMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(DeGasDamage).FromMonster(this).WithAttackerAnim("Attack", 0.45f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task ScreamMove(IReadOnlyList<Creature> targets)
	{
		IsInvisible = false;
		await DamageCmd.Attack(ScreamDamage).FromMonster(this).WithAttackerAnim("AttackDebuffTrigger", 0.65f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_wave")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await PowerCmd.Apply<VulnerablePower>(targets, ScreamMoveAmount, base.Creature, null);
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_reappear");
	}

	private async Task FadeMove(IReadOnlyList<Creature> targets)
	{
		IsInvisible = true;
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_intangible");
		await CreatureCmd.TriggerAnim(base.Creature, "IntangibleStart", 0.8f);
		await PowerCmd.Apply<IntangiblePower>(base.Creature, 2m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack_heavy");
		AnimState animState4 = new AnimState("attack_beckon");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState nextState = new AnimState("intangible_loop", isLooping: true);
		AnimState animState6 = new AnimState("intangible_start");
		AnimState animState7 = new AnimState("intangible_end");
		AnimState animState8 = new AnimState("hurt_intangible");
		AnimState state2 = new AnimState("die_intangible");
		AnimState animState9 = new AnimState("attack_debuff");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		animState6.NextState = nextState;
		animState8.NextState = nextState;
		animState9.NextState = animState7;
		animState7.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("AttackBeckon", animState4);
		creatureAnimator.AddAnyState("IntangibleStart", animState6);
		creatureAnimator.AddAnyState("AttackDebuffTrigger", animState9);
		creatureAnimator.AddAnyState("Dead", state, () => !IsInvisible);
		creatureAnimator.AddAnyState("Hit", animState5, () => !IsInvisible);
		creatureAnimator.AddAnyState("Dead", state2, () => IsInvisible);
		creatureAnimator.AddAnyState("Hit", animState8, () => IsInvisible);
		return creatureAnimator;
	}
}
