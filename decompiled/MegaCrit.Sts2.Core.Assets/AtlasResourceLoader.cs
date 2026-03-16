using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Assets;

[ScriptPath("res://src/Core/Assets/AtlasResourceLoader.cs")]
public class AtlasResourceLoader : ResourceFormatLoader
{
	public new class MethodName : ResourceFormatLoader.MethodName
	{
		public new static readonly StringName _GetRecognizedExtensions = "_GetRecognizedExtensions";

		public new static readonly StringName _HandlesType = "_HandlesType";

		public new static readonly StringName _GetResourceType = "_GetResourceType";

		public new static readonly StringName _RecognizePath = "_RecognizePath";

		public new static readonly StringName _Exists = "_Exists";

		public new static readonly StringName _Load = "_Load";

		public new static readonly StringName _GetDependencies = "_GetDependencies";

		public static readonly StringName IsSpritePath = "IsSpritePath";

		public static readonly StringName HasFallback = "HasFallback";

		public static readonly StringName LoadFallback = "LoadFallback";

		public static readonly StringName GetFallbackPath = "GetFallbackPath";

		public static readonly StringName GetRelicFallbackPath = "GetRelicFallbackPath";

		public static readonly StringName GetPowerFallbackPath = "GetPowerFallbackPath";

		public static readonly StringName GetCardFallbackPath = "GetCardFallbackPath";

		public static readonly StringName GetPotionFallbackPath = "GetPotionFallbackPath";

		public static readonly StringName GetMissingTexture = "GetMissingTexture";
	}

	public new class PropertyName : ResourceFormatLoader.PropertyName
	{
	}

	public new class SignalName : ResourceFormatLoader.SignalName
	{
	}

	private const string _atlasBasePath = "res://images/atlases/";

	private const string _spritesSuffix = ".sprites/";

	private static readonly StringName _typeAtlasTexture = new StringName("AtlasTexture");

	private static readonly StringName _typeTexture2D = new StringName("Texture2D");

	private static readonly StringName _typeResource = new StringName("Resource");

	private static readonly Regex _pathPattern = new Regex("^res://images/atlases/([^/]+)\\.sprites/(.+)\\.tres$", RegexOptions.Compiled);

	public override string[] _GetRecognizedExtensions()
	{
		return new string[1] { "tres" };
	}

	public override bool _HandlesType(StringName type)
	{
		if (!(type == _typeAtlasTexture) && !(type == _typeTexture2D))
		{
			return type == _typeResource;
		}
		return true;
	}

	public override string _GetResourceType(string path)
	{
		if (IsSpritePath(path))
		{
			return "AtlasTexture";
		}
		return "";
	}

	public override bool _RecognizePath(string path, StringName type)
	{
		return IsSpritePath(path);
	}

	public override bool _Exists(string path)
	{
		if (!IsSpritePath(path))
		{
			return false;
		}
		var (text, text2) = ParsePath(path);
		if (text == null || text2 == null)
		{
			return false;
		}
		if (!AtlasManager.IsAtlasLoaded(text))
		{
			AtlasManager.LoadAtlas(text);
		}
		if (!AtlasManager.HasSprite(text, text2))
		{
			return HasFallback(text, text2);
		}
		return true;
	}

	public override Variant _Load(string path, string originalPath, bool useSubThreads, int cacheMode)
	{
		if (!IsSpritePath(path))
		{
			return default(Variant);
		}
		var (text, text2) = ParsePath(path);
		if (text == null || text2 == null)
		{
			Log.Warn("AtlasResourceLoader: Failed to parse path: " + path);
			return 7L;
		}
		if (!AtlasManager.IsAtlasLoaded(text))
		{
			AtlasManager.LoadAtlas(text);
		}
		AtlasTexture sprite = AtlasManager.GetSprite(text, text2);
		if (sprite != null)
		{
			return sprite;
		}
		Texture2D texture2D = LoadFallback(text, text2);
		if (texture2D != null)
		{
			return texture2D;
		}
		if (!text2.StartsWith("mock_"))
		{
			Log.Warn($"AtlasResourceLoader: Missing sprite '{text2}' in {text} (requested: {path})");
		}
		return GetMissingTexture(text);
	}

