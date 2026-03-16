using System;

namespace MegaCrit.Sts2.Core.Localization;

public class LocException : Exception
{
	public LocException(string message)
		: base(message)
	{
	}

	public LocException(string message, Exception e)
		: base(message, e)
	{
	}
}
