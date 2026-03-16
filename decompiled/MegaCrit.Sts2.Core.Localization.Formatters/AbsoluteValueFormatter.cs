using System;
using System.Globalization;
using SmartFormat.Core.Extensions;

namespace MegaCrit.Sts2.Core.Localization.Formatters;

public class AbsoluteValueFormatter : IFormatter
{
	public string Name
	{
		get
		{
			return "abs";
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool CanAutoDetect { get; set; }

	private static CultureInfo Culture => LocManager.Instance.CultureInfo;

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		object currentValue = formattingInfo.CurrentValue;
		string text = ((currentValue is decimal value) ? Math.Abs(value).ToString(Culture) : ((currentValue is double value2) ? Math.Abs(value2).ToString(Culture) : ((currentValue is float value3) ? Math.Abs(value3).ToString(Culture) : ((currentValue is int value4) ? Math.Abs(value4).ToString() : ((currentValue is long value5) ? Math.Abs(value5).ToString() : ((!(currentValue is short value6)) ? null : Math.Abs(value6).ToString()))))));
		string text2 = text;
		if (text2 == null)
		{
			return false;
		}
		formattingInfo.Write(text2);
		return true;
	}
}
