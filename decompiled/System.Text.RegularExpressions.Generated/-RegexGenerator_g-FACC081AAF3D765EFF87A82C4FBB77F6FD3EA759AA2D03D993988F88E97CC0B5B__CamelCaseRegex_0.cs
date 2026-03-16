using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.12.31616")]
internal sealed class _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__CamelCaseRegex_0 : Regex
{
	private sealed class RunnerFactory : RegexRunnerFactory
	{
		private sealed class Runner : RegexRunner
		{
			protected override void Scan(ReadOnlySpan<char> inputSpan)
			{
				while (TryFindNextPossibleStartingPosition(inputSpan) && !TryMatchAtCurrentPosition(inputSpan) && runtextpos != inputSpan.Length)
				{
					runtextpos++;
					if (_003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
				}
			}

			private bool TryFindNextPossibleStartingPosition(ReadOnlySpan<char> inputSpan)
			{
				int num = runtextpos;
				if ((uint)num < (uint)inputSpan.Length)
				{
					int num2 = inputSpan.Slice(num).IndexOfAny(_003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__Utilities.s_asciiLettersAndDigits);
					if (num2 >= 0)
					{
						runtextpos = num + num2;
						return true;
					}
				}
				runtextpos = inputSpan.Length;
				return false;
			}

			private bool TryMatchAtCurrentPosition(ReadOnlySpan<char> inputSpan)
			{
				int num = runtextpos;
				int start = num;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				int num6 = 0;
				ReadOnlySpan<char> readOnlySpan = inputSpan.Slice(num);
				num5 = num;
				num4 = num;
				num3 = Crawlpos();
				if (readOnlySpan.IsEmpty || !char.IsAsciiLetterOrDigit(readOnlySpan[0]))
				{
					goto IL_0058;
				}
				num2 = 0;
				num++;
				readOnlySpan = inputSpan.Slice(num);
				goto IL_00c8;
				IL_00c8:
				while (true)
				{
					Capture(1, num5, num);
					num6 = num;
					if (readOnlySpan.IsEmpty || !char.IsAsciiLetterUpper(readOnlySpan[0]))
					{
						if (_003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						if (num2 == 0)
						{
							break;
						}
						if (num2 == 1)
						{
							UncaptureUntil(0);
							return false;
						}
						continue;
					}
					num++;
					readOnlySpan = inputSpan.Slice(num);
					Capture(2, num6, num);
					runtextpos = num;
					Capture(0, start, num);
					return true;
				}
				goto IL_0058;
				IL_0058:
				num = num4;
				readOnlySpan = inputSpan.Slice(num);
				UncaptureUntil(num3);
				if (num != runtextstart)
				{
					UncaptureUntil(0);
					return false;
				}
				int num7 = num;
				if (_003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__Utilities.s_hasTimeout)
				{
					CheckTimeout();
				}
				if (num == 0)
				{
					UncaptureUntil(0);
					return false;
				}
				num = num7;
				readOnlySpan = inputSpan.Slice(num);
				num2 = 1;
				goto IL_00c8;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void UncaptureUntil(int capturePosition)
				{
					while (Crawlpos() > capturePosition)
					{
						Uncapture();
					}
				}
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__CamelCaseRegex_0 Instance = new _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__CamelCaseRegex_0();

	private _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__CamelCaseRegex_0()
	{
		pattern = "([A-Za-z0-9]|\\G(?!^))([A-Z])";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 3;
	}
}
