using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Exceptions;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Implements the ISaveStore interface for managing game save files within Godot.
/// Handles file operations including reading, writing, and managing save directories.
/// All file I/O operations related to game saves should use this class to ensure
/// proper path handling, atomic writes, and consistent error handling across the application.
/// </summary>
public class GodotFileIo : ISaveStore
{
	/// <summary>
	/// WARNING: ONLY CHANGE THIS IN TESTS.
	/// The directory that your save files live at.
	/// On Windows, these will be at: C:\Users\{USER}\AppData\Roaming\SlayTheSpire2\{platform}\{userId}\profile{profileId}\saves
	/// </summary>
	public string SaveDir { get; set; }

	public GodotFileIo(string saveDir)
	{
		SaveDir = saveDir;
		CreateDirectory(SaveDir);
	}

	public string GetFullPath(string filename)
	{
		if (filename.StartsWith(SaveDir))
		{
			return filename;
		}
		return SaveDir + "/" + filename;
	}

	public string? ReadFile(string path)
	{
		path = GetFullPath(path);
		using Godot.FileAccess fileAccess = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		if (fileAccess == null)
		{
			Error openError = Godot.FileAccess.GetOpenError();
			if (openError == Error.FileNotFound)
			{
				Log.Warn("Tried to read file at " + path + ", but there was no such file");
				return null;
			}
			throw new SaveException($"Failed to open file for reading. path='{path}' error={openError}");
		}
		string asText = fileAccess.GetAsText();
		fileAccess.Close();
		return asText;
	}

	public async Task<string?> ReadFileAsync(string path)
	{
		path = GetFullPath(path);
		ValidateGodotFilePath(path);
		string result;
		await using (FileAccessStream stream = new FileAccessStream(path, Godot.FileAccess.ModeFlags.Read))
		{
			using MemoryStream memoryStream = new MemoryStream();
			await stream.CopyToAsync(memoryStream);
			result = Encoding.UTF8.GetString(memoryStream.ToArray());
		}
		return result;
	}

	public DateTimeOffset GetLastModifiedTime(string path)
	{
		path = GetFullPath(path);
		return DateTimeOffset.FromUnixTimeSeconds((long)Godot.FileAccess.GetModifiedTime(path));
	}

	public int GetFileSize(string path)
	{
		path = GetFullPath(path);
		return (int)Godot.FileAccess.GetSize(path);
	}

	public void SetLastModifiedTime(string path, DateTimeOffset time)
	{
		path = GetFullPath(path);
		File.SetLastWriteTimeUtc(ProjectSettings.GlobalizePath(path), time.UtcDateTime);
	}

	public void WriteFile(string path, string content)
	{
		if (string.IsNullOrWhiteSpace(content))
		{
			Log.Error("The content is empty for path='" + path + "'");
		}
		else
		{
			WriteFile(path, Encoding.UTF8.GetBytes(content));
		}
	}

	public void WriteFile(string path, byte[] bytes)
	{
		path = GetFullPath(path);
		ValidateGodotFilePath(path);
		CopyBackup(path);
		string text = path + ".tmp";
		using Godot.FileAccess fileAccess = Godot.FileAccess.Open(text, Godot.FileAccess.ModeFlags.Write);
		if (fileAccess == null)
		{
			throw new SaveException($"Failed to open file for writing. path='{text}' error={Godot.FileAccess.GetOpenError()}");
		}
		if (fileAccess.StoreBuffer(bytes))
		{
			fileAccess.Close();
			FsyncFile(text);
			RenameFile(text, path);
			Log.Info($"Wrote {bytes.Length} bytes to path={path} save_dir={SaveDir}");
			return;
		}
		throw new SaveException($"Failed to write {bytes.Length} bytes to path={path} save_dir={SaveDir}. Error: {fileAccess.GetError()}");
	}

	public Task WriteFileAsync(string path, string content)
	{
		return WriteFileAsync(path, Encoding.UTF8.GetBytes(content));
	}

