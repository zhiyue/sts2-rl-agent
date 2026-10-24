using System;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

/// <summary>
/// Base exception for migration path validation failures.
/// </summary>
public class InvalidMigrationPathException : Exception
{
	protected InvalidMigrationPathException(string message)
		: base(message)
	{
	}
}
