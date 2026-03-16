namespace MegaCrit.Sts2.Core.Saves.Migrations;

public class DuplicateMigrationException : InvalidMigrationPathException
{
	public DuplicateMigrationException(string message)
		: base(message)
	{
	}
}
