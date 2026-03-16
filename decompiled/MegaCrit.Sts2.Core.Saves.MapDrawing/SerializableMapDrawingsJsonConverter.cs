using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.MapDrawing;

public class SerializableMapDrawingsJsonConverter : JsonConverter<SerializableMapDrawings>
{
	private static readonly PacketWriter _packetWriter = new PacketWriter();

	public override SerializableMapDrawings Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(reader.GetString())))
		{
			using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
			gZipStream.CopyTo(memoryStream);
		}
		PacketReader packetReader = new PacketReader();
		packetReader.Reset(memoryStream.ToArray());
		return packetReader.Read<SerializableMapDrawings>();
	}

	public override void Write(Utf8JsonWriter writer, SerializableMapDrawings mapDrawings, JsonSerializerOptions options)
	{
		string value;
		lock (_packetWriter)
		{
			_packetWriter.Reset();
			_packetWriter.Write(mapDrawings);
			using MemoryStream memoryStream = new MemoryStream();
			using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionLevel.Fastest, leaveOpen: true))
			{
				gZipStream.Write(_packetWriter.Buffer, 0, _packetWriter.BytePosition);
			}
			value = Convert.ToBase64String(memoryStream.ToArray());
		}
		writer.WriteStringValue(value);
	}
}
