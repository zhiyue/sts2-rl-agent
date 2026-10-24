using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Platform.Steam;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.Metrics;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Saves.Migrations;
using MegaCrit.Sts2.Core.Saves.Test;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;
using Steamworks;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Manages saving and loading game data, including settings, progress, current runs, and run history.
/// Implements the Singleton pattern for global access to save functionality.
/// </summary>
/// <remarks>
/// The SaveManager coordinates multiple specialized save managers:
/// - <see cref="T:MegaCrit.Sts2.Core.Saves.Managers.SettingsSaveManager" />: Handles user settings (not cloud synced)
/// - <see cref="T:MegaCrit.Sts2.Core.Saves.Managers.ProgressSaveManager" />: Manages global game progress and statistics
/// - <see cref="T:MegaCrit.Sts2.Core.Saves.Managers.RunSaveManager" />: Handles saving/loading of active runs
/// - <see cref="T:MegaCrit.Sts2.Core.Saves.Managers.RunHistorySaveManager" />: Manages historical run data
///
/// This specialized manager architecture improves testability by allowing each save manager
/// to be tested independently with appropriate mocks and stubs.
///
/// Save files are stored in a user-scoped, platform-specific location:
/// - In editor: C:\Users\{USER}\AppData\Roaming\SlayTheSpire2\{platform}\{userId}\saves
/// - In builds: C:\Users\{USER}\AppData\Roaming\Godot\app_userdata\sts2\{platform}\{userId}\saves
/// </remarks>
public class SaveManager : IProfileIdProvider
{
	private static SaveManager? _mockInstance;

	private static SaveManager? _instance;

	private readonly SettingsSaveManager _settingsSaveManager;

	private readonly ProgressSaveManager _progressSaveManager;

	private readonly RunSaveManager _runSaveManager;

	private readonly RunHistorySaveManager _runHistorySaveManager;

	private readonly PrefsSaveManager _prefsSaveManager;

	private readonly ProfileSaveManager _profileSaveManager;

	private readonly ISaveStore _saveStore;

	private readonly MigrationManager _migrationManager;

	private int? _currentProfileId;

	public const int totalAgnosticUnlocks = 18;

	/// <summary>
	/// The agnostic epoch IDs in unlock order.
	/// Used by <see cref="M:MegaCrit.Sts2.Core.Saves.SaveManager.GetEpochIdForUnlock" /> and <see cref="M:MegaCrit.Sts2.Core.Saves.SaveManager.GetRevealableEpochs" /> to enforce prerequisite ordering.
	/// </summary>
	private static readonly string[] _agnosticEpochUnlockOrder = new string[18]
	{
		EpochModel.GetId<Colorless1Epoch>(),
		EpochModel.GetId<Relic1Epoch>(),
		EpochModel.GetId<Potion1Epoch>(),
		EpochModel.GetId<UnderdocksEpoch>(),
		EpochModel.GetId<Colorless2Epoch>(),
		EpochModel.GetId<Relic2Epoch>(),
		EpochModel.GetId<Potion2Epoch>(),
		EpochModel.GetId<Act2BEpoch>(),
		EpochModel.GetId<Colorless3Epoch>(),
		EpochModel.GetId<Relic3Epoch>(),
		EpochModel.GetId<Act3BEpoch>(),
		EpochModel.GetId<Colorless4Epoch>(),
		EpochModel.GetId<Relic4Epoch>(),
		EpochModel.GetId<Event1Epoch>(),
		EpochModel.GetId<Colorless5Epoch>(),
		EpochModel.GetId<Relic5Epoch>(),
		EpochModel.GetId<Event2Epoch>(),
		EpochModel.GetId<Event3Epoch>()
	};

	public static SaveManager Instance
	{
		get
		{
			if (_mockInstance != null)
			{
				return _mockInstance;
			}
			if (_instance == null)
			{
				_instance = ConstructDefault();
			}
			return _instance;
		}
	}

	public SettingsSave SettingsSave => _settingsSaveManager.Settings;

	public PrefsSave PrefsSave => _prefsSaveManager.Prefs;

	public ProgressState Progress
	{
		get
		{
			return _progressSaveManager.Progress;
		}
		set
		{
			_progressSaveManager.Progress = value;
		}
	}

	public bool HasRunSave => _runSaveManager.HasRunSave;

	public bool HasMultiplayerRunSave => _runSaveManager.HasMultiplayerRunSave;

	public bool IsProfileInitialized => _currentProfileId.HasValue;

	public int CurrentProfileId => _currentProfileId ?? throw new InvalidOperationException("InitProfileId must be called on SaveManager!");

	/// <summary>
	/// If the SaveManager is in the middle of saving the run, then this is non-null and not completed.
	/// </summary>
	public Task? CurrentRunSaveTask { get; private set; }

	public event Action? Saved
	{
		add
		{
			_runSaveManager.Saved += value;
		}
		remove
		{
			_runSaveManager.Saved -= value;
		}
	}

