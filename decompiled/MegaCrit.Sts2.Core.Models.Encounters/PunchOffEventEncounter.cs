using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class PunchOffEventEncounter : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<PunchConstruct>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		PunchConstruct punchConstruct = (PunchConstruct)ModelDb.Monster<PunchConstruct>().ToMutable();
		punchConstruct.StartsWithStrongPunch = true;
		punchConstruct.StartingHpReduction = base.Rng.NextInt(2, 10);
		PunchConstruct punchConstruct2 = (PunchConstruct)ModelDb.Monster<PunchConstruct>().ToMutable();
		punchConstruct2.StartingHpReduction = base.Rng.NextInt(2, 10);
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(punchConstruct, null),
			(punchConstruct2, null)
		});
	}
}
