using System;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Daily;

public struct TimeServerResult : IPacketSerializable
{
	public DateTimeOffset serverTime;

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
