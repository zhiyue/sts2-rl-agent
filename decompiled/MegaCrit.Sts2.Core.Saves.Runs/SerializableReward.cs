using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableReward : IPacketSerializable
{
	[JsonPropertyName("reward_type")]
	public RewardType RewardType { get; set; }

	[JsonPropertyName("special_card")]
	public SerializableCard SpecialCard { get; set; }

	[JsonPropertyName("gold_amount")]
	public int GoldAmount { get; set; }

	[JsonPropertyName("was_gold_stolen_back")]
	public bool WasGoldStolenBack { get; set; }

	[JsonPropertyName("source")]
	public CardCreationSource Source { get; set; }

	[JsonPropertyName("rarity_odds")]
	public CardRarityOddsType RarityOdds { get; set; }

	[JsonPropertyName("card_pools")]
	public List<ModelId> CardPoolIds { get; set; } = new List<ModelId>();

	[JsonPropertyName("option_count")]
	public int OptionCount { get; set; }

	[JsonPropertyName("custom_description_encounter_source_id")]
	public ModelId CustomDescriptionEncounterSourceId { get; set; } = ModelId.none;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt((int)RewardType);
		writer.Write(SpecialCard);
		writer.WriteInt(GoldAmount);
		writer.WriteBool(WasGoldStolenBack);
		writer.WriteEnum(Source);
		writer.WriteEnum(RarityOdds);
		writer.WriteModelEntriesInList(CardPoolIds);
		writer.WriteInt(OptionCount);
		writer.WriteModelEntry(CustomDescriptionEncounterSourceId);
	}

	public void Deserialize(PacketReader reader)
	{
		RewardType = (RewardType)reader.ReadInt();
		SpecialCard = reader.Read<SerializableCard>();
		GoldAmount = reader.ReadInt();
		WasGoldStolenBack = reader.ReadBool();
		Source = reader.ReadEnum<CardCreationSource>();
		RarityOdds = reader.ReadEnum<CardRarityOddsType>();
		CardPoolIds = reader.ReadModelIdListAssumingType<CardPoolModel>();
		OptionCount = reader.ReadInt();
		CustomDescriptionEncounterSourceId = reader.ReadModelIdAssumingType<EncounterModel>();
	}
}
