using System;
using System.Text;
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

	public bool ShouldBuffer => true;

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

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(29, 4, stringBuilder2);
		handler.AppendFormatted("RewardObtainedMessage");
		handler.AppendLiteral(" type: ");
		handler.AppendFormatted(rewardType);
		handler.AppendLiteral(" location: ");
		handler.AppendFormatted(location);
		handler.AppendLiteral(" skipped: ");
		handler.AppendFormatted(wasSkipped);
		handler.AppendLiteral(" ");
		stringBuilder3.Append(ref handler);
		switch (rewardType)
		{
		case RewardType.Card:
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder2);
			handler.AppendLiteral("Card: ");
			handler.AppendFormatted(cardModel);
			stringBuilder7.Append(ref handler);
			break;
		}
		case RewardType.Gold:
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder2);
			handler.AppendLiteral("Gold: ");
			handler.AppendFormatted(goldAmount);
			stringBuilder6.Append(ref handler);
			break;
		}
		case RewardType.Potion:
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(8, 1, stringBuilder2);
			handler.AppendLiteral("Potion: ");
			handler.AppendFormatted(potionModel);
			stringBuilder5.Append(ref handler);
			break;
		}
		case RewardType.Relic:
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(7, 1, stringBuilder2);
			handler.AppendLiteral("Relic: ");
			handler.AppendFormatted(relicModel);
			stringBuilder4.Append(ref handler);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		return stringBuilder.ToString();
	}
}