	public override string[] _GetDependencies(string path, bool addTypes)
	{
		return Array.Empty<string>();
	}

	private static bool IsSpritePath(string path)
	{
		if (path.StartsWith("res://images/atlases/") && path.Contains(".sprites/"))
		{
			return path.EndsWith(".tres");
		}
		return false;
	}

	public static (string? AtlasName, string? SpriteName) ParsePath(string path)
	{
		Match match = _pathPattern.Match(path);
		if (!match.Success)
		{
			return (AtlasName: null, SpriteName: null);
		}
		return (AtlasName: match.Groups[1].Value, SpriteName: match.Groups[2].Value);
	}

	private static bool HasFallback(string atlasName, string spriteName)
	{
		string fallbackPath = GetFallbackPath(atlasName, spriteName);
		if (fallbackPath != null)
		{
			return ResourceLoader.Exists(fallbackPath);
		}
		return false;
	}

	private static Texture2D? LoadFallback(string atlasName, string spriteName)
	{
		string fallbackPath = GetFallbackPath(atlasName, spriteName);
		if (fallbackPath == null)
		{
			return null;
		}
		if (!ResourceLoader.Exists(fallbackPath))
		{
			return null;
		}
		Log.Debug($"AtlasResourceLoader: Using fallback for {atlasName}/{spriteName}: {fallbackPath}");
		return ResourceLoader.Load<Texture2D>(fallbackPath, null, ResourceLoader.CacheMode.Reuse);
	}

	private static string? GetFallbackPath(string atlasName, string spriteName)
	{
		switch (atlasName)
		{
		case "relic_atlas":
		case "relic_outline_atlas":
			return GetRelicFallbackPath(spriteName);
		case "power_atlas":
			return GetPowerFallbackPath(spriteName);
		case "card_atlas":
			return GetCardFallbackPath(spriteName);
		case "potion_atlas":
		case "potion_outline_atlas":
			return GetPotionFallbackPath(spriteName);
		default:
			return null;
		}
	}

	private static string? GetRelicFallbackPath(string spriteName)
	{
		string text = "res://images/relics/" + spriteName + ".png";
		if (ResourceLoader.Exists(text))
		{
			return text;
		}
		string text2 = "res://images/relics/beta/" + spriteName + ".png";
		if (ResourceLoader.Exists(text2))
		{
			return text2;
		}
		return null;
	}

	private static string? GetPowerFallbackPath(string spriteName)
	{
		string text = "res://images/powers/" + spriteName + ".png";
		if (ResourceLoader.Exists(text))
		{
			return text;
		}
		string text2 = "res://images/powers/beta/" + spriteName + ".png";
		if (ResourceLoader.Exists(text2))
		{
			return text2;
		}
		return null;
	}

	private static string? GetCardFallbackPath(string spriteName)
	{
		string text = "res://images/packed/card_portraits/" + spriteName + ".png";
		if (ResourceLoader.Exists(text))
		{
			return text;
		}
		int num = spriteName.LastIndexOf('/');
		if (num > 0)
		{
			string value = spriteName.Substring(0, num);
			int num2 = num + 1;
			string value2 = spriteName.Substring(num2, spriteName.Length - num2);
			string text2 = $"res://images/packed/card_portraits/{value}/beta/{value2}.png";
			if (ResourceLoader.Exists(text2))
			{
				return text2;
			}
		}
		return null;
	}

	private static string? GetPotionFallbackPath(string spriteName)
	{
		string text = "res://images/potions/" + spriteName + ".png";
		if (ResourceLoader.Exists(text))
		{
			return text;
		}
		return null;
	}

