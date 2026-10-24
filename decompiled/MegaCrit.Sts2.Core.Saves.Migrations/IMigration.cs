using System;
using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.SourceGeneration;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

/// <summary>
/// Base interface for all migrations.
/// </summary>
[GenerateSubtypes(DynamicallyAccessedMemberTypes = DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public interface IMigration
{
	/// <summary>
	/// The version to migrate from.
	/// </summary>
	int FromVersion { get; }

	/// <summary>
	/// The version to migrate to.
	/// </summary>
	int ToVersion { get; }

	/// <summary>
	/// The type of save that this migration operates on.
	/// </summary>
	Type SaveType { get; }

	/// <summary>
	/// Migrates the JsonObject from the old version to the new version.
	/// </summary>
	/// <param name="saveData">The JsonObject to migrate</param>
	/// <returns>The migrated JsonObject</returns>
	MigratingData Migrate(MigratingData saveData);
}
/// <summary>
/// Strongly typed interface for migrations that operate on a specific save type.
/// </summary>
/// <typeparam name="T">The save type to migrate</typeparam>
public interface IMigration<T> : IMigration where T : ISaveSchema
{
}
