using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters.Mocks;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters.Mocks;

public sealed class MockAttackAndSummonEncounter : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override bool IsDebugEncounter => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<MockAttackAndSummonMinionMonster>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<MockAttackAndSummonMinionMonster>().ToMutable(), null));
	}
}
