using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NMirrorVfx.cs")]
public class NMirrorVfx : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _mask1 = "_mask1";

		public static readonly StringName _reflection1 = "_reflection1";

		public static readonly StringName _mask2 = "_mask2";

		public static readonly StringName _reflection2 = "_reflection2";

		public static readonly StringName _mask3 = "_mask3";

		public static readonly StringName _reflection3 = "_reflection3";

		public static readonly StringName _noise = "_noise";

		public static readonly StringName _totalTime = "_totalTime";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Sprite2D _mask1;

	private Control _reflection1;

	private Sprite2D _mask2;

	private Control _reflection2;

	private Sprite2D _mask3;

	private Control _reflection3;

	private FastNoiseLite _noise = new FastNoiseLite();

	private float _totalTime;

	private const float _noiseSpeed = 2f;

	private static string ScenePath => SceneHelper.GetScenePath("vfx/whole_screen/mirror_vfx");

	public static NMirrorVfx? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(ScenePath).Instantiate<NMirrorVfx>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		_mask1 = GetNode<Sprite2D>("Mask1");
		_reflection1 = GetNode<Control>("Mask1/Reflection");
		_mask2 = GetNode<Sprite2D>("Mask2");
		_reflection2 = GetNode<Control>("Mask2/Reflection");
		_mask3 = GetNode<Sprite2D>("Mask3");
		_reflection3 = GetNode<Control>("Mask3/Reflection");
	}

	public override void _Process(double delta)
	{
		_totalTime += (float)delta * 2f;
		_noise.Seed = 0;
		float num = 1.05f + _noise.GetNoise1D(_totalTime);
		_mask1.Scale = new Vector2(num, num);
		_reflection1.Scale = _mask1.Scale;
		_noise.Seed = 1;
		num = _noise.GetNoise1D(_totalTime);
		_mask1.RotationDegrees = Mathf.Abs(num) * 10f;
		_noise.Seed = 2;
		num = 1.05f + _noise.GetNoise1D(_totalTime);
		_mask2.Scale = new Vector2(num, num);
		_reflection2.Scale = new Vector2(num, num);
		_noise.Seed = 3;
		num = _noise.GetNoise1D(_totalTime);
		_mask2.RotationDegrees = Mathf.Abs(num) * 20f;
		_noise.Seed = 4;
		num = 1.05f + _noise.GetNoise1D(_totalTime);
		_mask3.Scale = new Vector2(num, num);
		_reflection3.Scale = new Vector2(num, num);
		_noise.Seed = 5;
		num = _noise.GetNoise1D(_totalTime);
		_mask3.RotationDegrees = Mathf.Abs(num) * 30f;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
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
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NMirrorVfx>(Create());
			return true;
		}
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
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NMirrorVfx>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
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
		if (name == PropertyName._mask1)
		{
			_mask1 = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._reflection1)
		{
			_reflection1 = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._mask2)
		{
			_mask2 = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._reflection2)
		{
			_reflection2 = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._mask3)
		{
			_mask3 = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._reflection3)
		{
			_reflection3 = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._noise)
		{
			_noise = VariantUtils.ConvertTo<FastNoiseLite>(in value);
			return true;
		}
		if (name == PropertyName._totalTime)
		{
			_totalTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._mask1)
		{
			value = VariantUtils.CreateFrom(in _mask1);
			return true;
		}
		if (name == PropertyName._reflection1)
		{
			value = VariantUtils.CreateFrom(in _reflection1);
			return true;
		}
		if (name == PropertyName._mask2)
		{
			value = VariantUtils.CreateFrom(in _mask2);
			return true;
		}
		if (name == PropertyName._reflection2)
		{
			value = VariantUtils.CreateFrom(in _reflection2);
			return true;
		}
		if (name == PropertyName._mask3)
		{
			value = VariantUtils.CreateFrom(in _mask3);
			return true;
		}
		if (name == PropertyName._reflection3)
		{
			value = VariantUtils.CreateFrom(in _reflection3);
			return true;
		}
		if (name == PropertyName._noise)
		{
			value = VariantUtils.CreateFrom(in _noise);
			return true;
		}
		if (name == PropertyName._totalTime)
		{
			value = VariantUtils.CreateFrom(in _totalTime);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mask1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._reflection1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mask2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._reflection2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mask3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._reflection3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noise, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._totalTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._mask1, Variant.From(in _mask1));
		info.AddProperty(PropertyName._reflection1, Variant.From(in _reflection1));
		info.AddProperty(PropertyName._mask2, Variant.From(in _mask2));
		info.AddProperty(PropertyName._reflection2, Variant.From(in _reflection2));
		info.AddProperty(PropertyName._mask3, Variant.From(in _mask3));
		info.AddProperty(PropertyName._reflection3, Variant.From(in _reflection3));
		info.AddProperty(PropertyName._noise, Variant.From(in _noise));
		info.AddProperty(PropertyName._totalTime, Variant.From(in _totalTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._mask1, out var value))
		{
			_mask1 = value.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._reflection1, out var value2))
		{
			_reflection1 = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._mask2, out var value3))
		{
			_mask2 = value3.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._reflection2, out var value4))
		{
			_reflection2 = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._mask3, out var value5))
		{
			_mask3 = value5.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._reflection3, out var value6))
		{
			_reflection3 = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._noise, out var value7))
		{
			_noise = value7.As<FastNoiseLite>();
		}
		if (info.TryGetProperty(PropertyName._totalTime, out var value8))
		{
			_totalTime = value8.As<float>();
		}
	}
}
