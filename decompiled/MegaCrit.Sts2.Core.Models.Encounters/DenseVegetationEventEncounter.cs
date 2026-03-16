using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class DenseVegetationEventEncounter : EncounterModel
{
	private const string _wrigglerSlotPrefix = "wriggler";

	public override RoomType RoomType => RoomType.Monster;

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlyArray<string>(new string[4] { "wriggler1", "wriggler2", "wriggler3", "wriggler4" });

	public override bool HasScene => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<Wriggler>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		List<(MonsterModel, string)> list = new List<(MonsterModel, string)>();
		foreach (string slot in Slots)
		{
			Wriggler wriggler = (Wriggler)ModelDb.Monster<Wriggler>().ToMutable();
			wriggler.StartStunned = false;
			list.Add((wriggler, slot));
		}
		return list;
	}
}
