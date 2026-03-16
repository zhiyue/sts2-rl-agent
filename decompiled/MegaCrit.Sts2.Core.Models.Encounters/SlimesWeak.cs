using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class SlimesWeak : EncounterModel
{
	private static readonly MonsterModel[] _smallSlimes = new MonsterModel[2]
	{
		ModelDb.Monster<LeafSlimeS>(),
		ModelDb.Monster<TwigSlimeS>()
	};

	private static readonly MonsterModel[] _mediumSlimes = new MonsterModel[2]
	{
		ModelDb.Monster<LeafSlimeM>(),
		ModelDb.Monster<TwigSlimeM>()
	};

	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlySingleElementList<EncounterTag>(EncounterTag.Slimes);

	public override bool IsWeak => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters
	{
		get
		{
			MonsterModel[] smallSlimes = _smallSlimes;
			MonsterModel[] mediumSlimes = _mediumSlimes;
			int num = 0;
			MonsterModel[] array = new MonsterModel[smallSlimes.Length + mediumSlimes.Length];
			ReadOnlySpan<MonsterModel> readOnlySpan = new ReadOnlySpan<MonsterModel>(smallSlimes);
			readOnlySpan.CopyTo(new Span<MonsterModel>(array).Slice(num, readOnlySpan.Length));
			num += readOnlySpan.Length;
			ReadOnlySpan<MonsterModel> readOnlySpan2 = new ReadOnlySpan<MonsterModel>(mediumSlimes);
			readOnlySpan2.CopyTo(new Span<MonsterModel>(array).Slice(num, readOnlySpan2.Length));
			num += readOnlySpan2.Length;
			return new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(array);
		}
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		List<(MonsterModel, string)> list = new List<(MonsterModel, string)>();
		List<MonsterModel> list2 = _smallSlimes.ToList();
		MonsterModel monsterModel = base.Rng.NextItem(list2);
		list2.Remove(monsterModel);
		MonsterModel monsterModel2 = base.Rng.NextItem(list2);
		list.Add((monsterModel.ToMutable(), null));
		list.Add((base.Rng.NextItem(_mediumSlimes).ToMutable(), null));
		list.Add((monsterModel2.ToMutable(), null));
		return list;
	}
}
