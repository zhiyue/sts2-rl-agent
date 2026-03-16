using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class DecimillipedeElite : EncounterModel
{
	private const string _segmentSlot = "segment";

	public override bool HasScene => true;

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "segment1", "segment2", "segment3" });

	public override RoomType RoomType => RoomType.Elite;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[3]
	{
		ModelDb.Monster<DecimillipedeSegmentFront>(),
		ModelDb.Monster<DecimillipedeSegmentMiddle>(),
		ModelDb.Monster<DecimillipedeSegmentBack>()
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
		DecimillipedeSegment decimillipedeSegment = (DecimillipedeSegment)ModelDb.Monster<DecimillipedeSegmentFront>().ToMutable();
		DecimillipedeSegment decimillipedeSegment2 = (DecimillipedeSegment)ModelDb.Monster<DecimillipedeSegmentMiddle>().ToMutable();
		DecimillipedeSegment decimillipedeSegment3 = (DecimillipedeSegment)ModelDb.Monster<DecimillipedeSegmentBack>().ToMutable();
		int num = (decimillipedeSegment.StarterMoveIdx = base.Rng.NextInt(3));
		decimillipedeSegment2.StarterMoveIdx = (num + 1) % 3;
		decimillipedeSegment3.StarterMoveIdx = (num + 2) % 3;
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[3]
		{
			(decimillipedeSegment, "segment1"),
			(decimillipedeSegment2, "segment2"),
			(decimillipedeSegment3, "segment3")
		});
	}
}
