using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs.History;

public struct CardEnchantmentHistoryEntry : IPacketSerializable
{
	[JsonPropertyName("card")]
	public SerializableCard Card { get; set; }

	[JsonPropertyName("enchantment")]
	public ModelId Enchantment { get; set; }

	public CardEnchantmentHistoryEntry(CardModel card, ModelId enchantment)
	{
		Card = card.ToSerializable();
		Enchantment = enchantment;
	}

	public void Serialize(PacketWriter writer)
	{
		writer.Write(Card);
		writer.WriteModelEntry(Enchantment);
	}

	public void Deserialize(PacketReader reader)
	{
		Card = reader.Read<SerializableCard>();
		Enchantment = reader.ReadModelIdAssumingType<EnchantmentModel>();
	}
}
