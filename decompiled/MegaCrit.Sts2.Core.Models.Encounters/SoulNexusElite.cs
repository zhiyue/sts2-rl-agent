using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class SoulNexusElite : EncounterModel
{
	public override RoomType RoomType => RoomType.Elite;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<SoulNexus>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<SoulNexus>().ToMutable(), null));
	}
}
