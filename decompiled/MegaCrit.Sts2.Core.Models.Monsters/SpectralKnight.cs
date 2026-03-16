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
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class SpectralKnight : MonsterModel
{
	private const int _soulFlameRepeat = 3;

	private const string _attackFlameTrigger = "AttackFlame";

	private const string _attackSwordTrigger = "AttackSword";

	private const string _hexSfx = "event:/sfx/enemy/enemy_attacks/spectral_knight/spectral_knight_hex";

	private const string _soulFlameSfx = "event:/sfx/enemy/enemy_attacks/spectral_knight/spectral_knight_soul_flame";

	private const string _soulSlashSfx = "event:/sfx/enemy/enemy_attacks/spectral_knight/spectral_knight_soul_slash";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 97, 93);

	public override int MaxInitialHp => MinInitialHp;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	private int SoulSlashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);

	private int SoulFlameDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("HEX", HexMove, new DebuffIntent());
		MoveState moveState2 = new MoveState("SOUL_SLASH", SoulSlashMove, new SingleAttackIntent(SoulSlashDamage));
		MoveState moveState3 = new MoveState("SOUL_FLAME", SoulFlameMove, new MultiAttackIntent(SoulFlameDamage, 3));
		RandomBranchState randomBranchState = new RandomBranchState("RAND");
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = randomBranchState;
		moveState3.FollowUpState = randomBranchState;
		randomBranchState.AddBranch(moveState2, 2);
		randomBranchState.AddBranch(moveState3, MoveRepeatType.CannotRepeat);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(randomBranchState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task HexMove(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.3f);
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/spectral_knight/spectral_knight_hex");
		foreach (Creature target in targets)
		{
			await PowerCmd.Apply<HexPower>(target, 2m, base.Creature, null);
		}
	}

	private async Task SoulSlashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SoulSlashDamage).FromMonster(this).WithAttackerAnim("AttackSword", 0.25f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/spectral_knight/spectral_knight_soul_slash")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task SoulFlameMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SoulFlameDamage).WithHitCount(3).FromMonster(this)
			.OnlyPlayAnimOnce()
			.WithAttackerAnim("AttackFlame", 0.25f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/spectral_knight/spectral_knight_soul_flame")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("debuff");
		AnimState animState3 = new AnimState("attack_sword");
		AnimState animState4 = new AnimState("attack_flame");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("AttackSword", animState3);
		creatureAnimator.AddAnyState("AttackFlame", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		return creatureAnimator;
	}
}
