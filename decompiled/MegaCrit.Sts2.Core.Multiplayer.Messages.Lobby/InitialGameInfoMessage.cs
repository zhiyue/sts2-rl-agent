using System.Collections.Generic;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

public struct InitialGameInfoMessage : INetMessage, IPacketSerializable
{
	public string version;

	public uint idDatabaseHash;

	public List<string>? mods;

	public GameMode gameMode;

	public RunSessionState sessionState;

	public ConnectionFailureReason? connectionFailureReason;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Info;

	public static InitialGameInfoMessage Basic()
	{
		return new InitialGameInfoMessage
		{
			version = (ReleaseInfoManager.Instance.ReleaseInfo?.Version ?? GitHelper.ShortCommitId ?? "UNKNOWN"),
			idDatabaseHash = ModelIdSerializationCache.Hash,
			mods = ModManager.GetModNameList()
		};
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteString(version);
		writer.WriteUInt(idDatabaseHash);
		writer.WriteEnum(gameMode);
		writer.WriteEnum(sessionState);
		writer.WriteBool(connectionFailureReason.HasValue);
		if (connectionFailureReason.HasValue)
		{
			writer.WriteEnum(connectionFailureReason.Value);
		}
		writer.WriteBool(mods != null);
		if (mods == null)
		{
			return;
		}
		writer.WriteInt(mods.Count);
		foreach (string mod in mods)
		{
			writer.WriteString(mod);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		version = reader.ReadString();
		idDatabaseHash = reader.ReadUInt();
		gameMode = reader.ReadEnum<GameMode>();
		sessionState = reader.ReadEnum<RunSessionState>();
		if (reader.ReadBool())
		{
			connectionFailureReason = reader.ReadEnum<ConnectionFailureReason>();
		}
		if (reader.ReadBool())
		{
			int num = reader.ReadInt();
			mods = new List<string>();
			for (int i = 0; i < num; i++)
			{
				mods.Add(reader.ReadString());
			}
		}
	}
}
