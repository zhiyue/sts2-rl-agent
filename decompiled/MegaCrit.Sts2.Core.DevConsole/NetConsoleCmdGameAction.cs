using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.DevConsole;

public struct NetConsoleCmdGameAction : INetAction, IPacketSerializable
{
	public string cmd;

	public bool inCombat;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteString(cmd);
		writer.WriteBool(inCombat);
	}

	public void Deserialize(PacketReader reader)
	{
		cmd = reader.ReadString();
		inCombat = reader.ReadBool();
	}

	public GameAction ToGameAction(Player player)
	{
		return new ConsoleCmdGameAction(player, cmd, inCombat);
	}

	public override string ToString()
	{
		return $"{"NetConsoleCmdGameAction"} cmd {cmd} inCombat {inCombat}";
	}
}
