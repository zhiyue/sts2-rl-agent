using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Saves.Migrations.SerializableRuns;

/// <summary>
/// Added GameMode. It defaults to None for error-checking purposes, but for existing real runs, it should default to
/// something reasonable.
/// </summary>
[Migration(typeof(SerializableRun), 14, 15)]
public class SerializableRunV14ToV15 : MigrationBase<SerializableRun>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
		Log.Info("SerializableRun migration v14 -> v15: Adding GameMode to save files, initialized as Standard");
		DateTimeOffset? asOrNull = saveData.GetAsOrNull<DateTimeOffset>("dailyTime");
		List<SerializableModifier> list = saveData.GetAs<List<SerializableModifier>>("modifiers");
		if (asOrNull.HasValue)
		{
			saveData.Set("game_mode", GameMode.Daily);
		}
		else if (list != null && list.Count > 0)
		{
			saveData.Set("game_mode", GameMode.Custom);
		}
		else
		{
			saveData.Set("game_mode", GameMode.Standard);
		}
	}
}
