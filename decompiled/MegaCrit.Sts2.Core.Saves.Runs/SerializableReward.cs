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

	/// <summary>
	/// The model ID that this reward is predetermined to be.
	/// This is used for rewards that are predetermined to be a specific model, such as
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Encounters.FakeMerchantEventEncounter" /> granting the specific relics that were originally being sold by
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Events.FakeMerchant" />.
	///
	/// Note: If you want to set a pre-determined CardModel reward, you should probably use
	/// <see cref="T:MegaCrit.Sts2.Core.Rewards.SpecialCardReward" /> and the <see cref="P:MegaCrit.Sts2.Core.Saves.Runs.SerializableReward.SpecialCard" /> property instead.
	///
	/// TODO: This only supports RelicModel right now. Add support for PotionModel later if necessary.
	/// </summary>
	[JsonPropertyName("predetermined_model_id")]
	public ModelId PredeterminedModelId { get; set; } = ModelId.none;

	/// <summary>
	/// The card for a <see cref="T:MegaCrit.Sts2.Core.Rewards.SpecialCardReward" />.
	/// This has to be a full serialized card instead of just an ID, because <see cref="T:MegaCrit.Sts2.Core.Models.Monsters.ThievingHopper" /> steals a card
	/// from you and then returns it via this reward. This card may have modifications (upgrade, enchantment, etc.), so
	/// we need to be able to fully reconstruct it.
	/// Only non-null when RewardType is SpecialCard.
	/// </summary>
	[JsonPropertyName("special_card")]
	public SerializableCard? SpecialCard { get; set; }

	/// <summary>
	/// These properties are only used for gold rewards.
	/// </summary>
	[JsonPropertyName("gold_amount")]
	public int GoldAmount { get; set; }

	/// <summary>
	/// Whether or not the gold was "stolen back".
	/// Usually false, but true in situations where an enemy/event "stole" some of your gold (such as
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Encounters.GremlinMercNormal" />), and this reward represents you taking it back.
	/// </summary>
	[JsonPropertyName("was_gold_stolen_back")]
	public bool WasGoldStolenBack { get; set; }

	/// <summary>
	/// These properties are only used for randomized card rewards.
	/// </summary>
	[JsonPropertyName("source")]
	public CardCreationSource Source { get; set; }

	[JsonPropertyName("rarity_odds")]
	public CardRarityOddsType RarityOdds { get; set; }

	/// <summary>
	/// The IDs of the card pools that this card reward can be rolled from.
	/// </summary>
	[JsonPropertyName("card_pools")]
	public List<ModelId> CardPoolIds { get; set; } = new List<ModelId>();

	/// <summary>
	/// The number of cards that should be available to choose from in this card reward.
	/// </summary>
	[JsonPropertyName("option_count")]
	public int OptionCount { get; set; }

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Models.EncounterModel.CustomRewardDescription" /> for details.
	/// </summary>
	[JsonPropertyName("custom_description_encounter_source_id")]
	public ModelId CustomDescriptionEncounterSourceId { get; set; } = ModelId.none;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt((int)RewardType);
		writer.WriteFullModelId(PredeterminedModelId);
		if (RewardType == RewardType.SpecialCard)
		{
			writer.Write(SpecialCard);
		}
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
		PredeterminedModelId = reader.ReadFullModelId();
		if (RewardType == RewardType.SpecialCard)
		{
			SpecialCard = reader.Read<SerializableCard>();
		}
		GoldAmount = reader.ReadInt();
		WasGoldStolenBack = reader.ReadBool();
		Source = reader.ReadEnum<CardCreationSource>();
		RarityOdds = reader.ReadEnum<CardRarityOddsType>();
		CardPoolIds = reader.ReadModelIdListAssumingType<CardPoolModel>();
		OptionCount = reader.ReadInt();
		CustomDescriptionEncounterSourceId = reader.ReadModelIdAssumingType<EncounterModel>();
	}
}
