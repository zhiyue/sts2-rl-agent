using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.GameActions;

public struct NetUndoEndPlayerTurnAction : INetAction, IPacketSerializable
{
	public int turnNumber;

	public GameAction ToGameAction(Player player)
	{
		return new UndoEndPlayerTurnAction(player, turnNumber);
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(turnNumber, 16);
	}

	public void Deserialize(PacketReader reader)
	{
		turnNumber = reader.ReadInt(16);
	}

	public override string ToString()
	{
		return $"{"NetUndoEndPlayerTurnAction"} turn: {turnNumber}";
	}
}
