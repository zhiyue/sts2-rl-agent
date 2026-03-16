using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.GameActions;

public struct NetPlayCardAction : INetAction, IPacketSerializable
{
	public NetCombatCard card;

	public ModelId modelId;

	public uint? targetId;

	public GameAction ToGameAction(Player player)
	{
		return new PlayCardAction(player, card, modelId, targetId);
	}

	public void Serialize(PacketWriter writer)
	{
		writer.Write(card);
		writer.WriteModelEntry(modelId);
		writer.WriteBool(targetId.HasValue);
		if (targetId.HasValue)
		{
			writer.WriteUInt(targetId.GetValueOrDefault(), 6);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		card = reader.Read<NetCombatCard>();
		modelId = reader.ReadModelIdAssumingType<CardModel>();
		if (reader.ReadBool())
		{
			targetId = reader.ReadUInt(6);
		}
		else
		{
			targetId = null;
		}
	}

	public override string ToString()
	{
		return $"NetPlayCardAction ({card}) target: {targetId?.ToString() ?? "null"}";
	}
}
