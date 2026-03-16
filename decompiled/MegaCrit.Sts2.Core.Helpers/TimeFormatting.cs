using System.Text;
using Godot;

namespace MegaCrit.Sts2.Core.Helpers;

public static class TimeFormatting
{
	private static readonly StringBuilder _timeStringBuilder = new StringBuilder();

	public static string Format(float time)
	{
		_timeStringBuilder.Clear();
		int num = Mathf.FloorToInt(time / 3600f);
		if (num > 0)
		{
			_timeStringBuilder.Append(num);
			_timeStringBuilder.Append(':');
		}
		string value = Mathf.Floor(time / 60f % 60f).ToString("00");
		string value2 = Mathf.RoundToInt(time % 60f).ToString("00");
		_timeStringBuilder.Append(value);
		_timeStringBuilder.Append(':');
		_timeStringBuilder.Append(value2);
		return _timeStringBuilder.ToString();
	}
}
