using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class PhantasmalGardenersElite : EncounterModel
{
	public const string firstSlot = "first";

	public const string secondSlot = "second";

	public const string thirdSlot = "third";

	public const string fourthSlot = "fourth";

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlyArray<string>(new string[4] { "first", "second", "third", "fourth" });

	public override RoomType RoomType => RoomType.Elite;

	public override bool HasScene => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<PhantasmalGardener>());

	public override float GetCameraScaling()
	{
		return 0.85f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 40f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[4]
		{
			(ModelDb.Monster<PhantasmalGardener>().ToMutable(), "first"),
			(ModelDb.Monster<PhantasmalGardener>().ToMutable(), "second"),
			(ModelDb.Monster<PhantasmalGardener>().ToMutable(), "third"),
			(ModelDb.Monster<PhantasmalGardener>().ToMutable(), "fourth")
		});
	}
}
