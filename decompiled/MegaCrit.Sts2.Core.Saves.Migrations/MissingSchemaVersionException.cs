using System;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

public class MissingSchemaVersionException : Exception
{
	public MissingSchemaVersionException(string message)
		: base(message)
	{
	}
}
