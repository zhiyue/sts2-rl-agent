using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.GameActions;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct NetReadyToBeginEnemyTurnAction : INetAction, IPacketSerializable
{
	public GameAction ToGameAction(Player player)
	{
		return new ReadyToBeginEnemyTurnAction(player);
	}

	public void Serialize(PacketWriter serializer)
	{
	}

	public void Deserialize(PacketReader deserializer)
	{
	}

	public override string ToString()
	{
		return "NetReadyToBeginEnemyTurnAction";
	}
}
