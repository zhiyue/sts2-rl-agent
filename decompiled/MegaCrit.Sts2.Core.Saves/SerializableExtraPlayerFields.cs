using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves;

public class SerializableExtraPlayerFields : IPacketSerializable
{
	[JsonPropertyName("card_shop_removals_used")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int CardShopRemovalsUsed { get; set; }

	[JsonPropertyName("wongo_points")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int WongoPoints { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(CardShopRemovalsUsed);
		writer.WriteInt(WongoPoints);
	}

	public void Deserialize(PacketReader reader)
	{
		CardShopRemovalsUsed = reader.ReadInt();
		WongoPoints = reader.ReadInt();
	}
}
