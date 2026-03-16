using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class ExoskeletonsWeak : EncounterModel
{
	public override bool IsWeak => true;

	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlySingleElementList<EncounterTag>(EncounterTag.Exoskeletons);

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "first", "second", "third" });

	public override bool HasScene => true;

	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<Exoskeleton>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[3]
		{
			(ModelDb.Monster<Exoskeleton>().ToMutable(), "first"),
			(ModelDb.Monster<Exoskeleton>().ToMutable(), "second"),
			(ModelDb.Monster<Exoskeleton>().ToMutable(), "third")
		});
	}
}
