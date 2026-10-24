using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// An implementation of ISaveStore which saves files to both local and cloud storage.
/// This is the main entry point for cloud saves. You should not use any cloud-enabled save stores directly - instead,
/// write and read through this abstraction layer.
/// When files are read, they are always read from local storage. SyncCloudToLocal should be called once at the beginning
/// of the game's lifecycle before any files are read so that you are reading the up-to-date data.
///
/// CLOUD ERROR POLICY: All cloud operations are best-effort. A cloud failure must never prevent local saves from
/// working or the game from starting. Exception handlers catch broadly (Exception, not specific types) because
/// SteamRemoteSaveStore methods are P/Invoke calls into Valve's native steam_api DLL, which can throw SEHException
/// at any time in addition to the managed exceptions (InvalidOperationException, SteamRemoteSaveStoreException)
/// that our wrapper code throws.
/// </summary>
public class CloudSaveStore : ICloudSaveStore, ISaveStore
{
	public ISaveStore LocalStore { get; }

	public ICloudSaveStore CloudStore { get; }

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="localStore">The class which will be used to save to local storage.</param>
	/// <param name="cloudStore">The class which will be used to save to cloud storage.</param>
	public CloudSaveStore(ISaveStore localStore, ICloudSaveStore cloudStore)
	{
		LocalStore = localStore;
		CloudStore = cloudStore;
	}

	/// <summary>
	/// Reads a file from local storage.
	/// </summary>
	public string? ReadFile(string path)
	{
		return LocalStore.ReadFile(path);
	}

	/// <summary>
	/// Reads a file asynchronously from local storage.
	/// </summary>
	public Task<string?> ReadFileAsync(string path)
	{
		return LocalStore.ReadFileAsync(path);
	}

	/// <summary>
	/// Checks if a file exists in local storage.
	/// </summary>
	public bool FileExists(string path)
	{
		return LocalStore.FileExists(path);
	}

	/// <summary>
	/// Checks if a directory exists in local storage.
	/// </summary>
	public bool DirectoryExists(string path)
	{
		return LocalStore.DirectoryExists(path);
	}

