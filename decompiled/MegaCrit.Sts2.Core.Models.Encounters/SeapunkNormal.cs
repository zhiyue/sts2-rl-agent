using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class SeapunkNormal : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlySingleElementList<EncounterTag>(EncounterTag.Seapunk);

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[2]
	{
		ModelDb.Monster<Seapunk>(),
		ModelDb.Monster<CalcifiedCultist>()
	});

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(ModelDb.Monster<CalcifiedCultist>().ToMutable(), null),
			(ModelDb.Monster<Seapunk>().ToMutable(), null)
		});
	}
}
