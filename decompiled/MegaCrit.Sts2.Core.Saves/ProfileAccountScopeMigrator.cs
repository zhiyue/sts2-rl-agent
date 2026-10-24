using System.IO;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// TEMPORARY: This class handles the one-time migration of user data from before profiles existed.
///
/// This migrator is designed to help existing playtesters transition their save data, replays,
/// and console history to the new platform-specific, user-scoped directory structure without
/// losing their progress.
///
/// This class should be removed a few months after the user profile feature has been
/// released to playtesters, once we're confident that all active users have migrated their data.
///
/// The migration moves all data from user://{platform}/{userId}/saves/ -&gt; user://{platform}/{userId}/profile1/saves/
/// </summary>
public static class ProfileAccountScopeMigrator
{
	private static bool _migrationPerformed;

	public static void MigrateToProfileScopedDirectories()
	{
		if (!HasLegacyData())
		{
			Log.VeryDebug("No legacy unscoped data found, skipping migration");
			return;
		}
		Log.Info("Starting migration of account-scoped data to profile-scoped directories");
		string text = ProjectSettings.GlobalizePath(UserDataPathProvider.GetAccountScopedBasePath(""));
		string text2 = ProjectSettings.GlobalizePath(UserDataPathProvider.GetProfileScopedBasePath(1));
		Directory.CreateDirectory(text2);
		bool flag = MigrationUtil.MigrateFile("settings.save", text + "/saves", text);
		bool flag2 = MigrationUtil.MigrateDirectory("saves", text, text2);
		bool flag3 = MigrationUtil.MigrateDirectory("replays", text, text2);
		bool flag4 = MigrationUtil.MigrateFile("console_history.log", text, text2);
		_migrationPerformed = flag || flag2 || flag3 || flag4;
		if (_migrationPerformed)
		{
			Log.Info("Migration to profile-scoped directories completed successfully");
		}
		else
		{
			Log.Info("No items were migrated (all destinations already existed)");
		}
	}

	private static bool HasLegacyData()
	{
		if (!DirAccess.DirExistsAbsolute(UserDataPathProvider.GetAccountScopedBasePath("saves")))
		{
			return DirAccess.DirExistsAbsolute(UserDataPathProvider.GetAccountScopedBasePath("replays"));
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
			Log.Info("Archiving account-scoped data after successful profile-scoped migration");
			string path = ProjectSettings.GlobalizePath(UserDataPathProvider.GetAccountScopedBasePath(""));
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
			string path2 = ProjectSettings.GlobalizePath(UserDataPathProvider.GetProfileScopedBasePath(1));
			File.Delete(Path.Combine(path2, "saves/settings.save"));
			Log.Info("Legacy data archived to 'legacy_backup' folder");
		}
	}
}
