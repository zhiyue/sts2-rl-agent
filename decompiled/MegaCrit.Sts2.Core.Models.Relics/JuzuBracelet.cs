using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class JuzuBracelet : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	public override bool IsAllowed(IRunState runState)
	{
		return RelicModel.IsBeforeAct3TreasureChest(runState);
	}

	public override IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypes(IReadOnlySet<RoomType> roomTypes)
	{
		HashSet<RoomType> hashSet = new HashSet<RoomType>();
		foreach (RoomType roomType in roomTypes)
		{
			hashSet.Add(roomType);
		}
		HashSet<RoomType> hashSet2 = hashSet;
		hashSet2.Remove(RoomType.Monster);
		return hashSet2;
	}
}
