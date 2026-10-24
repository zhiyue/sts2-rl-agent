using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Platform.Steam;
using Steamworks;

namespace MegaCrit.Sts2.Core.Saves.Test;

/// <summary>
/// A minimalist mock implementation of ISaveStore for testing.
/// Instead of relying on actual file system operations like the original implementation,
/// this mock keeps everything in memory and tracks method calls for verification.
/// This approach:
/// 1. Avoids duplicating the real implementation's logic
/// 2. Makes tests more predictable by not hitting the actual file system
/// 3. Allows verifying interactions through the Calls collection
/// 4. Provides customization points only where needed for specific tests
/// </summary>
public class MockGodotFileIo : ISaveStore
{
	/// <summary>
	/// Constants for method names to use when tracking or comparing method calls
	/// </summary>
	public static class Methods
	{
		public const string writeFile = "WriteFile";

		public const string writeFileAsync = "WriteFileAsync";

		public const string readFile = "ReadFile";

		public const string readFileAsync = "ReadFileAsync";

		public const string fileExists = "FileExists";

		public const string renameFile = "RenameFile";

		public const string deleteFile = "DeleteFile";

		public const string getFullPath = "GetFullPath";

		public const string getDirectoriesInDirectory = "GetDirectoriesInDirectory";

		public const string getFilesInDirectory = "GetFilesInDirectory";

		public const string createDirectory = "CreateDirectory";

		public const string deleteDirectory = "DeleteDirectory";

		public const string deleteTemporaryFiles = "DeleteTemporaryFiles";

		public const string getLastModifiedTime = "GetLastModifiedTime";
	}

	protected class File
	{
		public required string content;

		public DateTimeOffset? lastModifiedTime;

		public bool forgotten;
	}

	/// <summary>
	/// In-memory storage of file contents, with path as the key and content as the value
	/// </summary>
	protected readonly ConcurrentDictionary<string, File> _files = new ConcurrentDictionary<string, File>();

	/// <summary>
	/// In-memory representation of directory structure
	/// </summary>
	protected readonly ConcurrentDictionary<string, List<string>> _directories = new ConcurrentDictionary<string, List<string>>();

	/// <summary>
	/// Base directory for GetFullPath operations
	/// </summary>
	protected readonly string _saveDir;

	public Func<DateTimeOffset>? getCurrentTime;

	public bool ShouldFailWrites;

	public bool ShouldFailTimestampSync;

	public bool DoSteamSpecificError;

	/// <summary>
	/// Tracks all method calls for verification in tests
	/// </summary>
	public List<(string Method, object[] Args)> Calls { get; } = new List<(string, object[])>();

	/// <summary>
	/// Custom callback for RenameFile operation to allow tests to intercept and verify behavior
	/// </summary>
	public Action<string, string>? RenameFileAction { get; set; }

	/// <summary>
	/// Creates a new instance of the mock save store with the specified base directory
	/// </summary>
	/// <param name="saveDir">The base directory for file operations</param>
	public MockGodotFileIo(string saveDir)
	{
		CanonicalizePath(ref saveDir, getFullPath: false);
		_saveDir = saveDir;
		CreateDirectory(_saveDir);
	}

	public DateTimeOffset GetLastModifiedTime(string path)
	{
		CanonicalizePath(ref path);
		if (!_files.TryGetValue(path, out File value))
		{
			throw new InvalidOperationException("No file at " + path + "!");
		}
		if (!value.lastModifiedTime.HasValue)
		{
			throw new InvalidOperationException("getCurrentTime was not set when file " + path + " was created!");
		}
		return value.lastModifiedTime.Value;
	}

	public int GetFileSize(string path)
	{
		CanonicalizePath(ref path);
		if (!_files.TryGetValue(path, out File value))
		{
			throw new InvalidOperationException("No file at " + path + "!");
		}
		return Encoding.UTF8.GetByteCount(value.content);
	}

	public void SetLastModifiedTime(string path, DateTimeOffset time)
	{
		CanonicalizePath(ref path);
		if (ShouldFailTimestampSync)
		{
			throw new IOException("Simulated timestamp sync failure for " + path);
		}
		if (!_files.TryGetValue(path, out File value))
		{
			throw new InvalidOperationException("No file at " + path + "!");
		}
		value.lastModifiedTime = time;
	}

	/// <summary>
	/// Gets the full path for a filename relative to the base directory
	/// </summary>
	public string GetFullPath(string filename)
	{
		Calls.Add(("GetFullPath", new object[1] { filename }));
		CanonicalizePath(ref filename);
		return filename;
	}

	/// <summary>
	/// Directly sets the content of a file in the virtual file system without going through WriteFile.
	/// Used to simulate corrupt files (e.g. empty content from zeroed-out saves).
	/// </summary>
	public void SetFileContent(string path, string content)
	{
		CanonicalizePath(ref path);
		if (!_files.TryGetValue(path, out File value))
		{
			throw new InvalidOperationException("Cannot set content: no file at " + path + ". Write the file first.");
		}
		value.content = content;
	}

	/// <summary>
	/// Reads a file from the virtual file system
	/// </summary>
	public string? ReadFile(string path)
	{
		CanonicalizePath(ref path);
		Calls.Add(("ReadFile", new object[1] { path }));
		if (!_files.TryGetValue(path, out File value))
		{
			return null;
		}
		return value.content;
	}

	public Task<string?> ReadFileAsync(string path)
	{
		if (DoSteamSpecificError)
		{
			throw new SteamRemoteSaveStoreException("Simulating Steam Error", EResult.k_EResultFileNotFound);
		}
		CanonicalizePath(ref path);
		Calls.Add(("ReadFileAsync", new object[1] { path }));
		File value;
		return Task.FromResult(_files.TryGetValue(path, out value) ? value.content : null);
	}

