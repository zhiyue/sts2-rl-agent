using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class SlumberingBeetleNormal : EncounterModel
{
	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlySingleElementList<EncounterTag>(EncounterTag.Workers);

	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[3]
	{
		ModelDb.Monster<BowlbugRock>(),
		ModelDb.Monster<SlumberingBeetle>(),
		ModelDb.Monster<BowlbugSilk>()
	});

	public override bool HasScene => true;

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[3]
		{
			(ModelDb.Monster<BowlbugRock>().ToMutable(), "first"),
			(ModelDb.Monster<BowlbugSilk>().ToMutable(), "second"),
			(ModelDb.Monster<SlumberingBeetle>().ToMutable(), "third")
		});
	}

	public override float GetCameraScaling()
	{
		return 0.85f;
	}

	public override Vector2 GetCameraOffset()
	{
		return new Vector2(0f, 50f);
	}
}
