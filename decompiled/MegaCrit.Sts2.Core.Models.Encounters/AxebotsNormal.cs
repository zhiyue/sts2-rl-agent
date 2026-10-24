using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class AxebotsNormal : EncounterModel
{
	private const string _frontSlot = "front";

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlySingleElementList<string>("front");

	public override bool HasScene => true;

	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<Axebot>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<Axebot>().ToMutable(), "front"));
	}
}
