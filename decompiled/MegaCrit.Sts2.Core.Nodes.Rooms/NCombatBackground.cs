using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Nodes.Rooms;

[ScriptPath("res://src/Core/Nodes/Rooms/NCombatBackground.cs")]
public class NCombatBackground : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName SetForegroundLayer = "SetForegroundLayer";

		public static readonly StringName AddLayer = "AddLayer";
	}

	public new class PropertyName : Control.PropertyName
	{
	}

	public new class SignalName : Control.SignalName
	{
	}

	public static NCombatBackground Create(BackgroundAssets bg)
	{
		if (bg.BackgroundScenePath == null)
		{
			throw new InvalidOperationException("Encounter does not have a background.");
		}
		string backgroundScenePath = bg.BackgroundScenePath;
		NCombatBackground nCombatBackground = PreloadManager.Cache.GetScene(backgroundScenePath).Instantiate<NCombatBackground>(PackedScene.GenEditState.Disabled);
		nCombatBackground.SetLayers(bg);
		return nCombatBackground;
	}

	private void SetLayers(BackgroundAssets bg)
	{
		SetBackgroundLayers(bg.BgLayers);
		SetForegroundLayer(bg.FgLayer);
	}

	private void SetBackgroundLayers(IReadOnlyList<string> backgroundLayers)
	{
		for (int i = 0; i < backgroundLayers.Count; i++)
		{
			string layerName = $"Layer_{i:D2}";
			AddLayer(layerName, backgroundLayers[i]);
		}
	}

	private void SetForegroundLayer(string? foregroundLayer)
	{
		if (foregroundLayer != null)
		{
			AddLayer("Foreground", foregroundLayer);
		}
	}

	private void AddLayer(string layerName, string layerPath)
	{
		Node nodeOrNull = GetNodeOrNull(layerName);
		if (nodeOrNull == null)
		{
			throw new InvalidOperationException("Layer node='" + layerName + "' not found in combat background scene.");
		}
		Control control = PreloadManager.Cache.GetScene(layerPath).Instantiate<Control>(PackedScene.GenEditState.Disabled);
		control.Visible = true;
		nodeOrNull.AddChildSafely(control);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName.SetForegroundLayer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "foregroundLayer", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AddLayer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "layerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "layerPath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.SetForegroundLayer && args.Count == 1)
		{
			SetForegroundLayer(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddLayer && args.Count == 2)
		{
			AddLayer(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.SetForegroundLayer)
		{
			return true;
		}
		if (method == MethodName.AddLayer)
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
