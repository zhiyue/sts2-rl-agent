using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Migrations;

namespace MegaCrit.Sts2.Core.Saves.Managers;

public class RunSaveManager
{
	public const string runSaveFileName = "current_run.save";

	public const string multiplayerRunSaveFileName = "current_run_mp.save";

	private readonly ISaveStore _saveStore;

	private readonly MigrationManager _migrationManager;

	private readonly bool _forceSynchronous;

	private readonly IProfileIdProvider _profileIdProvider;

	private string CurrentRunSavePath => GetRunSavePath(_profileIdProvider.CurrentProfileId, "current_run.save");

	private string CurrentMultiplayerRunSavePath => GetRunSavePath(_profileIdProvider.CurrentProfileId, "current_run_mp.save");

	public bool HasRunSave => _saveStore.FileExists(CurrentRunSavePath);

	public bool HasMultiplayerRunSave => _saveStore.FileExists(CurrentMultiplayerRunSavePath);

	public int SchemaVersion => _migrationManager.GetLatestVersion<SerializableRun>();

	public event Action? Saved;

	public RunSaveManager(int profileId, ISaveStore saveStore, MigrationManager migrationManager, bool forceSynchronous = false)
		: this(saveStore, migrationManager, new StaticProfileIdProvider(profileId), forceSynchronous)
	{
	}

	public RunSaveManager(ISaveStore saveStore, MigrationManager migrationManager, IProfileIdProvider profileIdProvider, bool forceSynchronous = false)
	{
		_saveStore = saveStore;
		_migrationManager = migrationManager;
		_forceSynchronous = forceSynchronous;
		_profileIdProvider = profileIdProvider;
	}

	public async Task SaveRun(AbstractRoom? preFinishedRoom)
	{
		if (!RunManager.Instance.ShouldSave || (RunManager.Instance.NetService.Type != NetGameType.Singleplayer && RunManager.Instance.NetService.Type != NetGameType.Host))
		{
			return;
		}
		SerializableRun value = RunManager.Instance.ToSave(preFinishedRoom);
		string savePath = (RunManager.Instance.NetService.Type.IsMultiplayer() ? CurrentMultiplayerRunSavePath : CurrentRunSavePath);
		using MemoryStream stream = new MemoryStream();
		if (!_forceSynchronous)
		{
			await JsonSerializer.SerializeAsync((Stream)stream, value, JsonSerializationUtility.GetTypeInfo<SerializableRun>(), default(CancellationToken));
			stream.Seek(0L, SeekOrigin.Begin);
			await _saveStore.WriteFileAsync(savePath, stream.ToArray());
		}
		else
		{
			JsonSerializer.Serialize(stream, value, JsonSerializationUtility.GetTypeInfo<SerializableRun>());
			stream.Seek(0L, SeekOrigin.Begin);
			_saveStore.WriteFile(savePath, stream.ToArray());
		}
		this.Saved?.Invoke();
	}

	public ReadSaveResult<SerializableRun> LoadRunSave()
	{
		ReadSaveResult<SerializableRun> readSaveResult = _migrationManager.LoadSave<SerializableRun>(CurrentRunSavePath);
		if (readSaveResult.Success)
		{
			return readSaveResult;
		}
		if (readSaveResult.Status == ReadSaveStatus.FileNotFound)
		{
			Log.Info("Run save file not found at " + CurrentRunSavePath);
		}
		else if (!readSaveResult.Status.IsRecoverable())
		{
			Log.Error($"Failed to load run save: status={readSaveResult.Status} msg={readSaveResult.ErrorMessage}");
		}
		else
		{
			Log.Warn($"Run save had recoverable issues: status={readSaveResult.Status} msg={readSaveResult.ErrorMessage}");
		}
		return readSaveResult;
	}

	public ReadSaveResult<SerializableRun> LoadMultiplayerRunSave()
	{
		ReadSaveResult<SerializableRun> readSaveResult = _migrationManager.LoadSave<SerializableRun>(CurrentMultiplayerRunSavePath);
		if (readSaveResult.Success)
		{
			return readSaveResult;
		}
		if (readSaveResult.Status == ReadSaveStatus.FileNotFound)
		{
			Log.Info("Multiplayer run save file not found at " + CurrentMultiplayerRunSavePath);
		}
		else if (!readSaveResult.Status.IsRecoverable())
		{
			Log.Error($"Failed to load multiplayer run save: status={readSaveResult.Status} msg={readSaveResult.ErrorMessage}");
		}
		else
		{
			Log.Warn($"Multiplayer run save had recoverable issues: status={readSaveResult.Status} msg={readSaveResult.ErrorMessage}");
		}
		return readSaveResult;
	}

	public ReadSaveResult<SerializableRun> LoadAndCanonicalizeMultiplayerRunSave(ulong localPlayerId)
	{
		ReadSaveResult<SerializableRun> readSaveResult = LoadMultiplayerRunSave();
		if (readSaveResult != null && readSaveResult.Success && readSaveResult.SaveData != null)
		{
			try
			{
				SerializableRun data = RunManager.CanonicalizeSave(readSaveResult.SaveData, localPlayerId);
				return new ReadSaveResult<SerializableRun>(data, ReadSaveStatus.Success);
			}
			catch (Exception value)
			{
				Log.Error($"Multiplayer run save validation failed: {value}");
				RenameBrokenMultiplayerRunSave(ReadSaveStatus.ValidationFailed);
				return new ReadSaveResult<SerializableRun>(ReadSaveStatus.ValidationFailed, $"Save file validation failed: {value}");
			}
		}
		return readSaveResult;
	}

	public void DeleteCurrentRun()
	{
		_saveStore.DeleteFile(CurrentRunSavePath);
	}

	public void DeleteCurrentMultiplayerRun()
	{
		_saveStore.DeleteFile(CurrentMultiplayerRunSavePath);
	}

	public void RenameBrokenMultiplayerRunSave(ReadSaveStatus status)
	{
		try
		{
			if (HasMultiplayerRunSave)
			{
				string text = CorruptFileHandler.GenerateCorruptFilePath(CurrentMultiplayerRunSavePath, status);
				_saveStore.RenameFile(CurrentMultiplayerRunSavePath, text);
				Log.Error($"Corrupt multiplayer run save detected: Renamed '{CurrentMultiplayerRunSavePath}' to '{text}'");
			}
		}
		catch (Exception ex)
		{
			Log.Warn("Failed to rename broken multiplayer run save: " + ex.Message);
		}
	}

	public static string GetRunSavePath(int profileId, string fileName)
	{
		return Path.Combine(UserDataPathProvider.GetProfileDir(profileId), UserDataPathProvider.SavesDir, fileName);
	}
}
