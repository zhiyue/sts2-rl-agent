using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class KnowledgeDemonBoss : EncounterModel
{
	public override RoomType RoomType => RoomType.Boss;

	public override string CustomBgm => "event:/music/act2_boss_knowledge_demon";

	public override string BossNodePath => "res://images/map/placeholder/" + base.Id.Entry.ToLowerInvariant() + "_icon";

	public override MegaSkeletonDataResource? BossNodeSpineResource => null;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<KnowledgeDemon>());

	protected override bool HasCustomBackground => true;

	public override float GetCameraScaling()
	{
		return 0.85f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 70f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<KnowledgeDemon>().ToMutable(), null));
	}
}
