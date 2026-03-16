namespace MegaCrit.Sts2.Core.Rooms;

public static class RoomTypeExtensions
{
	public static bool IsCombatRoom(this RoomType room)
	{
		if ((uint)(room - 1) <= 2u)
		{
			return true;
		}
		return false;
	}
}
