using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent;

public class SerializableCrystalSphereItem : IPacketSerializable
{
	public CrystalSphereItemType type;

	public CardRarity cardRarity;

	public PotionRarity potionRarity;

	public bool isBigGold;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteEnum(type);
		if (type == CrystalSphereItemType.CardReward)
		{
			writer.WriteEnum(cardRarity);
		}
		else if (type == CrystalSphereItemType.Potion)
		{
			writer.WriteEnum(potionRarity);
		}
		else if (type == CrystalSphereItemType.Gold)
		{
			writer.WriteBool(isBigGold);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		type = reader.ReadEnum<CrystalSphereItemType>();
		if (type == CrystalSphereItemType.CardReward)
		{
			cardRarity = reader.ReadEnum<CardRarity>();
		}
		else if (type == CrystalSphereItemType.Potion)
		{
			potionRarity = reader.ReadEnum<PotionRarity>();
		}
		else if (type == CrystalSphereItemType.Gold)
		{
			isBigGold = reader.ReadBool();
		}
	}
}
