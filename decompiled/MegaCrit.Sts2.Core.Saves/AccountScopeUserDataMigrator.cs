using System.IO;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Saves;

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
