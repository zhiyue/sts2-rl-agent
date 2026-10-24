using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.GameActions;

public struct NetVoteForMapCoordAction : INetAction, IPacketSerializable
{
	public MapLocation source;

	public MapVote? destination;

	public GameAction ToGameAction(Player player)
	{
		return new VoteForMapCoordAction(player, source, destination);
	}

	public void Serialize(PacketWriter writer)
	{
		writer.Write(source);
		writer.WriteBool(destination.HasValue);
		if (destination.HasValue)
		{
			writer.Write(destination.Value);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		source = reader.Read<MapLocation>();
		if (reader.ReadBool())
		{
			destination = reader.Read<MapVote>();
		}
	}

	public override string ToString()
	{
		return $"{"NetVoteForMapCoordAction"} {source}->{destination}";
	}
}