	private static Variant GetMissingTexture(string atlasName)
	{
		string text = ((!(atlasName == "card_atlas")) ? "res://images/powers/missing_power.png" : "res://images/packed/card_portraits/beta.png");
		string path = text;
		if (ResourceLoader.Exists(path))
		{
			Texture2D texture2D = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
			if (texture2D != null)
			{
				return texture2D;
			}
		}
		return 7L;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(16);
		list.Add(new MethodInfo(MethodName._GetRecognizedExtensions, new PropertyInfo(Variant.Type.PackedStringArray, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._HandlesType, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.StringName, "type", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._GetResourceType, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._RecognizePath, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.StringName, "type", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Exists, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Load, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.NilIsVariant, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "originalPath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "useSubThreads", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "cacheMode", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._GetDependencies, new PropertyInfo(Variant.Type.PackedStringArray, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "addTypes", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.IsSpritePath, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HasFallback, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "atlasName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "spriteName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.LoadFallback, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "atlasName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "spriteName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetFallbackPath, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "atlasName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "spriteName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetRelicFallbackPath, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "spriteName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetPowerFallbackPath, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "spriteName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetCardFallbackPath, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "spriteName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetPotionFallbackPath, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "spriteName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetMissingTexture, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.NilIsVariant, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "atlasName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._GetRecognizedExtensions && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string[]>(_GetRecognizedExtensions());
			return true;
		}
		if (method == MethodName._HandlesType && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(_HandlesType(VariantUtils.ConvertTo<StringName>(in args[0])));
			return true;
		}
		if (method == MethodName._GetResourceType && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(_GetResourceType(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName._RecognizePath && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(_RecognizePath(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<StringName>(in args[1])));
			return true;
		}
		if (method == MethodName._Exists && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(_Exists(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName._Load && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<Variant>(_Load(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]), VariantUtils.ConvertTo<int>(in args[3])));
			return true;
		}
		if (method == MethodName._GetDependencies && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<string[]>(_GetDependencies(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1])));
			return true;
		}
		if (method == MethodName.IsSpritePath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsSpritePath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.HasFallback && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(HasFallback(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		if (method == MethodName.LoadFallback && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<Texture2D>(LoadFallback(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		if (method == MethodName.GetFallbackPath && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<string>(GetFallbackPath(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		if (method == MethodName.GetRelicFallbackPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetRelicFallbackPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.GetPowerFallbackPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetPowerFallbackPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.GetCardFallbackPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetCardFallbackPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.GetPotionFallbackPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetPotionFallbackPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.GetMissingTexture && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Variant>(GetMissingTexture(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.IsSpritePath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsSpritePath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.HasFallback && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(HasFallback(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		if (method == MethodName.LoadFallback && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<Texture2D>(LoadFallback(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		if (method == MethodName.GetFallbackPath && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<string>(GetFallbackPath(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		if (method == MethodName.GetRelicFallbackPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetRelicFallbackPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.GetPowerFallbackPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetPowerFallbackPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.GetCardFallbackPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetCardFallbackPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.GetPotionFallbackPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetPotionFallbackPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.GetMissingTexture && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Variant>(GetMissingTexture(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._GetRecognizedExtensions)
		{
			return true;
		}
		if (method == MethodName._HandlesType)
		{
			return true;
		}
		if (method == MethodName._GetResourceType)
		{
			return true;
		}
		if (method == MethodName._RecognizePath)
		{
			return true;
		}
		if (method == MethodName._Exists)
		{
			return true;
		}
		if (method == MethodName._Load)
		{
			return true;
		}
		if (method == MethodName._GetDependencies)
		{
			return true;
		}
		if (method == MethodName.IsSpritePath)
		{
			return true;
		}
		if (method == MethodName.HasFallback)
		{
			return true;
		}
		if (method == MethodName.LoadFallback)
		{
			return true;
		}
		if (method == MethodName.GetFallbackPath)
		{
			return true;
		}
		if (method == MethodName.GetRelicFallbackPath)
		{
			return true;
		}
		if (method == MethodName.GetPowerFallbackPath)
		{
			return true;
		}
		if (method == MethodName.GetCardFallbackPath)
		{
			return true;
		}
		if (method == MethodName.GetPotionFallbackPath)
		{
			return true;
		}
		if (method == MethodName.GetMissingTexture)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
