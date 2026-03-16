using System;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;

public struct HoveredModelData : IPacketSerializable, IEquatable<HoveredModelData>
{
	public HoveredModelType type;

	public NetCombatCard? hoveredCombatCard;

	public int? hoveredRelicIndex;

	public int? hoveredPotionIndex;

	public ModelId? hoveredModelId;

	public bool Equals(HoveredModelData other)
	{
		if (type == other.type)
		{
			NetCombatCard? netCombatCard = hoveredCombatCard;
			NetCombatCard? netCombatCard2 = other.hoveredCombatCard;
			if (netCombatCard.HasValue == netCombatCard2.HasValue && (!netCombatCard.HasValue || netCombatCard.GetValueOrDefault() == netCombatCard2.GetValueOrDefault()) && hoveredRelicIndex == other.hoveredRelicIndex && hoveredPotionIndex == other.hoveredPotionIndex)
			{
				return hoveredModelId == other.hoveredModelId;
			}
		}
		return false;
	}

	public static HoveredModelData FromModel(AbstractModel? model)
	{
		if (model == null)
		{
			return default(HoveredModelData);
		}
		HoveredModelData result = default(HoveredModelData);
		if (!(model is CardModel card))
		{
			if (!(model is RelicModel relicModel))
			{
				if (!(model is PotionModel potionModel))
				{
					throw new InvalidOperationException($"Model {model} has unsupported type for hovering");
				}
				result.type = HoveredModelType.Potion;
				result.hoveredPotionIndex = potionModel.Owner.GetPotionSlotIndex(potionModel);
			}
			else
			{
				result.type = HoveredModelType.Relic;
				result.hoveredRelicIndex = relicModel.Owner.Relics.IndexOf(relicModel);
			}
		}
		else
		{
			result.type = HoveredModelType.Card;
			result.hoveredCombatCard = NetCombatCard.FromModel(card);
		}
		result.hoveredModelId = model.Id;
		return result;
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteEnum(type);
		switch (type)
		{
		case HoveredModelType.Card:
			writer.Write(hoveredCombatCard.Value);
			break;
		case HoveredModelType.Relic:
			writer.WriteInt(hoveredRelicIndex.Value, 8);
			break;
		case HoveredModelType.Potion:
			writer.WriteInt(hoveredPotionIndex.Value, 4);
			break;
		}
		if (type != HoveredModelType.None)
		{
			writer.WriteModelEntry(hoveredModelId);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		type = reader.ReadEnum<HoveredModelType>();
		switch (type)
		{
		case HoveredModelType.Card:
			hoveredCombatCard = reader.Read<NetCombatCard>();
			hoveredModelId = reader.ReadModelIdAssumingType<CardModel>();
			break;
		case HoveredModelType.Relic:
			hoveredRelicIndex = reader.ReadInt(8);
			hoveredModelId = reader.ReadModelIdAssumingType<RelicModel>();
			break;
		case HoveredModelType.Potion:
			hoveredPotionIndex = reader.ReadInt(4);
			hoveredModelId = reader.ReadModelIdAssumingType<PotionModel>();
			break;
		}
	}

	public override string ToString()
	{
		return type switch
		{
			HoveredModelType.None => "HoveredModelData none", 
			HoveredModelType.Card => $"HoveredModelData {hoveredCombatCard}", 
			HoveredModelType.Relic => $"HoveredModelData relic index {hoveredRelicIndex}", 
			HoveredModelType.Potion => $"HoveredModelData potion index {hoveredPotionIndex}", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