	public event Action<int>? ProfileIdChanged;

	public static void MockInstanceForTesting(SaveManager saveManager)
	{
		_mockInstance = saveManager;
	}

	public static void ClearInstanceForTesting()
	{
		_mockInstance = null;
	}

	/// <summary>
	/// Constructor with dependency injection support.
	/// </summary>
	/// <param name="saveStore">The file I/O backend to use.</param>
	/// <param name="forceSynchronous">Force all operations to be performed synchronously. Only use in tests.</param>
	public SaveManager(ISaveStore saveStore, bool forceSynchronous = false)
		: this(saveStore, saveStore, forceSynchronous)
	{
	}

	/// <summary>
	/// Constructor with a separate local-only store for settings.
	/// Settings are machine-specific (display, controller, window) and must not be cloud-synced.
	/// </summary>
	public SaveManager(ISaveStore saveStore, ISaveStore localOnlyStore, bool forceSynchronous = false)
	{
		_saveStore = saveStore;
		_migrationManager = new MigrationManager(saveStore);
		_settingsSaveManager = new SettingsSaveManager(localOnlyStore, _migrationManager);
		_profileSaveManager = new ProfileSaveManager(saveStore, _migrationManager);
		_prefsSaveManager = new PrefsSaveManager(saveStore, _migrationManager, this);
		_progressSaveManager = new ProgressSaveManager(saveStore, _migrationManager, this);
		_runSaveManager = new RunSaveManager(saveStore, _migrationManager, this, forceSynchronous);
		_runHistorySaveManager = new RunHistorySaveManager(saveStore, _migrationManager, this);
	}

	/// <summary>
	/// Constructs the default SaveManager for this platform and configuration.
	///  - If we are currently in test mode, then this returns a SaveManager that doesn't actually save to disk.
	///  - If cloud saves are enabled, then this returns a SaveManager that saves both to cloud and to disk for the
	///    appropriate save backend.
	/// </summary>
	private static SaveManager ConstructDefault()
	{
		ISaveStore saveStore;
		if (TestMode.IsOn)
		{
			saveStore = new MockGodotFileIo("user://test");
			return new SaveManager(saveStore);
		}
		ISaveStore saveStore2 = new GodotFileIo(UserDataPathProvider.GetAccountScopedBasePath(null));
		saveStore = saveStore2;
		if (SteamInitializer.Initialized)
		{
			Log.Info($"Steam is enabled, we will write saves to steam storage. Enabled for account: {SteamRemoteStorage.IsCloudEnabledForAccount()}, app: {SteamRemoteStorage.IsCloudEnabledForApp()} ");
			SteamRemoteSaveStore cloudStore = new SteamRemoteSaveStore();
			CloudSaveStore cloudSaveStore = new CloudSaveStore(saveStore2, cloudStore);
			saveStore = cloudSaveStore;
		}
		return new SaveManager(saveStore, saveStore2);
	}

	/// <summary>
	/// Sets CurrentProfileId.
	/// This is called during the initialization of the game. It must be called after cloud sync has completed to allow
	/// the profile save to sync.
	/// </summary>
	/// <param name="profileId">The profile ID to use. If null, it will be read from file.</param>
	public void InitProfileId(int? profileId = null)
	{
		CleanupTemporaryFiles();
		if (!profileId.HasValue)
		{
			_profileSaveManager.LoadProfile();
			_currentProfileId = _profileSaveManager.Profile.LastProfileId;
		}
		else
		{
			_currentProfileId = profileId.Value;
		}
		string profileScopedBasePath = UserDataPathProvider.GetProfileScopedBasePath(CurrentProfileId);
		Log.Info("Profile-scoped data path initialized: " + profileScopedBasePath);
		_runHistorySaveManager.CreateRunHistoryDirectory();
	}

	public int GetLatestSchemaVersion<T>()
	{
		return _migrationManager.GetLatestVersion<T>();
	}

	public string GetProfileScopedPath(string userData)
	{
		return _saveStore.GetFullPath(Path.Combine(UserDataPathProvider.GetProfileDir(CurrentProfileId), userData));
	}

	/// <summary>
	/// Switches the current profile ID and saves it to the profile save file.
	/// Methods on the SaveManager will start reporting new data right after, but you will must call InitPrefsData and
	/// InitProgressData so that PrefsData and ProgressData will report old data.
	/// </summary>
	/// <param name="profileId">The profile ID to switch to.</param>
	public void SwitchProfileId(int profileId)
	{
		Log.Info($"Switching save profiles to {profileId}");
		_currentProfileId = profileId;
		_profileSaveManager.Profile.LastProfileId = profileId;
		SaveProfile();
		_runHistorySaveManager.CreateRunHistoryDirectory();
		this.ProfileIdChanged?.Invoke(profileId);
	}

