namespace MegaCrit.Sts2.Core.Multiplayer.Transport;

public interface INetHandler
{
	void OnPacketReceived(ulong senderId, byte[] packetBytes, NetTransferMode mode, int channel);
}
