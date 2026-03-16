using System;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

public class InvalidMigrationPathException : Exception
{
	protected InvalidMigrationPathException(string message)
		: base(message)
	{
	}
}
