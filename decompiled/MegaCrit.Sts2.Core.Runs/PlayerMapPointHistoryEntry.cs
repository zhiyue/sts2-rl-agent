using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs;

public class PlayerMapPointHistoryEntry : IPacketSerializable
{
	[JsonPropertyName("player_id")]
	public ulong PlayerId { get; set; }

	[JsonPropertyName("gold_gained")]
	public int GoldGained { get; set; }

	[JsonPropertyName("gold_spent")]
	public int GoldSpent { get; set; }

	[JsonPropertyName("gold_lost")]
	public int GoldLost { get; set; }

	[JsonPropertyName("gold_stolen")]
	public int GoldStolen { get; set; }

	[JsonPropertyName("current_gold")]
	public int CurrentGold { get; set; }

	[JsonPropertyName("current_hp")]
	public int CurrentHp { get; set; }

	[JsonPropertyName("max_hp")]
	public int MaxHp { get; set; }

	[JsonPropertyName("damage_taken")]
	public int DamageTaken { get; set; }

	[JsonPropertyName("hp_healed")]
	public int HpHealed { get; set; }

	[JsonPropertyName("max_hp_lost")]
	public int MaxHpLost { get; set; }

	[JsonPropertyName("max_hp_gained")]
	public int MaxHpGained { get; set; }

