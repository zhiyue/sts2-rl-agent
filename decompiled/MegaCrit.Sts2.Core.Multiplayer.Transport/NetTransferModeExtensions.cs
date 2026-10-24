using System;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport;

public static class NetTransferModeExtensions
{
	/// <summary>
	/// Returns the channel that should be used to send a message with the given mode.
	/// ENet uses the channel ID to denote priority: https://www.linuxjournal.com/content/network-programming-enet
	/// Our reliable messages should be considered higher priority than our unreliable messages.
	/// </summary>
	public static int ToChannelId(this NetTransferMode mode)
	{
		return mode switch
		{
			NetTransferMode.Unreliable => 1, 
			NetTransferMode.Reliable => 0, 
			_ => throw new ArgumentOutOfRangeException("mode", mode, null), 
		};
	}
}
