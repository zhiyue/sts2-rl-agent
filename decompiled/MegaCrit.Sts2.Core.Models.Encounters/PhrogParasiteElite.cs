using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class PhrogParasiteElite : EncounterModel
{
	private const string _wrigglerSlotPrefix = "wriggler";

	private const string _phrogSlot = "phrog";

	public override RoomType RoomType => RoomType.Elite;

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlyArray<string>(new string[5] { "phrog", "wriggler1", "wriggler2", "wriggler3", "wriggler4" });

	public override bool HasScene => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[2]
	{
		ModelDb.Monster<PhrogParasite>(),
		ModelDb.Monster<Wriggler>()
	});

	public override IEnumerable<string> ExtraAssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ModelDb.Card<Infection>().OverlayPath);

	public static string GetWrigglerSlotName(int index)
	{
		return $"{"wriggler"}{index + 1}";
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<PhrogParasite>().ToMutable(), "phrog"));
	}
}
