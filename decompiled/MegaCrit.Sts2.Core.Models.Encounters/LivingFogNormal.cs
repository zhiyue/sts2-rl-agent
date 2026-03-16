using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class LivingFogNormal : EncounterModel
{
	private const string _livingFogSlot = "livingFog";

	private const string _bombSlotPrefix = "bomb";

	public override RoomType RoomType => RoomType.Monster;

	public override bool HasScene => true;

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlyArray<string>(new string[6] { "bomb1", "bomb2", "bomb3", "bomb4", "bomb5", "livingFog" });

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[2]
	{
		ModelDb.Monster<LivingFog>(),
		ModelDb.Monster<GasBomb>()
	});

	public override IEnumerable<string>? ExtraAssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ModelDb.Affliction<Smog>().OverlayPath);

	public override float GetCameraScaling()
	{
		return 0.9f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<LivingFog>().ToMutable(), "livingFog"));
	}
}
