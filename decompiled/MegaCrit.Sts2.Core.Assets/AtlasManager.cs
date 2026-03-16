using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Assets;

public static class AtlasManager
{
	private class AtlasData
	{
		public required TpSheetData TpSheet { get; init; }

		public required Dictionary<string, Texture2D> PageTextures { get; init; }

		public required Dictionary<string, SpriteInfo> SpriteMap { get; init; }
	}

	private record SpriteInfo(Texture2D Atlas, TpSheetSprite Sprite);

	private static readonly string[] _knownAtlases = new string[12]
	{
		"ui_atlas", "compressed", "epoch_atlas", "relic_atlas", "relic_outline_atlas", "power_atlas", "card_atlas", "potion_atlas", "potion_outline_atlas", "stats_screen_atlas",
		"intent_atlas", "era_atlas"
	};

	private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	private static readonly ConcurrentDictionary<string, AtlasData> _atlases = new ConcurrentDictionary<string, AtlasData>();

	private static readonly ConcurrentDictionary<string, AtlasTexture> _spriteCache = new ConcurrentDictionary<string, AtlasTexture>();

	private static readonly Lock _loadLock = new Lock();

	private static readonly string[] _essentialAtlases = new string[2] { "ui_atlas", "compressed" };

	public static void LoadAllAtlases()
	{
		string[] knownAtlases = _knownAtlases;
		foreach (string atlasName in knownAtlases)
		{
			LoadAtlas(atlasName);
		}
	}

	public static void LoadEssentialAtlases()
	{
		string[] essentialAtlases = _essentialAtlases;
		foreach (string atlasName in essentialAtlases)
		{
			LoadAtlas(atlasName);
		}
	}

	public static void LoadAtlas(string atlasName)
	{
		if (_atlases.ContainsKey(atlasName))
		{
			return;
		}
		using (_loadLock.EnterScope())
		{
			if (!_atlases.ContainsKey(atlasName))
			{
				LoadAtlasInternal(atlasName);
			}
		}
	}

	private static void LoadAtlasInternal(string atlasName)
	{
		string text = "res://images/atlases/" + atlasName + ".tpsheet";
		if (!FileAccess.FileExists(text))
		{
			Log.Warn("AtlasManager: tpsheet not found: " + text);
			return;
		}
		using FileAccess fileAccess = FileAccess.Open(text, FileAccess.ModeFlags.Read);
		if (fileAccess == null)
		{
			Log.Warn("AtlasManager: Failed to open " + text);
			return;
		}
		string asText = fileAccess.GetAsText();
		TpSheetData tpSheetData = JsonSerializer.Deserialize<TpSheetData>(asText, _jsonOptions);
		if (tpSheetData == null)
		{
			Log.Warn("AtlasManager: Failed to parse " + text);
			return;
		}
		Dictionary<string, Texture2D> dictionary = new Dictionary<string, Texture2D>();
		Dictionary<string, SpriteInfo> dictionary2 = new Dictionary<string, SpriteInfo>();
		foreach (TpSheetTexture texture in tpSheetData.Textures)
		{
			string text2 = "res://images/atlases/" + texture.Image;
			Texture2D texture2D = ResourceLoader.Load<Texture2D>(text2, null, ResourceLoader.CacheMode.Reuse);
			if (texture2D == null)
			{
				Log.Warn("AtlasManager: Failed to load texture: " + text2);
				continue;
			}
			dictionary[texture.Image] = texture2D;
			foreach (TpSheetSprite sprite in texture.Sprites)
			{
				string key = NormalizeSpriteKey(sprite.Filename);
				dictionary2[key] = new SpriteInfo(texture2D, sprite);
			}
		}
		AtlasData value = new AtlasData
		{
			TpSheet = tpSheetData,
			PageTextures = dictionary,
			SpriteMap = dictionary2
		};
		_atlases[atlasName] = value;
		Log.Info($"AtlasManager: Loaded {atlasName} with {dictionary2.Count} sprites");
	}

	public static AtlasTexture? GetSprite(string atlasName, string spriteName)
	{
		if (!_atlases.TryGetValue(atlasName, out AtlasData value))
		{
			return null;
		}
		string key = NormalizeSpriteKey(spriteName);
		if (!value.SpriteMap.TryGetValue(key, out SpriteInfo spriteInfo))
		{
			return null;
		}
		string key2 = atlasName + "/" + spriteName;
		if (_spriteCache.TryGetValue(key2, out AtlasTexture value2))
		{
			if (GodotObject.IsInstanceValid(value2))
			{
				return value2;
			}
			_spriteCache.TryRemove(key2, out AtlasTexture _);
		}
		return _spriteCache.GetOrAdd(key2, (string _) => CreateAtlasTexture(spriteInfo));
	}

	public static bool HasSprite(string atlasName, string spriteName)
	{
		if (!_atlases.TryGetValue(atlasName, out AtlasData value))
		{
			return false;
		}
		string key = NormalizeSpriteKey(spriteName);
		return value.SpriteMap.ContainsKey(key);
	}

	public static int GetSpriteCount(string atlasName)
	{
		if (!_atlases.TryGetValue(atlasName, out AtlasData value))
		{
			return 0;
		}
		return value.SpriteMap.Count;
	}

	public static bool IsAtlasLoaded(string atlasName)
	{
		return _atlases.ContainsKey(atlasName);
	}

	public static void Clear()
	{
		_atlases.Clear();
		_spriteCache.Clear();
	}

	private static AtlasTexture CreateAtlasTexture(SpriteInfo spriteInfo)
	{
		TpSheetSprite sprite = spriteInfo.Sprite;
		AtlasTexture atlasTexture = new AtlasTexture
		{
			Atlas = spriteInfo.Atlas,
			Region = new Rect2(sprite.Region.X, sprite.Region.Y, sprite.Region.W, sprite.Region.H)
		};
		if (sprite.Margin.X != 0 || sprite.Margin.Y != 0 || sprite.Margin.W != 0 || sprite.Margin.H != 0)
		{
			atlasTexture.Margin = new Rect2(sprite.Margin.X, sprite.Margin.Y, sprite.Margin.W, sprite.Margin.H);
		}
		return atlasTexture;
	}

	private static string NormalizeSpriteKey(string filename)
	{
		if (filename.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
		{
			return filename.Substring(0, filename.Length - 4);
		}
		return filename;
	}
}
