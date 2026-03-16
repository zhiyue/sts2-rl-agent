using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class FakeMerchantEventEncounter : EncounterModel
{
	private const string _merchantSlot = "merchant";

	public override RoomType RoomType => RoomType.Monster;

	public override bool HasScene => true;

	protected override bool HasCustomBackground => true;

	public override IReadOnlyList<string> Slots => new global::_003C_003Ez__ReadOnlySingleElementList<string>("merchant");

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlySingleElementList<MonsterModel>(ModelDb.Monster<FakeMerchantMonster>());

	public override int MinGoldReward => 300;

	public override int MaxGoldReward => 300;

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<FakeMerchantMonster>().ToMutable(), "merchant"));
	}
}
