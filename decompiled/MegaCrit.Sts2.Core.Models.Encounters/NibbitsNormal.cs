using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class NibbitsNormal : EncounterModel
{
	private const string _backSlot = "back";

	private const string _frontSlot = "front";

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "front", "back" });

	public override RoomType RoomType => RoomType.Monster;

	public override bool HasScene => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<Nibbit>());

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		Nibbit nibbit = (Nibbit)ModelDb.Monster<Nibbit>().ToMutable();
		nibbit.IsFront = true;
		return new global::_003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(nibbit, "front"),
			(ModelDb.Monster<Nibbit>().ToMutable(), "back")
		});
	}
}
