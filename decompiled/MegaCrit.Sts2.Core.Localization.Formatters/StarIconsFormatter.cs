using System;
using System.Linq;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SmartFormat.Core.Extensions;

namespace MegaCrit.Sts2.Core.Localization.Formatters;

public class StarIconsFormatter : IFormatter
{
	private const string _starIconPath = "res://images/packed/sprite_fonts/star_icon.png";

	public const string starIconSprite = "[img]res://images/packed/sprite_fonts/star_icon.png[/img]";

	public string Name
	{
		get
		{
			return "starIcons";
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool CanAutoDetect { get; set; }

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		object currentValue = formattingInfo.CurrentValue;
		int num3;
		if (!(currentValue is DynamicVar dynamicVar))
		{
			if (!(currentValue is decimal num))
			{
				if (!(currentValue is int num2))
				{
					throw new LocException($"Unknown value='{formattingInfo.CurrentValue}' type={formattingInfo.CurrentValue?.GetType()}");
				}
				num3 = num2;
			}
			else
			{
				num3 = (int)num;
			}
		}
		else
		{
			num3 = (int)dynamicVar.PreviewValue;
		}
		int count = num3;
		string text = string.Concat(Enumerable.Repeat("[img]res://images/packed/sprite_fonts/star_icon.png[/img]", count));
		formattingInfo.Write(text);
		return true;
	}
}
