using System.IO;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// TEMPORARY: This class handles the one-time migration of user data from the legacy unscoped
/// directory structure to the new user-scoped directory structure introduced for playtesters.
///
/// This migrator is designed to help existing playtesters transition their save data, replays,
/// and console history to the new platform-specific, user-scoped directory structure without
/// losing their progress.
///
/// This class should be removed a few months after the user-scoped directory feature has been
/// released to playtesters, once we're confident that all active users have migrated their data.
///
/// The migration moves data from:
///   - user://saves/       -&gt; user://{platform}/{userId}/profile1/saves/
///   - user://replays/     -&gt; user://{platform}/{userId}/profile1/replays/
///   - user://console_history.log -&gt; user://{platform}/{userId}/profile1/console_history.log
///
/// Note: Logs are intentionally NOT migrated as they remain in the legacy location (user://logs/)
/// since Godot controls the log file creation.
/// </summary>
public static class AccountScopeUserDataMigrator
{
	private static bool _migrationPerformed;

	public static void MigrateToUserScopedDirectories()
	{
		if (!HasLegacyData())
		{
			Log.VeryDebug("No legacy unscoped data found, skipping migration");
			return;
		}
		Log.Info("Starting migration of legacy unscoped data to user-scoped directories");
		string legacyBasePath = ProjectSettings.GlobalizePath("user://");
		string text = ProjectSettings.GlobalizePath(UserDataPathProvider.GetProfileScopedBasePath(1));
		Directory.CreateDirectory(text);
		bool flag = MigrationUtil.MigrateDirectory("saves", legacyBasePath, text);
		bool flag2 = MigrationUtil.MigrateDirectory("replays", legacyBasePath, text);
		bool flag3 = MigrationUtil.MigrateFile("console_history.log", legacyBasePath, text);
		_migrationPerformed = flag || flag2 || flag3;
		if (_migrationPerformed)
		{
			Log.Info("Migration to user-scoped directories completed successfully");
		}
		else
		{
			Log.Info("No items were migrated (all destinations already existed)");
		}
	}

	private static bool HasLegacyData()
	{
		string path = ProjectSettings.GlobalizePath("user://");
		if (!Directory.Exists(Path.Combine(path, "saves")) && !Directory.Exists(Path.Combine(path, "replays")))
		{
			return File.Exists(Path.Combine(path, "console_history.log"));
		}
		return true;
	}

	public static void ArchiveLegacyData()
	{
		if (!_migrationPerformed)
		{
			Log.VeryDebug("No migration was performed, skipping archival of legacy data");
		}
		else if (HasLegacyData())
		{
			Log.Info("Archiving legacy unscoped data after successful migration");
			string path = ProjectSettings.GlobalizePath("user://");
			string text = Path.Combine(path, "legacy_backup");
			if (Directory.Exists(text))
			{
				Log.Warn("Deleting legacy data archive that already exists");
				Directory.Delete(text, recursive: true);
			}
			Directory.CreateDirectory(text);
			MigrationUtil.ArchiveLegacyDirectory(Path.Combine(path, "saves"), text);
			MigrationUtil.ArchiveLegacyDirectory(Path.Combine(path, "replays"), text);
			MigrationUtil.ArchiveLegacyFile(Path.Combine(path, "console_history.log"), text);
			Log.Info("Legacy data archived to 'legacy_backup' folder");
		}
	}
}
