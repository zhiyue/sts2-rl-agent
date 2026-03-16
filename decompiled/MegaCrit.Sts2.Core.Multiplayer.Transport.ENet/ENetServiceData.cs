using Godot;
using Godot.Collections;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport.ENet;

public struct ENetServiceData
{
	public ENetConnection.EventType type;

	public ENetPacketPeer peer;

	public int channel;

	public NetTransferMode mode;

	public byte[] packetData;

	public Error error;

	public Array originalData;
}
