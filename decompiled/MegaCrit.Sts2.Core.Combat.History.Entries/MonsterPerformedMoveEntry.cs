using System.Collections.Generic;
using System.Linq;
using System.Text;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

public class MonsterPerformedMoveEntry : CombatHistoryEntry
{
	public MonsterModel Monster { get; }

	public MoveState Move { get; }

	public IEnumerable<Creature>? Targets { get; }

	public override string Description
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(Monster.Id.Entry + " performed " + Move.Id);
			if (Targets != null)
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(11, 1, stringBuilder2);
				handler.AppendLiteral(" targeting ");
				handler.AppendFormatted(string.Join(",", Targets.Select(GetTargetName)));
				stringBuilder2.Append(ref handler);
			}
			return stringBuilder.ToString();
		}
	}

	public MonsterPerformedMoveEntry(MonsterModel monster, MoveState move, IEnumerable<Creature>? targets, int roundNumber, CombatSide currentSide, CombatHistory history)
		: base(monster.Creature, roundNumber, currentSide, history)
	{
		Monster = monster;
		Move = move;
		Targets = targets;
	}

	private static string GetTargetName(Creature creature)
	{
		if (!creature.IsPlayer)
		{
			return creature.Monster.Id.Entry;
		}
		return creature.Player.Character.Id.Entry;
	}
}
