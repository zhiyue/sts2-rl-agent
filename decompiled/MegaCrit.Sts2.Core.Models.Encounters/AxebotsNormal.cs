using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class AxebotsNormal : EncounterModel
{
	private const string _backSlot = "back";

	private const string _frontSlot = "front";

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "front", "back" });

	public override bool HasScene => true;

	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<Axebot>());

	public override float GetCameraScaling()
	{
		return 0.9f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(ModelDb.Monster<Axebot>().ToMutable(), "front"),
			(ModelDb.Monster<Axebot>().ToMutable(), "back")
		});
	}
}
