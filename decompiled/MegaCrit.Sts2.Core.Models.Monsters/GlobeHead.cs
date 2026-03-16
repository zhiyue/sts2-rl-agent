using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class GlobeHead : MonsterModel
{
	private const string _chargeSfx = "event:/sfx/enemy/enemy_attacks/globe_head/globe_head_charge";

	private const string _slapSfx = "event:/sfx/enemy/enemy_attacks/globe_head/globe_head_slap";

	private const int _thunderStrikeRepeat = 3;

	private const int _shockingSlapFrail = 2;

	private const int _galvanicBurstStr = 2;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 158, 148);

	public override int MaxInitialHp => MinInitialHp;

	private int ThunderStrikeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);

	private int ShockingSlapDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 13);

	private int GalvanicBurstDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 16);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<GalvanicPower>(base.Creature, 6m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("THUNDER_STRIKE", ThunderStrike, new MultiAttackIntent(ThunderStrikeDamage, 3));
		MoveState moveState2 = new MoveState("SHOCKING_SLAP", ShockingSlap, new SingleAttackIntent(ShockingSlapDamage), new DebuffIntent());
		MoveState moveState3 = new MoveState("GALVANIC_BURST", GalvanicBurstMove, new SingleAttackIntent(GalvanicBurstDamage), new BuffIntent());
		moveState2.FollowUpState = moveState;
		moveState.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState2;
		list.Add(moveState2);
		list.Add(moveState);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState2);
	}

	private async Task ThunderStrike(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ThunderStrikeDamage).WithHitCount(3).FromMonster(this)
			.WithAttackerAnim("Cast", 0.5f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/globe_head/globe_head_charge")
			.WithHitFx("vfx/vfx_attack_lightning")
			.Execute(null);
	}

	private async Task ShockingSlap(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ShockingSlapDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/globe_head/globe_head_slap")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<FrailPower>(targets, 2m, base.Creature, null);
	}

	private async Task GalvanicBurstMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(GalvanicBurstDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
			.WithHitFx("vfx/vfx_attack_lightning", null, "blunt_attack.mp3")
			.Execute(null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
	}
}
