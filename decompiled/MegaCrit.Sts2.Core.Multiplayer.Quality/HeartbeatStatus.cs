namespace MegaCrit.Sts2.Core.Multiplayer.Quality;

public struct HeartbeatStatus
{
	public int counter;

	public ulong sentMsec;

	public ulong? receivedMsec;
}
