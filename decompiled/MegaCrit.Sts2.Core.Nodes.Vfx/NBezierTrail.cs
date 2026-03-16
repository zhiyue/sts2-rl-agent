using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NBezierTrail.cs")]
public class NBezierTrail : Line2D
{
	public new class MethodName : Line2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName OnToggleVisibility = "OnToggleVisibility";

		public static readonly StringName CreatePoint = "CreatePoint";
	}

	public new class PropertyName : Line2D.PropertyName
	{
		public static readonly StringName _target = "_target";

		public static readonly StringName _pointDuration = "_pointDuration";
	}

	public new class SignalName : Line2D.SignalName
	{
	}

	private Node2D _target;

	private float _pointDuration = 0.5f;

	private readonly List<float> _pointAge = new List<float>();

	private const float _minSpawnDist = 12f;

	private const float _maxSpawnDist = 48f;

	private Vector2? _lastPointPosition;

	public override void _Ready()
	{
		_target = GetParent<Node2D>();
		Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(OnToggleVisibility));
	}

	public override void _Process(double delta)
	{
		base.GlobalPosition = Vector2.Zero;
		base.GlobalRotation = 0f;
		CreatePoint(_target.GlobalPosition, delta);
		float num = (float)delta;
		for (int i = 0; i < base.Points.Length; i++)
		{
			if (_pointAge[i] > _pointDuration)
			{
				RemovePoint(0);
				_pointAge.RemoveAt(0);
			}
			else
			{
				_pointAge[i] += num;
			}
		}
	}

	private void OnToggleVisibility()
	{
		base.ProcessMode = (ProcessModeEnum)(base.Visible ? 0 : 4);
		ClearPoints();
	}

	private void CreatePoint(Vector2 pointPos, double delta)
	{
		if (_lastPointPosition.HasValue)
		{
			float num = pointPos.DistanceTo(_lastPointPosition.Value);
			if (num < 12f)
			{
				return;
			}
			int pointCount = GetPointCount();
			if (pointCount > 2 && num > 48f)
			{
				Vector2 pointPosition = GetPointPosition(pointCount - 2);
				Vector2 pointPosition2 = GetPointPosition(pointCount - 1);
				for (float num2 = 48f; num2 < num - 12f; num2 += 48f)
				{
					float num3 = 0.5f + num2 / num * 0.5f;
					Vector2 vector = pointPosition.Lerp(pointPosition2, num3);
					Vector2 to = pointPosition2.Lerp(pointPos, num3);
					Vector2 position = vector.Lerp(to, num3);
					_pointAge.Add((float)delta * num3);
					AddPoint(position);
				}
			}
		}
		_pointAge.Add(0f);
		AddPoint(pointPos);
		_lastPointPosition = pointPos;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnToggleVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreatePoint, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "pointPos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
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
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnToggleVisibility && args.Count == 0)
		{
			OnToggleVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreatePoint && args.Count == 2)
		{
			CreatePoint(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<double>(in args[1]));
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
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.OnToggleVisibility)
		{
			return true;
		}
		if (method == MethodName.CreatePoint)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._target)
		{
			_target = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._pointDuration)
		{
			_pointDuration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._target)
		{
			value = VariantUtils.CreateFrom(in _target);
			return true;
		}
		if (name == PropertyName._pointDuration)
		{
			value = VariantUtils.CreateFrom(in _pointDuration);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._target, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._pointDuration, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._target, Variant.From(in _target));
		info.AddProperty(PropertyName._pointDuration, Variant.From(in _pointDuration));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._target, out var value))
		{
			_target = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._pointDuration, out var value2))
		{
			_pointDuration = value2.As<float>();
		}
	}
}
