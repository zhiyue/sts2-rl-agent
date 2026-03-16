using System.Buffers;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.12.31616")]
internal static class _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__Utilities
{
	internal static readonly TimeSpan s_defaultTimeout = ((AppContext.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") is TimeSpan timeSpan) ? timeSpan : Regex.InfiniteMatchTimeout);

	internal static readonly bool s_hasTimeout = s_defaultTimeout != Regex.InfiniteMatchTimeout;

	internal static readonly SearchValues<char> s_asciiLettersAndDigits = SearchValues.Create("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");

	internal static readonly SearchValues<char> s_ascii_FF03FEFFFF8700000000 = SearchValues.Create("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_");

	internal static readonly SearchValues<string> s_indexOfString_76561_Ordinal;

	internal static readonly SearchValues<char> s_whitespace;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsBoundary(ReadOnlySpan<char> inputSpan, int index)
	{
		int num = index - 1;
		return ((uint)num < (uint)inputSpan.Length && IsBoundaryWordChar(inputSpan[num])) != ((uint)index < (uint)inputSpan.Length && IsBoundaryWordChar(inputSpan[index]));
		static bool IsBoundaryWordChar(char ch)
		{
			if (!IsWordChar(ch))
			{
				return ch == '\u200c' || ch == '\u200d';
			}
			return true;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsWordChar(char ch)
	{
		ReadOnlySpan<byte> readOnlySpan = new byte[16]
		{
			0, 0, 0, 0, 0, 0, 255, 3, 254, 255,
			255, 135, 254, 255, 255, 7
		};
		int num = (int)ch >> 3;
		if ((uint)num >= (uint)readOnlySpan.Length)
		{
			return (0x4013F & (1 << (int)CharUnicodeInfo.GetUnicodeCategory(ch))) != 0;
		}
		return (readOnlySpan[num] & (1 << (ch & 7))) != 0;
	}

	static _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__Utilities()
	{
		string reference = "76561";
		s_indexOfString_76561_Ordinal = SearchValues.Create(new ReadOnlySpan<string>(in reference), StringComparison.Ordinal);
		s_whitespace = SearchValues.Create("\t\n\v\f\r \u0085\u00a0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u2028\u2029\u202f\u205f\u3000");
	}
}
