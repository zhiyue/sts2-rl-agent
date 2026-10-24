namespace MegaCrit.Sts2.Core.Saves.Migrations.SerializableRuns;

/// <summary>
/// No-op migration for release save wipe. All pre-release saves are invalidated
/// via minimum supported version bump.
/// </summary>
[Migration(typeof(SerializableRun), 12, 13)]
public class SerializableRunV12ToV13 : MigrationBase<SerializableRun>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
	}
}
