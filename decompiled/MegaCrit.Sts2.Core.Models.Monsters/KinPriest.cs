using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class KinPriest : MonsterModel
{
	private const string _theKinCustomTrackName = "the_kin_progress";

	private static readonly LocString _ritualApplyLine = MonsterModel.L10NMonsterLookup("KIN_PRIEST.moves.RITUAL.speakLine1");

	private static readonly LocString _followersDeathLine = MonsterModel.L10NMonsterLookup("KIN_PRIEST.followersDeathLine");

	private const string _grenadeTrigger = "AttackGrenade";

	private const string _laserTrigger = "AttackLaser";

	private const string _rallyTrigger = "Rally";

	private const string _attackGrenadeAnimId = "attack_grenade";

	private const string _soulBeamSfx = "event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_soul_beam";

	private const string _soulGrenadeSfx = "event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_soul_grenade";

	private const string _rallySfx = "event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_rally";

	private const int _beamRepeat = 3;

	private bool _speechUsed;

	protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_cast";

	public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_hurt";

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_die";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 199, 190);

	public override int MaxInitialHp => MinInitialHp;

	private int OrbOfFrailtyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	private int OrbOfWeaknessDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	private int BeamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 3);

	private int RitualStrength => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

	private bool SpeechUsed
	{
		get
		{
			return _speechUsed;
		}
		set
		{
			AssertMutable();
			_speechUsed = value;
		}
	}

	public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (creature.Monster is KinFollower)
		{
			if (base.Creature.IsDead)
			{
				return Task.CompletedTask;
			}
			IReadOnlyList<Creature> readOnlyList = base.Creature.CombatState?.GetTeammatesOf(base.Creature);
			if (readOnlyList == null)
			{
				return Task.CompletedTask;
			}
			NRunMusicController.Instance?.UpdateMusicParameter("the_kin_progress", 1f);
			if (!readOnlyList.Any((Creature c) => c != null && c.Monster is KinFollower && c.IsAlive))
			{
				Creature creature2 = readOnlyList.FirstOrDefault((Creature c) => c != null && c.Monster is KinPriest && c.IsAlive);
				if (creature2 != null && creature2.Monster is KinPriest kinPriest)
				{
					kinPriest.AllFollowerDeathResponse();
				}
			}
		}
		else if (creature == base.Creature)
		{
			NRunMusicController.Instance?.UpdateMusicParameter("the_kin_progress", 5f);
		}
		return Task.CompletedTask;
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("ORB_OF_FRAILTY_MOVE", OrbOfFrailtyMove, new SingleAttackIntent(OrbOfFrailtyDamage), new DebuffIntent());
		MoveState moveState2 = new MoveState("ORB_OF_WEAKNESS_MOVE", OrbOfWeaknessMove, new SingleAttackIntent(OrbOfWeaknessDamage), new DebuffIntent());
		MoveState moveState3 = new MoveState("BEAM_MOVE", BeamMove, new MultiAttackIntent(BeamDamage, 3));
		MoveState moveState4 = new MoveState("RITUAL_MOVE", RitualMove, new BuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task OrbOfFrailtyMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(OrbOfFrailtyDamage).FromMonster(this).WithAttackerAnim("AttackGrenade", 0f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_soul_grenade")
			.WithWaitBeforeHit(1f, 1f)
			.WithHitVfxNode((Creature t) => NKinPriestGrenadeVfx.Create(t))
			.Execute(null);
		await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, 1m, base.Creature, null);
	}

	private async Task OrbOfWeaknessMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(OrbOfWeaknessDamage).FromMonster(this).WithAttackerAnim("AttackGrenade", 0f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_soul_grenade")
			.WithWaitBeforeHit(1f, 1f)
			.WithHitVfxNode((Creature t) => NKinPriestGrenadeVfx.Create(t))
			.Execute(null);
		await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, 1m, base.Creature, null);
	}

	private async Task BeamMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(BeamDamage).WithHitCount(3).FromMonster(this)
			.WithAttackerAnim("AttackLaser", 0.4f)
			.AfterAttackerAnim(delegate
			{
				(NCombatRoom.Instance?.GetCreatureNode(base.Creature))?.GetSpecialNode<NKinPriestBeamVfx>("Visuals/Beam")?.Fire();
				SfxCmd.Play("event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_soul_beam");
				return Task.CompletedTask;
			})
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.OnlyPlayAnimOnce()
			.Execute(null);
	}

	private async Task RitualMove(IReadOnlyList<Creature> targets)
	{
		if (!SpeechUsed)
		{
			SpeechUsed = true;
			TalkCmd.Play(_ritualApplyLine, base.Creature, VfxColor.Purple, VfxDuration.Standard);
		}
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_rally");
		await CreatureCmd.TriggerAnim(base.Creature, "Rally", 1f);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), base.Creature, RitualStrength, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("rally");
		AnimState animState3 = new AnimState("attack_grenade");
		AnimState animState4 = new AnimState("attack_laser");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Rally", animState2);
		creatureAnimator.AddAnyState("AttackGrenade", animState3);
		creatureAnimator.AddAnyState("AttackLaser", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		return creatureAnimator;
	}

	public void AllFollowerDeathResponse()
	{
		TalkCmd.Play(_followersDeathLine, base.Creature, VfxColor.Purple, VfxDuration.Standard);
	}
}
