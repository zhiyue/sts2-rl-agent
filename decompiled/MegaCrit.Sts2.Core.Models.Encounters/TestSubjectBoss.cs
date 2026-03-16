using System.Collections.Generic;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class TestSubjectBoss : EncounterModel
{
	public override RoomType RoomType => RoomType.Boss;

	public override string BossNodePath => "res://images/map/placeholder/" + base.Id.Entry.ToLowerInvariant() + "_icon";

	public override string CustomBgm => "event:/music/act3_boss_test_subject";

	public override MegaSkeletonDataResource? BossNodeSpineResource => null;

	protected override bool HasCustomBackground => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<TestSubject>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<TestSubject>().ToMutable(), null));
	}
}
