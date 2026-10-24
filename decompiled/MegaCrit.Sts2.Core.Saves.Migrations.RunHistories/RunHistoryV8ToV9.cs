using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Migrations.Shared;

namespace MegaCrit.Sts2.Core.Saves.Migrations.RunHistories;

/// <summary>
/// Migrates renamed/deleted ModelIds in completed run history saves.
/// Same renames as SerializableRunV15ToV16, applied to the RunHistory schema.
/// </summary>
[Migration(typeof(RunHistory), 8, 9)]
public class RunHistoryV8ToV9 : MigrationBase<RunHistory>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
		Log.Info("RunHistory migration v8 -> v9: Migrating renamed/deleted ModelIds");
		SharedMigrationHelper.ReplaceModelIds(saveData.GetRawNode(), SharedMigrationHelper.V100Renames);
	}
}
