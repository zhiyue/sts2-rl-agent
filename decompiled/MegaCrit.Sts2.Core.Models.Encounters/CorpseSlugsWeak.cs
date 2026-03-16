using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class CorpseSlugsWeak : EncounterModel
{
	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlySingleElementList<EncounterTag>(EncounterTag.Slugs);

	public override RoomType RoomType => RoomType.Monster;

	public override bool IsWeak => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<CorpseSlug>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		int num = 2;
		List<(MonsterModel, string)> list = new List<(MonsterModel, string)>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<(MonsterModel, string)> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = (ModelDb.Monster<CorpseSlug>().ToMutable(), null);
		num2++;
		span[num2] = (ModelDb.Monster<CorpseSlug>().ToMutable(), null);
		List<(MonsterModel, string)> list2 = list;
		CorpseSlug.EnsureCorpseSlugsStartWithDifferentMoves(list2.Select(((MonsterModel, string) kvp) => kvp.Item1), base.Rng);
		return list2;
	}
}
