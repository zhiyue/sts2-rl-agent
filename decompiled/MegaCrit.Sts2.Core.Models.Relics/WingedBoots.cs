using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class WingedBoots : RelicModel
{
	private const string _roomsKey = "Rooms";

	private const int _roomCount = 3;

	private int _timesUsed;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool IsUsedUp => TimesUsed >= 3;

	public override bool ShowCounter => !IsUsedUp;

	public override int DisplayAmount => 3 - TimesUsed;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Rooms", 3m));

	[SavedProperty]
	public int TimesUsed
	{
		get
		{
			return _timesUsed;
		}
		set
		{
			AssertMutable();
			_timesUsed = value;
			base.DynamicVars["Rooms"].BaseValue = 3 - _timesUsed;
			InvokeDisplayAmountChanged();
			CheckIfUsedUp();
		}
	}

	public override bool IsAllowed(IRunState runState)
	{
		return runState.Players.Count == 1;
	}

	public override bool ShouldAllowFreeTravel()
	{
		return !IsUsedUp;
	}

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		if (IsUsedUp)
		{
			return Task.CompletedTask;
		}
		if (base.Owner.RunState.CurrentRoomCount > 1)
		{
			return Task.CompletedTask;
		}
		if (!(base.Owner.RunState is RunState runState))
		{
			return Task.CompletedTask;
		}
		if (runState.VisitedMapCoords.Count <= 1)
		{
			return Task.CompletedTask;
		}
		IReadOnlyList<MapCoord> visitedMapCoords = runState.VisitedMapCoords;
		MapCoord coord = visitedMapCoords[visitedMapCoords.Count - 2];
		MapPoint point = runState.Map.GetPoint(coord);
		if (point == null)
		{
			return Task.CompletedTask;
		}
		MapPoint currentMapPoint = base.Owner.RunState.CurrentMapPoint;
		if (currentMapPoint == null)
		{
			return Task.CompletedTask;
		}
		if (point.Children.Contains(currentMapPoint))
		{
			return Task.CompletedTask;
		}
		TimesUsed++;
		return Task.CompletedTask;
	}

	private void CheckIfUsedUp()
	{
		if (IsUsedUp)
		{
			base.Status = RelicStatus.Disabled;
		}
	}
}