	public SaveBatchScope BeginSaveBatch()
	{
		if (_saveStore is CloudSaveStore cloudSaveStore)
		{
			cloudSaveStore.BeginSaveBatch();
		}
		return new SaveBatchScope(this);
	}

	public void EndSaveBatch()
	{
		if (_saveStore is CloudSaveStore cloudSaveStore)
		{
			cloudSaveStore.EndSaveBatch();
		}
	}

	public async Task SaveRun(AbstractRoom? preFinishedRoom, bool saveProgress = true)
	{
		if (CurrentRunSaveTask != null)
		{
			await CurrentRunSaveTask;
		}
		using (BeginSaveBatch())
		{
			if (saveProgress)
			{
				SaveProgressFile();
			}
			try
			{
				CurrentRunSaveTask = _runSaveManager.SaveRun(preFinishedRoom);
				await CurrentRunSaveTask;
			}
			catch (Exception ex)
			{
				Log.Error($"Failed to save run: {ex}");
				SentryService.CaptureException(ex);
			}
			finally
			{
				CurrentRunSaveTask = null;
			}
		}
	}

	/// <summary>
	/// Increments and saves the NumReloads field of a save file.
	/// </summary>
	public async Task IncrementNumReloads(SerializableRun save, bool isMultiplayer)
	{
		if (CurrentRunSaveTask != null)
		{
			await CurrentRunSaveTask;
		}
		using (BeginSaveBatch())
		{
			_ = 1;
			try
			{
				save.NumReloads++;
				CurrentRunSaveTask = _runSaveManager.SaveRun(save, isMultiplayer);
				await CurrentRunSaveTask;
			}
			finally
			{
				CurrentRunSaveTask = null;
			}
		}
	}

	/// <summary>
	/// Called whenever the player wins, loses, or abandons run.
	/// Updates the progress.save file using the current_run.save file or similar.
	/// </summary>
	/// <param name="serializableRun">The serialized run data.</param>
	/// <param name="victory">Whether or not the run ended in a victory.</param>
	public void UpdateProgressWithRunData(SerializableRun serializableRun, bool victory)
	{
		_progressSaveManager.UpdateWithRunData(serializableRun, victory);
	}

	/// <summary>
	/// Called when the player wins a combat.
	/// </summary>
	public void UpdateProgressAfterCombatWon(Player localPlayer, CombatRoom combatRoom)
	{
		_progressSaveManager.UpdateAfterCombatWon(localPlayer, combatRoom);
	}

	public void DeleteCurrentRun()
	{
		_runSaveManager.DeleteCurrentRun();
	}

	public void DeleteCurrentMultiplayerRun()
	{
		_runSaveManager.DeleteCurrentMultiplayerRun();
	}

	public void DeleteProfile(int profileId)
	{
		string profileScopedBasePath = UserDataPathProvider.GetProfileScopedBasePath(profileId);
		Log.Info($"DELETING the profile id {profileId} at path {profileScopedBasePath}!!");
		DeleteDirectoryRecursive(UserDataPathProvider.GetProfileDir(profileId));
	}

	public void DeleteDirectoryRecursive(string directory)
	{
		DeleteInDirectoryRecursive(directory);
		Log.Info("Deleting directory at " + directory);
		_saveStore.DeleteDirectory(directory);
	}

	/// <summary>
	/// Recursively deletes all files and subdirectories within the given directory.
	/// </summary>
	/// <remarks>
	/// PRG-5240: When using cloud storage, we must also delete cloud-only files.
	/// Cloud-only files can exist when:
	/// - Old run history files were "forgotten" from cloud quota tracking but still exist on Steam servers
	/// - Files were uploaded from another device but not yet synced locally
	///
	/// If we only deleted local files, cloud-only files would be restored on next game launch
	/// by SyncCloudToLocal, causing deleted profile data (like old run history) to reappear.
	/// </remarks>
	private void DeleteInDirectoryRecursive(string directory)
	{
		string[] directoriesInDirectory = _saveStore.GetDirectoriesInDirectory(directory);
		foreach (string text in directoriesInDirectory)
		{
			string text2 = directory + "/" + text;
			DeleteInDirectoryRecursive(text2);
			Log.Info("Deleting directory at " + text2);
			_saveStore.DeleteDirectory(text2);
		}
		string[] filesInDirectory = _saveStore.GetFilesInDirectory(directory);
		foreach (string text3 in filesInDirectory)
		{
			string text4 = directory + "/" + text3;
			Log.Info("Deleting file at " + text4);
			_saveStore.DeleteFile(text4);
		}
		if (_saveStore is CloudSaveStore cloudSaveStore)
		{
			string[] filesInDirectory2 = cloudSaveStore.CloudStore.GetFilesInDirectory(directory);
			foreach (string text5 in filesInDirectory2)
			{
				string text6 = directory + "/" + text5;
				Log.Info("Deleting cloud-only file at " + text6);
				cloudSaveStore.CloudStore.DeleteFile(text6);
			}
		}
	}

