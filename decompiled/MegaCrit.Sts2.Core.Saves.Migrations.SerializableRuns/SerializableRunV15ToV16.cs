using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves.Migrations.Shared;

namespace MegaCrit.Sts2.Core.Saves.Migrations.SerializableRuns;

/// <summary>
/// Migrates renamed/deleted ModelIds:
/// - CARD.PREPARE -&gt; CARD.PREPARED (class Prepare renamed back to Prepared)
/// - ENCOUNTER.TOADPOLES_NORMAL -&gt; ENCOUNTER.SEAPUNK_NORMAL (class ToadpolesNormal renamed to SeapunkNormal)
/// - MONSTER.DOOR -&gt; MONSTER.DEPRECATED_MONSTER (Door monster deleted in Doormaker rework)
/// </summary>
[Migration(typeof(SerializableRun), 15, 16)]
public class SerializableRunV15ToV16 : MigrationBase<SerializableRun>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
		Log.Info("SerializableRun migration v15 -> v16: Migrating renamed/deleted ModelIds");
		SharedMigrationHelper.ReplaceModelIds(saveData.GetRawNode(), SharedMigrationHelper.V100Renames);
	}
}
