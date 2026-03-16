using System.Collections.Generic;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class WaterfallGiantBoss : EncounterModel
{
	public override RoomType RoomType => RoomType.Boss;

	public override string CustomBgm => "event:/music/act1_b_boss_waterfall_giant";

	public override string BossNodePath => "res://images/map/placeholder/" + base.Id.Entry.ToLowerInvariant() + "_icon";

	public override MegaSkeletonDataResource? BossNodeSpineResource => null;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<WaterfallGiant>());

	public override float GetCameraScaling()
	{
		return 0.9f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<WaterfallGiant>().ToMutable(), null));
	}
}
