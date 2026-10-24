using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class KnightsElite : EncounterModel
{
	public override RoomType RoomType => RoomType.Elite;

	public override IEnumerable<EncounterTag> Tags => new global::_003C_003Ez__ReadOnlySingleElementList<EncounterTag>(EncounterTag.Knights);

	public override bool HasScene => true;

	public override IEnumerable<string> ExtraAssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ModelDb.Affliction<Hexed>().OverlayPath);

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[3]
	{
		ModelDb.Monster<FlailKnight>(),
		ModelDb.Monster<SpectralKnight>(),
		ModelDb.Monster<MagiKnight>()
	});

	public override float GetCameraScaling()
	{
		return 0.87f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 50f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[3]
		{
			(ModelDb.Monster<FlailKnight>().ToMutable(), "first"),
			(ModelDb.Monster<SpectralKnight>().ToMutable(), "second"),
			(ModelDb.Monster<MagiKnight>().ToMutable(), "third")
		});
	}
}
