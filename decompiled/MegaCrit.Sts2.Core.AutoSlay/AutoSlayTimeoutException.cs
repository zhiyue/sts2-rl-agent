using System;

namespace MegaCrit.Sts2.Core.AutoSlay;

/// <summary>
/// Exception thrown when AutoSlay times out waiting for a condition or progress.
/// Filtered from Sentry since these are expected during automated testing.
/// </summary>
public class AutoSlayTimeoutException : TimeoutException
{
	public AutoSlayTimeoutException(string message)
		: base(message)
	{
	}
}
