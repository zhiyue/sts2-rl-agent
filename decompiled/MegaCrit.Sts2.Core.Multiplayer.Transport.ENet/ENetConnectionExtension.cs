using Godot;
using Godot.Collections;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport.ENet;

public static class ENetConnectionExtension
{
	public static bool TryService(this ENetConnection connection, out ENetServiceData? output)
	{
		Array array = connection.Service();
		output = null;
		if (array == null)
		{
			return false;
		}
		ENetConnection.EventType eventType = array[0].As<ENetConnection.EventType>();
		if (eventType == ENetConnection.EventType.None)
		{
			return false;
		}
		ENetServiceData value = new ENetServiceData
		{
			type = eventType,
			peer = array[1].As<ENetPacketPeer>(),
			originalData = array
		};
		if (eventType == ENetConnection.EventType.Receive)
		{
			value.channel = array[3].As<int>();
			value.packetData = value.peer.GetPacket();
			value.error = value.peer.GetPacketError();
			value.mode = NetTransferMode.None;
		}
		output = value;
		return true;
	}
}
