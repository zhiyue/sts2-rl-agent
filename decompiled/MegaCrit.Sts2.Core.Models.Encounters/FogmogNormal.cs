using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class FogmogNormal : EncounterModel
{
	public const string illusionSlot = "illusion";

	public const string fogmogSlot = "fogmog";

	public override RoomType RoomType => RoomType.Monster;

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "illusion", "fogmog" });

	public override bool HasScene => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[2]
	{
		ModelDb.Monster<Fogmog>(),
		ModelDb.Monster<EyeWithTeeth>()
	});

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<Fogmog>().ToMutable(), "fogmog"));
	}
}
