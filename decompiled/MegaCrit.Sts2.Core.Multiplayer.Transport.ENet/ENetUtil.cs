using System;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport.ENet;

public static class ENetUtil
{
	public static NetTransferMode ModeFromFlags(int flags)
	{
		if ((long)((ulong)flags & 1uL) > 0L)
		{
			return NetTransferMode.Reliable;
		}
		if ((long)((ulong)flags & 8uL) > 0L)
		{
			return NetTransferMode.Unreliable;
		}
		throw new ArgumentOutOfRangeException($"Flags {flags} cannot be mapped to NetTransferMode!");
	}

	public static int FlagsFromMode(NetTransferMode mode)
	{
		return mode switch
		{
			NetTransferMode.Unreliable => 8, 
			NetTransferMode.Reliable => 1, 
			_ => throw new ArgumentOutOfRangeException("mode", mode, null), 
		};
	}
}
