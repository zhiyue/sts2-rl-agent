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
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class TwoTailedRat : MonsterModel
{
	private const string _attackHandsSfx = "event:/sfx/enemy/enemy_attacks/two_tail_rats/two_tail_rats_attack_hands";

	private const string _summonSfx = "event:/sfx/enemy/enemy_attacks/two_tail_rats/two_tail_rats_summon";

	private const string _attackBite = "event:/sfx/enemy/enemy_attacks/two_tail_rats/two_tail_rats_attack_bite";

	private static readonly string[] _barnacleOptions = new string[3] { "barnacle1", "barnacle1", "barnacle3" };

	private static readonly string[] _headOptions = new string[3] { "head1", "head2", "head3" };

	private const string _callForBackupMoveId = "CALL_FOR_BACKUP_MOVE";

	private const float _callForBackupChance = 0.75f;

	private const int _callForBackupLimit = 3;

	private int _starterMoveIndex = -1;

	private int _turnsUntilSummonable = 2;

	private int _callForBackupCount;

	private const string _summonTrigger = "Summon";

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/two_tail_rats/two_tail_rats_die";

	public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/two_tail_rats/two_tail_rats_hurt";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 18, 17);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 22, 21);

	private int ScratchDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	private int DiseaseBiteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

	public int StarterMoveIndex
	{
		get
		{
			return _starterMoveIndex;
		}
		set
		{
			AssertMutable();
			_starterMoveIndex = value;
		}
	}

	private int TurnsUntilSummonable
	{
		get
		{
			return _turnsUntilSummonable;
		}
		set
		{
			AssertMutable();
			_turnsUntilSummonable = value;
		}
	}

	public int CallForBackupCount
	{
		get
		{
			return _callForBackupCount;
		}
		set
		{
			AssertMutable();
			_callForBackupCount = value;
		}
	}

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		MegaSkeleton skeleton = visuals.SpineBody.GetSkeleton();
		MegaSkin megaSkin = visuals.SpineBody.NewSkin("custom-skin");
		MegaSkeletonDataResource data = skeleton.GetData();
		megaSkin.AddSkin(data.FindSkin(MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextItem(_barnacleOptions)));
		megaSkin.AddSkin(data.FindSkin(MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextItem(_headOptions)));
		skeleton.SetSkin(megaSkin);
		skeleton.SetSlotsToSetupPose();
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SCRATCH_MOVE", ScratchMove, new SingleAttackIntent(ScratchDamage));
		MoveState moveState2 = new MoveState("DISEASE_BITE_MOVE", DiseaseBiteMove, new SingleAttackIntent(DiseaseBiteDamage));
		MoveState moveState3 = new MoveState("SCREECH_MOVE", ScreechMove, new DebuffIntent());
		MoveState moveState4 = new MoveState("CALL_FOR_BACKUP_MOVE", CallForBackup, new SummonIntent());
		RandomBranchState randomBranchState = (RandomBranchState)(moveState4.FollowUpState = (moveState3.FollowUpState = (moveState2.FollowUpState = (moveState.FollowUpState = new RandomBranchState("RAND")))));
		randomBranchState.AddBranch(moveState, MoveRepeatType.CannotRepeat, () => (!CanSummon()) ? 1f : (1f / 12f));
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat, () => (!CanSummon()) ? 1f : (1f / 12f));
		randomBranchState.AddBranch(moveState3, 3, MoveRepeatType.CannotRepeat, () => (!CanSummon()) ? 1f : (1f / 12f));
		randomBranchState.AddBranch(moveState4, MoveRepeatType.UseOnlyOnce, () => (!CanSummon()) ? 0f : 0.75f);
		list.Add(randomBranchState);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		if (StarterMoveIndex == -1)
		{
			return new MonsterMoveStateMachine(list, randomBranchState);
		}
		return new MonsterMoveStateMachine(list, (StarterMoveIndex % 3) switch
		{
			0 => moveState, 
			1 => moveState2, 
			_ => moveState3, 
		});
	}

	private async Task ScratchMove(IReadOnlyList<Creature> targets)
	{
		TurnsUntilSummonable--;
		await DamageCmd.Attack(ScratchDamage).FromMonster(this).WithAttackerAnim("Attack", 0.25f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/two_tail_rats/two_tail_rats_attack_hands")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task DiseaseBiteMove(IReadOnlyList<Creature> targets)
	{
		TurnsUntilSummonable--;
		await DamageCmd.Attack(DiseaseBiteDamage).FromMonster(this).WithAttackerAnim("Cast", 0.25f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/two_tail_rats/two_tail_rats_attack_bite")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task ScreechMove(IReadOnlyList<Creature> targets)
	{
		TurnsUntilSummonable--;
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/two_tail_rats/two_tail_rats_attack_bite");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.3f);
		await PowerCmd.Apply<FrailPower>(targets, 1m, base.Creature, null);
	}

	private async Task CallForBackup(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/two_tail_rats/two_tail_rats_summon");
		await CreatureCmd.TriggerAnim(base.Creature, "Summon", 0.3f);
		string nextSlot = base.CombatState.Encounter.Slots.LastOrDefault((string s) => base.CombatState.Enemies.All((Creature c) => c.SlotName != s), string.Empty);
		if (!string.IsNullOrEmpty(nextSlot))
		{
			await Cmd.Wait(0.5f);
			await CreatureCmd.Add<TwoTailedRat>(base.CombatState, nextSlot);
		}
		List<TwoTailedRat> list = base.Creature.CombatState.Enemies.Select((Creature c) => c.Monster).OfType<TwoTailedRat>().ToList();
		int maxCallForBackupCount = list.Max((TwoTailedRat c) => c.CallForBackupCount + 1);
		list.ForEach(delegate(TwoTailedRat r)
		{
			r.CallForBackupCount = maxCallForBackupCount;
		});
	}

	private bool CanSummon()
	{
		if (TurnsUntilSummonable > 0)
		{
			return false;
		}
		if (CallForBackupCount >= 3)
		{
			return false;
		}
		if (string.IsNullOrEmpty(base.CombatState.Encounter?.GetNextSlot(base.CombatState)))
		{
			return false;
		}
		List<Creature> list = (from c in base.Creature.CombatState.GetTeammatesOf(base.Creature)
			where c != base.Creature
			select c).ToList();
		foreach (Creature item in list)
		{
			if (item.Monster.NextMove.Id.Equals("CALL_FOR_BACKUP_MOVE"))
			{
				return false;
			}
		}
		return true;
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("debuff");
		AnimState animState3 = new AnimState("summon");
		AnimState animState4 = new AnimState("attack");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState3.NextState = animState;
		animState2.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Summon", animState3);
		creatureAnimator.AddAnyState("Attack", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		return creatureAnimator;
	}
}
