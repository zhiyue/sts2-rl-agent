using System;
using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.SourceGeneration;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

[GenerateSubtypes(DynamicallyAccessedMemberTypes = DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public interface IMigration
{
	int FromVersion { get; }

	int ToVersion { get; }

	Type SaveType { get; }

	MigratingData Migrate(MigratingData saveData);
}
public interface IMigration<T> : IMigration where T : ISaveSchema
{
}
