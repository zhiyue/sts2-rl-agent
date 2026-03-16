using System;
using System.Linq;

namespace MegaCrit.Sts2.Core.Saves.Test;

public class MockCloudGodotFileIo : MockGodotFileIo, ICloudSaveStore, ISaveStore
{
	public MockCloudGodotFileIo(string saveDir)
		: base(saveDir)
	{
	}

	public bool HasCloudFiles()
	{
		return _files.Any();
	}

	public void ForgetFile(string path)
	{
		CanonicalizePath(ref path);
		if (!_files.TryGetValue(path, out File value))
		{
			throw new InvalidOperationException("No file at " + path + "!");
		}
		value.forgotten = true;
	}

	public bool IsFilePersisted(string path)
	{
		CanonicalizePath(ref path);
		if (!_files.TryGetValue(path, out File value))
		{
			throw new InvalidOperationException("No file at " + path + "!");
		}
		return !value.forgotten;
	}

	public void BeginSaveBatch()
	{
	}

	public void EndSaveBatch()
	{
	}
}
