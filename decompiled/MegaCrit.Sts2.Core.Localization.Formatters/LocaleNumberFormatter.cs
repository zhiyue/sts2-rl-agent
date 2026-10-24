using System;
using System.Globalization;
using SmartFormat.Core.Extensions;

namespace MegaCrit.Sts2.Core.Localization.Formatters;

/// <summary>
/// Formats numeric types with locale-aware thousand separators (N0) by default.
/// Named formatters (diff, energyIcons, etc.) take priority over this auto-detect formatter.
/// </summary>
public class LocaleNumberFormatter : IFormatter
{
	public string Name
	{
		get
		{
			return "n";
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool CanAutoDetect { get; set; } = true;

	private static CultureInfo Culture => LocManager.Instance.CultureInfo;

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		object currentValue = formattingInfo.CurrentValue;
		if ((!(currentValue is int) && !(currentValue is uint) && !(currentValue is long) && !(currentValue is ulong) && !(currentValue is decimal) && !(currentValue is float) && !(currentValue is double)) || 1 == 0)
		{
			return false;
		}
		string text = (string.IsNullOrEmpty(formattingInfo.Format?.RawText) ? "N0" : formattingInfo.Format.RawText);
		string text2 = ((IFormattable)formattingInfo.CurrentValue).ToString(text, Culture);
		formattingInfo.Write(text2);
		return true;
	}
}
