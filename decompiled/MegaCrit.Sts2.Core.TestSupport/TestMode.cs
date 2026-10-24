namespace MegaCrit.Sts2.Core.TestSupport;

public static class TestMode
{
	/// <summary>
	/// Whether the game is running in test mode.
	/// True when we're running unit tests, true when we're running the normal game.
	/// </summary>
	public static bool IsOn { get; set; }

	/// <summary>
	/// Whether the game iS NOT running in test mode.
	/// True when we're running the normal game, false when we're running unit tests.
	/// </summary>
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

	/// <summary>
	/// NEVER CALL THIS. Only calls should be in NetCoreRunner and CiCoreRunner.
	/// </summary>
	public static void TurnOnInternal()
	{
		IsOn = true;
	}
}
