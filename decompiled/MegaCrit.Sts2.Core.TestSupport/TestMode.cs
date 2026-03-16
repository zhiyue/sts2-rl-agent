namespace MegaCrit.Sts2.Core.TestSupport;

public static class TestMode
{
	public static bool IsOn { get; set; }

	public static bool IsOff => !IsOn;

	public static void AssertOn()
	{
		if (IsOn)
		{
			return;
		}
		throw new TestModeOffException();
	}

	public static void AssertOff()
	{
		if (IsOff)
		{
			return;
		}
		throw new TestModeOnException();
	}

	public static void TurnOnInternal()
	{
		IsOn = true;
	}
}
