using System;

namespace MegaCrit.Sts2.Core.TestSupport;

public class TestModeOnException : Exception
{
	public TestModeOnException()
		: base("Never call this in test mode.")
	{
	}
}
