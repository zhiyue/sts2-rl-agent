using System;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;

public struct RewardObtainedMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public required RewardType rewardType;

	public required RunLocation location;

	public CardModel? cardModel;

	public PotionModel? potionModel;

	public RelicModel? relicModel;

	public int? goldAmount;

	public required bool wasSkipped;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public RunLocation Location => location;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteEnum(rewardType);
		writer.Write(location);
		switch (rewardType)
		{
		case RewardType.Card:
			writer.Write(cardModel.ToSerializable());
			break;
		case RewardType.Gold:
			writer.WriteInt(goldAmount.Value);
			break;
		case RewardType.Potion:
			writer.WriteModelEntry(potionModel.Id);
			break;
		case RewardType.Relic:
			writer.Write(relicModel.ToSerializable());
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		writer.WriteBool(wasSkipped);
	}

	public void Deserialize(PacketReader reader)
	{
		rewardType = reader.ReadEnum<RewardType>();
		location = reader.Read<RunLocation>();
		switch (rewardType)
		{
		case RewardType.Card:
			cardModel = CardModel.FromSerializable(reader.Read<SerializableCard>());
			break;
		case RewardType.Gold:
			goldAmount = reader.ReadInt();
			break;
		case RewardType.Potion:
			potionModel = ModelDb.GetById<PotionModel>(reader.ReadModelIdAssumingType<PotionModel>());
			break;
		case RewardType.Relic:
			relicModel = RelicModel.FromSerializable(reader.Read<SerializableRelic>());
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		wasSkipped = reader.ReadBool();
	}
}
