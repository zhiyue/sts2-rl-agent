using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Helpers;

public static class ImageHelper
{
	public static string GetImagePath(string innerPath)
	{
		if (innerPath.StartsWith('/'))
		{
			string text = innerPath;
			innerPath = text.Substring(1, text.Length - 1);
		}
		return "res://images/" + innerPath;
	}

	public static string? GetRoomIconPath(MapPointType mapPointType, RoomType roomType, ModelId? modelId)
	{
		if (mapPointType == MapPointType.Unassigned || roomType == RoomType.Map)
		{
			return null;
		}
		string roomIconSuffix = GetRoomIconSuffix(mapPointType, roomType, modelId);
		if (roomIconSuffix == null)
		{
			return null;
		}
		return GetImagePath("ui/run_history/" + roomIconSuffix + ".png");
	}

	public static string? GetRoomIconOutlinePath(MapPointType mapPointType, RoomType roomType, ModelId? modelId)
	{
		if (mapPointType == MapPointType.Unassigned || roomType == RoomType.Map)
		{
			return null;
		}
		string roomIconSuffix = GetRoomIconSuffix(mapPointType, roomType, modelId);
		if (roomIconSuffix == null)
		{
			return null;
		}
		return GetImagePath("ui/run_history/" + roomIconSuffix + "_outline.png");
	}

	private static string? GetRoomIconSuffix(MapPointType mapPointType, RoomType roomType, ModelId? modelId)
	{
		if (modelId != null)
		{
			return modelId.Entry.ToLowerInvariant();
		}
		if (roomType == RoomType.Boss)
		{
			return null;
		}
		string text = StringHelper.Slugify(roomType.ToString()).ToLowerInvariant();
		if (mapPointType == MapPointType.Unknown && roomType != RoomType.Event)
		{
			return "unknown_" + text;
		}
		return text;
	}
}
