using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.GameActions;

public struct NetEndPlayerTurnAction : INetAction, IPacketSerializable
{
	public int combatRound;

	public GameAction ToGameAction(Player player)
	{
		return new EndPlayerTurnAction(player, combatRound);
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(combatRound, 16);
	}

	public void Deserialize(PacketReader reader)
	{
		combatRound = reader.ReadInt(16);
	}

	public override string ToString()
	{
		return $"{"NetEndPlayerTurnAction"} round: {combatRound}";
	}
}
