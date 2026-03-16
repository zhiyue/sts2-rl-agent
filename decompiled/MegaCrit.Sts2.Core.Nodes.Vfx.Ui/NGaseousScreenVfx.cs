using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Ui;

[ScriptPath("res://src/Core/Nodes/Vfx/Ui/NGaseousScreenVfx.cs")]
public class NGaseousScreenVfx : AspectRatioContainer
{
	public new class MethodName : AspectRatioContainer.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetProperties = "SetProperties";

		public static readonly StringName Play = "Play";
	}

	public new class PropertyName : AspectRatioContainer.PropertyName
	{
		public static readonly StringName _gfx = "_gfx";

		public static readonly StringName _duration = "_duration";

		public static readonly StringName _alphaMultiplierCurve = "_alphaMultiplierCurve";

		public static readonly StringName _minBaseAlphaCurve = "_minBaseAlphaCurve";

		public static readonly StringName _erosionCurve = "_erosionCurve";

		public static readonly StringName _noiseAOffsetCurve = "_noiseAOffsetCurve";

		public static readonly StringName _noiseBOffsetCurve = "_noiseBOffsetCurve";

		public static readonly StringName _originalMaterial = "_originalMaterial";

		public static readonly StringName _materialCopy = "_materialCopy";

		public static readonly StringName _noiseAOffsetY = "_noiseAOffsetY";

		public static readonly StringName _noiseBOffsetY = "_noiseBOffsetY";
	}

	public new class SignalName : AspectRatioContainer.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/ui/vfx_gaseous_screen");

	[Export(PropertyHint.None, "")]
	private ColorRect _gfx;

	[Export(PropertyHint.None, "")]
	private float _duration = 1f;

	[Export(PropertyHint.None, "")]
	private Curve? _alphaMultiplierCurve;

	[Export(PropertyHint.None, "")]
	private Curve? _minBaseAlphaCurve;

	[Export(PropertyHint.None, "")]
	private Curve? _erosionCurve;

	[Export(PropertyHint.None, "")]
	private Curve? _noiseAOffsetCurve;

	[Export(PropertyHint.None, "")]
	private Curve? _noiseBOffsetCurve;

	private Material? _originalMaterial;

	private ShaderMaterial? _materialCopy;

	private float _noiseAOffsetY;

	private float _noiseBOffsetY;

	private static readonly StringName _alphaMultiplierString = new StringName("alpha_multiplier");

	private static readonly StringName _minBaseAlphaString = new StringName("min_base_alpha");

	private static readonly StringName _noiseAOffsetString = new StringName("noise_a_static_offset");

	private static readonly StringName _noiseBOffsetString = new StringName("noise_b_static_offset");

	private static readonly StringName _erosionString = new StringName("erosion_base");

	public static NGaseousScreenVfx? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(scenePath).Instantiate<NGaseousScreenVfx>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		_originalMaterial = _gfx.Material;
		_materialCopy = (ShaderMaterial)_originalMaterial.Duplicate(deep: true);
		_gfx.Material = _materialCopy;
		SetProperties(1f);
		Play();
	}

	private void SetProperties(float interpolation)
	{
		float num = _alphaMultiplierCurve.Sample(interpolation);
		float num2 = _minBaseAlphaCurve.Sample(interpolation);
		float x = _noiseAOffsetCurve.Sample(interpolation);
		float x2 = _noiseBOffsetCurve.Sample(interpolation);
		float num3 = _erosionCurve.Sample(interpolation);
		_materialCopy.SetShaderParameter(_alphaMultiplierString, num);
		_materialCopy.SetShaderParameter(_minBaseAlphaString, num2);
		_materialCopy.SetShaderParameter(_noiseAOffsetString, new Vector2(x, _noiseAOffsetY));
		_materialCopy.SetShaderParameter(_noiseBOffsetString, new Vector2(x2, _noiseBOffsetY));
		_materialCopy.SetShaderParameter(_erosionString, num3);
	}

	public void Play()
	{
		TaskHelper.RunSafely(PlaySequence());
	}

	private async Task PlaySequence()
	{
		_noiseAOffsetY = GD.Randf();
		_noiseBOffsetY = GD.Randf();
		double timer = 0.0;
		SetProperties(0f);
		while (timer < (double)_duration)
		{
			float properties = (float)(timer / (double)_duration);
			SetProperties(properties);
			timer += GetProcessDeltaTime();
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		SetProperties(1f);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("AspectRatioContainer"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetProperties, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "interpolation", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Play, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NGaseousScreenVfx>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetProperties && args.Count == 1)
		{
			SetProperties(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Play && args.Count == 0)
		{
			Play();
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
			ret = VariantUtils.CreateFrom<NGaseousScreenVfx>(Create());
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
		if (method == MethodName.SetProperties)
		{
			return true;
		}
		if (method == MethodName.Play)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._gfx)
		{
			_gfx = VariantUtils.ConvertTo<ColorRect>(in value);
			return true;
		}
		if (name == PropertyName._duration)
		{
			_duration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._alphaMultiplierCurve)
		{
			_alphaMultiplierCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._minBaseAlphaCurve)
		{
			_minBaseAlphaCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._erosionCurve)
		{
			_erosionCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._noiseAOffsetCurve)
		{
			_noiseAOffsetCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._noiseBOffsetCurve)
		{
			_noiseBOffsetCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._originalMaterial)
		{
			_originalMaterial = VariantUtils.ConvertTo<Material>(in value);
			return true;
		}
		if (name == PropertyName._materialCopy)
		{
			_materialCopy = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._noiseAOffsetY)
		{
			_noiseAOffsetY = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._noiseBOffsetY)
		{
			_noiseBOffsetY = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._gfx)
		{
			value = VariantUtils.CreateFrom(in _gfx);
			return true;
		}
		if (name == PropertyName._duration)
		{
			value = VariantUtils.CreateFrom(in _duration);
			return true;
		}
		if (name == PropertyName._alphaMultiplierCurve)
		{
			value = VariantUtils.CreateFrom(in _alphaMultiplierCurve);
			return true;
		}
		if (name == PropertyName._minBaseAlphaCurve)
		{
			value = VariantUtils.CreateFrom(in _minBaseAlphaCurve);
			return true;
		}
		if (name == PropertyName._erosionCurve)
		{
			value = VariantUtils.CreateFrom(in _erosionCurve);
			return true;
		}
		if (name == PropertyName._noiseAOffsetCurve)
		{
			value = VariantUtils.CreateFrom(in _noiseAOffsetCurve);
			return true;
		}
		if (name == PropertyName._noiseBOffsetCurve)
		{
			value = VariantUtils.CreateFrom(in _noiseBOffsetCurve);
			return true;
		}
		if (name == PropertyName._originalMaterial)
		{
			value = VariantUtils.CreateFrom(in _originalMaterial);
			return true;
		}
		if (name == PropertyName._materialCopy)
		{
			value = VariantUtils.CreateFrom(in _materialCopy);
			return true;
		}
		if (name == PropertyName._noiseAOffsetY)
		{
			value = VariantUtils.CreateFrom(in _noiseAOffsetY);
			return true;
		}
		if (name == PropertyName._noiseBOffsetY)
		{
			value = VariantUtils.CreateFrom(in _noiseBOffsetY);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._gfx, PropertyHint.NodeType, "ColorRect", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._duration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._alphaMultiplierCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._minBaseAlphaCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._erosionCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noiseAOffsetCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noiseBOffsetCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._originalMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._materialCopy, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._noiseAOffsetY, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._noiseBOffsetY, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._gfx, Variant.From(in _gfx));
		info.AddProperty(PropertyName._duration, Variant.From(in _duration));
		info.AddProperty(PropertyName._alphaMultiplierCurve, Variant.From(in _alphaMultiplierCurve));
		info.AddProperty(PropertyName._minBaseAlphaCurve, Variant.From(in _minBaseAlphaCurve));
		info.AddProperty(PropertyName._erosionCurve, Variant.From(in _erosionCurve));
		info.AddProperty(PropertyName._noiseAOffsetCurve, Variant.From(in _noiseAOffsetCurve));
		info.AddProperty(PropertyName._noiseBOffsetCurve, Variant.From(in _noiseBOffsetCurve));
		info.AddProperty(PropertyName._originalMaterial, Variant.From(in _originalMaterial));
		info.AddProperty(PropertyName._materialCopy, Variant.From(in _materialCopy));
		info.AddProperty(PropertyName._noiseAOffsetY, Variant.From(in _noiseAOffsetY));
		info.AddProperty(PropertyName._noiseBOffsetY, Variant.From(in _noiseBOffsetY));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._gfx, out var value))
		{
			_gfx = value.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._duration, out var value2))
		{
			_duration = value2.As<float>();
		}
		if (info.TryGetProperty(PropertyName._alphaMultiplierCurve, out var value3))
		{
			_alphaMultiplierCurve = value3.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._minBaseAlphaCurve, out var value4))
		{
			_minBaseAlphaCurve = value4.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._erosionCurve, out var value5))
		{
			_erosionCurve = value5.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._noiseAOffsetCurve, out var value6))
		{
			_noiseAOffsetCurve = value6.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._noiseBOffsetCurve, out var value7))
		{
			_noiseBOffsetCurve = value7.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._originalMaterial, out var value8))
		{
			_originalMaterial = value8.As<Material>();
		}
		if (info.TryGetProperty(PropertyName._materialCopy, out var value9))
		{
			_materialCopy = value9.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._noiseAOffsetY, out var value10))
		{
			_noiseAOffsetY = value10.As<float>();
		}
		if (info.TryGetProperty(PropertyName._noiseBOffsetY, out var value11))
		{
			_noiseBOffsetY = value11.As<float>();
		}
	}
}