	/// <summary>
	/// Writes a file to the virtual file system
	/// </summary>
	public void WriteFile(string path, string content)
	{
		CanonicalizePath(ref path);
		Calls.Add(("WriteFile", new object[2] { path, content }));
		if (ShouldFailWrites)
		{
			throw new InvalidOperationException("Simulated write failure");
		}
		string key = path + ".backup";
		if (_files.TryGetValue(path, out File value))
		{
			_files[key] = value;
		}
		File value2 = new File
		{
			content = content,
			lastModifiedTime = getCurrentTime?.Invoke()
		};
		_files[path] = value2;
	}

	/// <summary>
	/// Writes a file to the virtual file system
	/// </summary>
	public void WriteFile(string path, byte[] bytes)
	{
		WriteFile(path, Encoding.UTF8.GetString(bytes));
	}

	/// <summary>
	/// Asynchronously writes a file to the virtual file system
	/// </summary>
	public Task WriteFileAsync(string path, string content)
	{
		WriteFile(path, content);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a file to the virtual file system
	/// </summary>
	public Task WriteFileAsync(string path, byte[] bytes)
	{
		WriteFile(path, Encoding.UTF8.GetString(bytes));
		return Task.CompletedTask;
	}

	/// <summary>
	/// Checks if a file exists in the virtual file system
	/// </summary>
	public bool FileExists(string path)
	{
		CanonicalizePath(ref path);
		Calls.Add(("FileExists", new object[1] { path }));
		return _files.ContainsKey(path);
	}

	/// <summary>
	/// Checks if a directory exists in the virtual file system
	/// </summary>
	public bool DirectoryExists(string path)
	{
		return true;
	}

	/// <summary>
	/// Deletes a file from the virtual file system
	/// </summary>
	public void DeleteFile(string path)
	{
		CanonicalizePath(ref path);
		Calls.Add(("DeleteFile", new object[1] { path }));
		_files.Remove(path, out var _);
	}

	/// <summary>
	/// Renames a file in the virtual file system.
	/// If RenameFileAction is set, it will be called instead of performing the default behavior.
	/// </summary>
	public void RenameFile(string sourcePath, string destinationPath)
	{
		Calls.Add(("RenameFile", new object[2] { sourcePath, destinationPath }));
		if (RenameFileAction != null)
		{
			CanonicalizePath(ref sourcePath, getFullPath: false);
			CanonicalizePath(ref destinationPath, getFullPath: false);
			RenameFileAction(sourcePath, destinationPath);
			return;
		}
		CanonicalizePath(ref sourcePath);
		CanonicalizePath(ref destinationPath);
		if (_files.Remove(sourcePath, out var value))
		{
			_files[destinationPath] = value;
		}
	}

	/// <summary>
	/// Gets all files in a directory from the virtual file system
	/// </summary>
	public string[] GetFilesInDirectory(string directoryPath)
	{
		CanonicalizePath(ref directoryPath);
		Calls.Add(("GetFilesInDirectory", new object[1] { directoryPath }));
		string prefix = (directoryPath.EndsWith('/') ? directoryPath : (directoryPath + "/"));
		return (from path in _files.Keys
			where path.StartsWith(prefix)
			select Path.GetFileName(path)).ToArray();
	}

	/// <summary>
	/// Gets all directories in a directory from the virtual file system
	/// </summary>
	public string[] GetDirectoriesInDirectory(string directoryPath)
	{
		CanonicalizePath(ref directoryPath);
		Calls.Add(("GetDirectoriesInDirectory", new object[1] { directoryPath }));
		string prefix = (directoryPath.EndsWith('/') ? directoryPath : (directoryPath + "/"));
		return (from path in _files.Keys.Where((string path) => path.StartsWith(prefix)).Select(delegate(string path)
			{
				int num = prefix.Length + 1;
				return path.Substring(num, path.Length - num);
			})
			select new DirectoryInfo(path).Root.Name).ToArray();
	}

	/// <summary>
	/// Creates a directory in the virtual file system
	/// </summary>
	public void CreateDirectory(string directoryPath)
	{
		CanonicalizePath(ref directoryPath);
		Calls.Add(("CreateDirectory", new object[1] { directoryPath }));
		if (!_directories.ContainsKey(directoryPath))
		{
			_directories[directoryPath] = new List<string>();
		}
	}

	/// <summary>
	/// Deletes a directory and any remaining contents from the virtual file system.
	/// </summary>
	public void DeleteDirectory(string directoryPath)
	{
		CanonicalizePath(ref directoryPath);
		Calls.Add(("DeleteDirectory", new object[1] { directoryPath }));
	}

	/// <summary>
	/// Deletes temporary files (ending with .tmp) from a directory in the virtual file system
	/// </summary>
	public void DeleteTemporaryFiles(string directoryPath)
	{
		CanonicalizePath(ref directoryPath);
		Calls.Add(("DeleteTemporaryFiles", new object[1] { directoryPath }));
		string prefix = (directoryPath.EndsWith('/') ? directoryPath : (directoryPath + "/"));
		List<string> list = _files.Keys.Where((string path) => path.StartsWith(prefix) && path.EndsWith(".tmp")).ToList();
		foreach (string item in list)
		{
			_files.Remove(item, out var _);
		}
	}

	protected void CanonicalizePath(ref string path, bool getFullPath = true)
	{
		path = path.Replace('\\', '/');
		if (getFullPath)
		{
			path = _saveDir + "/" + path;
		}
	}
}
