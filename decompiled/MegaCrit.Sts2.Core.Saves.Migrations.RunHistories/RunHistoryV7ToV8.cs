using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Saves.Migrations.RunHistories;

/// <summary>
/// No-op migration for release save wipe. All pre-release saves are invalidated
/// via minimum supported version bump.
/// </summary>
[Migration(typeof(RunHistory), 7, 8)]
public class RunHistoryV7ToV8 : MigrationBase<RunHistory>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
	}
}
