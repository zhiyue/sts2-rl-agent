using System;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport;

public static class NetTransferModeExtensions
{
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
