using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class TwoTailedRatsNormal : EncounterModel
{
	public override bool HasScene => true;

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlyArray<string>(new string[5] { "first", "second", "third", "fourth", "fifth" });

	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<TwoTailedRat>());

	public override float GetCameraScaling()
	{
		return 0.85f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 25f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		TwoTailedRat twoTailedRat = (TwoTailedRat)ModelDb.Monster<TwoTailedRat>().ToMutable();
		TwoTailedRat twoTailedRat2 = (TwoTailedRat)ModelDb.Monster<TwoTailedRat>().ToMutable();
		TwoTailedRat twoTailedRat3 = (TwoTailedRat)ModelDb.Monster<TwoTailedRat>().ToMutable();
		int num = (twoTailedRat.StarterMoveIndex = base.Rng.NextInt(3));
		twoTailedRat2.StarterMoveIndex = (num + 1) % 3;
		twoTailedRat3.StarterMoveIndex = (num + 2) % 3;
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[3]
		{
			(twoTailedRat, Slots[2]),
			(twoTailedRat2, Slots[3]),
			(twoTailedRat3, Slots[4])
		});
	}
}
