using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

public struct LobbyPlayer : IPacketSerializable
{
	public ulong id;

	public int slotId;

	public CharacterModel character;

	public SerializableUnlockState unlockState;

	public int maxMultiplayerAscensionUnlocked;

	public bool isReady;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteULong(id);
		writer.WriteInt(slotId, 2);
		writer.WriteModel(character);
		writer.Write(unlockState);
		writer.WriteInt(maxMultiplayerAscensionUnlocked);
		writer.WriteBool(isReady);
	}

	public void Deserialize(PacketReader reader)
	{
		id = reader.ReadULong();
		slotId = reader.ReadInt(2);
		character = reader.ReadModel<CharacterModel>();
		unlockState = reader.Read<SerializableUnlockState>();
		maxMultiplayerAscensionUnlocked = reader.ReadInt();
		isReady = reader.ReadBool();
	}

	public override string ToString()
	{
		return $"Player {id}, {character.Id.Entry}";
	}
}
