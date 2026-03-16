using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class BowlbugsWeak : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlySingleElementList<EncounterTag>(EncounterTag.Workers);

	public override bool IsWeak => true;

	private static MonsterModel[] Bugs => new MonsterModel[2]
	{
		ModelDb.Monster<BowlbugEgg>(),
		ModelDb.Monster<BowlbugNectar>()
	};

	public override IEnumerable<MonsterModel> AllPossibleMonsters => Bugs.Concat(new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<BowlbugRock>()));

	public override bool HasScene => true;

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(ModelDb.Monster<BowlbugRock>().ToMutable(), "odd"),
			(base.Rng.NextItem(Bugs).ToMutable(), "even")
		});
	}
}
