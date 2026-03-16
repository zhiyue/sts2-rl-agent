using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class SnappingJaxfruitNormal : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlySingleElementList<EncounterTag>(EncounterTag.Mushroom);

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[2]
	{
		ModelDb.Monster<SnappingJaxfruit>(),
		ModelDb.Monster<Flyconid>()
	});

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(ModelDb.Monster<SnappingJaxfruit>().ToMutable(), null),
			(ModelDb.Monster<Flyconid>().ToMutable(), null)
		});
	}
}
