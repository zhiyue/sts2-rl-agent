using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models.Badges;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableBadge : IPacketSerializable
{
	[JsonPropertyName("id")]
	public required string Id { get; set; }

	[JsonPropertyName("rarity")]
	public required BadgeRarity Rarity { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteString(Id);
		writer.WriteEnum(Rarity);
	}

	public void Deserialize(PacketReader reader)
	{
		Id = reader.ReadString();
		Rarity = reader.ReadEnum<BadgeRarity>();
	}
}
