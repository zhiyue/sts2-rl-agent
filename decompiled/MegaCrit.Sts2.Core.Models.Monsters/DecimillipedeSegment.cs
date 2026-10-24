using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public abstract class DecimillipedeSegment : MonsterModel
{
	private int _starterMoveIdx;

	private const int _writheRepeat = 2;

	private MoveState _deadState;

	public static readonly string rocksVfxPath = SceneHelper.GetScenePath("vfx/vfx_decimillipede_rocks");

	private const string _healSfx = "event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_heal";

	private const string _attackTriple = "event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_triple";

	private const string _attackBuff = "event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_buff";

	private const string _attackWeaken = "event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_weaken";

	private string PhobiaModeAliveTexture => ImageHelper.GetImagePath("monsters/phobia_mode/" + base.Id.Entry.ToLowerInvariant() + "_phobia.png");

	private string PhobiaModeDeadTexture => ImageHelper.GetImagePath("monsters/phobia_mode/" + base.Id.Entry.ToLowerInvariant() + "_shriveled_phobia.png");

	public override LocString Title => MonsterModel.L10NMonsterLookup("DECIMILLIPEDE_SEGMENT.name");

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

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 46, 40);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 52, 46);

	private int WritheDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);

	private int ConstrictDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	private int BulkDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);

	private int BulkStrength => 2;

	public override float HpBarSizeReduction => 35f;

	public MoveState DeadState
	{
		get
		{
			return _deadState;
		}
		private set
		{
			AssertMutable();
			_deadState = value;
		}
	}

	public override bool ShouldFadeAfterDeath => false;

	public override bool ShouldDisappearFromDoom => false;

	public override bool ShouldShowInCompendium => false;

	public override bool CanChangeScale => false;

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_die";

	public override IEnumerable<string> AssetPaths
	{
		get
		{
			int num = 3;
			List<string> list = new List<string>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<string> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = rocksVfxPath;
			num2++;
			span[num2] = PhobiaModeAliveTexture;
			num2++;
			span[num2] = PhobiaModeDeadTexture;
			List<string> first = list;
			return first.Concat(base.AssetPaths);
		}
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		decimal maxHp = base.Creature.MaxHp;
		if (maxHp % 2m == 1m)
		{
			maxHp++;
		}
		IReadOnlyList<Player> players = base.CombatState.Players;
		int count = players.Count;
		int currentActIndex = base.CombatState.RunState.CurrentActIndex;
		List<Creature> source = (from c in base.CombatState.GetTeammatesOf(base.Creature)
			where c != base.Creature
			select c).ToList();
		while (source.Any((Creature c) => (decimal)c.MaxHp == maxHp))
		{
			maxHp += 2m;
			if (maxHp > MegaCrit.Sts2.Core.Entities.Creatures.Creature.ScaleHpForMultiplayer(MaxInitialHp, base.CombatState.Encounter, count, currentActIndex))
			{
				maxHp = MegaCrit.Sts2.Core.Entities.Creatures.Creature.ScaleHpForMultiplayer(MinInitialHp, base.CombatState.Encounter, count, currentActIndex);
			}
		}
		await CreatureCmd.SetMaxAndCurrentHp(base.Creature, maxHp);
		await PowerCmd.Apply<ReattachPower>(new ThrowingPlayerChoiceContext(), base.Creature, 25m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("WRITHE_MOVE", WritheMove, new MultiAttackIntent(WritheDamage, 2));
		MoveState moveState2 = new MoveState("BULK_MOVE", BulkMove, new SingleAttackIntent(BulkDamage), new BuffIntent());
		MoveState moveState3 = new MoveState("CONSTRICT_MOVE", ConstrictMove, new SingleAttackIntent(ConstrictDamage), new DebuffIntent());
		DeadState = new MoveState("DEAD_MOVE", DeadMove);
		MoveState moveState4 = new MoveState("REATTACH_MOVE", ReattachMove, new HealIntent())
		{
			MustPerformOnceBeforeTransitioning = true
		};
		moveState3.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState;
		moveState.FollowUpState = moveState3;
		RandomBranchState randomBranchState = new RandomBranchState("RAND");
		DeadState.FollowUpState = moveState4;
		moveState4.FollowUpState = randomBranchState;
		randomBranchState.AddBranch(moveState, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState3, MoveRepeatType.CannotRepeat);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(DeadState);
		list.Add(moveState4);
		list.Add(randomBranchState);
		return new MonsterMoveStateMachine(list, (StarterMoveIdx % 3) switch
		{
			0 => moveState, 
			1 => moveState2, 
			_ => moveState3, 
		});
	}

	private async Task WritheMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_triple");
		await AnimSegmentsAttack();
		await DamageCmd.Attack(WritheDamage).WithHitCount(2).FromMonster(this)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task BulkMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_buff");
		await AnimSegmentsAttack();
		await DamageCmd.Attack(BulkDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), base.Creature, BulkStrength, base.Creature, null);
	}

	private async Task ConstrictMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_weaken");
		await AnimSegmentsAttack();
		await DamageCmd.Attack(ConstrictDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, 1m, base.Creature, null);
	}

	private Task DeadMove(IReadOnlyList<Creature> targets)
	{
		return Task.CompletedTask;
	}

	private async Task ReattachMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_heal");
		if (base.CombatState.IsLiveCombat())
		{
			await base.Creature.GetPower<ReattachPower>().DoReattach();
		}
		ChangePhobiaModeTexture(ResourceLoader.Load<Texture2D>(PhobiaModeAliveTexture, null, ResourceLoader.CacheMode.Reuse));
	}

	private async Task AnimSegmentsAttack()
	{
		if (TestMode.IsOn)
		{
			return;
		}
		IEnumerable<Creature> enumerable = from c in base.Creature.CombatState.GetTeammatesOf(base.Creature)
			where c.Monster is DecimillipedeSegment
			select c;
		foreach (Creature item in enumerable)
		{
			((DecimillipedeSegment)item.Monster).SegmentAttack();
		}
		Node2D node2D = PreloadManager.Cache.GetScene(rocksVfxPath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
		base.Creature.GetVfxContainer()?.AddChildSafely(node2D);
		node2D.GlobalPosition = NGame.Instance.GetViewportRect().Size * 0.5f;
		await Cmd.Wait(0.5f);
	}

	public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (creature != base.Creature)
		{
			return Task.CompletedTask;
		}
		if (TestMode.IsOn)
		{
			return Task.CompletedTask;
		}
		ChangePhobiaModeTexture(ResourceLoader.Load<Texture2D>(PhobiaModeDeadTexture, null, ResourceLoader.CacheMode.Reuse));
		return Task.CompletedTask;
	}

	private void ChangePhobiaModeTexture(Texture2D tex)
	{
		NCreature creatureNode = base.Creature.GetCreatureNode();
		if (creatureNode != null && creatureNode.Visuals.IsUsingPhobiaModeBody)
		{
			((Sprite2D)creatureNode.Body).Texture = tex;
		}
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("hurt");
		AnimState nextState = new AnimState("dead_loop", isLooping: true);
		AnimState animState3 = new AnimState("wither");
		AnimState animState4 = new AnimState("regenerate");
		animState2.NextState = animState;
		animState3.NextState = nextState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Revive", animState4);
		creatureAnimator.AddAnyState("Hit", animState);
		creatureAnimator.AddAnyState("Dead", animState3);
		return creatureAnimator;
	}

	public abstract void SegmentAttack();
}
