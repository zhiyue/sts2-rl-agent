namespace MegaCrit.Sts2.Core.Multiplayer.Transport.ENet;

public enum ENetPacketType : byte
{
	HandshakeRequest,
	HandshakeResponse,
	Disconnection,
	ApplicationMessage
}
