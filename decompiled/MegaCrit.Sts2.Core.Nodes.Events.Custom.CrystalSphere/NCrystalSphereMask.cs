using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent;

namespace MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere;

[ScriptPath("res://src/Core/Nodes/Events/Custom/CrystalSphere/NCrystalSphereMask.cs")]
public class NCrystalSphereMask : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _material = "_material";

		public static readonly StringName _values = "_values";

		public static readonly StringName _time = "_time";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly StringName _gridFadeParams = new StringName("gridFadeParams");

	private static readonly StringName _timeStr = new StringName("time");

	private ShaderMaterial _material;

	private Array<Vector3> _values = new Array<Vector3>();

	private float _time;

	public override void _Ready()
	{
		_material = (ShaderMaterial)base.Material;
		for (int i = 0; i < 121; i++)
		{
			_values.Add(Vector3.Zero);
		}
		_values[3] = new Vector3(1f, 0f, 0f);
	}

	public override void _Process(double delta)
	{
		_time += (float)delta;
		_material.SetShaderParameter(_timeStr, _time);
	}

	public void UpdateMat(CrystalSphereCell cell)
	{
		int index = cell.Y * 11 + cell.X;
		float x = _values[index].Y;
		if (_values[index].Z == 0f)
		{
			x = 1f;
		}
		_values[index] = new Vector3(x, cell.IsHidden ? 1 : 0, _time);
		_material.SetShaderParameter(_gridFadeParams, _values);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._Process)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._material)
		{
			_material = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._values)
		{
			_values = VariantUtils.ConvertToArray<Vector3>(in value);
			return true;
		}
		if (name == PropertyName._time)
		{
			_time = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._material)
		{
			value = VariantUtils.CreateFrom(in _material);
			return true;
		}
		if (name == PropertyName._values)
		{
			value = VariantUtils.CreateFromArray(_values);
			return true;
		}
		if (name == PropertyName._time)
		{
			value = VariantUtils.CreateFrom(in _time);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._material, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._values, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._time, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._material, Variant.From(in _material));
		info.AddProperty(PropertyName._values, Variant.CreateFrom(_values));
		info.AddProperty(PropertyName._time, Variant.From(in _time));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._material, out var value))
		{
			_material = value.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._values, out var value2))
		{
			_values = value2.AsGodotArray<Vector3>();
		}
		if (info.TryGetProperty(PropertyName._time, out var value3))
		{
			_time = value3.As<float>();
		}
	}
}
