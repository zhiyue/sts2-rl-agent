using System;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

public abstract class MigrationBase<T> : IMigration<T>, IMigration where T : ISaveSchema
{
	private readonly Lazy<MigrationAttribute> _migrationAttribute;

	public int FromVersion => _migrationAttribute.Value.FromVersion;

	public int ToVersion => _migrationAttribute.Value.ToVersion;

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

	public MigratingData Migrate(MigratingData saveData)
	{
		ApplyMigration(saveData);
		saveData.Set("schema_version", ToVersion);
		return saveData;
	}

	protected abstract void ApplyMigration(MigratingData saveData);
}
