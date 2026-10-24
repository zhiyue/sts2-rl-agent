using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Helpers;

public static class ImageHelper
{
	/// <summary>
	/// Get the path to the image with the specified "inner" path.
	/// </summary>
	/// <param name="innerPath">The image's inner path. For example, for "images/cards/bash.png", pass "cards/bash.png".</param>
	/// <returns>The full path to the image.</returns>
	public static string GetImagePath(string innerPath)
	{
		if (innerPath.StartsWith('/'))
		{
			string text = innerPath;
			innerPath = text.Substring(1, text.Length - 1);
		}
		return "res://images/" + innerPath;
	}

	/// <summary>
	/// Get the path to the icon for the specified map point and room type.
	/// </summary>
	/// <param name="mapPointType">The type of map point that the room is in.</param>
	/// <param name="roomType">The type of room.</param>
	/// <param name="modelId">
	/// (Optional) The model ID corresponding to room. This is for bosses and ancients, which have unique icons.
	/// </param>
	/// <returns>The full path to the image</returns>
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

	/// <summary>
	/// Get the path to the icon outline for the specified map point and room type.
	/// </summary>
	/// <param name="mapPointType">The type of map point that the room is in.</param>
	/// <param name="roomType">The type of room.</param>
	/// <param name="modelId">
	/// (Optional) The model ID corresponding to room. This is for bosses and ancients, which have unique icons.
	/// </param>
	/// <returns>The full path to the image</returns>
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
