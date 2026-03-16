namespace MegaCrit.Sts2.Core.Saves;

public static class ReadSaveStatusExtensions
{
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
