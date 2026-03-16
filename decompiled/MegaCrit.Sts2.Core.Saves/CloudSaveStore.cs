using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Saves;

public class CloudSaveStore : ICloudSaveStore, ISaveStore
{
	public ISaveStore LocalStore { get; }

	public ICloudSaveStore CloudStore { get; }

	public CloudSaveStore(ISaveStore localStore, ICloudSaveStore cloudStore)
	{
		LocalStore = localStore;
		CloudStore = cloudStore;
	}

	public string? ReadFile(string path)
	{
		return LocalStore.ReadFile(path);
	}

	public Task<string?> ReadFileAsync(string path)
	{
		return LocalStore.ReadFileAsync(path);
	}

	public bool FileExists(string path)
	{
		return LocalStore.FileExists(path);
	}

	public bool DirectoryExists(string path)
	{
		return LocalStore.DirectoryExists(path);
	}

	public void WriteFile(string path, string content)
	{
		LocalStore.WriteFile(path, content);
		try
		{
			CloudStore.WriteFile(path, content);
			LocalStore.SetLastModifiedTime(path, CloudStore.GetLastModifiedTime(path));
		}
		catch (InvalidOperationException ex)
		{
			Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex.Message);
		}
	}

	public void WriteFile(string path, byte[] bytes)
	{
		LocalStore.WriteFile(path, bytes);
		try
		{
			CloudStore.WriteFile(path, bytes);
			LocalStore.SetLastModifiedTime(path, CloudStore.GetLastModifiedTime(path));
		}
		catch (InvalidOperationException ex)
		{
			Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex.Message);
		}
	}

	public async Task WriteFileAsync(string path, string content)
	{
		await LocalStore.WriteFileAsync(path, content);
		try
		{
			await CloudStore.WriteFileAsync(path, content);
			LocalStore.SetLastModifiedTime(path, CloudStore.GetLastModifiedTime(path));
		}
		catch (InvalidOperationException ex)
		{
			Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex.Message);
		}
	}

	public async Task WriteFileAsync(string path, byte[] bytes)
	{
		await LocalStore.WriteFileAsync(path, bytes);
		try
		{
			await CloudStore.WriteFileAsync(path, bytes);
			LocalStore.SetLastModifiedTime(path, CloudStore.GetLastModifiedTime(path));
		}
		catch (InvalidOperationException ex)
		{
			Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex.Message);
		}
	}

	public void DeleteFile(string path)
	{
		LocalStore.DeleteFile(path);
		CloudStore.DeleteFile(path);
	}

	public void RenameFile(string sourcePath, string destinationPath)
	{
		LocalStore.RenameFile(sourcePath, destinationPath);
		CloudStore.RenameFile(sourcePath, destinationPath);
	}

	public string[] GetFilesInDirectory(string directoryPath)
	{
		return LocalStore.GetFilesInDirectory(directoryPath);
	}

	public string[] GetDirectoriesInDirectory(string directoryPath)
	{
		return LocalStore.GetDirectoriesInDirectory(directoryPath);
	}

	public void CreateDirectory(string directoryPath)
	{
		LocalStore.CreateDirectory(directoryPath);
		CloudStore.CreateDirectory(directoryPath);
	}

	public void DeleteDirectory(string directoryPath)
	{
		LocalStore.DeleteDirectory(directoryPath);
		CloudStore.DeleteDirectory(directoryPath);
	}

	public void DeleteTemporaryFiles(string directoryPath)
	{
		LocalStore.DeleteTemporaryFiles(directoryPath);
		CloudStore.DeleteTemporaryFiles(directoryPath);
	}

	public DateTimeOffset GetLastModifiedTime(string path)
	{
		return LocalStore.GetLastModifiedTime(path);
	}

	public int GetFileSize(string path)
	{
		return LocalStore.GetFileSize(path);
	}

	public void SetLastModifiedTime(string path, DateTimeOffset time)
	{
		LocalStore.SetLastModifiedTime(path, time);
	}

	public string GetFullPath(string filename)
	{
		return LocalStore.GetFullPath(filename);
	}

	public bool HasCloudFiles()
	{
		return CloudStore.HasCloudFiles();
	}

	public void ForgetFile(string path)
	{
		CloudStore.ForgetFile(path);
	}

	public bool IsFilePersisted(string path)
	{
		return CloudStore.IsFilePersisted(path);
	}

	public void BeginSaveBatch()
	{
		CloudStore.BeginSaveBatch();
	}

	public void EndSaveBatch()
	{
		CloudStore.EndSaveBatch();
	}

	public async Task SyncCloudToLocal(string path)
	{
		bool flag = CloudStore.FileExists(path);
		bool flag2 = LocalStore.FileExists(path);
		if (flag)
		{
			DateTimeOffset lastModifiedTime = CloudStore.GetLastModifiedTime(path);
			DateTimeOffset? dateTimeOffset = (flag2 ? new DateTimeOffset?(LocalStore.GetLastModifiedTime(path)) : ((DateTimeOffset?)null));
			if (!flag2 || lastModifiedTime != dateTimeOffset)
			{
				Log.Info($"Copying {path} from cloud to local. Local file exists: {flag2} Cloud save time: {lastModifiedTime} Local save time: {dateTimeOffset}");
				string content = await CloudStore.ReadFileAsync(path);
				await LocalStore.WriteFileAsync(path, content);
				LocalStore.SetLastModifiedTime(path, CloudStore.GetLastModifiedTime(path));
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
		}
		else
		{
			Log.Debug("Skipping sync for " + path + ", it doesn't exist on either local or cloud");
		}
	}

	public IEnumerable<Task> SyncCloudToLocalDirectory(string directoryPath)
	{
		Log.Debug("Syncing all files in " + directoryPath + " from cloud to local");
		HashSet<string> filePathsRead = new HashSet<string>();
		string[] filesInDirectory;
		if (CloudStore.DirectoryExists(directoryPath))
		{
			filesInDirectory = CloudStore.GetFilesInDirectory(directoryPath);
			foreach (string text in filesInDirectory)
			{
				string text2 = directoryPath + "/" + text;
				filePathsRead.Add(text2);
				Log.Debug("Checking file " + text2 + " in cloud saves");
				yield return SyncCloudToLocal(text2);
			}
		}
		if (!LocalStore.DirectoryExists(directoryPath))
		{
			yield break;
		}
		filesInDirectory = LocalStore.GetFilesInDirectory(directoryPath);
		foreach (string text3 in filesInDirectory)
		{
			string text4 = directoryPath + "/" + text3;
			if (!filePathsRead.Contains(text4))
			{
				Log.Debug("Checking file " + text4 + " in local saves");
				yield return SyncCloudToLocal(text4);
			}
		}
	}

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
					Log.Debug("Immediately forgetting " + path);
					CloudStore.ForgetFile(path);
				}
				LocalStore.SetLastModifiedTime(path, CloudStore.GetLastModifiedTime(path));
				return;
			}
			catch (InvalidOperationException ex)
			{
				Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex.Message);
				return;
			}
		}
		if (CloudStore.FileExists(path))
		{
			Log.Debug("Deleting file " + path + " from cloud because it doesn't exist on local");
			CloudStore.DeleteFile(path);
		}
	}

	public IEnumerable<Task> OverwriteCloudWithLocalDirectory(string directoryPath, int? byteLimit, int? fileLimit)
	{
		Log.Debug("Writing all files in directory " + directoryPath + " to cloud");
		HashSet<string> filePathsRead = new HashSet<string>();
		if (CloudStore.DirectoryExists(directoryPath))
		{
			string[] filesInDirectory = CloudStore.GetFilesInDirectory(directoryPath);
			foreach (string text in filesInDirectory)
			{
				filePathsRead.Add(text);
				yield return OverwriteCloudWithLocal(directoryPath + "/" + text);
			}
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
			if (!filePathsRead.Contains(item))
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

	public void ForgetFilesInDirectoryBeforeWritingIfNecessary(string directoryPath, int bytesToBeWritten, int byteLimit, int fileLimit)
	{
		int num = bytesToBeWritten;
		int num2 = 1;
		string[] filesInDirectory = CloudStore.GetFilesInDirectory(directoryPath);
		List<string> list = new List<string>();
		string[] array = filesInDirectory;
		foreach (string text in array)
		{
			string text2 = directoryPath + "/" + text;
			if (CloudStore.IsFilePersisted(text2))
			{
				list.Add(text2);
				num += CloudStore.GetFileSize(text2);
				num2++;
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
}
