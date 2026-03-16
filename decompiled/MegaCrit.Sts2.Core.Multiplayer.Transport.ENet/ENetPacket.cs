using System;
using System.Buffers.Binary;
using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport.ENet;

public class ENetPacket
{
	private readonly byte[] _packetBytes;

	public ENetPacketType PacketType => (ENetPacketType)_packetBytes[0];

	public byte[] AllBytes => _packetBytes;

	public ENetPacket(byte[] bytes)
	{
		_packetBytes = bytes;
	}

	public static ENetPacket FromHandshakeRequest(ENetHandshakeRequest request)
	{
		byte[] array = new byte[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		Span<byte> span = array.AsSpan();
		BinaryPrimitives.WriteUInt64BigEndian(span.Slice(1, span.Length - 1), request.netId);
		return new ENetPacket(array);
	}

	public ENetHandshakeRequest AsHandshakeRequest()
	{
		if (PacketType != ENetPacketType.HandshakeRequest)
		{
			throw new InvalidOperationException($"Attempted to interpret ENet packet of type {PacketType} as handshake request");
		}
		ulong netId = BinaryPrimitives.ReadUInt64BigEndian(_packetBytes[1..]);
		return new ENetHandshakeRequest
		{
			netId = netId
		};
	}

	public static ENetPacket FromHandshakeResponse(ENetHandshakeResponse response)
	{
		byte[] array = new byte[10]
		{
			1,
			(byte)response.status,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0
		};
		Span<byte> span = array.AsSpan();
		BinaryPrimitives.WriteUInt64BigEndian(span.Slice(2, span.Length - 2), response.netId);
		return new ENetPacket(array);
	}

	public ENetHandshakeResponse AsHandshakeResponse()
	{
		if (PacketType != ENetPacketType.HandshakeResponse)
		{
			throw new InvalidOperationException($"Attempted to interpret ENet packet of type {PacketType} as handshake response");
		}
		ENetHandshakeStatus status = (ENetHandshakeStatus)_packetBytes[1];
		ulong netId = BinaryPrimitives.ReadUInt64BigEndian(_packetBytes[2..]);
		return new ENetHandshakeResponse
		{
			netId = netId,
			status = status
		};
	}

	public static ENetPacket FromDisconnection(ENetDisconnection disconnection)
	{
		return new ENetPacket(new byte[2]
		{
			2,
			(byte)disconnection.reason
		});
	}

	public ENetDisconnection AsDisconnection()
	{
		if (PacketType != ENetPacketType.Disconnection)
		{
			throw new InvalidOperationException($"Attempted to interpret ENet packet of type {PacketType} as disconnection");
		}
		NetError reason = (NetError)_packetBytes[1];
		return new ENetDisconnection
		{
			reason = reason
		};
	}

	public static ENetPacket FromAppMessage(byte[] messageBytes, int count)
	{
		byte[] array = new byte[count + 1];
		array[0] = 3;
		Array.Copy(messageBytes, 0, array, 1, count);
		return new ENetPacket(array);
	}

	public byte[] AsAppMessage()
	{
		if (PacketType != ENetPacketType.ApplicationMessage)
		{
			throw new InvalidOperationException($"Attempted to interpret ENet packet of type {PacketType} as disconnection");
		}
		return _packetBytes[1..];
	}
}
