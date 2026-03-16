using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class LanternKey : CardModel
{
	private const int _gloryActIndex = 2;

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Unplayable);

	public LanternKey()
		: base(-1, CardType.Quest, CardRarity.Quest, TargetType.Self)
	{
	}

	public override IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypes(IReadOnlySet<RoomType> roomTypes)
	{
		if (2 != base.Owner.RunState.CurrentActIndex)
		{
			return roomTypes;
		}
		return new HashSet<RoomType> { RoomType.Event };
	}

	public override EventModel ModifyNextEvent(EventModel currentEvent)
	{
		if (2 != base.Owner.RunState.CurrentActIndex)
		{
			return currentEvent;
		}
		return ModelDb.Event<WarHistorianRepy>();
	}
}
