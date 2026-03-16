using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters.Mocks;

public sealed class MockNoRewardsEncounter : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override bool IsDebugEncounter => true;

	public override bool ShouldGiveRewards => false;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<BigDummy>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<BigDummy>().ToMutable(), null));
	}
}
