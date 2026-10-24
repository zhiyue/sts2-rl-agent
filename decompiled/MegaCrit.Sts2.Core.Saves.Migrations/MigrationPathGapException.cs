namespace MegaCrit.Sts2.Core.Saves.Migrations;

/// <summary>
/// Exception thrown when there are gaps in the migration path.
/// </summary>
public class MigrationPathGapException : InvalidMigrationPathException
{
	public MigrationPathGapException(string message)
		: base(message)
	{
	}
}
