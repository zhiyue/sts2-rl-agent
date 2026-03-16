using System.Collections.Generic;
using System.IO;
using System.Text;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Migrations;

namespace MegaCrit.Sts2.Core.Saves.Managers;

public class RunHistorySaveManager
{
	public const int maxCloudBytes = 5242880;

	public const int maxCloudFileCount = 100;

	private const string _historyDirName = "history";

	private readonly ISaveStore _saveStore;

	private readonly MigrationManager _migrationManager;

	private readonly IProfileIdProvider _profileIdProvider;

	private string HistoryPath => GetHistoryPath(_profileIdProvider.CurrentProfileId);

	public RunHistorySaveManager(int profileId, ISaveStore saveStore, MigrationManager migrationManager)
		: this(saveStore, migrationManager, new StaticProfileIdProvider(profileId))
	{
	}

	public RunHistorySaveManager(ISaveStore saveStore, MigrationManager migrationManager, IProfileIdProvider profileIdProvider)
	{
		_saveStore = saveStore;
		_migrationManager = migrationManager;
		_profileIdProvider = profileIdProvider;
	}

	public void CreateRunHistoryDirectory()
	{
		_saveStore.CreateDirectory(HistoryPath);
	}

	public static string GetHistoryPath(int profileId)
	{
		return Path.Combine(UserDataPathProvider.GetProfileDir(profileId), UserDataPathProvider.SavesDir, "history");
	}

	public void SaveHistory(RunHistory history)
	{
		history.SchemaVersion = _migrationManager.GetLatestVersion<RunHistory>();
		string content = JsonSerializationUtility.ToJson(history);
		string path = Path.Combine(HistoryPath, $"{history.StartTime}.run");
		SaveHistoryInternal(path, content);
		Log.Info($"Saved run history: {history.StartTime}.run");
	}

	private void SaveHistoryInternal(string path, string content)
	{
		if (_saveStore is CloudSaveStore cloudSaveStore)
		{
			int byteCount = Encoding.UTF8.GetByteCount(content);
			cloudSaveStore.ForgetFilesInDirectoryBeforeWritingIfNecessary(HistoryPath, byteCount, 5242880, 100);
		}
		_saveStore.WriteFile(path, content);
	}

	public int GetHistoryCount()
	{
		return _saveStore.GetFilesInDirectory(HistoryPath).Length;
	}

	public ReadSaveResult<RunHistory> LoadHistory(string fileName)
	{
		string filePath = Path.Combine(HistoryPath, fileName);
		ReadSaveResult<RunHistory> readSaveResult = _migrationManager.LoadSave<RunHistory>(filePath);
		if (readSaveResult.Success)
		{
			Log.Info("Successfully loaded run history: " + fileName);
		}
		else
		{
			Log.Warn($"Failed to load run history {fileName}: {readSaveResult.Status}");
		}
		return readSaveResult;
	}

	public List<string> LoadAllRunHistoryNames()
	{
		string[] filesInDirectory = _saveStore.GetFilesInDirectory(HistoryPath);
		int num = 0;
		List<string> list = new List<string>();
		string[] array = filesInDirectory;
		foreach (string text in array)
		{
			if (text.EndsWith(".corrupt"))
			{
				num++;
			}
			else
			{
				list.Add(text);
			}
		}
		if (num > 0)
		{
			Log.Warn($"Skipping {num} corrupt save files in history directory");
		}
		Log.Debug($"Found {list.Count} run history files");
		return list;
	}
}
