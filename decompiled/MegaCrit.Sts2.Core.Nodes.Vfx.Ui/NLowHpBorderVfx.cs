using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Ui;

[ScriptPath("res://src/Core/Nodes/Vfx/Ui/NLowHpBorderVfx.cs")]
public class NLowHpBorderVfx : ColorRect
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : ColorRect.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'SetProperties' method.
		/// </summary>
		public static readonly StringName SetProperties = "SetProperties";

		/// <summary>
		/// Cached name for the 'RandomizeInitialOffset' method.
		/// </summary>
		public static readonly StringName RandomizeInitialOffset = "RandomizeInitialOffset";

		/// <summary>
		/// Cached name for the 'Play' method.
		/// </summary>
		public static readonly StringName Play = "Play";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : ColorRect.PropertyName
	{
		/// <summary>
		/// Cached name for the '_duration' field.
		/// </summary>
		public static readonly StringName _duration = "_duration";

		/// <summary>
		/// Cached name for the '_alphaMultiplierCurve' field.
		/// </summary>
		public static readonly StringName _alphaMultiplierCurve = "_alphaMultiplierCurve";

		/// <summary>
		/// Cached name for the '_noiseOffsetCurve' field.
		/// </summary>
		public static readonly StringName _noiseOffsetCurve = "_noiseOffsetCurve";

		/// <summary>
		/// Cached name for the '_gradient' field.
		/// </summary>
		public static readonly StringName _gradient = "_gradient";

		/// <summary>
		/// Cached name for the '_originalMaterial' field.
		/// </summary>
		public static readonly StringName _originalMaterial = "_originalMaterial";

		/// <summary>
		/// Cached name for the '_materialCopy' field.
		/// </summary>
		public static readonly StringName _materialCopy = "_materialCopy";

		/// <summary>
		/// Cached name for the '_isPlaying' field.
		/// </summary>
		public static readonly StringName _isPlaying = "_isPlaying";

		/// <summary>
		/// Cached name for the '_currentTimer' field.
		/// </summary>
		public static readonly StringName _currentTimer = "_currentTimer";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : ColorRect.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/ui/vfx_low_hp_border");

	[Export(PropertyHint.None, "")]
	private float _duration = 1f;

	[Export(PropertyHint.None, "")]
	private Curve? _alphaMultiplierCurve;

	[Export(PropertyHint.None, "")]
	private Curve? _noiseOffsetCurve;

	[Export(PropertyHint.None, "")]
	private Gradient? _gradient;

	private Material? _originalMaterial;

	private ShaderMaterial? _materialCopy;

	private bool _isPlaying;

	private double _currentTimer;

	private static readonly StringName _alphaMultiplierString = new StringName("alpha_multiplier");

	private static readonly StringName _noiseInitialOffsetString = new StringName("noise_initial_offset");

	private static readonly StringName _mainColorString = new StringName("main_color");

	public static NLowHpBorderVfx? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(scenePath).Instantiate<NLowHpBorderVfx>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		_isPlaying = false;
		_originalMaterial = base.Material;
		_materialCopy = (ShaderMaterial)_originalMaterial.Duplicate(deep: true);
		base.Material = _materialCopy;
		SetProperties(1f);
	}

	private void SetProperties(float interpolation)
	{
		float num = _alphaMultiplierCurve.Sample(interpolation);
		Color color = _gradient.Sample(interpolation);
		_materialCopy.SetShaderParameter(_alphaMultiplierString, num);
		_materialCopy.SetShaderParameter(_mainColorString, color);
	}

	private void RandomizeInitialOffset()
	{
		_materialCopy.SetShaderParameter(_noiseInitialOffsetString, new Vector2(GD.Randf(), GD.Randf()));
	}

	public void Play()
	{
		if (_isPlaying)
		{
			_currentTimer = 0.0;
		}
		else
		{
			TaskHelper.RunSafely(PlaySequence());
		}
	}

	private async Task PlaySequence()
	{
		_isPlaying = true;
		_currentTimer = 0.0;
		RandomizeInitialOffset();
		SetProperties(0f);
		while (_currentTimer < (double)_duration)
		{
			float properties = (float)(_currentTimer / (double)_duration);
			SetProperties(properties);
			_currentTimer += GetProcessDeltaTime();
			await this.AwaitProcessFrame();
		}
		SetProperties(1f);
		_isPlaying = false;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("ColorRect"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetProperties, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "interpolation", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RandomizeInitialOffset, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Play, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NLowHpBorderVfx>(Create());
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
		if (method == MethodName.RandomizeInitialOffset && args.Count == 0)
		{
			RandomizeInitialOffset();
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
			ret = VariantUtils.CreateFrom<NLowHpBorderVfx>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
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
		if (method == MethodName.RandomizeInitialOffset)
		{
			return true;
		}
		if (method == MethodName.Play)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
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
		if (name == PropertyName._noiseOffsetCurve)
		{
			_noiseOffsetCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._gradient)
		{
			_gradient = VariantUtils.ConvertTo<Gradient>(in value);
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
		if (name == PropertyName._isPlaying)
		{
			_isPlaying = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._currentTimer)
		{
			_currentTimer = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
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
		if (name == PropertyName._noiseOffsetCurve)
		{
			value = VariantUtils.CreateFrom(in _noiseOffsetCurve);
			return true;
		}
		if (name == PropertyName._gradient)
		{
			value = VariantUtils.CreateFrom(in _gradient);
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
		if (name == PropertyName._isPlaying)
		{
			value = VariantUtils.CreateFrom(in _isPlaying);
			return true;
		}
		if (name == PropertyName._currentTimer)
		{
			value = VariantUtils.CreateFrom(in _currentTimer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._duration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._alphaMultiplierCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noiseOffsetCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._gradient, PropertyHint.ResourceType, "Gradient", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._originalMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._materialCopy, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isPlaying, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._currentTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._duration, Variant.From(in _duration));
		info.AddProperty(PropertyName._alphaMultiplierCurve, Variant.From(in _alphaMultiplierCurve));
		info.AddProperty(PropertyName._noiseOffsetCurve, Variant.From(in _noiseOffsetCurve));
		info.AddProperty(PropertyName._gradient, Variant.From(in _gradient));
		info.AddProperty(PropertyName._originalMaterial, Variant.From(in _originalMaterial));
		info.AddProperty(PropertyName._materialCopy, Variant.From(in _materialCopy));
		info.AddProperty(PropertyName._isPlaying, Variant.From(in _isPlaying));
		info.AddProperty(PropertyName._currentTimer, Variant.From(in _currentTimer));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._duration, out var value))
		{
			_duration = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._alphaMultiplierCurve, out var value2))
		{
			_alphaMultiplierCurve = value2.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._noiseOffsetCurve, out var value3))
		{
			_noiseOffsetCurve = value3.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._gradient, out var value4))
		{
			_gradient = value4.As<Gradient>();
		}
		if (info.TryGetProperty(PropertyName._originalMaterial, out var value5))
		{
			_originalMaterial = value5.As<Material>();
		}
		if (info.TryGetProperty(PropertyName._materialCopy, out var value6))
		{
			_materialCopy = value6.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._isPlaying, out var value7))
		{
			_isPlaying = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._currentTimer, out var value8))
		{
			_currentTimer = value8.As<double>();
		}
	}
}
