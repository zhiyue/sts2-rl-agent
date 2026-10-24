namespace MegaCrit.Sts2.Core.Saves.Migrations;

/// <summary>
/// Exception thrown when there are duplicate migrations from the same version.
/// </summary>
public class DuplicateMigrationException : InvalidMigrationPathException
{
	public DuplicateMigrationException(string message)
		: base(message)
	{
	}
}
