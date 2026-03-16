using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Exceptions;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Saves;

public class GodotFileIo : ISaveStore
{
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
			return null;
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
		using Godot.FileAccess fileAccess = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		if (fileAccess == null)
		{
			return 0;
		}
		return (int)fileAccess.GetLength();
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
		RotateBackup(path);
		string text = path + ".tmp";
		using Godot.FileAccess fileAccess = Godot.FileAccess.Open(text, Godot.FileAccess.ModeFlags.Write);
		if (fileAccess == null)
		{
			throw new SaveException($"Failed to open file for writing. path='{text}' error={Godot.FileAccess.GetOpenError()}");
		}
		fileAccess.StoreBuffer(bytes);
		fileAccess.Close();
		RenameFile(text, path);
		Log.Info($"Wrote {bytes.Length} bytes to path={path} save_dir={SaveDir}");
	}

	public Task WriteFileAsync(string path, string content)
	{
		return WriteFileAsync(path, Encoding.UTF8.GetBytes(content));
	}

	public async Task WriteFileAsync(string path, byte[] bytes)
	{
		path = GetFullPath(path);
		ValidateGodotFilePath(path);
		RotateBackup(path);
		string tempPath = path + ".tmp";
		await using FileAccessStream stream = new FileAccessStream(tempPath, Godot.FileAccess.ModeFlags.Write);
		await stream.WriteAsync(bytes);
		long position = stream.Position;
		stream.Close();
		RenameFile(tempPath, path);
		Log.Info($"Wrote {position} bytes to path={path} save_dir={path}");
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
		DirAccess.RemoveAbsolute(GetFullPath(path));
	}

	public void RenameFile(string sourcePath, string destinationPath)
	{
		if (!FileExists(sourcePath))
		{
			throw new SaveException("Cannot rename file: source does not exist. source=" + GetFullPath(sourcePath));
		}
		sourcePath = GetFullPath(sourcePath);
		destinationPath = GetFullPath(destinationPath);
		Error error = DirAccess.RenameAbsolute(sourcePath, destinationPath);
		if (error != Error.Ok)
		{
			throw new SaveException($"Failed to rename file. error={error} source={sourcePath} destination={destinationPath}");
		}
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
		foreach (string path in files)
		{
			dirAccess.Remove(path);
		}
		string[] directories = dirAccess.GetDirectories();
		foreach (string text in directories)
		{
			DeleteDirectory(directoryPath + "/" + text);
		}
		Error error = dirAccess.Remove("");
		if (error != Error.Ok)
		{
			throw new InvalidOperationException($"Got error {error} trying to delete directory {directoryPath}");
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
				Log.Info("Cleaned up orphaned " + text + " in " + directoryPath);
				dirAccess.Remove(text);
			}
		}
	}

	private static void RotateBackup(string fullPath)
	{
		string text = fullPath + ".backup";
		if (Godot.FileAccess.FileExists(text))
		{
			DirAccess.RemoveAbsolute(text);
		}
		if (Godot.FileAccess.FileExists(fullPath))
		{
			Error error = DirAccess.RenameAbsolute(fullPath, text);
			if (error != Error.Ok)
			{
				Log.Warn($"Failed to rotate backup. error={error} source={fullPath} backup={text}");
			}
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
