using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class QueenBoss : EncounterModel
{
	private const string _queenSlot = "queen";

	private const string _amalgamSlot = "amalgam";

	public override RoomType RoomType => RoomType.Boss;

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "amalgam", "queen" });

	public override string CustomBgm => "event:/music/act3_boss_queen";

	protected override bool HasCustomBackground => true;

	public override bool HasScene => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[2]
	{
		ModelDb.Monster<Queen>(),
		ModelDb.Monster<TorchHeadAmalgam>()
	});

	public override IEnumerable<string>? ExtraAssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ModelDb.Affliction<Bound>().OverlayPath);

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
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(ModelDb.Monster<TorchHeadAmalgam>().ToMutable(), "amalgam"),
			(ModelDb.Monster<Queen>().ToMutable(), "queen")
		});
	}
}
