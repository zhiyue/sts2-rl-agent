using System;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;

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
			NetError netError = (NetError)(steamReason - 1000);
			if (!Enum.IsDefined(netError))
			{
				Log.Error($"Received unknown application error from Steam: {steamReason}");
				return NetError.UnknownNetworkError;
			}
			return netError;
		}
		if (steamReason >= SteamDisconnectionReason.LocalMin)
		{
			if (steamReason > SteamDisconnectionReason.LocalMax)
			{
				switch (steamReason)
				{
				case SteamDisconnectionReason.RelayConnectivity:
				case SteamDisconnectionReason.SteamConnectivity:
					break;
				case SteamDisconnectionReason.RemoteTimeout:
				case SteamDisconnectionReason.MiscTimeout:
					return NetError.Timeout;
				case SteamDisconnectionReason.RendezvousTimeout:
				case SteamDisconnectionReason.RelayReceivedUnexpectedNoConnectionPacket:
					return NetError.TryAgainLater;
				case SteamDisconnectionReason.BadCrypt:
				case SteamDisconnectionReason.BadCert:
					return NetError.SecureConnectionFailed;
				default:
					goto IL_00b1;
				}
			}
			return NetError.NoInternet;
		}
		if (steamReason == SteamDisconnectionReason.None)
		{
			return NetError.None;
		}
		goto IL_00b1;
		IL_00b1:
		return NetError.UnknownNetworkError;
	}
}
