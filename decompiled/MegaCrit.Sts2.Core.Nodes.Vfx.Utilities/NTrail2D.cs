using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

[ScriptPath("res://src/Core/Nodes/Vfx/Utilities/NTrail2D.cs")]
public class NTrail2D : Line2D
{
	public new class MethodName : Line2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnToggleVisibility = "OnToggleVisibility";

		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Line2D.PropertyName
	{
		public static readonly StringName _maxSegments = "_maxSegments";

		public static readonly StringName _parent = "_parent";

		public static readonly StringName _isActive = "_isActive";
	}

	public new class SignalName : Line2D.SignalName
	{
	}

	private int _maxSegments = 20;

	private Node2D _parent;

	private readonly List<Vector2> _pointQueue = new List<Vector2>();

	private bool _isActive;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(OnToggleVisibility));
		OnToggleVisibility();
	}

	private void OnToggleVisibility()
	{
		_isActive = base.Visible;
		if (!base.Visible)
		{
			_pointQueue.Clear();
			base.Points = _pointQueue.ToArray();
		}
	}

	public override void _Process(double delta)
	{
		if (_isActive)
		{
			_pointQueue.Insert(0, _parent.GlobalPosition);
			if (_pointQueue.Count >= _maxSegments)
			{
				_pointQueue.RemoveAt(_pointQueue.Count - 1);
			}
			base.Points = _pointQueue.Select((Vector2 point) => GetParent<Node2D>().ToLocal(point)).ToArray();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnToggleVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.OnToggleVisibility && args.Count == 0)
		{
			OnToggleVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
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
		if (method == MethodName.OnToggleVisibility)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._maxSegments)
		{
			_maxSegments = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._isActive)
		{
			_isActive = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._maxSegments)
		{
			value = VariantUtils.CreateFrom(in _maxSegments);
			return true;
		}
		if (name == PropertyName._parent)
		{
			value = VariantUtils.CreateFrom(in _parent);
			return true;
		}
		if (name == PropertyName._isActive)
		{
			value = VariantUtils.CreateFrom(in _isActive);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._maxSegments, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isActive, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._maxSegments, Variant.From(in _maxSegments));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
		info.AddProperty(PropertyName._isActive, Variant.From(in _isActive));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._maxSegments, out var value))
		{
			_maxSegments = value.As<int>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value2))
		{
			_parent = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._isActive, out var value3))
		{
			_isActive = value3.As<bool>();
		}
	}
}
