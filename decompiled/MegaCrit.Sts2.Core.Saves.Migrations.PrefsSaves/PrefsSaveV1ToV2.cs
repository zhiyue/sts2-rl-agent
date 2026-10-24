namespace MegaCrit.Sts2.Core.Saves.Migrations.PrefsSaves;

/// <summary>
/// No-op migration for release save wipe. All pre-release saves are invalidated
/// via minimum supported version bump.
/// </summary>
[Migration(typeof(PrefsSave), 1, 2)]
public class PrefsSaveV1ToV2 : MigrationBase<PrefsSave>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
	}
}
