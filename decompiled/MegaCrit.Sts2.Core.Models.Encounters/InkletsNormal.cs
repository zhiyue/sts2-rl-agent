using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class InkletsNormal : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<Inklet>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		Inklet item = (Inklet)ModelDb.Monster<Inklet>().ToMutable();
		Inklet inklet = (Inklet)ModelDb.Monster<Inklet>().ToMutable();
		Inklet item2 = (Inklet)ModelDb.Monster<Inklet>().ToMutable();
		inklet.MiddleInklet = true;
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[3]
		{
			(item, null),
			(inklet, null),
			(item2, null)
		});
	}
}
