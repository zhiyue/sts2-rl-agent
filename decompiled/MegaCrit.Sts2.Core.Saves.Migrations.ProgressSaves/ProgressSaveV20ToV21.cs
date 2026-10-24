namespace MegaCrit.Sts2.Core.Saves.Migrations.ProgressSaves;

/// <summary>
/// No-op migration for release save wipe. All pre-release saves are invalidated
/// via minimum supported version bump.
/// </summary>
[Migration(typeof(SerializableProgress), 20, 21)]
public class ProgressSaveV20ToV21 : MigrationBase<SerializableProgress>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
	}
}
