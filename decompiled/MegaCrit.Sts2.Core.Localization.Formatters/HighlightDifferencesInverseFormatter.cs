using System;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SmartFormat.Core.Extensions;

namespace MegaCrit.Sts2.Core.Localization.Formatters;

public class HighlightDifferencesInverseFormatter : IFormatter
{
	public string Name
	{
		get
		{
			return "inverseDiff";
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool CanAutoDetect { get; set; }

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		if (!(formattingInfo.CurrentValue is DynamicVar dynamicVar))
		{
			return false;
		}
		formattingInfo.Write(dynamicVar.ToHighlightedString(inverse: true));
		return true;
	}
}
