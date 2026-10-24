using System;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

/// <summary>
/// Exception thrown when a schema version is missing from a save file.
/// </summary>
public class MissingSchemaVersionException : Exception
{
	public MissingSchemaVersionException(string message)
		: base(message)
	{
	}
}
