using System.IO;
using Godot;

namespace MegaCrit.Sts2.Core.Modding;

/// <summary>
/// A file IO abstraction layer for use with ModManager.
/// It's a little different than ISaveStore, which is scoped only to a specific directory. This class allows global access
/// to the filesystem.
/// </summary>
public interface IModManagerFileIo
{
	string[] GetFilesAt(string path);

	string[] GetDirectoriesAt(string path);

	bool FileExists(string path);

	bool DirectoryExists(string path);

	Stream OpenStream(string path, Godot.FileAccess.ModeFlags mode);
}
