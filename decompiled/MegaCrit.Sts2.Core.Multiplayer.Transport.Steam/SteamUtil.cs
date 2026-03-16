using System;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Logging;
using Steamworks;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport.Steam;

public static class SteamUtil
{
	public const uint handshakeMagicBytes = 2204332656u;

	private static readonly nint[] _messageBuffer = new nint[64];

	public static NetTransferMode ModeFromFlags(int flags)
	{
		if ((flags & 8) > 0)
		{
			return NetTransferMode.Reliable;
		}
		if (flags == 0)
		{
			return NetTransferMode.Unreliable;
		}
		throw new ArgumentOutOfRangeException();
	}

	public static int FlagsFromMode(NetTransferMode mode)
	{
		return mode switch
		{
			NetTransferMode.Unreliable => 0, 
			NetTransferMode.Reliable => 8, 
			_ => throw new ArgumentOutOfRangeException("mode", mode, null), 
		};
	}

	public static SteamNetworkingIdentity ToNetId(this CSteamID id)
	{
		SteamNetworkingIdentity result = default(SteamNetworkingIdentity);
		result.SetSteamID(id);
		return result;
	}

	public static SteamNetworkingIdentity ToNetId64(this ulong id)
	{
		SteamNetworkingIdentity result = default(SteamNetworkingIdentity);
		result.SetSteamID64(id);
		return result;
	}

	public static void ProcessMessages(HSteamNetConnection conn, INetHandler handler, Logger logger)
	{
		int num;
		do
		{
			num = SteamNetworkingSockets.ReceiveMessagesOnConnection(conn, _messageBuffer, _messageBuffer.Length);
			if (num > 0)
			{
				logger.VeryDebug($"Processing {num} packets");
			}
			for (int i = 0; i < num; i++)
			{
				nint num2 = _messageBuffer[i];
				SteamNetworkingMessage_t steamNetworkingMessage_t = Marshal.PtrToStructure<SteamNetworkingMessage_t>(num2);
				byte[] array = new byte[steamNetworkingMessage_t.m_cbSize];
				Marshal.Copy(steamNetworkingMessage_t.m_pData, array, 0, steamNetworkingMessage_t.m_cbSize);
				NetTransferMode netTransferMode = ModeFromFlags(steamNetworkingMessage_t.m_nFlags);
				logger.VeryDebug($"Received packet of size {array.Length} from sender {steamNetworkingMessage_t.m_identityPeer.GetSteamID64()} ({netTransferMode}, {steamNetworkingMessage_t.m_nChannel})");
				handler.OnPacketReceived(steamNetworkingMessage_t.m_identityPeer.GetSteamID().m_SteamID, array, netTransferMode, steamNetworkingMessage_t.m_nChannel);
				SteamNetworkingMessage_t.Release(num2);
			}
		}
		while (num == _messageBuffer.Length);
	}
}
