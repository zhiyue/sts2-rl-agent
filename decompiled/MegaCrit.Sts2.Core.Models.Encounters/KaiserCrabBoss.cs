using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class KaiserCrabBoss : EncounterModel
{
	private const string _crusherSlot = "crusher";

	private const string _rocketSlot = "rocket";

	public override string BossNodePath => "res://images/map/placeholder/" + base.Id.Entry.ToLowerInvariant() + "_icon";

	protected override bool HasCustomBackground => true;

	public override MegaSkeletonDataResource? BossNodeSpineResource => null;

	public override RoomType RoomType => RoomType.Boss;

	public override bool FullyCenterPlayers => true;

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "crusher", "rocket" });

	public override bool HasScene => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[2]
	{
		ModelDb.Monster<Crusher>(),
		ModelDb.Monster<Rocket>()
	});

	public override float GetCameraScaling()
	{
		return 0.75f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 35f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(ModelDb.Monster<Crusher>().ToMutable(), "crusher"),
			(ModelDb.Monster<Rocket>().ToMutable(), "rocket")
		});
	}
}
