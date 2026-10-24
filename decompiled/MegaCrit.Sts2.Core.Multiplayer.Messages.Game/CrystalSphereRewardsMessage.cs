using System.Collections.Generic;
using MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

public class CrystalSphereRewardsMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public List<SerializableCrystalSphereItem> rewards = new List<SerializableCrystalSphereItem>();

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

	public RunLocation Location { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.Write(Location);
		writer.WriteList(rewards);
	}

	public void Deserialize(PacketReader reader)
	{
		Location = reader.Read<RunLocation>();
		rewards = reader.ReadList<SerializableCrystalSphereItem>();
	}
}
