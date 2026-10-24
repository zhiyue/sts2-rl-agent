using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

/// <summary>
/// Represents an encounter that has been removed from the game. Mostly used for the run history.
/// </summary>
public sealed class DeprecatedEncounter : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override bool IsDebugEncounter => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => Array.Empty<MonsterModel>();

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return Array.Empty<(MonsterModel, string)>();
	}
}
