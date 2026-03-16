using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class TurretOperatorWeak : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override bool IsWeak => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[2]
	{
		ModelDb.Monster<LivingShield>(),
		ModelDb.Monster<TurretOperator>()
	});

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(ModelDb.Monster<LivingShield>().ToMutable(), null),
			(ModelDb.Monster<TurretOperator>().ToMutable(), null)
		});
	}
}
