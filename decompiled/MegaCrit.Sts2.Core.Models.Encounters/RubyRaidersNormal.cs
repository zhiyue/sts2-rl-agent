using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class RubyRaidersNormal : EncounterModel
{
	private static readonly Dictionary<MonsterModel, int> _raiderValidCounts = new Dictionary<MonsterModel, int>
	{
		{
			ModelDb.Monster<AxeRubyRaider>(),
			1
		},
		{
			ModelDb.Monster<AssassinRubyRaider>(),
			1
		},
		{
			ModelDb.Monster<BruteRubyRaider>(),
			1
		},
		{
			ModelDb.Monster<CrossbowRubyRaider>(),
			1
		},
		{
			ModelDb.Monster<TrackerRubyRaider>(),
			1
		}
	};

	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => _raiderValidCounts.Keys;

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		List<MonsterModel> currentRaiders = new List<MonsterModel>();
		List<(MonsterModel, string)> list = new List<(MonsterModel, string)>();
		for (int i = 0; i < 3; i++)
		{
			List<MonsterModel> items = _raiderValidCounts.Keys.Where((MonsterModel r) => currentRaiders.Count((MonsterModel c) => c == r) < _raiderValidCounts[r]).ToList();
			MonsterModel monsterModel = base.Rng.NextItem(items);
			currentRaiders.Add(monsterModel);
			list.Add((monsterModel.ToMutable(), null));
		}
		return list;
	}
}
