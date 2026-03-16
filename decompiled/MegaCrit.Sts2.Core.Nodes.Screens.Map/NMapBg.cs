using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

[ScriptPath("res://src/Core/Nodes/Screens/Map/NMapBg.cs")]
public class NMapBg : VBoxContainer
{
	public new class MethodName : VBoxContainer.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnVisibilityChanged = "OnVisibilityChanged";

		public static readonly StringName OnWindowChange = "OnWindowChange";
	}

	public new class PropertyName : VBoxContainer.PropertyName
	{
		public static readonly StringName _mapTop = "_mapTop";

		public static readonly StringName _mapMid = "_mapMid";

		public static readonly StringName _mapBot = "_mapBot";

		public static readonly StringName _drawings = "_drawings";

		public static readonly StringName _window = "_window";

		public static readonly StringName _offsetX = "_offsetX";
	}

	public new class SignalName : VBoxContainer.SignalName
	{
	}

	private IRunState _runState;

	private TextureRect _mapTop;

	private TextureRect _mapMid;

	private TextureRect _mapBot;

	private NMapDrawings _drawings;

	private Window _window;

	private const float _sixteenByNine = 1.7777778f;

	private const float _fourByThree = 1.3333334f;

	private const float _defaultY = -1620f;

	private const float _adjustY = -1540f;

	private float _offsetX;

	public override void _Ready()
	{
		_mapTop = GetNode<TextureRect>("MapTop");
		_mapMid = GetNode<TextureRect>("MapMid");
		_mapBot = GetNode<TextureRect>("MapBot");
		_drawings = GetNode<NMapDrawings>("%Drawings");
		_window = GetTree().Root;
		_window.Connect(Viewport.SignalName.SizeChanged, Callable.From(OnWindowChange));
		OnWindowChange();
		_offsetX = base.Position.X;
		Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(OnVisibilityChanged));
	}

	public void Initialize(IRunState runState)
	{
		_runState = runState;
	}

	private void OnVisibilityChanged()
	{
		ActModel act = _runState.Act;
		_mapTop.Texture = act.MapTopBg;
		_mapMid.Texture = act.MapMidBg;
		_mapBot.Texture = act.MapBotBg;
	}

	private void OnWindowChange()
	{
		float num = Math.Max(1.3333334f, (float)_window.Size.X / (float)_window.Size.Y);
		if (num < 1.7777778f)
		{
			float p = (num - 1.3333334f) / 0.44444442f;
			base.Position = new Vector2(_offsetX, Mathf.Remap(Ease.CubicOut(p), 0f, 1f, -1540f, -1620f));
		}
		else
		{
			base.Position = new Vector2(_offsetX, -1620f);
		}
		_drawings.RepositionBasedOnBackground(this);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnVisibilityChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnVisibilityChanged && args.Count == 0)
		{
			OnVisibilityChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnWindowChange && args.Count == 0)
		{
			OnWindowChange();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnVisibilityChanged)
		{
			return true;
		}
		if (method == MethodName.OnWindowChange)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._mapTop)
		{
			_mapTop = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._mapMid)
		{
			_mapMid = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._mapBot)
		{
			_mapBot = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._drawings)
		{
			_drawings = VariantUtils.ConvertTo<NMapDrawings>(in value);
			return true;
		}
		if (name == PropertyName._window)
		{
			_window = VariantUtils.ConvertTo<Window>(in value);
			return true;
		}
		if (name == PropertyName._offsetX)
		{
			_offsetX = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._mapTop)
		{
			value = VariantUtils.CreateFrom(in _mapTop);
			return true;
		}
		if (name == PropertyName._mapMid)
		{
			value = VariantUtils.CreateFrom(in _mapMid);
			return true;
		}
		if (name == PropertyName._mapBot)
		{
			value = VariantUtils.CreateFrom(in _mapBot);
			return true;
		}
		if (name == PropertyName._drawings)
		{
			value = VariantUtils.CreateFrom(in _drawings);
			return true;
		}
		if (name == PropertyName._window)
		{
			value = VariantUtils.CreateFrom(in _window);
			return true;
		}
		if (name == PropertyName._offsetX)
		{
			value = VariantUtils.CreateFrom(in _offsetX);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mapTop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mapMid, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mapBot, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._drawings, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._window, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._offsetX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._mapTop, Variant.From(in _mapTop));
		info.AddProperty(PropertyName._mapMid, Variant.From(in _mapMid));
		info.AddProperty(PropertyName._mapBot, Variant.From(in _mapBot));
		info.AddProperty(PropertyName._drawings, Variant.From(in _drawings));
		info.AddProperty(PropertyName._window, Variant.From(in _window));
		info.AddProperty(PropertyName._offsetX, Variant.From(in _offsetX));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._mapTop, out var value))
		{
			_mapTop = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._mapMid, out var value2))
		{
			_mapMid = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._mapBot, out var value3))
		{
			_mapBot = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._drawings, out var value4))
		{
			_drawings = value4.As<NMapDrawings>();
		}
		if (info.TryGetProperty(PropertyName._window, out var value5))
		{
			_window = value5.As<Window>();
		}
		if (info.TryGetProperty(PropertyName._offsetX, out var value6))
		{
			_offsetX = value6.As<float>();
		}
	}
}
