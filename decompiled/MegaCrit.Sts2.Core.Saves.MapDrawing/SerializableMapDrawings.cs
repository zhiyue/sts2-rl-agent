using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.MapDrawing;

public class SerializableMapDrawings : IPacketSerializable
{
	public List<SerializablePlayerMapDrawings> drawings = new List<SerializablePlayerMapDrawings>();

	public void Serialize(PacketWriter writer)
	{
		writer.WriteList(drawings);
	}

	public void Deserialize(PacketReader reader)
	{
		drawings = reader.ReadList<SerializablePlayerMapDrawings>();
	}

	public SerializableMapDrawings Anonymized()
	{
		return new SerializableMapDrawings
		{
			drawings = drawings.Select((SerializablePlayerMapDrawings d) => d.Anonymized()).ToList()
		};
	}
}
