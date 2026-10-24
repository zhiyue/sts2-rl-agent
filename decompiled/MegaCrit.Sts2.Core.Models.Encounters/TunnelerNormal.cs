using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class TunnelerNormal : EncounterModel
{
	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlyArray<EncounterTag>(new EncounterTag[2]
	{
		EncounterTag.Burrower,
		EncounterTag.Chomper
	});

	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[2]
	{
		ModelDb.Monster<Tunneler>(),
		ModelDb.Monster<Chomper>()
	});

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		Chomper chomper = (Chomper)ModelDb.Monster<Chomper>().ToMutable();
		chomper.ScreamFirst = true;
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(chomper, null),
			(ModelDb.Monster<Tunneler>().ToMutable(), null)
		});
	}
}
