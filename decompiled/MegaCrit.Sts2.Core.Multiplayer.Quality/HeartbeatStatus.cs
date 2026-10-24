namespace MegaCrit.Sts2.Core.Multiplayer.Quality;

/// <summary>
/// Represents the status of a heartbeat message. See <see cref="T:MegaCrit.Sts2.Core.Multiplayer.Quality.NetQualityTracker" />.
/// </summary>
public struct HeartbeatStatus
{
	/// <summary>
	/// The heartbeat index this is tracking.
	/// </summary>
	public int counter;

	/// <summary>
	/// The time at which this heartbeat was sent.
	/// </summary>
	public ulong sentMsec;

	/// <summary>
	/// The time at which this heartbeat was received, or null if it has not been received.
	/// </summary>
	public ulong? receivedMsec;
}
