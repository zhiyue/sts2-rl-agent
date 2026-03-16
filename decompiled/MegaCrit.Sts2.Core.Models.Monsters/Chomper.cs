using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Chomper : MonsterModel
{
	public const string screechMoveId = "SCREECH_MOVE";

	private const int _screechStatusCount = 3;

	private const int _clampRepeat = 2;

	private bool _screamFirst;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 63, 60);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 67, 64);

	private static int ClampDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	public bool ScreamFirst
	{
		get
		{
			return _screamFirst;
		}
		set
		{
			AssertMutable();
			_screamFirst = value;
		}
	}

	public override string TakeDamageSfx => "event:/sfx/enemy/enemy_attacks/chomper/chomper_hurt";

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<ArtifactPower>(base.Creature, 2m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("CLAMP_MOVE", ClampMove, new MultiAttackIntent(ClampDamage, 2));
		MoveState moveState2 = (MoveState)(moveState.FollowUpState = new MoveState("SCREECH_MOVE", ScreechMove, new StatusIntent(3)));
		moveState2.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		MoveState initialState = (_screamFirst ? moveState2 : moveState);
		return new MonsterMoveStateMachine(list, initialState);
	}

	private async Task ClampMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ClampDamage).WithHitCount(2).FromMonster(this)
			.WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task ScreechMove(IReadOnlyList<Creature> targets)
	{
		LocString line = MonsterModel.L10NMonsterLookup("CHOMPER.moves.SCREECH.title");
		TalkCmd.Play(line, base.Creature);
		SfxCmd.Play(CastSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 1f);
		await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Discard, 3, addedByPlayer: false);
	}
}
