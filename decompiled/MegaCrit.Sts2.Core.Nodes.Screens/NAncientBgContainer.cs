using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Screens;

[ScriptPath("res://src/Core/Nodes/Screens/NAncientBgContainer.cs")]
public class NAncientBgContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnWindowChange = "OnWindowChange";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _window = "_window";

		public static readonly StringName pos4_3 = "pos4_3";

		public static readonly StringName scale4_3 = "scale4_3";

		public static readonly StringName pos16_9 = "pos16_9";

		public static readonly StringName scale16_9 = "scale16_9";

		public static readonly StringName pos21_9 = "pos21_9";

		public static readonly StringName scale21_9 = "scale21_9";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Window _window;

	private const float _ratioMin = 1.3333f;

	private const float _ratioNormal = 1.7777f;

	private const float _ratioMax = 2.3333f;

	private Vector2 pos4_3 = new Vector2(-140f, 110f);

	private Vector2 scale4_3 = new Vector2(1f, 1f);

	private Vector2 pos16_9 = new Vector2(0f, 40f);

	private Vector2 scale16_9 = new Vector2(0.89f, 0.89f);

	private Vector2 pos21_9 = new Vector2(330f, 40f);

	private Vector2 scale21_9 = new Vector2(1f, 1f);

	public override void _Ready()
	{
		_window = GetTree().Root;
		_window.Connect(Viewport.SignalName.SizeChanged, Callable.From(OnWindowChange));
		OnWindowChange();
	}

	private void OnWindowChange()
	{
		float num = Mathf.Clamp(base.Size.X / base.Size.Y, 1.3333f, 2.3333f);
		base.PivotOffset = base.Size * 0.5f;
		if (num < 1.7777f)
		{
			float weight = Mathf.InverseLerp(1.3333f, 1.7777f, num);
			base.Position = pos4_3.Lerp(pos16_9, weight);
			base.Scale = scale4_3.Lerp(scale16_9, weight);
		}
		else
		{
			float weight2 = Mathf.InverseLerp(1.7777f, 2.3333f, num);
			base.Position = pos16_9.Lerp(pos21_9, weight2);
			base.Scale = scale16_9.Lerp(scale21_9, weight2);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnWindowChange)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._window)
		{
			_window = VariantUtils.ConvertTo<Window>(in value);
			return true;
		}
		if (name == PropertyName.pos4_3)
		{
			pos4_3 = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName.scale4_3)
		{
			scale4_3 = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName.pos16_9)
		{
			pos16_9 = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName.scale16_9)
		{
			scale16_9 = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName.pos21_9)
		{
			pos21_9 = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName.scale21_9)
		{
			scale21_9 = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._window)
		{
			value = VariantUtils.CreateFrom(in _window);
			return true;
		}
		if (name == PropertyName.pos4_3)
		{
			value = VariantUtils.CreateFrom(in pos4_3);
			return true;
		}
		if (name == PropertyName.scale4_3)
		{
			value = VariantUtils.CreateFrom(in scale4_3);
			return true;
		}
		if (name == PropertyName.pos16_9)
		{
			value = VariantUtils.CreateFrom(in pos16_9);
			return true;
		}
		if (name == PropertyName.scale16_9)
		{
			value = VariantUtils.CreateFrom(in scale16_9);
			return true;
		}
		if (name == PropertyName.pos21_9)
		{
			value = VariantUtils.CreateFrom(in pos21_9);
			return true;
		}
		if (name == PropertyName.scale21_9)
		{
			value = VariantUtils.CreateFrom(in scale21_9);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._window, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.pos4_3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.scale4_3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.pos16_9, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.scale16_9, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.pos21_9, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.scale21_9, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._window, Variant.From(in _window));
		info.AddProperty(PropertyName.pos4_3, Variant.From(in pos4_3));
		info.AddProperty(PropertyName.scale4_3, Variant.From(in scale4_3));
		info.AddProperty(PropertyName.pos16_9, Variant.From(in pos16_9));
		info.AddProperty(PropertyName.scale16_9, Variant.From(in scale16_9));
		info.AddProperty(PropertyName.pos21_9, Variant.From(in pos21_9));
		info.AddProperty(PropertyName.scale21_9, Variant.From(in scale21_9));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._window, out var value))
		{
			_window = value.As<Window>();
		}
		if (info.TryGetProperty(PropertyName.pos4_3, out var value2))
		{
			pos4_3 = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName.scale4_3, out var value3))
		{
			scale4_3 = value3.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName.pos16_9, out var value4))
		{
			pos16_9 = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName.scale16_9, out var value5))
		{
			scale16_9 = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName.pos21_9, out var value6))
		{
			pos21_9 = value6.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName.scale21_9, out var value7))
		{
			scale21_9 = value7.As<Vector2>();
		}
	}
}
