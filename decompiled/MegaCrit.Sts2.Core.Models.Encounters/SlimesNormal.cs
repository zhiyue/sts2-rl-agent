using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class SlimesNormal : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlySingleElementList<EncounterTag>(EncounterTag.Slimes);

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[4]
	{
		ModelDb.Monster<LeafSlimeS>(),
		ModelDb.Monster<TwigSlimeS>(),
		ModelDb.Monster<TwigSlimeM>(),
		ModelDb.Monster<LeafSlimeM>()
	});

	public override float GetCameraScaling()
	{
		return 0.9f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 50f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		bool flag = base.Rng.NextBool();
		MonsterModel monsterModel = (flag ? ((MonsterModel)ModelDb.Monster<LeafSlimeS>()) : ((MonsterModel)ModelDb.Monster<TwigSlimeS>()));
		MonsterModel monsterModel2 = (flag ? ((MonsterModel)ModelDb.Monster<TwigSlimeS>()) : ((MonsterModel)ModelDb.Monster<LeafSlimeS>()));
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[4]
		{
			(ModelDb.Monster<TwigSlimeM>().ToMutable(), null),
			(ModelDb.Monster<LeafSlimeM>().ToMutable(), null),
			(monsterModel.ToMutable(), null),
			(monsterModel2.ToMutable(), null)
		});
	}
}
