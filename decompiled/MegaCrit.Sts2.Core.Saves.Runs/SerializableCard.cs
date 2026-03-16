using System;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableCard : IPacketSerializable
{
	[JsonPropertyName("id")]
	public ModelId? Id { get; set; }

	[JsonPropertyName("current_upgrade_level")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int CurrentUpgradeLevel { get; set; }

	[JsonPropertyName("enchantment")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public SerializableEnchantment? Enchantment { get; set; }

	[JsonPropertyName("props")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public SavedProperties? Props { get; set; }

	[JsonPropertyName("floor_added_to_deck")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? FloorAddedToDeck { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteModelEntry(Id);
		writer.WriteInt(CurrentUpgradeLevel, 8);
		writer.WriteBool(Enchantment != null);
		if (Enchantment != null)
		{
			writer.Write(Enchantment);
		}
		writer.WriteBool(Props != null);
		if (Props != null)
		{
			writer.Write(Props);
		}
		writer.WriteBool(FloorAddedToDeck.HasValue);
		if (FloorAddedToDeck.HasValue)
		{
			writer.WriteInt(FloorAddedToDeck.Value, 8);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		Id = reader.ReadModelIdAssumingType<CardModel>();
		CurrentUpgradeLevel = reader.ReadInt(8);
		if (reader.ReadBool())
		{
			Enchantment = reader.Read<SerializableEnchantment>();
		}
		if (reader.ReadBool())
		{
			Props = reader.Read<SavedProperties>();
		}
		if (reader.ReadBool())
		{
			FloorAddedToDeck = reader.ReadInt(8);
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
		SerializableCard serializableCard = (SerializableCard)obj;
		if (Id.Equals(serializableCard.Id) && CurrentUpgradeLevel == serializableCard.CurrentUpgradeLevel)
		{
			return object.Equals(Enchantment, serializableCard.Enchantment);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Id, CurrentUpgradeLevel, Enchantment);
	}

	public override string ToString()
	{
		return $"SerializableCard {Id}. Upgrades: {CurrentUpgradeLevel} Enchantment: {Enchantment} Props: {Props} Floor: {FloorAddedToDeck}";
	}
}
