using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.GameActions;

public struct NetUsePotionAction : INetAction, IPacketSerializable
{
	public uint potionIndex;

	public uint? targetId;

	public ulong? targetPlayerId;

	public bool enqueuedInCombat;

	public GameAction ToGameAction(Player player)
	{
		return new UsePotionAction(player, potionIndex, targetId, targetPlayerId, enqueuedInCombat);
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteUInt(potionIndex, 4);
		writer.WriteBool(enqueuedInCombat);
		writer.WriteBool(targetId.HasValue);
		if (targetId.HasValue)
		{
			writer.WriteUInt(targetId.GetValueOrDefault(), 6);
		}
		writer.WriteBool(targetPlayerId.HasValue);
		if (targetPlayerId.HasValue)
		{
			writer.WriteULong(targetPlayerId.Value);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		potionIndex = reader.ReadUInt(4);
		enqueuedInCombat = reader.ReadBool();
		if (reader.ReadBool())
		{
			targetId = reader.ReadUInt(6);
		}
		else
		{
			targetId = null;
		}
		if (reader.ReadBool())
		{
			targetPlayerId = reader.ReadULong();
		}
		else
		{
			targetPlayerId = null;
		}
	}

	public override string ToString()
	{
		return $"NetUsePotionAction {potionIndex} target: {targetId} player: {targetPlayerId} combat: {enqueuedInCombat}";
	}
}
