using System;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Daily;

public struct TimeServerResult : IPacketSerializable
{
	/// <summary>
	/// The time that was returned from the time server.
	/// </summary>
	public DateTimeOffset serverTime;

	/// <summary>
	/// The local time at which we requested the time from the server.
	/// </summary>
	public DateTimeOffset localReceivedTime;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteLong(serverTime.ToUnixTimeSeconds());
		writer.WriteLong(localReceivedTime.ToUnixTimeSeconds());
	}

	public void Deserialize(PacketReader reader)
	{
		serverTime = DateTimeOffset.FromUnixTimeSeconds(reader.ReadLong());
		localReceivedTime = DateTimeOffset.FromUnixTimeSeconds(reader.ReadLong());
	}
}
