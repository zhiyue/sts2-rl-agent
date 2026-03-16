using System;
using Godot;

namespace MegaCrit.Sts2.Core.Multiplayer.Serialization;

internal static class BitSerializationUtil
{
	public static byte GetByteMask(int bits, int startBit)
	{
		if (bits > 8 || startBit + bits > 8)
		{
			throw new InvalidOperationException();
		}
		return (byte)((1 << bits) - 1 << startBit);
	}

	public static byte GetBitsAtPosition(byte[] bytes, int bitPosition, int bitsToObtain)
	{
		int num = bitPosition / 8;
		int num2 = bitPosition % 8;
		if (bitsToObtain > 8)
		{
			throw new InvalidOperationException();
		}
		if (num2 == 0)
		{
			return (byte)(bytes[num] & GetByteMask(bitsToObtain, 0));
		}
		int num3 = 8 - num2;
		if (bitsToObtain <= num3)
		{
			return (byte)((bytes[num] >> num2) & GetByteMask(bitsToObtain, 0));
		}
		byte b = (byte)(bytes[num] >> num2);
		byte byteMask = GetByteMask(num3, 0);
		byte b2 = (byte)(bytes[num + 1] << num3);
		byte byteMask2 = GetByteMask(bitsToObtain - num3, num3);
		byte b3 = (byte)((b & byteMask) | (b2 & byteMask2));
		return (byte)(b3 & GetByteMask(bitsToObtain, 0));
	}

	public static void WriteBytes(byte[] originBuffer, byte[] destinationBuffer, int destinationBitPosition, int totalBitsToWrite)
	{
		int num3;
		for (int i = 0; i < totalBitsToWrite; i += num3)
		{
			int num = (destinationBitPosition + i) / 8;
			int num2 = (destinationBitPosition + i) % 8;
			num3 = Mathf.Min(totalBitsToWrite - i, 8 - num2);
			byte bitsAtPosition = GetBitsAtPosition(originBuffer, i, num3);
			byte b = destinationBuffer[num];
			byte byteMask = GetByteMask(num2, 0);
			byte byteMask2 = GetByteMask(num3, num2);
			byte b2 = (byte)(bitsAtPosition << num2);
			byte b3 = (byte)((b & byteMask) | (b2 & byteMask2));
			destinationBuffer[num] = b3;
		}
	}

	public static void ReadBits(byte[] originBuffer, int originBitPosition, byte[] destinationBuffer, int totalBitsToRead)
	{
		int num3;
		for (int i = 0; i < totalBitsToRead; i += num3)
		{
			int num = i / 8;
			int num2 = i % 8;
			num3 = Mathf.Min(totalBitsToRead - i, 8 - num2);
			byte bitsAtPosition = GetBitsAtPosition(originBuffer, originBitPosition + i, num3);
			byte b = destinationBuffer[num];
			byte byteMask = GetByteMask(num2, 0);
			byte byteMask2 = GetByteMask(num3, num2);
			byte b2 = (byte)(bitsAtPosition << num2);
			byte b3 = (byte)((b & byteMask) | (b2 & byteMask2));
			destinationBuffer[num] = b3;
		}
	}
}
