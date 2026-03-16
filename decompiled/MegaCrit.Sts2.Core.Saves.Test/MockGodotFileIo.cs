using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaCrit.Sts2.Core.Saves.Test;

public class MockGodotFileIo : ISaveStore
{
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

	protected readonly ConcurrentDictionary<string, File> _files = new ConcurrentDictionary<string, File>();

	protected readonly ConcurrentDictionary<string, List<string>> _directories = new ConcurrentDictionary<string, List<string>>();

	protected readonly string _saveDir;

	public Func<DateTimeOffset>? getCurrentTime;

	public bool ShouldFailWrites;

	public List<(string Method, object[] Args)> Calls { get; } = new List<(string, object[])>();

	public Action<string, string>? RenameFileAction { get; set; }

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
		if (!_files.TryGetValue(path, out File value))
		{
			throw new InvalidOperationException("No file at " + path + "!");
		}
		value.lastModifiedTime = time;
	}

	public string GetFullPath(string filename)
	{
		Calls.Add(("GetFullPath", new object[1] { filename }));
		CanonicalizePath(ref filename);
		return filename;
	}

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
		CanonicalizePath(ref path);
		Calls.Add(("ReadFileAsync", new object[1] { path }));
		File value;
		return Task.FromResult(_files.TryGetValue(path, out value) ? value.content : null);
	}

	public void WriteFile(string path, string content)
	{
		CanonicalizePath(ref path);
		Calls.Add(("WriteFile", new object[2] { path, content }));
		if (ShouldFailWrites)
		{
			throw new InvalidOperationException("Simulated write failure");
		}
		string key = path + ".backup";
		_files.Remove(key, out var _);
		if (_files.Remove(path, out var value2))
		{
			_files[key] = value2;
		}
		File value3 = new File
		{
			content = content,
			lastModifiedTime = getCurrentTime?.Invoke()
		};
		_files[path] = value3;
	}

	public void WriteFile(string path, byte[] bytes)
	{
		WriteFile(path, Encoding.UTF8.GetString(bytes));
	}

	public Task WriteFileAsync(string path, string content)
	{
		WriteFile(path, content);
		return Task.CompletedTask;
	}

	public Task WriteFileAsync(string path, byte[] bytes)
	{
		WriteFile(path, Encoding.UTF8.GetString(bytes));
		return Task.CompletedTask;
	}

	public bool FileExists(string path)
	{
		CanonicalizePath(ref path);
		Calls.Add(("FileExists", new object[1] { path }));
		return _files.ContainsKey(path);
	}

	public bool DirectoryExists(string path)
	{
		return true;
	}

	public void DeleteFile(string path)
	{
		CanonicalizePath(ref path);
		Calls.Add(("DeleteFile", new object[1] { path }));
		_files.Remove(path, out var _);
	}

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

	public string[] GetFilesInDirectory(string directoryPath)
	{
		CanonicalizePath(ref directoryPath);
		Calls.Add(("GetFilesInDirectory", new object[1] { directoryPath }));
		string prefix = (directoryPath.EndsWith('/') ? directoryPath : (directoryPath + "/"));
		return (from path in _files.Keys
			where path.StartsWith(prefix)
			select Path.GetFileName(path)).ToArray();
	}

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

	public void CreateDirectory(string directoryPath)
	{
		CanonicalizePath(ref directoryPath);
		Calls.Add(("CreateDirectory", new object[1] { directoryPath }));
		if (!_directories.ContainsKey(directoryPath))
		{
			_directories[directoryPath] = new List<string>();
		}
	}

	public void DeleteDirectory(string directoryPath)
	{
		CanonicalizePath(ref directoryPath);
		Calls.Add(("DeleteDirectory", new object[1] { directoryPath }));
	}

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
