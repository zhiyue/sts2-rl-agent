using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class ConstructMenagerieNormal : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[2]
	{
		ModelDb.Monster<PunchConstruct>(),
		ModelDb.Monster<CubexConstruct>()
	});

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[3]
		{
			(ModelDb.Monster<PunchConstruct>().ToMutable(), null),
			(ModelDb.Monster<CubexConstruct>().ToMutable(), null),
			(ModelDb.Monster<CubexConstruct>().ToMutable(), null)
		});
	}
}