	/// <summary>
	/// Writes a file synchronously to both local and remote storage.
	/// </summary>
	public void WriteFile(string path, string content)
	{
		LocalStore.WriteFile(path, content);
		try
		{
			CloudStore.WriteFile(path, content);
			SyncLocalTimestamp(path);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex.Message);
			SyncLocalTimestamp(path);
		}
	}

	/// <summary>
	/// Writes a file synchronously to both local and remote storage.
	/// </summary>
	public void WriteFile(string path, byte[] bytes)
	{
		LocalStore.WriteFile(path, bytes);
		try
		{
			CloudStore.WriteFile(path, bytes);
			SyncLocalTimestamp(path);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex.Message);
			SyncLocalTimestamp(path);
		}
	}

	/// <summary>
	/// Writes a file asynchronously to both local and remote storage.
	/// </summary>
	public async Task WriteFileAsync(string path, string content)
	{
		await LocalStore.WriteFileAsync(path, content);
		try
		{
			await CloudStore.WriteFileAsync(path, content);
			SyncLocalTimestamp(path);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex.Message);
			SyncLocalTimestamp(path);
		}
	}

	/// <summary>
	/// Writes a file asynchronously to both local and remote storage.
	/// </summary>
	public async Task WriteFileAsync(string path, byte[] bytes)
	{
		await LocalStore.WriteFileAsync(path, bytes);
		try
		{
			await CloudStore.WriteFileAsync(path, bytes);
			SyncLocalTimestamp(path);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex.Message);
			SyncLocalTimestamp(path);
		}
	}

	/// <summary>
	/// Deletes a file from both local and remote storage.
	/// </summary>
	public void DeleteFile(string path)
	{
		LocalStore.DeleteFile(path);
		try
		{
			CloudStore.DeleteFile(path);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud delete failed for " + path + ", local delete preserved: " + ex.Message);
		}
	}

	/// <summary>
	/// Renames a file in both local and remote storage.
	/// Try to avoid using this, Steam's remote storage doesn't allow atomic renaming.
	/// </summary>
	public void RenameFile(string sourcePath, string destinationPath)
	{
		LocalStore.RenameFile(sourcePath, destinationPath);
		try
		{
			CloudStore.RenameFile(sourcePath, destinationPath);
		}
		catch (Exception ex)
		{
			Log.Warn($"Cloud rename failed for {sourcePath} -> {destinationPath}, local rename preserved: {ex.Message}");
		}
	}

	/// <summary>
	/// Returns the files in the directory from local storage.
	/// </summary>
	public string[] GetFilesInDirectory(string directoryPath)
	{
		return LocalStore.GetFilesInDirectory(directoryPath);
	}

	/// <summary>
	/// Returns the directories in the directory read from local storage.
	/// </summary>
	public string[] GetDirectoriesInDirectory(string directoryPath)
	{
		return LocalStore.GetDirectoriesInDirectory(directoryPath);
	}

	/// <summary>
	/// Creates a directory in both local and remote storage.
	/// </summary>
	public void CreateDirectory(string directoryPath)
	{
		LocalStore.CreateDirectory(directoryPath);
		try
		{
			CloudStore.CreateDirectory(directoryPath);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud create directory failed for " + directoryPath + ": " + ex.Message);
		}
	}

	/// <summary>
	/// Deletes a directory and any remaining contents in both local and remote storage.
	/// </summary>
	public void DeleteDirectory(string directoryPath)
	{
		LocalStore.DeleteDirectory(directoryPath);
		try
		{
			CloudStore.DeleteDirectory(directoryPath);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud delete directory failed for " + directoryPath + ": " + ex.Message);
		}
	}

	/// <summary>
	/// Deletes temporary files from both local and remote storage.
	/// </summary>
	public void DeleteTemporaryFiles(string directoryPath)
	{
		LocalStore.DeleteTemporaryFiles(directoryPath);
		try
		{
			CloudStore.DeleteTemporaryFiles(directoryPath);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud delete temporary files failed for " + directoryPath + ": " + ex.Message);
		}
	}

	/// <summary>
	/// Gets the last modified time of a file in local storage.
	/// </summary>
	public DateTimeOffset GetLastModifiedTime(string path)
	{
		return LocalStore.GetLastModifiedTime(path);
	}

	/// <summary>
	/// Gets the size of a file without reading the entire file.
	/// </summary>
	public int GetFileSize(string path)
	{
		return LocalStore.GetFileSize(path);
	}

	/// <summary>
	/// Sets the last modified time of a file in local storage.
	/// </summary>
	public void SetLastModifiedTime(string path, DateTimeOffset time)
	{
		LocalStore.SetLastModifiedTime(path, time);
	}

	/// <summary>
	/// Returns the full path of a file in local storage.
	/// </summary>
	public string GetFullPath(string filename)
	{
		return LocalStore.GetFullPath(filename);
	}

	/// <summary>
	/// Checks if the cloud storage has any files stored.
	/// </summary>
	public bool HasCloudFiles()
	{
		return CloudStore.HasCloudFiles();
	}

	/// <summary>
	/// Removes a file from remote storage, but keeps it in the local storage.
	/// </summary>
	public void ForgetFile(string path)
	{
		try
		{
			CloudStore.ForgetFile(path);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud forget failed for " + path + ": " + ex.Message);
		}
	}

	/// <summary>
	/// Returns true if the file at the path is in remote storage.
	/// The file can also be in local storage, but this is disregarded.
	/// </summary>
	public bool IsFilePersisted(string path)
	{
		return CloudStore.IsFilePersisted(path);
	}

	/// <inheritdoc />
	public void BeginSaveBatch()
	{
		CloudStore.BeginSaveBatch();
	}

	/// <inheritdoc />
	public void EndSaveBatch()
	{
		CloudStore.EndSaveBatch();
	}

	/// <summary>
	/// Synchronizes a file from the remote to local storage.
	/// If the file exists on remote storage and the last-modified timestamp differs at all from the local one, then the
	/// local file will be overwritten with the one from remote storage.
	/// If the file doesn't exist on remote storage but does locally, then the local file will be deleted.
	/// </summary>
	public async Task SyncCloudToLocal(string path)
	{
		try
		{
			await SyncCloudToLocalInternal(path);
		}
		catch (Exception ex)
		{
			Log.Warn("SteamRemoteStorage: Failed to sync " + path + " from cloud, skipping: " + ex.Message);
			SentryService.CaptureException(ex);
		}
	}

	private async Task SyncCloudToLocalInternal(string path)
	{
		bool flag = CloudStore.FileExists(path);
		bool flag2 = LocalStore.FileExists(path);
		if (flag)
		{
			DateTimeOffset lastModifiedTime = CloudStore.GetLastModifiedTime(path);
			DateTimeOffset? dateTimeOffset = (flag2 ? new DateTimeOffset?(LocalStore.GetLastModifiedTime(path)) : ((DateTimeOffset?)null));
			bool flag3 = !flag2 || lastModifiedTime != dateTimeOffset;
			if (!flag3)
			{
				string value = LocalStore.ReadFile(path);
				if (string.IsNullOrWhiteSpace(value))
				{
					Log.Warn("Local file " + path + " appears corrupt (empty content) despite matching cloud timestamp, forcing re-download from cloud");
					flag3 = true;
				}
			}
			if (flag3)
			{
				Log.Info($"Copying {path} from cloud to local. Local file exists: {flag2} Cloud save time: {lastModifiedTime} Local save time: {dateTimeOffset}");
				string text = await CloudStore.ReadFileAsync(path);
				if (string.IsNullOrWhiteSpace(text) || text[0] == '\0')
				{
					Log.Warn("Cloud file " + path + " has empty content, skipping download");
					return;
				}
				await LocalStore.WriteFileAsync(path, text);
				SyncLocalTimestamp(path);
			}
			else
			{
				Log.Debug($"Skipping sync for {path}, last modified time matches on local and remote ({lastModifiedTime})");
			}
		}
		else if (flag2)
		{
			Log.Info("Deleting " + path + " because it does not exist on remote");
			LocalStore.DeleteFile(path);
			LocalStore.DeleteFile(path + ".backup");
		}
		else
		{
			Log.Debug("Skipping sync for " + path + ", it doesn't exist on either local or cloud");
		}
	}

	/// <summary>
	/// Synchronizes an entire directory from cloud to local storage.
	/// The rules for <see cref="M:MegaCrit.Sts2.Core.Saves.CloudSaveStore.SyncCloudToLocal(System.String)" /> are followed for every file found in the given directory, first for
	/// those found on the cloud, then for those found on the local side (if any files existed locally but not in the
	/// cloud).
	/// </summary>
	public IEnumerable<Task> SyncCloudToLocalDirectory(string directoryPath)
	{
		Log.Debug("Syncing all files in " + directoryPath + " from cloud to local");
		HashSet<string> filePathsRead = new HashSet<string>();
		string[] array = Array.Empty<string>();
		try
		{
			if (CloudStore.DirectoryExists(directoryPath))
			{
				array = CloudStore.GetFilesInDirectory(directoryPath);
			}
		}
		catch (Exception ex)
		{
			Log.Warn("Failed to list cloud files in " + directoryPath + ", skipping cloud sync: " + ex.Message);
			SentryService.CaptureException(ex);
		}
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!ShouldSyncFileToCloud(text))
			{
				DeleteStaleBackupFromCloud(directoryPath, text);
				continue;
			}
			string text2 = directoryPath + "/" + text;
			filePathsRead.Add(text2);
			Log.Debug("Checking file " + text2 + " in cloud saves");
			yield return SyncCloudToLocal(text2);
		}
		if (!LocalStore.DirectoryExists(directoryPath))
		{
			yield break;
		}
		array2 = LocalStore.GetFilesInDirectory(directoryPath);
		foreach (string text3 in array2)
		{
			if (ShouldSyncFileToCloud(text3))
			{
				string text4 = directoryPath + "/" + text3;
				if (!filePathsRead.Contains(text4))
				{
					Log.Debug("Checking file " + text4 + " in local saves");
					yield return SyncCloudToLocal(text4);
				}
			}
		}
	}

	/// <summary>
	/// Overwrites the state of a file in the cloud with the state of a file on the local side.
	/// This is used when the player launches the game for the first time with cloud sync enabled.
	/// If the file exists locally, then the file on the cloud is replaced unconditionally. If the file doesn't exist
	/// locally but exists on the cloud, the file is deleted from the cloud.
	/// </summary>
	/// <param name="path">The relative path to the file.</param>
	/// <param name="forgetImmediately">If this is true, then after the file is written to the cloud, then it is forgotten
	/// immediately. See comments in <see cref="M:MegaCrit.Sts2.Core.Saves.CloudSaveStore.OverwriteCloudWithLocalDirectory(System.String,System.Nullable{System.Int32},System.Nullable{System.Int32})" /> for when you might use this.</param>
	public async Task OverwriteCloudWithLocal(string path, bool forgetImmediately = false)
	{
		if (LocalStore.FileExists(path))
		{
			Log.Debug("Writing file " + path + " to cloud");
			string content = await LocalStore.ReadFileAsync(path);
			try
			{
				await CloudStore.WriteFileAsync(path, content);
				if (forgetImmediately)
				{
					try
					{
						Log.Debug("Immediately forgetting " + path);
						CloudStore.ForgetFile(path);
					}
					catch (Exception ex)
					{
						Log.Warn("Cloud forget failed for " + path + ": " + ex.Message);
					}
				}
				SyncLocalTimestamp(path);
				return;
			}
			catch (Exception ex2)
			{
				Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex2.Message);
				SyncLocalTimestamp(path);
				return;
			}
		}
		try
		{
			if (CloudStore.FileExists(path))
			{
				Log.Debug("Deleting file " + path + " from cloud because it doesn't exist on local");
				CloudStore.DeleteFile(path);
			}
		}
		catch (Exception ex3)
		{
			Log.Warn("Cloud delete failed for " + path + ": " + ex3.Message);
		}
	}

	/// <summary>
	/// Overwrites the state of a directory in the cloud with the state of a directory on the local side.
	/// This is used when the player launches the game for the first time with cloud sync enabled.
	/// The rules for <see cref="M:MegaCrit.Sts2.Core.Saves.CloudSaveStore.SyncCloudToLocal(System.String)" /> are followed for every file found in the given directory, first for
	/// those found on the cloud, then for those found on the local side (if any files existed locally but not in the
	/// cloud).
	///
	/// Files are written in order of last-modified time. Once we exceed either byteLimit or fileLimit, then files are
	/// written to the cloud storage, but they are forgotten from the remote cloud storage (not deleted, just forgotten).
	///
	/// So why write-and-forget? Why not just... not write? Later, when we go to sync cloud files to the local storage,
	/// we need the files to exist in the remote storage. Otherwise, we'll delete the files in the local storage.
	/// </summary>
	/// <param name="directoryPath">The directory to sync.</param>
	/// <param name="byteLimit">The maximum number of bytes that can be written to storage.</param>
	/// <param name="fileLimit">The maximum number of files that can be written to storage.</param>
	public IEnumerable<Task> OverwriteCloudWithLocalDirectory(string directoryPath, int? byteLimit, int? fileLimit)
	{
		Log.Debug("Writing all files in directory " + directoryPath + " to cloud");
		HashSet<string> filePathsRead = new HashSet<string>();
		string[] array = Array.Empty<string>();
		try
		{
			if (CloudStore.DirectoryExists(directoryPath))
			{
				array = CloudStore.GetFilesInDirectory(directoryPath);
			}
		}
		catch (Exception ex)
		{
			Log.Warn("Failed to list cloud files in " + directoryPath + ", skipping cloud delete sync: " + ex.Message);
			SentryService.CaptureException(ex);
		}
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!ShouldSyncFileToCloud(text))
			{
				DeleteStaleBackupFromCloud(directoryPath, text);
				continue;
			}
			filePathsRead.Add(text);
			yield return OverwriteCloudWithLocal(directoryPath + "/" + text);
		}
		if (!LocalStore.DirectoryExists(directoryPath))
		{
			yield break;
		}
		List<string> list = LocalStore.GetFilesInDirectory(directoryPath).ToList();
		int i = 0;
		int totalFilesWritten = 0;
		if (byteLimit.HasValue || fileLimit.HasValue)
		{
			list.Sort((string p1, string p2) => LocalStore.GetLastModifiedTime(directoryPath + "/" + p2).CompareTo(LocalStore.GetLastModifiedTime(directoryPath + "/" + p1)));
		}
		foreach (string item in list)
		{
			if (!filePathsRead.Contains(item) && ShouldSyncFileToCloud(item))
			{
				string path = directoryPath + "/" + item;
				int bytesToWrite = LocalStore.GetFileSize(path);
				bool flag = (byteLimit.HasValue && i + bytesToWrite > byteLimit.Value) || (fileLimit.HasValue && totalFilesWritten + 1 > fileLimit.Value);
				if (flag)
				{
					Log.Info($"File {item} will be immediately forgotten after writing to cloud. Bytes written:{i + bytesToWrite}. Files written: {totalFilesWritten + 1}");
				}
				yield return OverwriteCloudWithLocal(path, flag);
				i += bytesToWrite;
				totalFilesWritten++;
			}
		}
	}

	/// <summary>
	/// Forgets the oldest files that would cause us to go over the byte/file limit quotas.
	/// This is used when the player is writing a new run history file, which might exceed a limit that we set on the
	/// count/size of run histories.
	/// </summary>
	/// <param name="directoryPath">The directory to sync.</param>
	/// <param name="bytesToBeWritten">The number bytes that will be written to the new run history file.</param>
	/// <param name="byteLimit">The maximum number of bytes that can be written to storage.</param>
	/// <param name="fileLimit">The maximum number of files that can be written to storage.</param>
	public void ForgetFilesInDirectoryBeforeWritingIfNecessary(string directoryPath, int bytesToBeWritten, int byteLimit, int fileLimit)
	{
		try
		{
			ForgetFilesInDirectoryBeforeWritingIfNecessaryInternal(directoryPath, bytesToBeWritten, byteLimit, fileLimit);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud quota management failed for " + directoryPath + ": " + ex.Message);
			SentryService.CaptureException(ex);
		}
	}

	private void ForgetFilesInDirectoryBeforeWritingIfNecessaryInternal(string directoryPath, int bytesToBeWritten, int byteLimit, int fileLimit)
	{
		int num = bytesToBeWritten;
		int num2 = 1;
		string[] filesInDirectory = CloudStore.GetFilesInDirectory(directoryPath);
		List<string> list = new List<string>();
		string[] array = filesInDirectory;
		foreach (string text in array)
		{
			if (ShouldSyncFileToCloud(text))
			{
				string text2 = directoryPath + "/" + text;
				if (CloudStore.IsFilePersisted(text2))
				{
					list.Add(text2);
					num += CloudStore.GetFileSize(text2);
					num2++;
				}
			}
		}
		if (num > byteLimit || num2 > fileLimit)
		{
			list.Sort((string p1, string p2) => GetLastModifiedTime(p2).CompareTo(GetLastModifiedTime(p1)));
			while (num > byteLimit || num2 > fileLimit)
			{
				string text3 = list[list.Count - 1];
				num -= CloudStore.GetFileSize(text3);
				num2--;
				Log.Info($"Forgetting file {text3} from cloud storage because we're past our quota. Bytes after forgetting: {num}. Files after forgetting: {num2}");
				CloudStore.ForgetFile(text3);
				list.RemoveAt(list.Count - 1);
			}
		}
	}

	/// <summary>
	/// Returns true if the file should be included in cloud sync operations. Filters out .backup files,
	/// which are local crash-recovery artifacts created by CopyBackup. These should never be uploaded to
	/// or synced from cloud storage.
	/// </summary>
	private static bool ShouldSyncFileToCloud(string fileName)
	{
		return !fileName.EndsWith(".backup");
	}

	/// <summary>
	/// Removes a .backup file that should never have been uploaded to cloud storage. These are local-only
	/// artifacts that were uploaded before OverwriteCloudWithLocalDirectory filtered them. Cleaning them
	/// up frees cloud quota.
	/// </summary>
	private void DeleteStaleBackupFromCloud(string directoryPath, string cloudPath)
	{
		string text = directoryPath + "/" + cloudPath;
		Log.Info("Removing stale .backup file from cloud: " + text);
		try
		{
			CloudStore.DeleteFile(text);
		}
		catch (Exception ex)
		{
			Log.Warn("Failed to remove .backup from cloud " + text + ": " + ex.Message);
		}
	}

	/// <summary>
	/// Syncs the local file's last modified time to the cloud file's last modified time.
	/// This is best-effort: on Windows, the file can become temporarily unavailable (antivirus, cloud sync tools, file
	/// system contention) between the write and the timestamp update. A failed sync just means the next cloud sync might
	/// redundantly re-copy the file, which is harmless.
	///
	/// Called in both the success path (to sync timestamps after a cloud write) and the failure path (to prevent
	/// SyncCloudToLocal from overwriting local with stale cloud data on next startup).
	///
	/// Catches Exception broadly (not just IOException) because CloudStore.GetLastModifiedTime is a P/Invoke call
	/// that can throw several exception types (SEHException, InvalidOperationException, etc.). The consequence of
	/// not catching broadly here is a game crash, which is not acceptable for a timestamp sync failure.
	/// </summary>
	private void SyncLocalTimestamp(string path)
	{
		try
		{
			LocalStore.SetLastModifiedTime(path, CloudStore.GetLastModifiedTime(path));
		}
		catch (Exception ex)
		{
			Log.Warn("Failed to sync timestamp for " + path + ", will re-sync on next launch: " + ex.Message);
		}
	}

	public bool HasUserEnabledCloudSync()
	{
		return CloudStore.HasUserEnabledCloudSync();
	}
}
