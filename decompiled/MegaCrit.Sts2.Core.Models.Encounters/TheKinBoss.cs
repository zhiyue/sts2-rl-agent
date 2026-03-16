using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class TheKinBoss : EncounterModel
{
	public override RoomType RoomType => RoomType.Boss;

	public override bool HasScene => true;

	protected override bool HasCustomBackground => true;

	public override string CustomBgm => "event:/music/act1_boss_the_kin";

	public override string BossNodePath => "res://images/map/placeholder/" + base.Id.Entry.ToLowerInvariant() + "_icon";

	public override MegaSkeletonDataResource? BossNodeSpineResource => null;

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "slot1", "slot2", "leaderSlot" });

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[2]
	{
		ModelDb.Monster<KinFollower>(),
		ModelDb.Monster<KinPriest>()
	});

	public override float GetCameraScaling()
	{
		return 0.85f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 50f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		KinFollower kinFollower = (KinFollower)ModelDb.Monster<KinFollower>().ToMutable();
		kinFollower.StartsWithDance = true;
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[3]
		{
			(kinFollower, "slot1"),
			(ModelDb.Monster<KinFollower>().ToMutable(), "slot2"),
			(ModelDb.Monster<KinPriest>().ToMutable(), "leaderSlot")
		});
	}
}