	public void SaveSettings()
	{
		try
		{
			_settingsSaveManager.SaveSettings();
		}
		catch (Exception ex)
		{
			Log.Error($"Failed to save settings: {ex}");
			SentryService.CaptureException(ex);
		}
	}

	public void SaveProfile()
	{
		try
		{
			_profileSaveManager.SaveProfile();
		}
		catch (Exception ex)
		{
			Log.Error($"Failed to save profile: {ex}");
			SentryService.CaptureException(ex);
		}
	}

	/// <summary>
	/// Initializes a default settings file for testing purposes.
	/// This is separate from InitSettingsData because that is used directly in tests.
	/// </summary>
	public ReadSaveResult<SettingsSave> InitSettingsDataForTest()
	{
		_settingsSaveManager.Settings = new SettingsSave();
		return new ReadSaveResult<SettingsSave>(_settingsSaveManager.Settings);
	}

	/// <summary>
	/// Initializes a default prefs file for testing purposes.
	/// This is separate from InitPrefsData because that is used directly in tests.
	/// </summary>
	public ReadSaveResult<PrefsSave> InitPrefsDataForTest()
	{
		_prefsSaveManager.Prefs = new PrefsSave();
		return new ReadSaveResult<PrefsSave>(_prefsSaveManager.Prefs);
	}

	/// <summary>
	/// Loads the settings file for the first time. This should only be called once early on in the lifetime of the game.
	/// If the settings save could not be read because it was corrupt or did not exist, a new one is created.
	/// </summary>
	/// <returns>The result of reading the settings save file.</returns>
	public ReadSaveResult<SettingsSave> InitSettingsData()
	{
		return _settingsSaveManager.LoadSettings();
	}

	/// <summary>
	/// Loads the prefs file for the first time.
	/// This should be called once early on in the lifetime of the game, and then only when the player switches profiles
	/// after that. If the progress save could not be read because it was corrupt or did not exist, a new one is created.
	/// </summary>
	/// <returns>The result of reading the prefs save file.</returns>
	public ReadSaveResult<PrefsSave> InitPrefsData()
	{
		return _prefsSaveManager.LoadPrefs();
	}

	/// <summary>
	/// Loads the progress file for the first time.
	/// This should be called once early on in the lifetime of the game, and then only when the player switches profiles
	/// after that. If the progress save could not be read because it was corrupt or did not exist, a new one is created.
	/// </summary>
	/// <returns>The result of reading the progress save file.</returns>
	public ReadSaveResult<SerializableProgress> InitProgressData()
	{
		return _progressSaveManager.LoadProgress();
	}

	/// <summary>
	/// Synchronizes files that are cloud-saved in the background.
	/// When this is called, we copy all newer files from the cloud save backend to the user:// directory, overwriting
	/// the saves that are there.
	/// If there is no active cloud save backend, then this method does nothing.
	/// </summary>
	public async Task SyncCloudToLocal()
	{
		if (!(_saveStore is CloudSaveStore cloudStore))
		{
			return;
		}
		Log.Info("Syncing cloud save files to the local save directory");
		DeleteSettingsFromCloud(cloudStore);
		List<Task> tasks = new List<Task>();
		foreach (Task item in EnumerateCloudSyncTasks(cloudStore))
		{
			tasks.Add(item);
			if (tasks.Count >= 8)
			{
				await Task.WhenAll(tasks);
				tasks.Clear();
			}
		}
		if (tasks.Count > 0)
		{
			await Task.WhenAll(tasks);
		}
		CleanupStaleCurrentRunSaves();
	}

	private IEnumerable<Task> EnumerateCloudSyncTasks(CloudSaveStore cloudStore)
	{
		yield return cloudStore.SyncCloudToLocal(ProfileSaveManager.ProfilePath);
		for (int i = 1; i <= 3; i++)
		{
			yield return cloudStore.SyncCloudToLocal(ProgressSaveManager.GetProgressPathForProfile(i));
			yield return cloudStore.SyncCloudToLocal(RunSaveManager.GetRunSavePath(i, "current_run.save"));
			yield return cloudStore.SyncCloudToLocal(RunSaveManager.GetRunSavePath(i, "current_run_mp.save"));
			yield return cloudStore.SyncCloudToLocal(PrefsSaveManager.GetPrefsPath(i));
			foreach (Task item in cloudStore.SyncCloudToLocalDirectory(RunHistorySaveManager.GetHistoryPath(i)))
			{
				yield return item;
			}
		}
	}

	/// <summary>
	/// Deletes settings.save from cloud storage if it exists. Settings are machine-specific (display,
	/// controller, window) and were never intended to be cloud-synced. A stale cloud copy was uploaded
	/// by earlier versions and can cause sync conflicts when the local settings legitimately differ
	/// (e.g., different platform, different display configuration).
	/// </summary>
	private static void DeleteSettingsFromCloud(CloudSaveStore cloudStore)
	{
		try
		{
			if (cloudStore.CloudStore.FileExists("settings.save"))
			{
				Log.Info("Deleting stale settings.save from cloud storage");
				cloudStore.CloudStore.DeleteFile("settings.save");
			}
		}
		catch (Exception ex)
		{
			Log.Warn("Failed to delete settings from cloud: " + ex.Message);
		}
	}

