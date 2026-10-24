namespace MegaCrit.Sts2.Core.Map;

/// <summary>
/// Represents a type of point that can appear on the map. This is subtly different from RoomType.
/// Please see <see cref="T:MegaCrit.Sts2.Core.Rooms.RoomType" /> for more details.
/// </summary>
public enum MapPointType
{
	Unassigned,
	Unknown,
	Shop,
	Treasure,
	RestSite,
	Monster,
	Elite,
	Boss,
	Ancient
}
