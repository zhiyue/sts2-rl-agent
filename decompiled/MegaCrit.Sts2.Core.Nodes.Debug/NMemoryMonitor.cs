using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Nodes.Debug;

[ScriptPath("res://src/Core/Nodes/Debug/NMemoryMonitor.cs")]
public class NMemoryMonitor : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName StopMemoryMonitoring = "StopMemoryMonitoring";

		public static readonly StringName PrintMemoryUsage = "PrintMemoryUsage";

		public static readonly StringName FormatMemoryUsage = "FormatMemoryUsage";

		public static readonly StringName PrintLargestAssets = "PrintLargestAssets";

		public static readonly StringName GetFileSizeInMb = "GetFileSizeInMb";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _isMonitoring = "_isMonitoring";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private bool _isMonitoring;

	public override void _EnterTree()
	{
	}

	public override void _ExitTree()
	{
		StopMemoryMonitoring();
	}

	private async Task StartMemoryMonitoring()
	{
		_isMonitoring = true;
		while (_isMonitoring)
		{
			PrintMemoryUsage();
			PrintLargestAssets();
			await Task.Delay(10000);
		}
	}

	private void StopMemoryMonitoring()
	{
		_isMonitoring = false;
	}

	private static void PrintMemoryUsage()
	{
		Log.Info($"GetStaticMemoryUsage={OS.GetStaticMemoryUsage()}");
		Log.Info($"GetStaticMemoryPeakUsage={OS.GetStaticMemoryPeakUsage()}");
	}

	private static string FormatMemoryUsage(long bytes)
	{
		string[] array = new string[5] { "B", "KB", "MB", "GB", "TB" };
		int num = 0;
		while (bytes >= 1024 && num < array.Length - 1)
		{
			num++;
			bytes /= (long)Math.Pow(1024.0, num);
		}
		return $"{bytes:0.##} {array[num]}";
	}

	private static void PrintLargestAssets()
	{
		var enumerable = (from assetPath in PreloadManager.Cache.GetCacheKeys()
			select new
			{
				Path = assetPath,
				Size = GetFileSizeInMb(assetPath)
			} into file
			orderby file.Size descending
			select file).Take(10);
		Log.Info("Top 10 largest files in asset cache:");
		foreach (var item in enumerable)
		{
			Log.Info($"Size: {item.Size:F3} MB, Path: {item.Path}");
		}
	}

	private static float GetFileSizeInMb(string godotPath)
	{
		string text = ProjectSettings.GlobalizePath(godotPath);
		if (File.Exists(text))
		{
			long length = new FileInfo(text).Length;
			return (float)length / 1048576f;
		}
		Log.Info("File does not exist: " + text);
		return 0f;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StopMemoryMonitoring, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PrintMemoryUsage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.FormatMemoryUsage, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "bytes", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PrintLargestAssets, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.GetFileSizeInMb, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "godotPath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopMemoryMonitoring && args.Count == 0)
		{
			StopMemoryMonitoring();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PrintMemoryUsage && args.Count == 0)
		{
			PrintMemoryUsage();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FormatMemoryUsage && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(FormatMemoryUsage(VariantUtils.ConvertTo<long>(in args[0])));
			return true;
		}
		if (method == MethodName.PrintLargestAssets && args.Count == 0)
		{
			PrintLargestAssets();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetFileSizeInMb && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<float>(GetFileSizeInMb(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.PrintMemoryUsage && args.Count == 0)
		{
			PrintMemoryUsage();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FormatMemoryUsage && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(FormatMemoryUsage(VariantUtils.ConvertTo<long>(in args[0])));
			return true;
		}
		if (method == MethodName.PrintLargestAssets && args.Count == 0)
		{
			PrintLargestAssets();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetFileSizeInMb && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<float>(GetFileSizeInMb(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.StopMemoryMonitoring)
		{
			return true;
		}
		if (method == MethodName.PrintMemoryUsage)
		{
			return true;
		}
		if (method == MethodName.FormatMemoryUsage)
		{
			return true;
		}
		if (method == MethodName.PrintLargestAssets)
		{
			return true;
		}
		if (method == MethodName.GetFileSizeInMb)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._isMonitoring)
		{
			_isMonitoring = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._isMonitoring)
		{
			value = VariantUtils.CreateFrom(in _isMonitoring);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isMonitoring, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._isMonitoring, Variant.From(in _isMonitoring));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._isMonitoring, out var value))
		{
			_isMonitoring = value.As<bool>();
		}
	}
}
