using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
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
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class MagiKnight : MonsterModel
{
	private static readonly LocString _dampenDialogue = new LocString("powers", "DAMPEN_POWER.banter");

	private const string _bombTrigger = "BombCast";

	private const string _ramAttackTrigger = "RamAttack";

	private const string _shieldTrigger = "ShieldAttack";

	private const string _ramSfx = "event:/sfx/enemy/enemy_attacks/magi_knight/magi_knight_attack_ram";

	private const string _bombSfx = "event:/sfx/enemy/enemy_attacks/magi_knight/magi_knight_attack_bomb";

	private const string _castShieldSfx = "event:/sfx/enemy/enemy_attacks/magi_knight/magi_knight_cast_shield";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 89, 82);

	public override int MaxInitialHp => MinInitialHp;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	private int PowerShieldDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);

	private int PowerShieldBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 9, 5);

	private int SpearDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);

	private int BombDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 40, 35);

	public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/magi_knight/magi_knight_hurt";

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("FIRST_POWER_SHIELD_MOVE", PowerShieldMove, new SingleAttackIntent(PowerShieldDamage), new DefendIntent());
		MoveState moveState2 = new MoveState("DAMPEN_MOVE", DampenMove, new DebuffIntent());
		MoveState moveState3 = new MoveState("PREP_MOVE", PrepMove, new DefendIntent());
		MoveState moveState4 = new MoveState("MAGIC_BOMB", MagicBombMove, new SingleAttackIntent(BombDamage));
		MoveState moveState5 = new MoveState("RAM_MOVE", SpearMove, new SingleAttackIntent(SpearDamage));
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState5;
		moveState5.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState5;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState5);
		list.Add(moveState3);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task DampenMove(IReadOnlyList<Creature> targets)
	{
		foreach (Creature target in targets)
		{
			DampenPower dampenPower = target.GetPower<DampenPower>();
			bool flag = dampenPower == null;
			if (flag)
			{
				dampenPower = (DampenPower)ModelDb.Power<DampenPower>().ToMutable();
			}
			dampenPower.AddCaster(base.Creature);
			if (flag)
			{
				await PowerCmd.Apply(dampenPower, target, 1m, base.Creature, null);
			}
		}
		NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NSpeechBubbleVfx.Create(_dampenDialogue.GetFormattedText(), base.Creature, 2.0));
		await Cmd.Wait(0.25f);
	}

	private async Task PowerShieldMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(PowerShieldDamage).FromMonster(this).WithAttackerAnim("ShieldAttack", 0.6f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/magi_knight/magi_knight_cast_shield")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await CreatureCmd.GainBlock(base.Creature, PowerShieldBlock, ValueProp.Move, null);
	}

	private async Task PrepMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/magi_knight/magi_knight_cast_shield");
		await CreatureCmd.TriggerAnim(base.Creature, "ShieldAttack", 0.6f);
		await CreatureCmd.GainBlock(base.Creature, PowerShieldBlock, ValueProp.Move, null);
	}

	private async Task MagicBombMove(IReadOnlyList<Creature> targets)
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
				float x = creatureNode2.Visuals.Body.Scale.X;
				specialNode.Position = Vector2.Left * ((creatureNode2.GlobalPosition.X - vector.Value.X - 600f) / x);
			}
		}
		await DamageCmd.Attack(BombDamage).FromMonster(this).WithAttackerAnim("BombCast", 1.2f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/magi_knight/magi_knight_attack_bomb")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task SpearMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SpearDamage).FromMonster(this).WithAttackerAnim("RamAttack", 1.2f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/magi_knight/magi_knight_attack_ram")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("attack_bomb");
		AnimState animState3 = new AnimState("attack_ram");
		AnimState animState4 = new AnimState("cast_shield");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState4.NextState = animState;
		animState3.NextState = animState;
		animState5.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("BombCast", animState2);
		creatureAnimator.AddAnyState("RamAttack", animState3);
		creatureAnimator.AddAnyState("ShieldAttack", animState4);
		return creatureAnimator;
	}
}
