using System;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Helpers;

public static class NonInteractiveMode
{
	public static Func<bool> AutoSlayerCheck { get; set; } = () => false;

	public static bool IsActive
	{
		get
		{
			if (!TestMode.IsOn)
			{
				return AutoSlayerCheck();
			}
			return true;
		}
	}
}
