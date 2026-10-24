namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Extension methods for ReadSaveStatus.
/// </summary>
public static class ReadSaveStatusExtensions
{
	/// <summary>
	/// Determines if a save read status is recoverable.
	/// </summary>
	/// <param name="status">The read status to check</param>
	/// <returns>True if the status indicates a recoverable error or success</returns>
	public static bool IsRecoverable(this ReadSaveStatus status)
	{
		bool flag;
		switch (status)
		{
		case ReadSaveStatus.JsonParseError:
		case ReadSaveStatus.FileEmpty:
		case ReadSaveStatus.MigrationFailed:
		case ReadSaveStatus.FutureVersion:
		case ReadSaveStatus.VersionTooOld:
		case ReadSaveStatus.FileAccessError:
		case ReadSaveStatus.Unrecoverable:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		return !flag;
	}
}
