namespace MegaCrit.Sts2.Core.Saves.Migrations.SettingsSaves;

[Migration(typeof(SettingsSave), 4, 5)]
public class SettingsSaveV4ToV5 : MigrationBase<SettingsSave>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
		saveData.Remove("disabled_mods");
	}
}
