using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Daily;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

public struct ClientLobbyJoinResponseMessage : INetMessage, IPacketSerializable
{
	public List<LobbyPlayer>? playersInLobby;

	public TimeServerResult? dailyTime;

	public int ascension;

	public string? seed;

	public List<SerializableModifier> modifiers;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Info;

	public void Serialize(PacketWriter writer)
	{
		if (playersInLobby == null)
		{
			throw new InvalidOperationException("Tried to serialize ClientSlotGrantedMessage with null list!");
		}
		writer.WriteList(playersInLobby, 3);
		writer.WriteBool(dailyTime.HasValue);
		if (dailyTime.HasValue)
		{
			writer.Write(dailyTime.Value);
		}
		writer.WriteBool(seed != null);
		if (seed != null)
		{
			writer.WriteString(seed);
		}
		writer.WriteInt(ascension, 5);
		writer.WriteList(modifiers);
	}

	public void Deserialize(PacketReader reader)
	{
		playersInLobby = reader.ReadList<LobbyPlayer>(3);
		if (reader.ReadBool())
		{
			dailyTime = reader.Read<TimeServerResult>();
		}
		if (reader.ReadBool())
		{
			seed = reader.ReadString();
		}
		ascension = reader.ReadInt(5);
		modifiers = reader.ReadList<SerializableModifier>();
	}

	public override string ToString()
	{
		return $"{"ClientLobbyJoinResponseMessage"} Players: {playersInLobby?.Count} Ascension: {ascension}";
	}
}
