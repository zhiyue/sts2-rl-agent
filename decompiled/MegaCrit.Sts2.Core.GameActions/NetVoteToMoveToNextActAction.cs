using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.GameActions;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct NetVoteToMoveToNextActAction : INetAction, IPacketSerializable
{
	public GameAction ToGameAction(Player player)
	{
		return new VoteToMoveToNextActAction(player);
	}

	public void Serialize(PacketWriter writer)
	{
	}

	public void Deserialize(PacketReader reader)
	{
	}

	public override string ToString()
	{
		return "NetVoteForMapCoordAction";
	}
}
