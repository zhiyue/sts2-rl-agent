using System;
using System.Collections.Generic;
using System.Linq;
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
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class KinFollower : MonsterModel
{
	private static readonly string[] _hairOptions = new string[3] { "hair_1", "hair_2", "hair_3" };

	private const string _kinPoofPath = "vfx/vfx_kin_poof";

	private const string _slashTrigger = "SlashTrigger";

	private const string _boomerangTrigger = "BoomerangTrigger";

	private const string _quickSlashSfx = "event:/sfx/enemy/enemy_attacks/the_kin_minion/the_kin_minion_quick_slash";

	private const string _boomerangSfx = "event:/sfx/enemy/enemy_attacks/the_kin_minion/the_kin_minion_boomerang_slashh";

	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/the_kin_minion/the_kin_minion_buff";

	private const int _boomerangRepeat = 2;

	private bool _startsWithDance;

	public override IEnumerable<string> AssetPaths
	{
		get
		{
			int num = 1;
			List<string> list = new List<string>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<string> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = SceneHelper.GetScenePath("vfx/vfx_kin_poof");
			List<string> first = list;
			return first.Concat(base.AssetPaths);
		}
	}

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_kin_minion/the_kin_minion_die";

	public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_hurt";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 62, 58);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 63, 59);

	private int QuickSlashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 5);

	private int BoomerangDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 2);

	private int DanceStrength => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

	public bool StartsWithDance
	{
		get
		{
			return _startsWithDance;
		}
		set
		{
			AssertMutable();
			_startsWithDance = value;
		}
	}

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		MegaSkeleton skeleton = visuals.SpineBody.GetSkeleton();
		MegaSkin megaSkin = visuals.SpineBody.NewSkin("custom-skin");
		MegaSkeletonDataResource data = skeleton.GetData();
		MegaSkin skin = data.FindSkin(MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextItem(_hairOptions));
		megaSkin.AddSkin(skin);
		skeleton.SetSkin(megaSkin);
		skeleton.SetSlotsToSetupPose();
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<MinionPower>(base.Creature, 1m, base.Creature, null);
		NRunMusicController.Instance?.UpdateMusicParameter("the_kin_progress", 0f);
		base.Creature.Died += AfterDeath;
	}

	private void AfterDeath(Creature _)
	{
		base.Creature.Died -= AfterDeath;
		IReadOnlyList<Creature> teammatesOf = base.Creature.CombatState.GetTeammatesOf(base.Creature);
		if (teammatesOf.Any((Creature c) => c != null && c.Monster is KinPriest && c.IsAlive))
		{
			NRunMusicController.Instance?.UpdateMusicParameter("the_kin_progress", 1f);
		}
		if (!teammatesOf.Any((Creature c) => c != null && c.Monster is KinFollower && c.IsAlive))
		{
			Creature creature = teammatesOf.FirstOrDefault((Creature c) => c != null && c.Monster is KinPriest && c.IsAlive);
			if (creature != null && creature.Monster is KinPriest kinPriest)
			{
				kinPriest.AllFollowerDeathResponse();
			}
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("QUICK_SLASH_MOVE", QuickSlashMove, new SingleAttackIntent(QuickSlashDamage));
		MoveState moveState2 = new MoveState("BOOMERANG_MOVE", BoomerangMove, new MultiAttackIntent(BoomerangDamage, 2));
		MoveState moveState3 = new MoveState("POWER_DANCE_MOVE", PowerDanceMove, new BuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		MoveState initialState = (StartsWithDance ? moveState3 : moveState);
		return new MonsterMoveStateMachine(list, initialState);
	}

	private async Task QuickSlashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(QuickSlashDamage).FromMonster(this).WithAttackerAnim("SlashTrigger", 0.2f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/the_kin_minion/the_kin_minion_quick_slash")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task PowerDanceMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/the_kin_minion/the_kin_minion_buff");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.9f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, DanceStrength, base.Creature, null);
	}

	private async Task BoomerangMove(IReadOnlyList<Creature> targets)
	{
		if (TestMode.IsOff)
		{
			Vector2? vector = null;
			foreach (Creature target in targets)
			{
				NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
				if (!vector.HasValue || vector.Value.X > creatureNode.GlobalPosition.X)
				{
					vector = creatureNode.GlobalPosition;
				}
			}
			NCreature creatureNode2 = NCombatRoom.Instance.GetCreatureNode(base.Creature);
			Node2D specialNode = creatureNode2.GetSpecialNode<Node2D>("Visuals/AttackDistanceControl");
			if (specialNode != null)
			{
				specialNode.Position = Vector2.Left * (creatureNode2.GlobalPosition.X - vector.Value.X) / creatureNode2.Body.Scale;
			}
		}
		await DamageCmd.Attack(BoomerangDamage).WithHitCount(2).FromMonster(this)
			.WithAttackerAnim("BoomerangTrigger", 0.2f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/the_kin_minion/the_kin_minion_boomerang_slashh")
			.WithHitFx("vfx/vfx_attack_slash")
			.OnlyPlayAnimOnce()
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("attack_slash");
		AnimState animState3 = new AnimState("attack_boomerang");
		AnimState animState4 = new AnimState("buff");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState5.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("BoomerangTrigger", animState3);
		creatureAnimator.AddAnyState("SlashTrigger", animState2);
		creatureAnimator.AddAnyState("Cast", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		return creatureAnimator;
	}
}
