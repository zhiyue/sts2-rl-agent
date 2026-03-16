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
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class TheInsatiable : MonsterModel
{
	private const int _liquifyStatusDrawCount = 3;

	private const int _liquifyStatusDiscardCount = 3;

	private const int _thrashRepeat = 2;

	private const string _liquifySandTrigger = "LiquifySand";

	private const string _salivateTrigger = "Salivate";

	private const string _biteTrigger = "Bite";

	private const string _thrashTrigger = "Thrash";

	public const string eatPlayerTrigger = "EatPlayerTrigger";

	public const string finisherSfx = "event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_finisher";

	private const string _liquifyGroundSfx = "event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_liquify_ground";

	private const string _lungingBiteSfx = "event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_lunging_bite";

	private const string _salivateSfx = "event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_salivate";

	private const string _thrashSfx = "event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_thrash";

	private bool _hasLiquified;

	public static string TheInsatiableTrackName => "insatiable_progress";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 341, 321);

	public override int MaxInitialHp => MinInitialHp;

	private int ThrashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	private int BiteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 31, 28);

	private int SalivateStrength => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

	private bool HasLiquified
	{
		get
		{
			return _hasLiquified;
		}
		set
		{
			AssertMutable();
			_hasLiquified = value;
		}
	}

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Insect;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		base.Creature.Died += AfterDeath;
	}

	private void AfterDeath(Creature _)
	{
		base.Creature.Died -= AfterDeath;
		NRunMusicController.Instance?.UpdateMusicParameter(TheInsatiableTrackName, 10f);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("LIQUIFY_GROUND_MOVE", LiquifyMove, new BuffIntent(), new StatusIntent(6));
		MoveState moveState2 = new MoveState("THRASH_MOVE_1", ThrashMove, new MultiAttackIntent(ThrashDamage, 2));
		MoveState moveState3 = new MoveState("THRASH_MOVE_2", ThrashMove, new MultiAttackIntent(ThrashDamage, 2));
		MoveState moveState4 = new MoveState("LUNGING_BITE_MOVE", BiteMove, new SingleAttackIntent(BiteDamage));
		MoveState moveState5 = new MoveState("SALIVATE_MOVE", SalivateMove, new BuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState5;
		moveState5.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(moveState4);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState5);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task LiquifyMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_liquify_ground");
		await CreatureCmd.TriggerAnim(base.Creature, "LiquifySand", 0f);
		await Cmd.Wait(0.5f);
		VfxCmd.PlayOnCreatureCenter(base.Creature, "vfx/vfx_scream");
		await Cmd.Wait(0.75f);
		foreach (Creature target in targets)
		{
			SandpitPower sandpitPower = (SandpitPower)ModelDb.Power<SandpitPower>().ToMutable();
			sandpitPower.Target = target;
			await PowerCmd.Apply(sandpitPower, base.Creature, 4m, base.Creature, null);
		}
		foreach (Creature target2 in targets)
		{
			Player player = target2.Player ?? target2.PetOwner;
			List<CardPileAddResult> statusCards = new List<CardPileAddResult>();
			for (int i = 0; i < 6; i++)
			{
				CardModel card = base.CombatState.CreateCard<FranticEscape>(player);
				PileType newPileType = ((i < 3) ? PileType.Draw : PileType.Discard);
				List<CardPileAddResult> list = statusCards;
				list.Add(await CardPileCmd.AddGeneratedCardToCombat(card, newPileType, addedByPlayer: false, CardPilePosition.Random));
			}
			if (LocalContext.IsMe(player))
			{
				CardCmd.PreviewCardPileAdd(statusCards);
				await Cmd.Wait(1f);
			}
		}
		HasLiquified = true;
	}

	private async Task ThrashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ThrashDamage).WithHitCount(2).FromMonster(this)
			.WithHitFx("vfx/vfx_scratch")
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_thrash")
			.WithAttackerAnim("Thrash", 0.3f)
			.OnlyPlayAnimOnce()
			.Execute(null);
	}

	private async Task BiteMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(BiteDamage).FromMonster(this).WithAttackerAnim("Bite", 0.25f)
			.OnlyPlayAnimOnce()
			.WithHitFx("vfx/vfx_bite")
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_lunging_bite")
			.Execute(null);
	}

	private async Task SalivateMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_salivate");
		await CreatureCmd.TriggerAnim(base.Creature, "Salivate", 0.5f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, SalivateStrength, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("intro_loop", isLooping: true);
		AnimState animState2 = new AnimState("liquify_sand");
		AnimState nextState = new AnimState("idle_loop", isLooping: true);
		AnimState animState3 = new AnimState("salivate");
		AnimState animState4 = new AnimState("attack_thrash");
		AnimState animState5 = new AnimState("attack_bite");
		AnimState animState6 = new AnimState("eat_player");
		AnimState animState7 = new AnimState("intro_hurt");
		AnimState animState8 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = nextState;
		animState3.NextState = nextState;
		animState4.NextState = nextState;
		animState5.NextState = nextState;
		animState8.NextState = nextState;
		animState6.NextState = nextState;
		animState7.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState3);
		creatureAnimator.AddAnyState("Attack", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("EatPlayerTrigger", animState6);
		creatureAnimator.AddAnyState("LiquifySand", animState2);
		creatureAnimator.AddAnyState("Salivate", animState3);
		creatureAnimator.AddAnyState("Thrash", animState4);
		creatureAnimator.AddAnyState("Bite", animState5);
		creatureAnimator.AddAnyState("Hit", animState7, () => !HasLiquified);
		creatureAnimator.AddAnyState("Hit", animState8, () => HasLiquified);
		return creatureAnimator;
	}
}
