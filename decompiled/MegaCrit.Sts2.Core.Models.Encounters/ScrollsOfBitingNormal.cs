using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class ScrollsOfBitingNormal : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlySingleElementList<EncounterTag>(EncounterTag.Scrolls);

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<ScrollOfBiting>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		ScrollOfBiting scrollOfBiting = (ScrollOfBiting)ModelDb.Monster<ScrollOfBiting>().ToMutable();
		ScrollOfBiting scrollOfBiting2 = (ScrollOfBiting)ModelDb.Monster<ScrollOfBiting>().ToMutable();
		ScrollOfBiting scrollOfBiting3 = (ScrollOfBiting)ModelDb.Monster<ScrollOfBiting>().ToMutable();
		ScrollOfBiting scrollOfBiting4 = (ScrollOfBiting)ModelDb.Monster<ScrollOfBiting>().ToMutable();
		int num = (scrollOfBiting.StarterMoveIdx = base.Rng.NextInt(3));
		scrollOfBiting2.StarterMoveIdx = (num + 1) % 3;
		scrollOfBiting3.StarterMoveIdx = (num + 2) % 3;
		scrollOfBiting4.StarterMoveIdx = 2;
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[4]
		{
			(scrollOfBiting, null),
			(scrollOfBiting2, null),
			(scrollOfBiting3, null),
			(scrollOfBiting4, null)
		});
	}
}
