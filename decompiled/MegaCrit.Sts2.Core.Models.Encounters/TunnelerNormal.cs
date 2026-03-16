using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class TunnelerNormal : EncounterModel
{
	private static readonly MonsterModel[] _bugs = new MonsterModel[2]
	{
		ModelDb.Monster<BowlbugEgg>(),
		ModelDb.Monster<BowlbugSilk>()
	};

	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlyArray<EncounterTag>(new EncounterTag[2]
	{
		EncounterTag.Burrower,
		EncounterTag.Workers
	});

	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters
	{
		get
		{
			MonsterModel monsterModel = ModelDb.Monster<Tunneler>();
			MonsterModel[] bugs = _bugs;
			int num = 0;
			MonsterModel[] array = new MonsterModel[1 + bugs.Length];
			array[num] = monsterModel;
			num++;
			ReadOnlySpan<MonsterModel> readOnlySpan = new ReadOnlySpan<MonsterModel>(bugs);
			readOnlySpan.CopyTo(new Span<MonsterModel>(array).Slice(num, readOnlySpan.Length));
			num += readOnlySpan.Length;
			return new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(array);
		}
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(base.Rng.NextItem(_bugs).ToMutable(), null),
			(ModelDb.Monster<Tunneler>().ToMutable(), null)
		});
	}
}
