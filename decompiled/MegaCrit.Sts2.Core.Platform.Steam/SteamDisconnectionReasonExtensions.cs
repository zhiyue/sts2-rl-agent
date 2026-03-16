using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.Platform.Steam;

public static class SteamDisconnectionReasonExtensions
{
	public static SteamDisconnectionReason ToSteam(this NetError reason)
	{
		return (SteamDisconnectionReason)(1000 + reason);
	}

	public static NetError ToApp(this SteamDisconnectionReason steamReason)
	{
		if (steamReason >= SteamDisconnectionReason.AppGeneric && steamReason <= (SteamDisconnectionReason)1999)
		{
			return (NetError)(steamReason - 1000);
		}
		if (steamReason >= SteamDisconnectionReason.LocalMin)
		{
			if (steamReason > SteamDisconnectionReason.LocalMax)
			{
				if (steamReason == SteamDisconnectionReason.RemoteTimeout || steamReason == SteamDisconnectionReason.MiscTimeout)
				{
					return NetError.Timeout;
				}
				if ((uint)(steamReason - 5004) > 1u)
				{
					goto IL_004f;
				}
			}
			return NetError.NoInternet;
		}
		if (steamReason == SteamDisconnectionReason.None)
		{
			return NetError.None;
		}
		goto IL_004f;
		IL_004f:
		return NetError.UnknownNetworkError;
	}
}
