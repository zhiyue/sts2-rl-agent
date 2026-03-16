namespace MegaCrit.Sts2.Core.Saves;

public enum ReadSaveStatus
{
	Success,
	JsonParseError,
	FileNotFound,
	FileEmpty,
	MigrationFailed,
	MissingSchemaVersion,
	FutureVersion,
	VersionTooOld,
	MigrationRequired,
	JsonRepaired,
	RecoveredWithDataLoss,
	FileAccessError,
	Unrecoverable,
	ValidationFailed
}
