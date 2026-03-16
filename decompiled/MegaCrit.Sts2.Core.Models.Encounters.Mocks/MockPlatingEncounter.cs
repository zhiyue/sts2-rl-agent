using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters.Mocks;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters.Mocks;

public sealed class MockPlatingEncounter : EncounterModel
{
	private int _platingAmount = 1;

	public override RoomType RoomType => RoomType.Monster;

	public override bool IsDebugEncounter => true;

	public int PlatingAmount
	{
		get
		{
			return _platingAmount;
		}
		set
		{
			AssertMutable();
			_platingAmount = value;
		}
	}

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<MockPlatingMonster>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		MonsterModel monsterModel = ModelDb.Monster<MockPlatingMonster>().ToMutable();
		((MockPlatingMonster)monsterModel).PlatingAmount = PlatingAmount;
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((monsterModel, null));
	}
}
