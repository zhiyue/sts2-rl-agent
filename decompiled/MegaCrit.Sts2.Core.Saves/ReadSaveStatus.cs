namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Status codes for save file reading operations.
/// </summary>
public enum ReadSaveStatus
{
	/// <summary>
	/// The operation succeeded.
	/// </summary>
	Success,
	/// <summary>
	/// The JSON could not be parsed.
	/// </summary>
	JsonParseError,
	/// <summary>
	/// The file was not found.
	/// </summary>
	FileNotFound,
	/// <summary>
	/// The file was empty.
	/// </summary>
	FileEmpty,
	/// <summary>
	/// The migration failed.
	/// </summary>
	MigrationFailed,
	/// <summary>
	/// The schema version is missing from the save file.
	/// </summary>
	MissingSchemaVersion,
	/// <summary>
	/// The save file version is newer than the current version.
	/// </summary>
	FutureVersion,
	/// <summary>
	/// The save file version is too old to migrate.
	/// </summary>
	VersionTooOld,
	/// <summary>
	/// The save required migration and was migrated in memory.
	/// </summary>
	MigrationRequired,
	/// <summary>
	/// The JSON had errors that were automatically repaired.
	/// </summary>
	JsonRepaired,
	/// <summary>
	/// Data was recovered but some information may be lost.
	/// </summary>
	RecoveredWithDataLoss,
	/// <summary>
	/// File access error (I/O error, permissions, etc).
	/// </summary>
	FileAccessError,
	/// <summary>
	/// The save is completely unrecoverable.
	/// </summary>
	Unrecoverable,
	/// <summary>
	/// The save file loaded successfully but failed content validation.
	/// </summary>
	ValidationFailed
}
