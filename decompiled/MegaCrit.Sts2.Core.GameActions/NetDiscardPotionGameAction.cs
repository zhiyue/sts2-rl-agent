using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.GameActions;

public struct NetDiscardPotionGameAction : INetAction, IPacketSerializable
{
	public uint potionSlotIndex;

	public bool wasEnqueuedInCombat;

	public GameAction ToGameAction(Player player)
	{
		return new DiscardPotionGameAction(player, potionSlotIndex, wasEnqueuedInCombat);
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteUInt(potionSlotIndex, 4);
		writer.WriteBool(wasEnqueuedInCombat);
	}

	public void Deserialize(PacketReader reader)
	{
		potionSlotIndex = reader.ReadUInt(4);
		wasEnqueuedInCombat = reader.ReadBool();
	}

	public override string ToString()
	{
		return $"{"NetDiscardPotionGameAction"} slot {potionSlotIndex} in combat: {wasEnqueuedInCombat}";
	}
}
