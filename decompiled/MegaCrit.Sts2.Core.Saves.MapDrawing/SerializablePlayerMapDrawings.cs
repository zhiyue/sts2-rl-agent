using System.Collections.Generic;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.MapDrawing;

public class SerializablePlayerMapDrawings : IPacketSerializable
{
	public ulong playerId;

	public List<SerializableMapDrawingLine> lines = new List<SerializableMapDrawingLine>();

	public void Serialize(PacketWriter writer)
	{
		writer.WriteULong(playerId);
		writer.WriteList(lines);
	}

	public void Deserialize(PacketReader reader)
	{
		playerId = reader.ReadULong();
		lines = reader.ReadList<SerializableMapDrawingLine>();
	}

	public SerializablePlayerMapDrawings Anonymized()
	{
		return new SerializablePlayerMapDrawings
		{
			playerId = IdAnonymizer.Anonymize(playerId),
			lines = lines
		};
	}
}
