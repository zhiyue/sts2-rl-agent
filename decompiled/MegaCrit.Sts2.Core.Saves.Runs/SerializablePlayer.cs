using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializablePlayer : IPacketSerializable
{
	[JsonPropertyName("character_id")]
	public ModelId? CharacterId { get; set; }

	[JsonPropertyName("current_hp")]
	public int CurrentHp { get; set; }

	[JsonPropertyName("max_hp")]
	public int MaxHp { get; set; }

	[JsonPropertyName("max_energy")]
	public int MaxEnergy { get; set; }

	[JsonPropertyName("max_potion_slot_count")]
	public int MaxPotionSlotCount { get; set; } = 3;

	[JsonPropertyName("gold")]
	public int Gold { get; set; }

	[JsonPropertyName("base_orb_slot_count")]
	public int BaseOrbSlotCount { get; set; }

	[JsonPropertyName("net_id")]
	public ulong NetId { get; set; }

	[JsonPropertyName("deck")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<SerializableCard> Deck { get; set; } = new List<SerializableCard>();

	[JsonPropertyName("relics")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<SerializableRelic> Relics { get; set; } = new List<SerializableRelic>();

	[JsonPropertyName("potions")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<SerializablePotion> Potions { get; set; } = new List<SerializablePotion>();

	[JsonPropertyName("rng")]
	public SerializablePlayerRngSet Rng { get; set; }

	[JsonPropertyName("odds")]
	public SerializablePlayerOddsSet Odds { get; set; }

	[JsonPropertyName("relic_grab_bag")]
	public SerializableRelicGrabBag RelicGrabBag { get; set; }

	[JsonPropertyName("extra_fields")]
	public SerializableExtraPlayerFields ExtraFields { get; set; }

	[JsonPropertyName("unlock_state")]
	public SerializableUnlockState UnlockState { get; set; }

	[JsonPropertyName("discovered_cards")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> DiscoveredCards { get; set; } = new List<ModelId>();

	[JsonPropertyName("discovered_enemies")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> DiscoveredEnemies { get; set; } = new List<ModelId>();

	[JsonPropertyName("discovered_epochs")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	[JsonConverter(typeof(EpochIdListConverter))]
	public List<string> DiscoveredEpochs { get; set; } = new List<string>();

	[JsonPropertyName("discovered_potions")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> DiscoveredPotions { get; set; } = new List<ModelId>();

	[JsonPropertyName("discovered_relics")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> DiscoveredRelics { get; set; } = new List<ModelId>();

	public void Serialize(PacketWriter writer)
	{
		writer.WriteULong(NetId);
		writer.WriteModelEntry(CharacterId);
		writer.WriteInt(CurrentHp);
		writer.WriteInt(MaxHp);
		writer.WriteInt(MaxEnergy, 16);
		writer.WriteInt(MaxPotionSlotCount, 8);
		writer.WriteInt(Gold);
		writer.WriteInt(BaseOrbSlotCount, 16);
		writer.WriteList(Deck);
		writer.WriteList(Relics);
		writer.WriteList(Potions);
		writer.Write(Rng);
		writer.Write(Odds);
		writer.Write(RelicGrabBag);
		writer.Write(ExtraFields);
		writer.Write(UnlockState);
		writer.WriteFullModelIdList(DiscoveredCards);
		writer.WriteFullModelIdList(DiscoveredEnemies);
		writer.WriteInt(DiscoveredEpochs.Count);
		foreach (string discoveredEpoch in DiscoveredEpochs)
		{
			writer.WriteEpochId(discoveredEpoch);
		}
		writer.WriteFullModelIdList(DiscoveredPotions);
		writer.WriteFullModelIdList(DiscoveredRelics);
	}

	public void Deserialize(PacketReader reader)
	{
		NetId = reader.ReadULong();
		CharacterId = reader.ReadModelIdAssumingType<CharacterModel>();
		CurrentHp = reader.ReadInt();
		MaxHp = reader.ReadInt();
		MaxEnergy = reader.ReadInt(16);
		MaxPotionSlotCount = reader.ReadInt(8);
		Gold = reader.ReadInt();
		BaseOrbSlotCount = reader.ReadInt(16);
		Deck = reader.ReadList<SerializableCard>();
		Relics = reader.ReadList<SerializableRelic>();
		Potions = reader.ReadList<SerializablePotion>();
		Rng = reader.Read<SerializablePlayerRngSet>();
		Odds = reader.Read<SerializablePlayerOddsSet>();
		RelicGrabBag = reader.Read<SerializableRelicGrabBag>();
		ExtraFields = reader.Read<SerializableExtraPlayerFields>();
		UnlockState = reader.Read<SerializableUnlockState>();
		DiscoveredCards = reader.ReadFullModelIdList();
		DiscoveredEnemies = reader.ReadFullModelIdList();
		int num = reader.ReadInt();
		for (int i = 0; i < num; i++)
		{
			DiscoveredEpochs.Add(reader.ReadEpochId());
		}
		DiscoveredPotions = reader.ReadFullModelIdList();
		DiscoveredRelics = reader.ReadFullModelIdList();
	}

	public SerializablePlayer Anonymized()
	{
		return new SerializablePlayer
		{
			CharacterId = CharacterId,
			CurrentHp = CurrentHp,
			MaxHp = MaxHp,
			MaxEnergy = MaxEnergy,
			MaxPotionSlotCount = MaxPotionSlotCount,
			Gold = Gold,
			BaseOrbSlotCount = BaseOrbSlotCount,
			NetId = IdAnonymizer.Anonymize(NetId),
			Deck = Deck,
			Relics = Relics,
			Potions = Potions,
			Rng = Rng,
			Odds = Odds,
			RelicGrabBag = RelicGrabBag,
			ExtraFields = ExtraFields,
			UnlockState = UnlockState,
			DiscoveredCards = DiscoveredCards,
			DiscoveredEnemies = DiscoveredEnemies,
			DiscoveredEpochs = DiscoveredEpochs,
			DiscoveredPotions = DiscoveredPotions,
			DiscoveredRelics = DiscoveredRelics
		};
	}
}
