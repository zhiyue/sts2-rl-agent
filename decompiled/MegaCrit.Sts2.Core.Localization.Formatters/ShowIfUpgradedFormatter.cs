using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace MegaCrit.Sts2.Core.Localization.Formatters;

public class ShowIfUpgradedFormatter : IFormatter
{
	public string Name
	{
		get
		{
			return "show";
		}
		set
		{
			throw new NotSupportedException("Setting the 'Names' property is not supported.");
		}
	}

	public bool CanAutoDetect { get; set; }

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		if (!(formattingInfo.CurrentValue is IfUpgradedVar ifUpgradedVar))
		{
			return false;
		}
		IList<Format> list = formattingInfo.Format?.Split('|');
		if (list == null)
		{
			throw new LocException($"Format expression must contain at least 1 option. format={formattingInfo.Format}.");
		}
		if (list.Count > 2)
		{
			throw new LocException($"Format expression cannot contain more than 2 options. num_of_options={list.Count} format={formattingInfo.Format}.");
		}
		Format format = list[0];
		Format format2 = ((list.Count > 1) ? list[1] : null);
		switch (ifUpgradedVar.upgradeDisplay)
		{
		case UpgradeDisplay.Normal:
			formattingInfo.FormatAsChild(format2, formattingInfo.CurrentValue);
			break;
		case UpgradeDisplay.Upgraded:
			formattingInfo.FormatAsChild(format, formattingInfo.CurrentValue);
			break;
		case UpgradeDisplay.UpgradePreview:
			formattingInfo.Write("[green]");
			formattingInfo.FormatAsChild(format, formattingInfo.CurrentValue);
			formattingInfo.Write("[/green]");
			break;
		default:
			throw new ArgumentOutOfRangeException("upgradeDisplay", $"Unexpected value: {ifUpgradedVar.upgradeDisplay}");
		}
		return true;
	}
}