	public async Task WriteFileAsync(string path, byte[] bytes)
	{
		path = GetFullPath(path);
		ValidateGodotFilePath(path);
		CopyBackup(path);
		string tempPath = path + ".tmp";
		await using FileAccessStream stream = new FileAccessStream(tempPath, Godot.FileAccess.ModeFlags.Write);
		await stream.WriteAsync(bytes);
		long position = stream.Position;
		stream.Close();
		FsyncFile(tempPath);
		RenameFile(tempPath, path);
		Log.Info($"Wrote {position} bytes to path={path} save_dir={SaveDir}");
	}

	public bool FileExists(string path)
	{
		return Godot.FileAccess.FileExists(GetFullPath(path));
	}

	public bool DirectoryExists(string path)
	{
		return DirAccess.DirExistsAbsolute(GetFullPath(path));
	}

	public void DeleteFile(string path)
	{
		Error error = DirAccess.RemoveAbsolute(GetFullPath(path));
		if (error != Error.Ok && error != Error.FileNotFound)
		{
			Log.Error($"Error deleting path {path}: {error}");
		}
	}

	public void RenameFile(string sourcePath, string destinationPath)
	{
		if (!FileExists(sourcePath))
		{
			throw new SaveException("Cannot rename file: source does not exist. source=" + GetFullPath(sourcePath));
		}
		sourcePath = GetFullPath(sourcePath);
		destinationPath = GetFullPath(destinationPath);
		Error error = Error.Failed;
		for (int i = 1; i <= 4; i++)
		{
			error = DirAccess.RenameAbsolute(sourcePath, destinationPath);
			if (error == Error.Ok)
			{
				return;
			}
			if (Godot.FileAccess.FileExists(destinationPath) && !Godot.FileAccess.FileExists(sourcePath))
			{
				Log.Warn($"Rename reported error={error} but destination exists, treating as success. source={sourcePath}");
				return;
			}
			if (i < 4)
			{
				Log.Warn($"Rename failed (attempt {i}/{4}), retrying. error={error} source={sourcePath}");
				Thread.Sleep(50);
			}
		}
		Thread.Sleep(100);
		if (Godot.FileAccess.FileExists(destinationPath) && !Godot.FileAccess.FileExists(sourcePath))
		{
			Log.Warn("Rename appeared to fail but destination exists after delay, treating as success. source=" + sourcePath);
			return;
		}
		throw new SaveException($"Failed to rename file. error={error} source={sourcePath} destination={destinationPath} source_exists={Godot.FileAccess.FileExists(sourcePath)} destination_exists={Godot.FileAccess.FileExists(destinationPath)}");
	}

	public string[] GetFilesInDirectory(string directoryPath)
	{
		directoryPath = GetFullPath(directoryPath);
		return DirAccess.GetFilesAt(directoryPath);
	}

	public string[] GetDirectoriesInDirectory(string directoryPath)
	{
		directoryPath = GetFullPath(directoryPath);
		return DirAccess.GetDirectoriesAt(directoryPath);
	}

	public void CreateDirectory(string directoryPath)
	{
		directoryPath = GetFullPath(directoryPath);
		if (!DirAccess.DirExistsAbsolute(directoryPath))
		{
			DirAccess.MakeDirRecursiveAbsolute(directoryPath);
		}
	}

	public void DeleteDirectory(string directoryPath)
	{
		directoryPath = GetFullPath(directoryPath);
		if (!DirAccess.DirExistsAbsolute(directoryPath))
		{
			return;
		}
		using DirAccess dirAccess = DirAccess.Open(directoryPath);
		dirAccess.IncludeHidden = true;
		string[] files = dirAccess.GetFiles();
		foreach (string text in files)
		{
			Error error = dirAccess.Remove(text);
			if (error != Error.Ok)
			{
				throw new InvalidOperationException($"Got error {error} trying to delete file {text} in directory {directoryPath}");
			}
		}
		string[] directories = dirAccess.GetDirectories();
		foreach (string text2 in directories)
		{
			DeleteDirectory(directoryPath + "/" + text2);
		}
		Error error2 = dirAccess.Remove("");
		if (error2 != Error.Ok)
		{
			throw new InvalidOperationException($"Got error {error2} trying to delete directory {directoryPath}");
		}
	}

