using System;

namespace MegaCrit.Sts2.Core.AutoSlay;

public class AutoSlayTimeoutException : TimeoutException
{
	public AutoSlayTimeoutException(string message)
		: base(message)
	{
	}
}
