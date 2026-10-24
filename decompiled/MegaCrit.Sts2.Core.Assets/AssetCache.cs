using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Assets;

/// <summary>
/// This class is responsible for preloading and caching assets. It solves a problem where we want to load whole
/// groups of assets, but we also want to eliminate duplicate loads of the same asset. This class is thread-safe.
/// </summary>
public class AssetCache
{
	private readonly ConcurrentDictionary<string, Resource> _cache = new ConcurrentDictionary<string, Resource>();

	private readonly HashSet<string> _missedCacheAssets = new HashSet<string>();

	private readonly HashSet<string> _failedAssets = new HashSet<string>();

	/// <summary>Gets the current count of missed cache assets (loaded outside preloading).</summary>
	public int MissedCacheAssetCount => _missedCacheAssets.Count;

	/// <summary>
	/// Gets the asset if it is cached, otherwise falls back to loading it.
	/// </summary>
	private Resource GetAsset(string path)
	{
		if (_cache.TryGetValue(path, out Resource value))
		{
			if (GodotObject.IsInstanceValid(value))
			{
				return value;
			}
			_cache[path] = ResourceLoader.Load<Resource>(path, null, ResourceLoader.CacheMode.Reuse);
			return _cache[path];
		}
		return LoadAsset(path);
	}

	public TS GetAsset<TS>(string path) where TS : Resource
	{
		return (TS)GetAsset(path);
	}

	private Resource LoadAsset(string path)
	{
		if (_failedAssets.Contains(path))
		{
			throw new AssetLoadException("Asset previously failed to load: " + path + ". The game installation may be corrupted.");
		}
		_missedCacheAssets.Add(path);
		Log.Warn("Asset not cached: " + path);
		_cache[path] = ResourceLoader.Load<Resource>(path, null, ResourceLoader.CacheMode.Reuse);
		return _cache[path];
	}

	/// <summary>
	/// Marks an asset as failed so that synchronous fallback loading does not re-attempt it.
	/// This prevents repeated native crashes in the Godot resource parser when files are
	/// missing or corrupted.
	/// </summary>
	public void MarkAssetFailed(string path)
	{
		_failedAssets.Add(path);
	}

	public AssetLoadingSession CreateSession(string name, IEnumerable<string> paths)
	{
		return new AssetLoadingSession(name, paths, _cache, this);
	}

	public void UnloadAssets(IEnumerable<string> assetsToUnloadSet)
	{
		foreach (string item in assetsToUnloadSet)
		{
			if (!_missedCacheAssets.Contains(item))
			{
				Resource resource = RemoveAndGetResource(item);
				if (resource != null && GodotObject.IsInstanceValid(resource))
				{
					Callable.From(resource.Dispose).CallDeferred();
				}
			}
		}
	}

	/// <summary>
	/// Clears and unloads all missed cache assets. Should be called at safe boundaries
	/// like returning to the main menu to prevent unbounded memory growth.
	/// </summary>
	public void UnloadMissedCacheAssets()
	{
		if (_missedCacheAssets.Count == 0)
		{
			return;
		}
		Log.Info($"Unloading {_missedCacheAssets.Count} missed cache assets");
		foreach (string missedCacheAsset in _missedCacheAssets)
		{
			Resource resource = RemoveAndGetResource(missedCacheAsset);
			if (resource != null && GodotObject.IsInstanceValid(resource))
			{
				Callable.From(resource.Dispose).CallDeferred();
			}
		}
		_missedCacheAssets.Clear();
	}

	public IReadOnlySet<string> GetLoadedCacheAssets()
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (KeyValuePair<string, Resource> item in _cache)
		{
			if (GodotObject.IsInstanceValid(item.Value))
			{
				hashSet.Add(item.Key);
			}
			else
			{
				_cache.TryRemove(item.Key, out Resource _);
			}
		}
		return hashSet;
	}

	public IEnumerable<string> GetCacheKeys()
	{
		return _cache.Keys;
	}

	private Resource? RemoveAndGetResource(string key)
	{
		if (_cache.TryRemove(key, out Resource value))
		{
			return value;
		}
		return null;
	}

	public PackedScene GetScene(string path)
	{
		return (PackedScene)GetAsset(path);
	}

	public Texture2D GetTexture2D(string path)
	{
		return (Texture2D)GetAsset(path);
	}

	public Material GetMaterial(string path)
	{
		return (Material)GetAsset(path);
	}

	public CompressedTexture2D GetCompressedTexture2D(string path)
	{
		return (CompressedTexture2D)GetAsset(path);
	}

	public bool ContainsKey(string s)
	{
		return _cache.ContainsKey(s);
	}

	public void SetAsset(string path, Resource resource)
	{
		_cache[path] = resource;
	}
}
