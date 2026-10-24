using System;

namespace MegaCrit.Sts2.Core.AutoSlay;

/// <summary>
/// Configuration constants for AutoSlay timeouts and settings.
/// </summary>
public static class AutoSlayConfig
{
	/// <summary>Maximum time for a complete run.</summary>
	public static readonly TimeSpan runTimeout = TimeSpan.FromMinutes(25L);

	/// <summary>Default timeout for room handlers.</summary>
	public static readonly TimeSpan defaultRoomTimeout = TimeSpan.FromMinutes(2L);

	/// <summary>Default timeout for screen handlers.</summary>
	public static readonly TimeSpan defaultScreenTimeout = TimeSpan.FromSeconds(30L);

	/// <summary>Timeout for waiting for game instance to initialize.</summary>
	public static readonly TimeSpan gameInitTimeout = TimeSpan.FromSeconds(10L);

	/// <summary>Timeout for waiting for run state to initialize.</summary>
	public static readonly TimeSpan runStateTimeout = TimeSpan.FromSeconds(30L);

	/// <summary>Timeout for waiting for nodes to appear.</summary>
	public static readonly TimeSpan nodeWaitTimeout = TimeSpan.FromSeconds(10L);

	/// <summary>Timeout for waiting for map screen.</summary>
	public static readonly TimeSpan mapScreenTimeout = TimeSpan.FromSeconds(10L);

	/// <summary>Polling interval for condition checks.</summary>
	public static readonly TimeSpan pollingInterval = TimeSpan.FromMilliseconds(100L, 0L);

	/// <summary>Delay between button interactions.</summary>
	public static readonly TimeSpan buttonClickDelay = TimeSpan.FromMilliseconds(100L, 0L);

	/// <summary>Maximum floor to play to (floor 49 is the final boss).</summary>
	public const int maxFloor = 49;

	/// <summary>Watchdog timeout. If no progress for this long, dump state and fail.</summary>
	public static readonly TimeSpan watchdogTimeout = TimeSpan.FromSeconds(30L);

	/// <summary>Interval for periodic state logging when stuck detection is active.</summary>
	public static readonly TimeSpan watchdogLogInterval = TimeSpan.FromSeconds(5L);
}
