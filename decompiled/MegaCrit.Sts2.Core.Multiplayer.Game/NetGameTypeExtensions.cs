namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public static class NetGameTypeExtensions
{
	public static bool IsMultiplayer(this NetGameType type)
	{
		if ((uint)(type - 2) <= 1u)
		{
			return true;
		}
		return false;
	}
}