	[JsonPropertyName("ancient_choice")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<AncientChoiceHistoryEntry> AncientChoices { get; set; } = new List<AncientChoiceHistoryEntry>();

	[JsonPropertyName("cards_gained")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<SerializableCard> CardsGained { get; set; } = new List<SerializableCard>();

	[JsonPropertyName("card_choices")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<CardChoiceHistoryEntry> CardChoices { get; set; } = new List<CardChoiceHistoryEntry>();

	[JsonPropertyName("relic_choices")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelChoiceHistoryEntry> RelicChoices { get; set; } = new List<ModelChoiceHistoryEntry>();

	[JsonPropertyName("potion_choices")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelChoiceHistoryEntry> PotionChoices { get; set; } = new List<ModelChoiceHistoryEntry>();

	[JsonPropertyName("potion_discarded")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> PotionDiscarded { get; set; } = new List<ModelId>();

	[JsonPropertyName("potion_used")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> PotionUsed { get; set; } = new List<ModelId>();

	[JsonPropertyName("cards_removed")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<SerializableCard> CardsRemoved { get; set; } = new List<SerializableCard>();

	[JsonPropertyName("relics_removed")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> RelicsRemoved { get; set; } = new List<ModelId>();

	[JsonPropertyName("cards_enchanted")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<CardEnchantmentHistoryEntry> CardsEnchanted { get; set; } = new List<CardEnchantmentHistoryEntry>();

	[JsonPropertyName("cards_transformed")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<CardTransformationHistoryEntry> CardsTransformed { get; set; } = new List<CardTransformationHistoryEntry>();

	[JsonPropertyName("upgraded_cards")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> UpgradedCards { get; set; } = new List<ModelId>();

	[JsonPropertyName("downgraded_cards")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> DowngradedCards { get; set; } = new List<ModelId>();

	[JsonPropertyName("event_choices")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<EventOptionHistoryEntry> EventChoices { get; set; } = new List<EventOptionHistoryEntry>();

	[JsonPropertyName("rest_site_choices")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<string> RestSiteChoices { get; set; } = new List<string>();

	[JsonPropertyName("bought_relics")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> BoughtRelics { get; set; } = new List<ModelId>();

	[JsonPropertyName("bought_potions")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> BoughtPotions { get; set; } = new List<ModelId>();

	[JsonPropertyName("bought_colorless")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> BoughtColorless { get; set; } = new List<ModelId>();

	[JsonPropertyName("completed_quests")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> CompletedQuests { get; set; } = new List<ModelId>();

	public LocString? GetAncientPickedChoiceLoc()
	{
		return AncientChoices.FirstOrDefault((AncientChoiceHistoryEntry o) => o.WasChosen)?.Title;
	}

	public List<LocString> GetAncientSkippedChoiceLoc()
	{
		return (from o in AncientChoices
			where !o.WasChosen
			select o.Title).ToList();
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteULong(PlayerId);
		writer.WriteInt(GoldGained);
		writer.WriteInt(GoldSpent);
		writer.WriteInt(GoldLost);
		writer.WriteInt(GoldStolen);
		writer.WriteInt(CurrentGold);
		writer.WriteInt(CurrentHp);
		writer.WriteInt(MaxHp);
		writer.WriteInt(DamageTaken);
		writer.WriteInt(HpHealed);
		writer.WriteInt(MaxHpGained);
		writer.WriteInt(MaxHpLost);
		writer.WriteList(EventChoices);
		writer.WriteList(AncientChoices);
		writer.WriteList(CardsGained);
		writer.WriteInt(CardChoices.Count);
		foreach (CardChoiceHistoryEntry cardChoice in CardChoices)
		{
			cardChoice.Serialize<CardModel>(writer);
		}
		writer.WriteInt(RelicChoices.Count);
		foreach (ModelChoiceHistoryEntry relicChoice in RelicChoices)
		{
			relicChoice.Serialize<RelicModel>(writer);
		}
		writer.WriteInt(PotionChoices.Count);
		foreach (ModelChoiceHistoryEntry potionChoice in PotionChoices)
		{
			potionChoice.Serialize<PotionModel>(writer);
		}
		writer.WriteModelEntriesInList(PotionDiscarded);
		writer.WriteModelEntriesInList(PotionUsed);
		writer.WriteList(CardsRemoved);
		writer.WriteModelEntriesInList(RelicsRemoved);
		writer.WriteList(CardsEnchanted);
		writer.WriteList(CardsTransformed);
		writer.WriteModelEntriesInList(UpgradedCards);
		writer.WriteModelEntriesInList(DowngradedCards);
		writer.WriteList(EventChoices);
		writer.WriteInt(RestSiteChoices.Count);
		foreach (string restSiteChoice in RestSiteChoices)
		{
			writer.WriteString(restSiteChoice);
		}
		writer.WriteModelEntriesInList(BoughtRelics);
		writer.WriteModelEntriesInList(BoughtPotions);
		writer.WriteModelEntriesInList(BoughtColorless);
		writer.WriteModelEntriesInList(CompletedQuests);
	}

	public void Deserialize(PacketReader reader)
	{
		PlayerId = reader.ReadULong();
		GoldGained = reader.ReadInt();
		GoldSpent = reader.ReadInt();
		GoldLost = reader.ReadInt();
		GoldStolen = reader.ReadInt();
		CurrentGold = reader.ReadInt();
		CurrentHp = reader.ReadInt();
		MaxHp = reader.ReadInt();
		DamageTaken = reader.ReadInt();
		HpHealed = reader.ReadInt();
		MaxHpGained = reader.ReadInt();
		MaxHpLost = reader.ReadInt();
		EventChoices = reader.ReadList<EventOptionHistoryEntry>();
		AncientChoices = reader.ReadList<AncientChoiceHistoryEntry>();
		CardsGained = reader.ReadList<SerializableCard>();
		int num = reader.ReadInt();
		for (int i = 0; i < num; i++)
		{
			CardChoiceHistoryEntry item = default(CardChoiceHistoryEntry);
			item.Deserialize<CardModel>(reader);
			CardChoices.Add(item);
		}
		int num2 = reader.ReadInt();
		for (int j = 0; j < num2; j++)
		{
			ModelChoiceHistoryEntry item2 = default(ModelChoiceHistoryEntry);
			item2.Deserialize<RelicModel>(reader);
			RelicChoices.Add(item2);
		}
		int num3 = reader.ReadInt();
		for (int k = 0; k < num3; k++)
		{
			ModelChoiceHistoryEntry item3 = default(ModelChoiceHistoryEntry);
			item3.Deserialize<PotionModel>(reader);
			PotionChoices.Add(item3);
		}
		PotionDiscarded = reader.ReadModelIdListAssumingType<PotionModel>();
		PotionUsed = reader.ReadModelIdListAssumingType<PotionModel>();
		CardsRemoved = reader.ReadList<SerializableCard>();
		RelicsRemoved = reader.ReadModelIdListAssumingType<RelicModel>();
		CardsEnchanted = reader.ReadList<CardEnchantmentHistoryEntry>();
		CardsTransformed = reader.ReadList<CardTransformationHistoryEntry>();
		UpgradedCards = reader.ReadModelIdListAssumingType<CardModel>();
		DowngradedCards = reader.ReadModelIdListAssumingType<CardModel>();
		EventChoices = reader.ReadList<EventOptionHistoryEntry>();
		int num4 = reader.ReadInt();
		for (int l = 0; l < num4; l++)
		{
			RestSiteChoices.Add(reader.ReadString());
		}
		BoughtRelics = reader.ReadModelIdListAssumingType<RelicModel>();
		BoughtPotions = reader.ReadModelIdListAssumingType<PotionModel>();
		BoughtColorless = reader.ReadModelIdListAssumingType<CardModel>();
		CompletedQuests = reader.ReadModelIdListAssumingType<CardModel>();
	}

	public PlayerMapPointHistoryEntry Anonymized()
	{
		return new PlayerMapPointHistoryEntry
		{
			PlayerId = IdAnonymizer.Anonymize(PlayerId),
			GoldGained = GoldGained,
			GoldSpent = GoldSpent,
			GoldLost = GoldLost,
			GoldStolen = GoldStolen,
			CurrentGold = CurrentGold,
			CurrentHp = CurrentHp,
			MaxHp = MaxHp,
			DamageTaken = DamageTaken,
			HpHealed = HpHealed,
			MaxHpLost = MaxHpLost,
			MaxHpGained = MaxHpGained,
			AncientChoices = AncientChoices,
			CardsGained = CardsGained,
			CardChoices = CardChoices,
			RelicChoices = RelicChoices,
			PotionChoices = PotionChoices,
			PotionDiscarded = PotionDiscarded,
			PotionUsed = PotionUsed,
			CardsRemoved = CardsRemoved,
			RelicsRemoved = RelicsRemoved,
			CardsEnchanted = CardsEnchanted,
			CardsTransformed = CardsTransformed,
			UpgradedCards = UpgradedCards,
			DowngradedCards = DowngradedCards,
			EventChoices = EventChoices,
			RestSiteChoices = RestSiteChoices,
			BoughtRelics = BoughtRelics,
			BoughtPotions = BoughtPotions,
			BoughtColorless = BoughtColorless,
			CompletedQuests = CompletedQuests
		};
	}
}
