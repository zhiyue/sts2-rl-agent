using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Assets;

public class AssetLoadingSession
{
	private const int _batchSize = 128;

	private readonly string _name;

	private readonly ConcurrentDictionary<string, Resource> _cache;

	private readonly AssetCache? _assetCache;

	private readonly Queue<string> _toLoad = new Queue<string>();

	private readonly Queue<string> _loading = new Queue<string>();

	private readonly Queue<string> _finalizing = new Queue<string>();

	private readonly Queue<string> _vfxScenes = new Queue<string>();

	private readonly TaskCompletionSource<bool> _completionSource = new TaskCompletionSource<bool>();

	private readonly Stopwatch _stopwatch = new Stopwatch();

	private int _totalLoaded;

	private bool _vfxLoading;

	private string? _currentVfxPath;

	public Task<bool> Task => _completionSource.Task;

	public bool IsCompleted => _completionSource.Task.IsCompleted;

	public AssetLoadingSession(string name, IEnumerable<string> paths, ConcurrentDictionary<string, Resource> cache, AssetCache? assetCache = null)
	{
		_cache = cache;
		_assetCache = assetCache;
		_name = name;
		foreach (string path in paths)
		{
			if (IsVfxScene(path))
			{
				_vfxScenes.Enqueue(path);
			}
			else
			{
				_toLoad.Enqueue(path);
			}
		}
		_stopwatch.Start();
		Log.Info($"Preloading '{name}' assets... count={_toLoad.Count} vfx={_vfxScenes.Count}");
	}

	private AssetLoadingSession()
	{
		_name = "EMPTY";
		_cache = null;
		_toLoad = new Queue<string>();
		_vfxScenes = new Queue<string>();
		_completionSource.SetResult(result: true);
	}

	private static bool IsVfxScene(string path)
	{
		if (path.EndsWith(".tscn"))
		{
			return path.Contains("/vfx/");
		}
		return false;
	}

	public static AssetLoadingSession Empty()
	{
		return new AssetLoadingSession();
	}

	public void Process()
	{
		FinalizeLoading();
		ProcessLoadingQueue();
		CheckLoadingStatus();
		if (_toLoad.Count == 0 && _loading.Count == 0 && _finalizing.Count == 0)
		{
			ProcessVfxQueue();
		}
		Log.Debug($"Preloading '{_name}' Process: toLoad={_toLoad.Count} loading={_loading.Count} finalizing={_finalizing.Count} vfx={_vfxScenes.Count}");
		if (_toLoad.Count == 0 && _loading.Count == 0 && _finalizing.Count == 0 && _vfxScenes.Count == 0 && !_vfxLoading)
		{
			Log.Info($"Preloading '{_name}' Complete: assets={_totalLoaded} time_elapsed={_stopwatch.ElapsedMilliseconds:N0}ms");
			_stopwatch.Stop();
			_completionSource.TrySetResult(result: true);
		}
	}

	private void ProcessVfxQueue()
	{
		if (_vfxLoading)
		{
			switch (ResourceLoader.LoadThreadedGetStatus(_currentVfxPath))
			{
			case ResourceLoader.ThreadLoadStatus.Loaded:
				AddToCache(ResourceLoader.LoadThreadedGet(_currentVfxPath), _currentVfxPath);
				_vfxLoading = false;
				break;
			case ResourceLoader.ThreadLoadStatus.InvalidResource:
			case ResourceLoader.ThreadLoadStatus.Failed:
				Log.Error("Failed to load VFX scene: " + _currentVfxPath);
				_vfxLoading = false;
				break;
			}
			return;
		}
		string result;
		while (_vfxScenes.TryDequeue(out result))
		{
			if (!_cache.ContainsKey(result))
			{
				if (ResourceLoader.LoadThreadedRequest(result, "", useSubThreads: false, ResourceLoader.CacheMode.Reuse) == Error.Ok)
				{
					_currentVfxPath = result;
					_vfxLoading = true;
					break;
				}
				Log.Error("Error requesting VFX load for path: " + result);
			}
		}
	}

	private void FinalizeLoading()
	{
		while (_finalizing.Count != 0)
		{
			if (!_finalizing.TryDequeue(out string result))
			{
				Log.Error("Failed to dequeue finalizing asset!");
			}
			else
			{
				AddToCache(ResourceLoader.LoadThreadedGet(result), result);
			}
		}
	}

	private void AddToCache(Resource? resource, string path)
	{
		if (resource == null)
		{
			Log.Error("Resource loaded as null for path: " + path);
			return;
		}
		_totalLoaded++;
		_cache[path] = resource;
	}

	private void ProcessLoadingQueue()
	{
		string result;
		while (_loading.Count < 128 && _toLoad.TryDequeue(out result))
		{
			if (!_cache.ContainsKey(result))
			{
				if (ResourceLoader.LoadThreadedRequest(result, "", useSubThreads: false, ResourceLoader.CacheMode.Reuse) == Error.Ok)
				{
					_loading.Enqueue(result);
				}
				else
				{
					Log.Error("Error requesting load for path: " + result);
				}
			}
		}
	}

	private void CheckLoadingStatus()
	{
		int count = _loading.Count;
		for (int i = 0; i < count; i++)
		{
			if (!_loading.TryDequeue(out string result))
			{
				Log.Error("Failed to dequeue loading asset!");
				break;
			}
			ResourceLoader.ThreadLoadStatus threadLoadStatus = ResourceLoader.LoadThreadedGetStatus(result);
			if ((ulong)threadLoadStatus <= 3uL)
			{
				switch (threadLoadStatus)
				{
				case ResourceLoader.ThreadLoadStatus.Loaded:
					_finalizing.Enqueue(result);
					continue;
				case ResourceLoader.ThreadLoadStatus.Failed:
					Log.Error("Failed loading asset: " + result);
					_assetCache?.MarkAssetFailed(result);
					continue;
				case ResourceLoader.ThreadLoadStatus.InvalidResource:
				{
					Log.Warn("InvalidResource status for " + result + ", falling back to sync load");
					Resource resource = ResourceLoader.Load<Resource>(result, null, ResourceLoader.CacheMode.Reuse);
					if (resource != null)
					{
						AddToCache(resource, result);
					}
					else
					{
						Log.Error("Failed to load resource synchronously: " + result);
					}
					continue;
				}
				case ResourceLoader.ThreadLoadStatus.InProgress:
					_loading.Enqueue(result);
					continue;
				}
			}
			Log.Error("Unexpected thread load status for path: " + result);
		}
	}

	public void PrintStatus()
	{
		Log.Info($"LOADING_STATUS: ToLoad={_toLoad.Count} Loading={_loading.Count} Finishing={_finalizing.Count} VfxScenes={_vfxScenes.Count}");
	}

	public Task WaitForCompletion()
	{
		return _completionSource.Task;
	}
}
