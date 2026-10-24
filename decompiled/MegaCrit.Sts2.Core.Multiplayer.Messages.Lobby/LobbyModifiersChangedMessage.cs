using System.Collections.Generic;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

/// <summary>
/// Sent by the host when the modifiers in the custom run menu changes.
/// </summary>
public struct LobbyModifiersChangedMessage : INetMessage, IPacketSerializable
{
	public List<SerializableModifier> modifiers;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteList(modifiers);
	}

	public void Deserialize(PacketReader reader)
	{
		modifiers = reader.ReadList<SerializableModifier>();
	}
}
