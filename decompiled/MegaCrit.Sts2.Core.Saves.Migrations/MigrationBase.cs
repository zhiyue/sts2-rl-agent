using System;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

/// <summary>
/// Base class for migrations that automatically derives migration properties from the Migration attribute
/// and uses JsonObject for migration operations.
/// </summary>
/// <typeparam name="T">The save type to migrate</typeparam>
public abstract class MigrationBase<T> : IMigration<T>, IMigration where T : ISaveSchema
{
	private readonly Lazy<MigrationAttribute> _migrationAttribute;

	/// <summary>
	/// The version to migrate from. Derived from the Migration attribute.
	/// </summary>
	public int FromVersion => _migrationAttribute.Value.FromVersion;

	/// <summary>
	/// The version to migrate to. Derived from the Migration attribute.
	/// </summary>
	public int ToVersion => _migrationAttribute.Value.ToVersion;

	/// <summary>
	/// The type of save that this migration operates on. Derived from the Migration attribute.
	/// </summary>
	public Type SaveType => _migrationAttribute.Value.SaveType;

	protected MigrationBase()
	{
		_migrationAttribute = new Lazy<MigrationAttribute>(delegate
		{
			object[] customAttributes = GetType().GetCustomAttributes(typeof(MigrationAttribute), inherit: false);
			if (customAttributes.Length == 0)
			{
				throw new InvalidOperationException(GetType().Name + " is missing the [Migration] attribute");
			}
			return (MigrationAttribute)customAttributes[0];
		});
	}

	/// <summary>
	/// Migrates the JSON object from the old version to the new version.
	/// </summary>
	/// <param name="saveData">The JSON object to migrate</param>
	/// <returns>The migrated JSON object</returns>
	public MigratingData Migrate(MigratingData saveData)
	{
		ApplyMigration(saveData);
		saveData.Set("schema_version", ToVersion);
		return saveData;
	}

	/// <summary>
	/// Implement this in derived classes to modify the JSON object.
	/// </summary>
	/// <param name="saveData">The JSON object to modify</param>
	protected abstract void ApplyMigration(MigratingData saveData);
}
