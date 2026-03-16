using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.DevConsole;

public struct NetConsoleCmdGameAction : INetAction, IPacketSerializable
{
	public string cmd;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteString(cmd);
	}

	public void Deserialize(PacketReader reader)
	{
		cmd = reader.ReadString();
	}

	public GameAction ToGameAction(Player player)
	{
		return new ConsoleCmdGameAction(player, cmd);
	}
}
