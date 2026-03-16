using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class SlitheringStranglerNormal : EncounterModel
{
	private enum SecondaryEnemyType
	{
		SnappingJaxfruit,
		MediumSlime,
		SmallSlimes
	}

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

	public override IEnumerable<MonsterModel> AllPossibleMonsters
	{
		get
		{
			MonsterModel[] smallSlimes = _smallSlimes;
			MonsterModel[] mediumSlimes = _mediumSlimes;
			int num = 0;
			MonsterModel[] array = new MonsterModel[2 + (smallSlimes.Length + mediumSlimes.Length)];
			ReadOnlySpan<MonsterModel> readOnlySpan = new ReadOnlySpan<MonsterModel>(smallSlimes);
			readOnlySpan.CopyTo(new Span<MonsterModel>(array).Slice(num, readOnlySpan.Length));
			num += readOnlySpan.Length;
			ReadOnlySpan<MonsterModel> readOnlySpan2 = new ReadOnlySpan<MonsterModel>(mediumSlimes);
			readOnlySpan2.CopyTo(new Span<MonsterModel>(array).Slice(num, readOnlySpan2.Length));
			num += readOnlySpan2.Length;
			array[num] = ModelDb.Monster<SnappingJaxfruit>();
			num++;
			array[num] = ModelDb.Monster<SlitheringStrangler>();
			return new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(array);
		}
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		List<MonsterModel> list2;
		switch (base.Rng.NextItem(Enum.GetValues<SecondaryEnemyType>()))
		{
		case SecondaryEnemyType.SnappingJaxfruit:
		{
			int num = 1;
			List<MonsterModel> list4 = new List<MonsterModel>(num);
			CollectionsMarshal.SetCount(list4, num);
			Span<MonsterModel> span = CollectionsMarshal.AsSpan(list4);
			int num2 = 0;
			span[num2] = ModelDb.Monster<SnappingJaxfruit>();
			list2 = list4;
			break;
		}
		case SecondaryEnemyType.MediumSlime:
		{
			int num2 = 1;
			List<MonsterModel> list3 = new List<MonsterModel>(num2);
			CollectionsMarshal.SetCount(list3, num2);
			Span<MonsterModel> span = CollectionsMarshal.AsSpan(list3);
			int num = 0;
			span[num] = base.Rng.NextItem(_mediumSlimes);
			list2 = list3;
			break;
		}
		case SecondaryEnemyType.SmallSlimes:
		{
			int num = 2;
			List<MonsterModel> list = new List<MonsterModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<MonsterModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = base.Rng.NextItem(_smallSlimes);
			num2++;
			span[num2] = base.Rng.NextItem(_smallSlimes);
			list2 = list;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		List<MonsterModel> list5 = list2;
		list5.Add(ModelDb.Monster<SlitheringStrangler>());
		return list5.Select((MonsterModel m) => ((MonsterModel, string?))(m.ToMutable(), null)).ToList();
	}
}
