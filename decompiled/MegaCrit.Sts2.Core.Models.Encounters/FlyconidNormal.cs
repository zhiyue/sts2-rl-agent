using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class FlyconidNormal : EncounterModel
{
	private static readonly MonsterModel[] _mediumSlimes = new MonsterModel[2]
	{
		ModelDb.Monster<LeafSlimeM>(),
		ModelDb.Monster<TwigSlimeM>()
	};

	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlyArray<EncounterTag>(new EncounterTag[2]
	{
		EncounterTag.Mushroom,
		EncounterTag.Slimes
	});

	public override IEnumerable<MonsterModel> AllPossibleMonsters
	{
		get
		{
			MonsterModel monsterModel = ModelDb.Monster<Flyconid>();
			MonsterModel[] mediumSlimes = _mediumSlimes;
			int num = 0;
			MonsterModel[] array = new MonsterModel[1 + mediumSlimes.Length];
			array[num] = monsterModel;
			num++;
			ReadOnlySpan<MonsterModel> readOnlySpan = new ReadOnlySpan<MonsterModel>(mediumSlimes);
			readOnlySpan.CopyTo(new Span<MonsterModel>(array).Slice(num, readOnlySpan.Length));
			num += readOnlySpan.Length;
			return new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(array);
		}
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(base.Rng.NextItem(_mediumSlimes).ToMutable(), null),
			(ModelDb.Monster<Flyconid>().ToMutable(), null)
		});
	}
}
