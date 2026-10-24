using System;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

/// <summary>
/// Attribute used to mark a class as a migration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class MigrationAttribute : Attribute
{
	/// <summary>
	/// The type of save that this migration operates on.
	/// </summary>
	public Type SaveType { get; }

	/// <summary>
	/// The version to migrate from.
	/// </summary>
	public int FromVersion { get; }

	/// <summary>
	/// The version to migrate to.
	/// </summary>
	public int ToVersion { get; }

	/// <summary>
	/// Creates a new migration attribute.
	/// </summary>
	/// <param name="saveType">The type of save that this migration operates on</param>
	/// <param name="fromVersion">The version to migrate from</param>
	/// <param name="toVersion">The version to migrate to</param>
	public MigrationAttribute(Type saveType, int fromVersion, int toVersion)
	{
		SaveType = saveType;
		FromVersion = fromVersion;
		ToVersion = toVersion;
	}
}
