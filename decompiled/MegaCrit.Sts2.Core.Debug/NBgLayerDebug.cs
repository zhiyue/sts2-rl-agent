using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Debug;

[Tool]
[ScriptPath("res://src/Core/Debug/NBgLayerDebug.cs")]
public class NBgLayerDebug : Control
{
	public enum LayerVisibility
	{
		A,
		B,
		C
	}

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _EnterTree = "_EnterTree";

		public static readonly StringName ReloadLayers = "ReloadLayers";

		public static readonly StringName UpdateLayers = "UpdateLayers";

		public static readonly StringName AddLayer = "AddLayer";

		public static readonly StringName ToLayerName = "ToLayerName";

		public static readonly StringName ClearLayers = "ClearLayers";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName VisibleLayer = "VisibleLayer";

		public static readonly StringName ReloadLayersCallable = "ReloadLayersCallable";

		public static readonly StringName _layerA = "_layerA";

		public static readonly StringName _layerB = "_layerB";

		public static readonly StringName _layerC = "_layerC";

		public static readonly StringName _visibleLayer = "_visibleLayer";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const string _layerNodePrefix = "Layer_";

	private PackedScene? _layerA;

	private PackedScene? _layerB;

	private PackedScene? _layerC;

	private LayerVisibility _visibleLayer;

	[Export(PropertyHint.None, "")]
	public LayerVisibility VisibleLayer
	{
		get
		{
			return _visibleLayer;
		}
		set
		{
			_visibleLayer = value;
			if (Engine.IsEditorHint())
			{
				UpdateLayers();
			}
		}
	}

	[ExportToolButton("Reload Layers")]
	private Callable ReloadLayersCallable => Callable.From(ReloadLayers);

	public override void _EnterTree()
	{
		if (Engine.IsEditorHint())
		{
			ReloadLayers();
		}
	}

	private void ReloadLayers()
	{
		string sceneFilePath = GetTree().GetEditedSceneRoot().SceneFilePath;
		if (sceneFilePath == null)
		{
			return;
		}
		string text;
		if (base.Name.ToString() == "Foreground")
		{
			text = "fg";
		}
		else
		{
			string text2 = base.Name.ToString();
			int length = "Layer_".Length;
			if (!int.TryParse(text2.Substring(length, text2.Length - length), out var result))
			{
				return;
			}
			text = $"bg_{result:D2}";
		}
		string file = sceneFilePath.GetFile();
		string text3 = file.Substring(0, file.LastIndexOf('_'));
		string text4 = Path.Combine(sceneFilePath.GetBaseDir(), "layers", text3 + "_" + text);
		string path = text4 + "_a.tscn";
		string path2 = text4 + "_b.tscn";
		string path3 = text4 + "_c.tscn";
		if (ResourceLoader.Exists(path))
		{
			_layerA = ResourceLoader.Load<PackedScene>(path, null, ResourceLoader.CacheMode.Reuse);
		}
		if (ResourceLoader.Exists(path2))
		{
			_layerB = ResourceLoader.Load<PackedScene>(path2, null, ResourceLoader.CacheMode.Reuse);
		}
		if (ResourceLoader.Exists(path3))
		{
			_layerC = ResourceLoader.Load<PackedScene>(path3, null, ResourceLoader.CacheMode.Reuse);
		}
		UpdateLayers();
	}

	private void UpdateLayers()
	{
		ClearLayers();
		if (_visibleLayer == LayerVisibility.A && _layerA != null)
		{
			AddLayer(LayerVisibility.A, _layerA);
		}
		if (_visibleLayer == LayerVisibility.B && _layerB != null)
		{
			AddLayer(LayerVisibility.B, _layerB);
		}
		if (_visibleLayer == LayerVisibility.C && _layerC != null)
		{
			AddLayer(LayerVisibility.C, _layerC);
		}
	}

	private void AddLayer(LayerVisibility name, PackedScene layerScene)
	{
		Control control = layerScene.Instantiate<Control>(PackedScene.GenEditState.Disabled);
		control.Name = ToLayerName(name);
		this.AddChildSafely(control);
	}