	/// <summary>
	/// Removes current_run.save files that have already been saved to run history.
	/// This prevents stale saves from being restored after cloud sync issues (e.g., Steam Deck suspend).
	/// </summary>
	private void CleanupStaleCurrentRunSaves()
	{
		for (int i = 1; i <= 3; i++)
		{
			CleanupStaleCurrentRunSaveForProfile(i, "current_run.save");
			CleanupStaleCurrentRunSaveForProfile(i, "current_run_mp.save");
		}
	}

	/// <summary>
	/// Checks if a current_run.save file for a specific profile is stale (already exists in history) and deletes it.
	/// </summary>
	private void CleanupStaleCurrentRunSaveForProfile(int profileId, string runSaveFileName)
	{
		string runSavePath = RunSaveManager.GetRunSavePath(profileId, runSaveFileName);
		string text = runSavePath + ".backup";
		string text2 = null;
		if (_saveStore.FileExists(runSavePath))
		{
			text2 = runSavePath;
		}
		else if (_saveStore.FileExists(text))
		{
			text2 = text;
		}
		if (text2 == null)
		{
			return;
		}
		try
		{
			string text3 = _saveStore.ReadFile(text2);
			if (text3 == null)
			{
				Log.Warn("Could not read " + text2 + ", skipping staleness check");
				return;
			}
			long? value = ExtractStartTimeFromRunSave(text3);
			if (!value.HasValue)
			{
				Log.Warn("Could not extract start_time from " + text2 + ", skipping staleness check");
				return;
			}
			string text4 = Path.Combine(RunHistorySaveManager.GetHistoryPath(profileId), $"{value}.run");
			if (_saveStore.FileExists(text4))
			{
				Log.Warn($"Deleting stale {runSaveFileName} for profile {profileId}: run with StartTime {value} already exists in history at {text4}");
				_saveStore.DeleteFile(runSavePath);
				_saveStore.DeleteFile(runSavePath + ".backup");
			}
		}
		catch (Exception ex)
		{
			Log.Warn($"Error checking for stale current_run.save in profile {profileId}: {ex.Message}");
		}
	}

	/// <summary>
	/// Removes orphaned .tmp files from all save directories.
	/// These can be left behind if the game crashes or is force-killed during an async write.
	/// </summary>
	private void CleanupTemporaryFiles()
	{
		_saveStore.DeleteTemporaryFiles("");
		for (int i = 1; i <= 3; i++)
		{
			string directoryPath = Path.Combine(UserDataPathProvider.GetProfileDir(i), UserDataPathProvider.SavesDir);
			_saveStore.DeleteTemporaryFiles(directoryPath);
			_saveStore.DeleteTemporaryFiles(RunHistorySaveManager.GetHistoryPath(i));
		}
	}

