namespace MegaCrit.Sts2.Core.Saves.Migrations.SettingsSaves;

/// <summary>
/// No-op migration for release save wipe. All pre-release saves are invalidated
/// via minimum supported version bump.
/// </summary>
[Migration(typeof(SettingsSave), 3, 4)]
public class SettingsSaveV3ToV4 : MigrationBase<SettingsSave>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
	}
}
