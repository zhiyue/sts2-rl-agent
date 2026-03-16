using System;

namespace MegaCrit.Sts2.Core.AutoSlay;

public static class AutoSlayConfig
{
	public static readonly TimeSpan runTimeout = TimeSpan.FromMinutes(25L);

	public static readonly TimeSpan defaultRoomTimeout = TimeSpan.FromMinutes(2L);

	public static readonly TimeSpan defaultScreenTimeout = TimeSpan.FromSeconds(30L);

	public static readonly TimeSpan gameInitTimeout = TimeSpan.FromSeconds(10L);

	public static readonly TimeSpan runStateTimeout = TimeSpan.FromSeconds(30L);

	public static readonly TimeSpan nodeWaitTimeout = TimeSpan.FromSeconds(10L);

	public static readonly TimeSpan mapScreenTimeout = TimeSpan.FromSeconds(10L);

	public static readonly TimeSpan pollingInterval = TimeSpan.FromMilliseconds(100L, 0L);

	public static readonly TimeSpan buttonClickDelay = TimeSpan.FromMilliseconds(100L, 0L);

	public const int maxFloor = 49;

	public static readonly TimeSpan watchdogTimeout = TimeSpan.FromSeconds(30L);

	public static readonly TimeSpan watchdogLogInterval = TimeSpan.FromSeconds(5L);
}
