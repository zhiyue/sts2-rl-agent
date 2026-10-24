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

	[JsonPropertyName("ccccombo_badge_unlocked")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public bool CccomboBadgeUnlocked { get; set; }

	[JsonPropertyName("damage_dealt")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int DamageDealt { get; set; }

	[JsonPropertyName("debuffs_applied")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int DebuffsApplied { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(CardShopRemovalsUsed);
		writer.WriteInt(WongoPoints);
		writer.WriteBool(CccomboBadgeUnlocked);
		writer.WriteInt(DamageDealt);
		writer.WriteInt(DebuffsApplied);
	}

	public void Deserialize(PacketReader reader)
	{
		CardShopRemovalsUsed = reader.ReadInt();
		WongoPoints = reader.ReadInt();
		CccomboBadgeUnlocked = reader.ReadBool();
		DamageDealt = reader.ReadInt();
		DebuffsApplied = reader.ReadInt();
	}
}
