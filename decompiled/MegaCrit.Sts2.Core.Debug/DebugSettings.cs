using System;

namespace MegaCrit.Sts2.Core.Debug;

public static class DebugSettings
{
	public static bool DevSkip { get; } = Environment.GetEnvironmentVariable("STS2_DEV_SKIP") != null;

	public static bool IgnorePackedImages => false;
}
