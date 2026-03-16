using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MegaCrit.Sts2.Core.Multiplayer;

public static class MultiplayerDebugUtil
{
	private static IEnumerable<BigInteger> Chunk(IReadOnlyList<byte> source, int size)
	{
		return from e in source.Select((byte _, int index) => source.Skip(size * index).Take(size).ToArray()).TakeWhile((byte[] bucket) => bucket.Length != 0)
			select new BigInteger(e);
	}

	private static byte ReplaceControlCharacterWithDot(byte character)
	{
		if ((character >= 31 && character < 127) || 1 == 0)
		{
			return character;
		}
		return 46;
	}

	private static byte[] ReplaceControlCharactersWithDots(byte[] characters)
	{
		return characters.Select(ReplaceControlCharacterWithDot).ToArray();
	}

	public static string FormatAsHex(ReadOnlySpan<byte> data, int lineWidth = 16, int byteWidth = 1)
	{
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array;
		for (int i = 0; i < data.Length; i += array.Length)
		{
			array = data.Slice(i, Math.Min(lineWidth * byteWidth, data.Length - i)).ToArray();
			string text = string.Join(" ", from v in Chunk(array, byteWidth)
				select v.ToString("X" + byteWidth * 2, CultureInfo.InvariantCulture));
			text += new string(' ', lineWidth * (byteWidth * 2 + 1) - 1 - text.Length);
			string value = Encoding.ASCII.GetString(ReplaceControlCharactersWithDots(array));
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 3, stringBuilder2);
			handler.AppendFormatted(i, "X4");
			handler.AppendLiteral(" ");
			handler.AppendFormatted(text);
			handler.AppendLiteral(" ");
			handler.AppendFormatted(value);
			handler.AppendLiteral("\n");
			stringBuilder2.Append(ref handler);
		}
		return stringBuilder.ToString();
	}

	public static byte[] StringToByteArray(string hex)
	{
		if (hex.Length % 2 == 1)
		{
			throw new Exception("The binary key cannot have an odd number of digits");
		}
		byte[] array = new byte[hex.Length >> 1];
		for (int i = 0; i < hex.Length >> 1; i++)
		{
			array[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + GetHexVal(hex[(i << 1) + 1]));
		}
		return array;
	}

	private static int GetHexVal(char hex)
	{
		return hex - ((hex < ':') ? 48 : ((hex < 'a') ? 55 : 87));
	}
}
