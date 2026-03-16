using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class CultistsNormal : EncounterModel
{
	private static readonly MonsterModel[] _cultists = new MonsterModel[2]
	{
		ModelDb.Monster<CalcifiedCultist>(),
		ModelDb.Monster<DampCultist>()
	};

	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => _cultists;

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(ModelDb.Monster<CalcifiedCultist>().ToMutable(), null),
			(ModelDb.Monster<DampCultist>().ToMutable(), null)
		});
	}
}
