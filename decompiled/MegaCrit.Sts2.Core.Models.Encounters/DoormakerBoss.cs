using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class DoormakerBoss : EncounterModel
{
	private const string _doorSlot = "door";

	public const string doormakerSlot = "doormaker";

	public override RoomType RoomType => RoomType.Boss;

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "doormaker", "door" });

	public override MegaSkeletonDataResource? BossNodeSpineResource => null;

	public override string BossNodePath => "res://images/map/placeholder/" + base.Id.Entry.ToLowerInvariant() + "_icon";

	public override string CustomBgm => "event:/music/act3_boss_queen";

	protected override bool HasCustomBackground => false;

	public override bool HasScene => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[2]
	{
		ModelDb.Monster<Door>(),
		ModelDb.Monster<Doormaker>()
	});

	public override float GetCameraScaling()
	{
		return 0.9f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 60f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<Door>().ToMutable(), "door"));
	}
}
