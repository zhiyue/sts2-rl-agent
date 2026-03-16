using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class ToadpolesWeak : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override bool IsWeak => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<Toadpole>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		Toadpole toadpole = (Toadpole)ModelDb.Monster<Toadpole>().ToMutable();
		toadpole.IsFront = true;
		Toadpole toadpole2 = (Toadpole)ModelDb.Monster<Toadpole>().ToMutable();
		toadpole2.IsFront = false;
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(toadpole, null),
			(toadpole2, null)
		});
	}
}
