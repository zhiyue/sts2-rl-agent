using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.12.31616")]
internal sealed class _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__SnakeCaseRegex_1 : Regex
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
				if (num <= inputSpan.Length - 2)
				{
					ReadOnlySpan<char> readOnlySpan = inputSpan.Slice(num);
					for (int i = 0; i < readOnlySpan.Length; i++)
					{
						if (readOnlySpan[i] != '\n')
						{
							runtextpos = num + i;
							return true;
						}
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
				ReadOnlySpan<char> readOnlySpan = inputSpan.Slice(num);
				num2 = num;
				num5 = num;
				while (true)
				{
					num4 = Crawlpos();
					Capture(1, num2, num);
					if (!readOnlySpan.IsEmpty && readOnlySpan[0] == '_')
					{
						num++;
						readOnlySpan = inputSpan.Slice(num);
						num3 = num;
						if (!readOnlySpan.IsEmpty && char.IsAsciiLetterOrDigit(readOnlySpan[0]))
						{
							break;
						}
					}
					UncaptureUntil(num4);
					if (_003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					num = num5;
					readOnlySpan = inputSpan.Slice(num);
					if (readOnlySpan.IsEmpty || readOnlySpan[0] == '\n')
					{
						UncaptureUntil(0);
						return false;
					}
					num++;
					readOnlySpan = inputSpan.Slice(num);
					num5 = readOnlySpan.IndexOfAny('\n', '_');
					if ((uint)num5 >= (uint)readOnlySpan.Length || readOnlySpan[num5] == '\n')
					{
						UncaptureUntil(0);
						return false;
					}
					num += num5;
					readOnlySpan = inputSpan.Slice(num);
					num5 = num;
				}
				num++;
				readOnlySpan = inputSpan.Slice(num);
				Capture(2, num3, num);
				runtextpos = num;
				Capture(0, start, num);
				return true;
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

	internal static readonly _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__SnakeCaseRegex_1 Instance = new _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__SnakeCaseRegex_1();

	private _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__SnakeCaseRegex_1()
	{
		pattern = "(.*?)_([a-zA-Z0-9])";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 3;
	}
}
