namespace MegaCrit.Sts2.Core.Rooms;

/// <summary>
/// <para>
/// Represents a type of room that a player can be in. This is different from <see cref="T:MegaCrit.Sts2.Core.Map.MapPointType" /> in subtle but
/// important ways.
/// </para>
///
/// <para>
/// A MapPointType represents a type of point on the map. When a player clicks on one of these points, a RoomType is
/// "resolved". Most of the time, this resolution is straightforward:
///
/// <list type="bullet">
///     <item>MapPointType.Monster always resolves to RoomType.Monster.</item>
///     <item>MapPointType.Treasure always resolves to RoomType.Treasure.</item>
///     <item>etc.</item>
/// </list>
///
/// However, MapPointType.Unknown ("?" on the map) can resolve to RoomType.Event, RoomType.Monster, and others.
/// Furthermore, the current RoomType can change without the MapPointType changing. For example, imagine this sequence:
///
/// <list type="number">
///     <item>The player clicks on a ? map point (MapPointType.Unknown).</item>
///     <item>The room type resolves to RoomType.Event.</item>
///     <item>In the event, the player selects the "Start a Fight!" option.</item>
///     <item>The room type changes to RoomType.Monster.</item>
/// </list>
///
/// With this in mind, it's important to treat these two concepts as distinct.
/// </para>
/// </summary>
public enum RoomType
{
	Unassigned,
	Monster,
	Elite,
	Boss,
	Treasure,
	Shop,
	Event,
	RestSite,
	Map
}
