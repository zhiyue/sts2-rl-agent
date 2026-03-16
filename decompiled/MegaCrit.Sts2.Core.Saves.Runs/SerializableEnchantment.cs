using System;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableEnchantment : IPacketSerializable
{
	[JsonPropertyName("id")]
	public ModelId? Id { get; set; }

	[JsonPropertyName("amount")]
	public int Amount { get; set; }

	[JsonPropertyName("props")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public SavedProperties? Props { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteModelEntry(Id);
		writer.WriteInt(Amount, 8);
		writer.WriteBool(Props != null);
		if (Props != null)
		{
			writer.Write(Props);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		Id = reader.ReadModelIdAssumingType<EnchantmentModel>();
		Amount = reader.ReadInt(8);
		if (reader.ReadBool())
		{
			Props = reader.Read<SavedProperties>();
		}
	}

	public override bool Equals(object? obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		SerializableEnchantment serializableEnchantment = (SerializableEnchantment)obj;
		if (Id.Equals(serializableEnchantment.Id))
		{
			return Amount.Equals(serializableEnchantment.Amount);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Id, Amount);
	}

	public override string ToString()
	{
		return $"SerializableEnchantment {Id} with amount {Amount} Props: {Props}";
	}
}
