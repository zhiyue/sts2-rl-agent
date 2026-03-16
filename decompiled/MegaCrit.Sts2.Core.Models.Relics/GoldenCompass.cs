using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class GoldenCompass : RelicModel
{
	private int _goldenPathAct = -1;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	[SavedProperty]
	public int GoldenPathAct
	{
		get
		{
			return _goldenPathAct;
		}
		set
		{
			AssertMutable();
			_goldenPathAct = value;
		}
	}

	public override async Task AfterObtained()
	{
		GoldenPathAct = base.Owner.RunState.CurrentActIndex;
		await RunManager.Instance.GenerateMap();
	}

	public override ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
	{
		if (GoldenPathAct != actIndex)
		{
			return map;
		}
		return new GoldenPathActMap(runState);
	}

	public override IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypes(IReadOnlySet<RoomType> roomTypes)
	{
		if (GoldenPathAct != base.Owner.RunState.CurrentActIndex)
		{
			return roomTypes;
		}
		return new HashSet<RoomType> { RoomType.Event };
	}
}
