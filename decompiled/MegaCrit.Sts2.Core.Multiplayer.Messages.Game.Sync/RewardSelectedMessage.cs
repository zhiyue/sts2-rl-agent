using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;

/// <summary>
/// Message that is sent when a reward is selected from a rewards screen.
/// This is sent BEFORE obtaining a reward. For instance, when a player selects a card reward, this message is sent
/// before the player finishes selecting the card they wish to choose. The card selection occurs as a separate process.
/// </summary>
public class RewardSelectedMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public RunLocation location;

	public int setId;

	public int rewardIndex;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Debug;

	public bool ShouldBuffer => true;

	public RunLocation Location => location;

	public void Serialize(PacketWriter writer)
	{
		writer.Write(location);
		writer.WriteInt(setId);
		writer.WriteInt(rewardIndex, 8);
	}

	public void Deserialize(PacketReader reader)
	{
		location = reader.Read<RunLocation>();
		setId = reader.ReadInt();
		rewardIndex = reader.ReadInt(8);
	}
}
