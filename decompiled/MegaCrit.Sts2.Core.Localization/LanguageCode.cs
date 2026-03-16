using System;

namespace MegaCrit.Sts2.Core.Localization;

public class LanguageCode
{
	public string Code { get; }

	public LanguageCode(string code)
	{
		if (string.IsNullOrEmpty(code) || code.Length < 2 || code.Length > 3)
		{
			throw new ArgumentException("Language code must be 2 to 3 characters long.");
		}
		Code = code.ToLowerInvariant();
	}

	public bool IsValid()
	{
		int length = Code.Length;
		if (length >= 2)
		{
			return length <= 3;
		}
		return false;
	}

	public override string ToString()
	{
		return Code;
	}
}
