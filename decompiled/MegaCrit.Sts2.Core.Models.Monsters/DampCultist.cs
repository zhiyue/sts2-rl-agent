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

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class DampCultist : MonsterModel
{
	private static readonly LocString _cawCawDialogue = new LocString("monsters", "DAMP_CULTIST.moves.INCANTATION.banter");

	private float _attackSfxStrength;

	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/cultists/cultists_buff_damp";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 52, 51);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 54, 53);

	private int DarkStrikeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 1);

	private int IncantationAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

	public override Vector2 ExtraDeathVfxPadding => new Vector2(1.5f, 1.2f);

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/cultists/cultists_die_damp";

	private float AttackSfxStrength
	{
		get
		{
			return _attackSfxStrength;
		}
		set
		{
			AssertMutable();
			_attackSfxStrength = value;
		}
	}

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/cultists/cultists_attack";

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		MegaSkeleton skeleton = visuals.SpineBody.GetSkeleton();
		MegaSkin megaSkin = visuals.SpineBody.NewSkin("custom-skin");
		MegaSkeletonDataResource data = skeleton.GetData();
		megaSkin.AddSkin(data.FindSkin("slug"));
		skeleton.SetSkin(megaSkin);
		skeleton.SetSlotsToSetupPose();
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("INCANTATION_MOVE", IncantationMove, new BuffIntent());
		MoveState moveState2 = (MoveState)(moveState.FollowUpState = new MoveState("DARK_STRIKE_MOVE", DarkStrikeMove, new SingleAttackIntent(DarkStrikeDamage)));
		moveState2.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task IncantationMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/cultists/cultists_buff_damp");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.6f);
		TalkCmd.Play(_cawCawDialogue, base.Creature);
		await PowerCmd.Apply<RitualPower>(base.Creature, IncantationAmount, base.Creature, null);
	}

	private async Task DarkStrikeMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(DarkStrikeDamage).FromMonster(this).WithAttackerAnim("Attack", 0.2f)
			.BeforeDamage(PlayAttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("buff");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		return creatureAnimator;
	}

	private Task PlayAttackSfx()
	{
		SfxCmd.Play(AttackSfx, "enemy_strength", AttackSfxStrength);
		AttackSfxStrength += 0.2f;
		return Task.CompletedTask;
	}
}
