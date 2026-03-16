using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class ToughEgg : MonsterModel
{
	private bool _hatched;

	private const string _hatchTrigger = "Hatch";

	private const string _hatchSfx = "event:/sfx/enemy/enemy_attacks/tough_egg/tough_egg_hatch";

	private static readonly string[] _eggOptions = new string[2] { "egg1", "egg2" };

	private MonsterState? _afterHatchedState;

	private bool _isHatched;

	private Vector2? _hatchPos;

	public override LocString Title
	{
		get
		{
			if (!_hatched)
			{
				return MonsterModel.L10NMonsterLookup(base.Id.Entry + ".name");
			}
			return MonsterModel.L10NMonsterLookup("HATCHLING.name");
		}
	}

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 15, 14);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 19, 18);

	public int HatchlingMinHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 20, 19);

	public int HatchlingMaxHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 23, 22);

	private static int NibbleDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);

	public override string DeathSfx
	{
		get
		{
			if (!_hatched)
			{
				return "event:/sfx/enemy/enemy_attacks/tough_egg/tough_egg_die";
			}
			return "event:/sfx/enemy/enemy_attacks/tough_egg/hatchling_die";
		}
	}

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

	public MonsterState? AfterHatchedState
	{
		get
		{
			return _afterHatchedState;
		}
		set
		{
			AssertMutable();
			_afterHatchedState = value;
		}
	}

	public bool IsHatched
	{
		get
		{
			return _isHatched;
		}
		set
		{
			AssertMutable();
			_isHatched = value;
		}
	}

	public Vector2? HatchPos
	{
		get
		{
			return _hatchPos;
		}
		set
		{
			AssertMutable();
			_hatchPos = value;
		}
	}

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		MegaSkeleton skeleton = visuals.SpineBody.GetSkeleton();
		MegaSkin megaSkin = visuals.SpineBody.NewSkin("custom-skin");
		MegaSkeletonDataResource data = skeleton.GetData();
		megaSkin.AddSkin(data.FindSkin(base.Rng.NextItem(_eggOptions)));
		skeleton.SetSkin(megaSkin);
		skeleton.SetSlotsToSetupPose();
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		if (TestMode.IsOff && HatchPos.HasValue)
		{
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(base.Creature);
			creatureNode.GlobalPosition = HatchPos.Value;
		}
		if (!IsHatched)
		{
			int num = ((base.CombatState.CurrentSide != CombatSide.Enemy) ? 1 : 2);
			await PowerCmd.Apply<HatchPower>(base.Creature, num, base.Creature, null);
		}
		else
		{
			await Hatch();
			base.MoveStateMachine?.ForceCurrentState(AfterHatchedState);
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("HATCH_MOVE", HatchMove, new SummonIntent());
		MoveState moveState2 = (MoveState)(moveState.FollowUpState = new MoveState("NIBBLE_MOVE", NibbleMove, new SingleAttackIntent(NibbleDamage)));
		moveState2.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(moveState2);
		AfterHatchedState = moveState2;
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task HatchMove(IReadOnlyList<Creature> targets)
	{
		IsHatched = true;
		await PowerCmd.Remove<HatchPower>(base.Creature);
		_hatched = true;
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/tough_egg/tough_egg_hatch");
		List<PowerModel> list = base.Creature.Powers.Where((PowerModel p) => !(p is MinionPower)).ToList();
		foreach (PowerModel item in list)
		{
			await PowerCmd.Remove(item);
		}
		await Hatch();
	}

	private async Task Hatch()
	{
		await CreatureCmd.TriggerAnim(base.Creature, "Hatch", 0.5f);
		decimal amount = MegaCrit.Sts2.Core.Entities.Creatures.Creature.ScaleHpForMultiplayer(base.RunRng.Niche.NextInt(HatchlingMinHp, HatchlingMaxHp), base.CombatState.Encounter, base.Creature.CombatState.Players.Count, base.Creature.CombatState.Players[0].RunState.CurrentActIndex);
		await CreatureCmd.SetMaxAndCurrentHp(base.Creature, amount);
	}

	private async Task NibbleMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(NibbleDamage).FromMonster(this).WithAttackerAnim("Attack", 0.2f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_bite")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState nextState = new AnimState("idle_loop", isLooping: true);
		AnimState animState = new AnimState("hurt");
		AnimState animState2 = new AnimState("attack");
		AnimState state = new AnimState("die");
		AnimState animState3 = new AnimState("egg_spawn");
		AnimState nextState2 = new AnimState("egg_idle_loop", isLooping: true);
		AnimState animState4 = new AnimState("egg_hurt");
		AnimState state2 = new AnimState("egg_die");
		AnimState animState5 = new AnimState("egg_hatch");
		animState.NextState = nextState;
		animState2.NextState = nextState;
		animState3.NextState = nextState2;
		animState5.NextState = nextState;
		animState4.NextState = nextState2;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState3, controller);
		creatureAnimator.AddAnyState("Hatch", animState5);
		creatureAnimator.AddAnyState("Hit", animState4, () => !IsHatched);
		creatureAnimator.AddAnyState("Dead", state2, () => !IsHatched);
		creatureAnimator.AddAnyState("Hit", animState, () => IsHatched);
		creatureAnimator.AddAnyState("Dead", state, () => IsHatched);
		creatureAnimator.AddAnyState("Attack", animState2);
		return creatureAnimator;
	}
}