	private static string ToLayerName(LayerVisibility layer)
	{
		return $"{"Layer_"}{layer}";
	}

	private IEnumerable<Control> GetLayerNodes()
	{
		foreach (Node child in GetChildren())
		{
			if (child.Name.ToString().StartsWith("Layer_"))
			{
				yield return (Control)child;
			}
		}
	}

	private void ClearLayers()
	{
		foreach (Control layerNode in GetLayerNodes())
		{
			this.RemoveChildSafely(layerNode);
			layerNode.QueueFreeSafely();
		}
	}

	public override void _ExitTree()
	{
		ClearLayers();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ReloadLayers, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateLayers, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AddLayer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "name", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Object, "layerScene", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("PackedScene"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ToLayerName, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "layer", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ClearLayers, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ReloadLayers && args.Count == 0)
		{
			ReloadLayers();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateLayers && args.Count == 0)
		{
			UpdateLayers();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddLayer && args.Count == 2)
		{
			AddLayer(VariantUtils.ConvertTo<LayerVisibility>(in args[0]), VariantUtils.ConvertTo<PackedScene>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToLayerName && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ToLayerName(VariantUtils.ConvertTo<LayerVisibility>(in args[0])));
			return true;
		}
		if (method == MethodName.ClearLayers && args.Count == 0)
		{
			ClearLayers();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.ToLayerName && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ToLayerName(VariantUtils.ConvertTo<LayerVisibility>(in args[0])));
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
		if (method == MethodName.ReloadLayers)
		{
			return true;
		}
		if (method == MethodName.UpdateLayers)
		{
			return true;
		}
		if (method == MethodName.AddLayer)
		{
			return true;
		}
		if (method == MethodName.ToLayerName)
		{
			return true;
		}
		if (method == MethodName.ClearLayers)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.VisibleLayer)
		{
			VisibleLayer = VariantUtils.ConvertTo<LayerVisibility>(in value);
			return true;
		}
		if (name == PropertyName._layerA)
		{
			_layerA = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._layerB)
		{
			_layerB = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._layerC)
		{
			_layerC = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._visibleLayer)
		{
			_visibleLayer = VariantUtils.ConvertTo<LayerVisibility>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.VisibleLayer)
		{
			value = VariantUtils.CreateFrom<LayerVisibility>(VisibleLayer);
			return true;
		}
		if (name == PropertyName.ReloadLayersCallable)
		{
			value = VariantUtils.CreateFrom<Callable>(ReloadLayersCallable);
			return true;
		}
		if (name == PropertyName._layerA)
		{
			value = VariantUtils.CreateFrom(in _layerA);
			return true;
		}
		if (name == PropertyName._layerB)
		{
			value = VariantUtils.CreateFrom(in _layerB);
			return true;
		}
		if (name == PropertyName._layerC)
		{
			value = VariantUtils.CreateFrom(in _layerC);
			return true;
		}
		if (name == PropertyName._visibleLayer)
		{
			value = VariantUtils.CreateFrom(in _visibleLayer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._layerA, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._layerB, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._layerC, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._visibleLayer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.VisibleLayer, PropertyHint.Enum, "A,B,C", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Callable, PropertyName.ReloadLayersCallable, PropertyHint.ToolButton, "Reload Layers", PropertyUsageFlags.Editor, exported: true));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.VisibleLayer, Variant.From<LayerVisibility>(VisibleLayer));
		info.AddProperty(PropertyName._layerA, Variant.From(in _layerA));
		info.AddProperty(PropertyName._layerB, Variant.From(in _layerB));
		info.AddProperty(PropertyName._layerC, Variant.From(in _layerC));
		info.AddProperty(PropertyName._visibleLayer, Variant.From(in _visibleLayer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.VisibleLayer, out var value))
		{
			VisibleLayer = value.As<LayerVisibility>();
		}
		if (info.TryGetProperty(PropertyName._layerA, out var value2))
		{
			_layerA = value2.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._layerB, out var value3))
		{
			_layerB = value3.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._layerC, out var value4))
		{
			_layerC = value4.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._visibleLayer, out var value5))
		{
			_visibleLayer = value5.As<LayerVisibility>();
		}
	}
}
