namespace MegaCrit.Sts2.Core.Saves.Migrations;

public class MigrationPathGapException : InvalidMigrationPathException
{
	public MigrationPathGapException(string message)
		: base(message)
	{
	}
}