	/// <summary>
	/// Extracts the start_time field from a serialized run save JSON without full deserialization.
	/// Returns null if the field cannot be extracted.
	/// </summary>
	private static long? ExtractStartTimeFromRunSave(string json)
	{
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(json);
			if (jsonDocument.RootElement.TryGetProperty("start_time", out var value))
			{
				return value.GetInt64();
			}
		}
		catch
		{
		}
		return null;
	}

	/// <summary>
	/// This returns true under the following circumstances:
	/// - If there is a cloud save backend with no files uploaded and we have save files locally
	/// - If the cloud save backend is present and has a local cache, but cloud sync is not enabled
	///
	/// If true is returned, then OverwriteCloudWithLocal should be called.
	/// If false is returned, then SyncCloudToLocal should be called.
	/// </summary>
	public bool ShouldOverwriteCloudWithLocal()
	{
		if (!(_saveStore is CloudSaveStore cloudSaveStore))
		{
			return false;
		}
		if (!cloudSaveStore.HasUserEnabledCloudSync())
		{
			return true;
		}
		if (!_saveStore.FileExists(ProfileSaveManager.ProfilePath))
		{
			return false;
		}
		if (cloudSaveStore.HasCloudFiles())
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// If any cloud backend is enabled, but there are no files uploaded to that backend and we have save files on our
	/// end, then this uploads all of our files to the cloud.
	/// If there is no active cloud save backend, then this method does nothing.
	/// This is used to upload all save files to the cloud for the first time when we enable a cloud save backend.
	/// </summary>
	public async Task OverwriteCloudWithLocal()
	{
		if (!(_saveStore is CloudSaveStore cloudStore))
		{
			return;
		}
		Log.Info("OVERWRITING cloud saves with local saves.");
		List<Task> tasks = new List<Task>();
		foreach (Task item in EnumerateOverwriteCloudWithLocalTasks(cloudStore))
		{
			tasks.Add(item);
			if (tasks.Count >= 8)
			{
				await Task.WhenAll(tasks);
				tasks.Clear();
			}
		}
		if (tasks.Count > 0)
		{
			await Task.WhenAll(tasks);
		}
	}

	private IEnumerable<Task> EnumerateOverwriteCloudWithLocalTasks(CloudSaveStore cloudStore)
	{
		yield return cloudStore.OverwriteCloudWithLocal(ProfileSaveManager.ProfilePath);
		for (int i = 1; i <= 3; i++)
		{
			yield return cloudStore.OverwriteCloudWithLocal(ProgressSaveManager.GetProgressPathForProfile(i));
			yield return cloudStore.OverwriteCloudWithLocal(RunSaveManager.GetRunSavePath(i, "current_run.save"));
			yield return cloudStore.OverwriteCloudWithLocal(RunSaveManager.GetRunSavePath(i, "current_run_mp.save"));
			yield return cloudStore.OverwriteCloudWithLocal(PrefsSaveManager.GetPrefsPath(i));
			foreach (Task item in cloudStore.OverwriteCloudWithLocalDirectory(RunHistorySaveManager.GetHistoryPath(i), 5242880, 100))
			{
				yield return item;
			}
		}
	}

	/// <summary>
	/// Load the current run save.
	/// </summary>
	/// <returns>The current run save, wrapped in a result object that contains some status info.</returns>
	public ReadSaveResult<SerializableRun> LoadRunSave()
	{
		return _runSaveManager.LoadRunSave();
	}

	/// <summary>
	/// Load and validate the current multiplayer run save.
	/// Performs deep validation to ensure all save content is valid and handles corruption automatically.
	/// If the save contains deprecated content, the model IDs are automatically replaced with the deprecated versions.
	/// </summary>
	/// <param name="localPlayerId">The local player ID to validate against</param>
	/// <returns>The current multiplayer run save with validation status</returns>
	public ReadSaveResult<SerializableRun> LoadAndCanonicalizeMultiplayerRunSave(ulong localPlayerId)
	{
		return _runSaveManager.LoadAndCanonicalizeMultiplayerRunSave(localPlayerId);
	}

	public void SaveRunHistory(RunHistory history)
	{
		try
		{
			_runHistorySaveManager.SaveHistory(history);
		}
		catch (Exception ex)
		{
			Log.Error($"Failed to save run history: {ex}");
			SentryService.CaptureException(ex);
		}
	}

	public int GetRunHistoryCount()
	{
		return _runHistorySaveManager.GetHistoryCount();
	}

	public List<string> GetAllRunHistoryNames()
	{
		return _runHistorySaveManager.LoadAllRunHistoryNames();
	}

	public ReadSaveResult<RunHistory> LoadRunHistory(string fileName)
	{
		return _runHistorySaveManager.LoadHistory(fileName);
	}

	public static string ToJson<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(T obj) where T : ISaveSchema
	{
		return JsonSerializationUtility.ToJson(obj);
	}

	public static ReadSaveResult<T> FromJson<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string json) where T : ISaveSchema, new()
	{
		return JsonSerializationUtility.FromJson<T>(json);
	}

	/// <summary>
	/// Returns true if all ftues are disabled OR if the given ftue key exists (seen by the player before).
	/// Is also disabled if there's no game (test mode)
	/// </summary>
	public bool SeenFtue(string ftueKey)
	{
		return _progressSaveManager.SeenFtue(ftueKey);
	}

	public bool SeenPopup(string popupKey)
	{
		return _progressSaveManager.SeenPopup(popupKey);
	}

	public void SaveProgressFile()
	{
		_progressSaveManager.SaveProgress();
	}

	/// <summary>
	/// Generates an unlock state from the current progress save.
	/// This object can be used to query what the player has unlocked.
	/// This should only be called from the main menu. When in a run, you should use the unlock state on <see cref="T:MegaCrit.Sts2.Core.Runs.RunManager" />
	/// or on Player. See the documentation in UnlockState for why.
	/// </summary>
	public UnlockState GenerateUnlockStateFromProgress()
	{
		return _progressSaveManager.GenerateUnlockState();
	}

	public void SavePrefsFile()
	{
		try
		{
			_prefsSaveManager.SavePrefs();
		}
		catch (Exception ex)
		{
			Log.Error($"Failed to save prefs: {ex}");
			SentryService.CaptureException(ex);
		}
	}

	public void MarkFtueAsComplete(string ftueId)
	{
		_progressSaveManager.MarkFtueAsComplete(ftueId);
	}

	public void SetFtuesEnabled(bool enabled)
	{
		_progressSaveManager.SetFtuesEnabled(enabled);
	}

	public void ResetFtues()
	{
		_progressSaveManager.ResetFtues();
	}

	public void MarkPotionAsSeen(PotionModel potion)
	{
		_progressSaveManager.MarkPotionAsSeen(potion);
	}

	public void MarkCardAsSeen(CardModel card)
	{
		_progressSaveManager.MarkCardAsSeen(card);
	}

	public void MarkRelicAsSeen(RelicModel relic)
	{
		_progressSaveManager.MarkRelicAsSeen(relic);
	}

	public bool IsRelicSeen(RelicModel relic)
	{
		return Progress.DiscoveredRelics.Contains(relic.Id);
	}

	/// <summary>
	/// Sets an Epoch Slot to be available but not revealed/obtained.
	/// </summary>
	public void UnlockSlot(string epochId)
	{
		Progress.UnlockSlot(epochId);
	}

	/// <summary>
	/// Sets or creates an Epoch to any EpochState we wish.
	/// Used by TimelineExpansions for overriding behaviors.
	/// </summary>
	public void ObtainEpoch(string epochId)
	{
		Progress.ObtainEpoch(epochId);
	}

	/// <summary>
	/// Sets or creates an Epoch to any EpochState we wish.
	/// Used by TimelineExpansions for overriding behaviors.
	/// </summary>
	public void ObtainEpochOverride(string epochId, EpochState state)
	{
		Progress.ObtainEpochOverride(epochId, state);
	}

	/// <summary>
	/// Reveals an Epoch. Sets an Epoch to IsComplete.
	/// Occurs when the player clicks on an Obtained Epoch in the Timeline screen.
	/// </summary>
	public void RevealEpoch(string epochId, bool isDebug = false)
	{
		Progress.RevealEpoch(epochId);
		if (!isDebug)
		{
			MetricUtilities.UploadEpochMetric(epochId);
		}
	}

	/// <summary>
	/// Called by the debug Reset Progress button in the Timeline screen.
	/// </summary>
	public void ResetTimelineProgress()
	{
		Progress.ResetEpochs();
		ObtainEpochOverride(EpochModel.GetId<NeowEpoch>(), EpochState.Obtained);
		SaveProgressFile();
	}

	/// <summary>
	/// Checks if an epoch has been revealed on the timeline.
	/// You should only be querying this from the main menu. When you are inside of a run, you should be using
	/// <see cref="P:MegaCrit.Sts2.Core.Runs.RunState.UnlockState" /> or <see cref="P:MegaCrit.Sts2.Core.Entities.Players.Player.UnlockState" />.
	/// </summary>
	public bool IsEpochRevealed<T>() where T : EpochModel
	{
		return Progress.IsEpochRevealed(EpochModel.GetId<T>());
	}

	/// <summary>
	/// Checks if an epoch has been revealed on the timeline.
	/// You should only be querying this from the main menu. When you are inside of a run, you should be using
	/// <see cref="P:MegaCrit.Sts2.Core.Runs.RunState.UnlockState" /> or <see cref="P:MegaCrit.Sts2.Core.Entities.Players.Player.UnlockState" />.
	/// </summary>
	public bool IsEpochRevealed(string id)
	{
		return Progress.IsEpochRevealed(id);
	}

	public int GetTotalUnlockedCards()
	{
		return GetCardUnlockEpochIds().Count(IsEpochRevealed) * 3;
	}

	public static int GetUnlockableCardCount()
	{
		return GetCardUnlockEpochIds().Length * 3;
	}

	/// <summary>
	/// Helper method which returns every Epoch that unlocks cards in the game.
	/// Modify this list to affect our total card unlock statistics.
	/// </summary>
	private static string[] GetCardUnlockEpochIds()
	{
		return new string[20]
		{
			EpochModel.GetId<Colorless1Epoch>(),
			EpochModel.GetId<Colorless2Epoch>(),
			EpochModel.GetId<Colorless3Epoch>(),
			EpochModel.GetId<Colorless4Epoch>(),
			EpochModel.GetId<Colorless5Epoch>(),
			EpochModel.GetId<Ironclad2Epoch>(),
			EpochModel.GetId<Ironclad5Epoch>(),
			EpochModel.GetId<Ironclad7Epoch>(),
			EpochModel.GetId<Silent2Epoch>(),
			EpochModel.GetId<Silent5Epoch>(),
			EpochModel.GetId<Silent7Epoch>(),
			EpochModel.GetId<Regent2Epoch>(),
			EpochModel.GetId<Regent5Epoch>(),
			EpochModel.GetId<Regent7Epoch>(),
			EpochModel.GetId<Defect2Epoch>(),
			EpochModel.GetId<Defect5Epoch>(),
			EpochModel.GetId<Defect7Epoch>(),
			EpochModel.GetId<Necrobinder2Epoch>(),
			EpochModel.GetId<Necrobinder5Epoch>(),
			EpochModel.GetId<Necrobinder7Epoch>()
		};
	}

	public int GetTotalUnlockedRelics()
	{
		return GetRelicUnlockEpochIds().Count(IsEpochRevealed) * 3;
	}

	public static int GetUnlockableRelicCount()
	{
		return GetRelicUnlockEpochIds().Length * 3;
	}

	/// <summary>
	/// Helper method which returns every Epoch that unlocks relics in the game.
	/// Modify this list to affect our total relic unlock statistics.
	/// </summary>
	private static string[] GetRelicUnlockEpochIds()
	{
		return new string[15]
		{
			EpochModel.GetId<Relic1Epoch>(),
			EpochModel.GetId<Relic2Epoch>(),
			EpochModel.GetId<Relic3Epoch>(),
			EpochModel.GetId<Relic4Epoch>(),
			EpochModel.GetId<Relic5Epoch>(),
			EpochModel.GetId<Ironclad3Epoch>(),
			EpochModel.GetId<Ironclad6Epoch>(),
			EpochModel.GetId<Silent3Epoch>(),
			EpochModel.GetId<Silent6Epoch>(),
			EpochModel.GetId<Regent3Epoch>(),
			EpochModel.GetId<Regent6Epoch>(),
			EpochModel.GetId<Defect3Epoch>(),
			EpochModel.GetId<Defect6Epoch>(),
			EpochModel.GetId<Necrobinder3Epoch>(),
			EpochModel.GetId<Necrobinder6Epoch>()
		};
	}

	public int GetTotalUnlockedPotions()
	{
		return GetPotionUnlockEpochIds().Count(IsEpochRevealed) * 3;
	}

	public static int GetUnlockablePotionCount()
	{
		return GetPotionUnlockEpochIds().Length * 3;
	}

	/// <summary>
	/// Helper method which returns every Epoch that unlocks relics in the game.
	/// Modify this list to affect our total relic unlock statistics.
	/// </summary>
	private static string[] GetPotionUnlockEpochIds()
	{
		return new string[7]
		{
			EpochModel.GetId<Potion1Epoch>(),
			EpochModel.GetId<Potion2Epoch>(),
			EpochModel.GetId<Ironclad4Epoch>(),
			EpochModel.GetId<Silent4Epoch>(),
			EpochModel.GetId<Regent4Epoch>(),
			EpochModel.GetId<Defect4Epoch>(),
			EpochModel.GetId<Necrobinder4Epoch>()
		};
	}

	/// <summary>
	/// Returns the sum of the Ascension progress of every character the player has.
	/// </summary>
	public int GetAggregateAscensionProgress()
	{
		return Progress.CharacterStats.Values.Sum((CharacterStats stat) => stat.MaxAscension);
	}

	public static int GetAggregateAscensionCount()
	{
		return ModelDb.AllCharacters.Count() * 10;
	}

	public int GetTotalKills()
	{
		return Progress.EnemyStats.Values.Sum((EnemyStats enemy) => enemy.TotalWins);
	}

	/// <summary>
	/// <see cref="M:MegaCrit.Sts2.Core.Saves.Managers.ProgressSaveManager.GetRevealableEpochs" />
	/// </summary>
	public IEnumerable<SerializableEpoch> GetRevealableEpochs()
	{
		return _progressSaveManager.GetRevealableEpochs();
	}

	/// <summary>
	/// Returns the number of Epochs we have that the player has discovered but not yet revealed in the Timeline.
	/// </summary>
	public int GetDiscoveredEpochCount()
	{
		return GetRevealableEpochs().Count();
	}

	public bool IsNeowDiscovered()
	{
		SerializableEpoch serializableEpoch = Progress.Epochs.FirstOrDefault((SerializableEpoch e) => e.Id == EpochModel.GetId<NeowEpoch>());
		if (serializableEpoch == null)
		{
			return false;
		}
		return serializableEpoch.State != EpochState.Revealed;
	}

	public int GetUnlocksRemaining()
	{
		return 18 - Progress.TotalUnlocks;
	}

	public int GetCurrentScore()
	{
		return Progress.CurrentScore;
	}

	/// <summary>
	/// Called whenever the score bar is filled.
	/// Increments TotalUnlocks and grants an Epoch.
	/// </summary>
	public string? IncrementUnlock()
	{
		Progress.TotalUnlocks++;
		return GetEpochIdForUnlock();
	}

	/// <summary>
	/// Look-up function to get an Epoch ID based on the
	/// player's TotalUnlocks (the score bar system at the end of run).
	/// Must match <see cref="M:MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen.NGameOverScreen.GetScoreThreshold(System.Int32)" />.
	/// </summary>
	private string? GetEpochIdForUnlock()
	{
		int num = Progress.TotalUnlocks - 1;
		if (num < 0 || num >= _agnosticEpochUnlockOrder.Length)
		{
			return null;
		}
		return _agnosticEpochUnlockOrder[num];
	}

	/// <summary>
	/// The player needs to complete any run to access the compendium or be a dev.
	/// </summary>
	public bool IsCompendiumAvailable()
	{
		if (Progress.NumberOfRuns <= 0)
		{
			return !NGame.IsReleaseGame();
		}
		return true;
	}
}
