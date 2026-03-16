using System;

namespace MegaCrit.Sts2.Core.Saves;

public readonly struct SaveBatchScope : IDisposable
{
	public SaveBatchScope(SaveManager saveManager)
	{
		_003CsaveManager_003EP = saveManager;
	}

	public void Dispose()
	{
		_003CsaveManager_003EP.EndSaveBatch();
	}
}
