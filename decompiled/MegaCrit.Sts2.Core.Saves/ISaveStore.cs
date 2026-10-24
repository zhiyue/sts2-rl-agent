using System;
using System.Threading.Tasks;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// An abstraction layer around how we store save data.
/// </summary>
public interface ISaveStore
{
	string? ReadFile(string path);

	Task<string?> ReadFileAsync(string path);

	void WriteFile(string path, string content);

	void WriteFile(string path, byte[] content);

	Task WriteFileAsync(string path, string content);

	Task WriteFileAsync(string path, byte[] content);

	bool FileExists(string path);

	bool DirectoryExists(string path);

	void DeleteFile(string path);

	void RenameFile(string sourcePath, string destinationPath);

	string[] GetFilesInDirectory(string directoryPath);

	string[] GetDirectoriesInDirectory(string directoryPath);

	/// <summary>
	/// Creates a directory if it doesn't already exist.
	/// Implementors must ensure that this does not throw if the directory already exists.
	/// </summary>
	void CreateDirectory(string directoryPath);

	/// <summary>
	/// Deletes a directory and any remaining contents.
	/// No error is emitted if the directory doesn't already exist.
	/// </summary>
	void DeleteDirectory(string directoryPath);

	void DeleteTemporaryFiles(string directoryPath);

	DateTimeOffset GetLastModifiedTime(string path);

	int GetFileSize(string path);

	void SetLastModifiedTime(string path, DateTimeOffset time);

	string GetFullPath(string filename);
}
