using System.Text;

namespace MegaCrit.Sts2.Core.TextEffects;

public static class StsTextUtilities
{
	private const string _creamColorCode = "#FFF6E2";

	private const string _buffPopColorCode = "#77ff67";

	private const string _debuffPopColorCode = "#ff6563";

	private const string _normalPopColor = "yellow";

	public static string HighlightChangeText(string text, int baseComparison)
	{
		StringBuilder stringBuilder = new StringBuilder(text);
		if (baseComparison == 0)
		{
			return stringBuilder.ToString();
		}
		string text2 = ((baseComparison > 0) ? "green" : "red");
		stringBuilder.Insert(0, "[" + text2 + "]");
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 1, stringBuilder2);
		handler.AppendLiteral("[/");
		handler.AppendFormatted(text2);
		handler.AppendLiteral("]");
		stringBuilder2.Append(ref handler);
		return stringBuilder.ToString();
	}
}
