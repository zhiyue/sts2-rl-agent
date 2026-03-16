using System;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class MigrationAttribute : Attribute
{
	public Type SaveType { get; }

	public int FromVersion { get; }

	public int ToVersion { get; }

	public MigrationAttribute(Type saveType, int fromVersion, int toVersion)
	{
		SaveType = saveType;
		FromVersion = fromVersion;
		ToVersion = toVersion;
	}
}