	public void DeleteTemporaryFiles(string directoryPath)
	{
		directoryPath = GetFullPath(directoryPath);
		using DirAccess dirAccess = DirAccess.Open(directoryPath);
		if (dirAccess == null)
		{
			return;
		}
		string[] files = dirAccess.GetFiles();
		foreach (string text in files)
		{
			if (text.EndsWith(".tmp"))
			{
				Log.Info("Cleaning up orphaned " + text + " in " + directoryPath);
				Error error = dirAccess.Remove(text);
				if (error != Error.Ok)
				{
					Log.Warn($"Couldn't delete temporary file {text} in {directoryPath}, error={error}");
				}
			}
		}
	}

	/// <summary>
	/// Copies the current save to a .backup file using temp+rename for crash safety.
	/// The .backup serves as a fallback for the non-atomic rename on Windows and for
	/// recovery when the primary save is corrupted. Writing to a .tmp file first ensures
	/// the old .backup is preserved if a crash occurs mid-write.
	/// </summary>
	private void CopyBackup(string fullPath)
	{
		if (!Godot.FileAccess.FileExists(fullPath))
		{
			return;
		}
		string destinationPath = fullPath + ".backup";
		string text = fullPath + ".backup.tmp";
		using Godot.FileAccess fileAccess = Godot.FileAccess.Open(fullPath, Godot.FileAccess.ModeFlags.Read);
		if (fileAccess == null)
		{
			Log.Warn($"Failed to open source for backup copy. path={fullPath} error={Godot.FileAccess.GetOpenError()}");
			return;
		}
		byte[] buffer = fileAccess.GetBuffer((long)fileAccess.GetLength());
		fileAccess.Close();
		using Godot.FileAccess fileAccess2 = Godot.FileAccess.Open(text, Godot.FileAccess.ModeFlags.Write);
		if (fileAccess2 == null)
		{
			Log.Warn($"Failed to open backup for writing. path={text} error={Godot.FileAccess.GetOpenError()}");
			return;
		}
		if (!fileAccess2.StoreBuffer(buffer))
		{
			Log.Warn($"Copying backup from {fullPath} to {text} failed: {fileAccess2.GetError()}");
			return;
		}
		fileAccess2.Close();
		try
		{
			FsyncFile(text);
			RenameFile(text, destinationPath);
		}
		catch (Exception ex)
		{
			Log.Warn("Failed to finalize backup for " + fullPath + ": " + ex.Message);
		}
	}

	/// <summary>
	/// Forces the OS to flush all buffered data for the specified file to the storage device.
	/// Without this, data may sit in the OS page cache and be lost on power loss or OS crash.
	///
	/// Why .NET FileStream instead of Godot's FileAccess.Flush()?
	/// Godot's Flush() calls fflush() (see file_access_unix.cpp:322 and file_access_windows.cpp:374)
	/// which only pushes data from the C stdio buffer to the OS kernel page cache. Godot does not
	/// expose fsync/fdatasync/FlushFileBuffers anywhere in its FileAccess API. .NET's Flush(flushToDisk: true)
	/// calls fsync (Linux) / FlushFileBuffers (Windows) which forces the kernel to write to the physical device.
	///
	/// This is a platform-dependent call and may not work on all OSes (e.g. consoles with sandboxed
	/// filesystems). On unsupported platforms, the catch block logs a warning and the save proceeds
	/// without durability guarantees (same as before this change). Console ports will need a
	/// platform-specific ISaveStore implementation in C# anyway, which can use that platform's
	/// native flush/commit API directly.
	/// </summary>
	private static void FsyncFile(string godotPath)
	{
		try
		{
			string path = ProjectSettings.GlobalizePath(godotPath);
			using FileStream fileStream = new FileStream(path, FileMode.Open, System.IO.FileAccess.Write, FileShare.ReadWrite);
			fileStream.Flush(flushToDisk: true);
		}
		catch (Exception ex)
		{
			Log.Warn("Failed to fsync " + godotPath + ": " + ex.Message);
		}
	}

	private static void ValidateGodotFilePath(string godotFilePath)
	{
		if (!godotFilePath.Contains("://"))
		{
			throw new SaveException("The path='" + godotFilePath + "' is not a godot file path");
		}
		string baseDir = godotFilePath.GetBaseDir();
		if (!DirAccess.DirExistsAbsolute(baseDir))
		{
			DirAccess.MakeDirRecursiveAbsolute(baseDir);
		}
	}
}
