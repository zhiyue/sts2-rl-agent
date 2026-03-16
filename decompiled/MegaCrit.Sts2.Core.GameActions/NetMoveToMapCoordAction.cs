using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.GameActions;

public struct NetMoveToMapCoordAction : INetAction, IPacketSerializable
{
	public MapCoord destination;

	public GameAction ToGameAction(Player player)
	{
		return new MoveToMapCoordAction(player, destination);
	}

	public void Serialize(PacketWriter writer)
	{
		writer.Write(destination);
	}

	public void Deserialize(PacketReader reader)
	{
		destination = reader.Read<MapCoord>();
	}

	public override string ToString()
	{
		return $"{"MoveToMapCoordAction"} to {destination}";
	}
}
