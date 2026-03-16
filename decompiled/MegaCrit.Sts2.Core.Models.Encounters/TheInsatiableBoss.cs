using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class TheInsatiableBoss : EncounterModel
{
	public override string CustomBgm => "event:/music/act2_boss_the_insatiable";

	public override RoomType RoomType => RoomType.Boss;

	protected override bool HasCustomBackground => true;

	public override string AmbientSfx => "event:/sfx/ambience/act2_ambience_the_insatiable";

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<TheInsatiable>());

	public override float GetCameraScaling()
	{
		return 0.9f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<TheInsatiable>().ToMutable(), null));
	}
}
