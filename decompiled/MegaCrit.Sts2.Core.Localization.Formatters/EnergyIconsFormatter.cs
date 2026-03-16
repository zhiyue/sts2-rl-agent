using System;
using System.Linq;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using SmartFormat.Core.Extensions;

namespace MegaCrit.Sts2.Core.Localization.Formatters;

public class EnergyIconsFormatter : IFormatter
{
	private const string _fallbackIconPrefix = "colorless";

	public string Name
	{
		get
		{
			return "energyIcons";
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool CanAutoDetect { get; set; }

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		string text = null;
		object currentValue = formattingInfo.CurrentValue;
		int result;
		if (!(currentValue is EnergyVar energyVar))
		{
			if (!(currentValue is CalculatedVar calculatedVar))
			{
				if (!(currentValue is decimal num))
				{
					if (!(currentValue is int num2))
					{
						if (!(currentValue is string text2))
						{
							throw new LocException($"Unknown value='{formattingInfo.CurrentValue}' type={formattingInfo.CurrentValue?.GetType()}");
						}
						if (!int.TryParse(formattingInfo.FormatterOptions, out result))
						{
							return false;
						}
						text = text2;
					}
					else
					{
						result = num2;
					}
				}
				else
				{
					result = (int)num;
				}
			}
			else
			{
				result = Convert.ToInt32(calculatedVar.Calculate(null));
			}
		}
		else
		{
			result = Convert.ToInt32(energyVar.PreviewValue);
			if (!string.IsNullOrEmpty(energyVar.ColorPrefix))
			{
				text = energyVar.ColorPrefix;
			}
		}
		if (string.IsNullOrEmpty(text) || text == "colorless")
		{
			text = RunManager.Instance.GetLocalCharacterEnergyIconPrefix();
		}
		if (text == null)
		{
			Log.Warn("No energy prefix found for EnergyIconsFormatter! Using colorless as a fallback.");
			if (text == null)
			{
				text = "colorless";
			}
		}
		string text3 = "[img]res://images/packed/sprite_fonts/" + text + "_energy_icon.png[/img]";
		string text4 = ((result <= 0 || result >= 4) ? $"{result}{text3}" : string.Concat(Enumerable.Repeat(text3, result)));
		formattingInfo.Write(text4);
		return true;
	}
}
