using System;

namespace MegaCrit.Sts2.Core.TestSupport;

public class TestModeOffException : Exception
{
	public TestModeOffException()
		: base("Only call this in test mode.")
	{
	}
}
