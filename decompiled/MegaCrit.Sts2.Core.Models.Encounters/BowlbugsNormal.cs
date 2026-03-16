using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class BowlbugsNormal : EncounterModel
{
	private static readonly Dictionary<MonsterModel, int> _workerValidCounts = new Dictionary<MonsterModel, int>
	{
		{
			ModelDb.Monster<BowlbugEgg>(),
			1
		},
		{
			ModelDb.Monster<BowlbugSilk>(),
			1
		},
		{
			ModelDb.Monster<BowlbugNectar>(),
			1
		}
	};

	private static readonly string[] _slotNames = new string[3] { "first", "middle", "last" };

	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlySingleElementList<EncounterTag>(EncounterTag.Workers);

	public override RoomType RoomType => RoomType.Monster;

	public override bool HasScene => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => _workerValidCounts.Keys;

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		List<MonsterModel> currentWorkers = new List<MonsterModel>();
		int num = 1;
		List<(MonsterModel, string)> list = new List<(MonsterModel, string)>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<(MonsterModel, string)> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = (ModelDb.Monster<BowlbugRock>().ToMutable(), _slotNames[0]);
		List<(MonsterModel, string)> list2 = list;
		for (int i = 0; i < 2; i++)
		{
			List<MonsterModel> items = _workerValidCounts.Keys.Where((MonsterModel r) => currentWorkers.Count((MonsterModel c) => c == r) < _workerValidCounts[r]).ToList();
			MonsterModel monsterModel = base.Rng.NextItem(items);
			currentWorkers.Add(monsterModel);
			list2.Add((monsterModel.ToMutable(), _slotNames[i + 1]));
		}
		return list2;
	}
}
