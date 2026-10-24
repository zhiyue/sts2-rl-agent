using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

/// <summary>
/// Broadcast to all peers when a player in the lobby changes their selected character.
/// </summary>
public struct LobbyPlayerChangedCharacterMessage : INetMessage, IPacketSerializable
{
	public CharacterModel character;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteModel(character);
	}

	public void Deserialize(PacketReader reader)
	{
		character = reader.ReadModel<CharacterModel>();
	}
}
