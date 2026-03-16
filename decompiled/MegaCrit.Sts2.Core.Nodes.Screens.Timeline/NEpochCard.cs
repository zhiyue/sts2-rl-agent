using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Timeline;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

[ScriptPath("res://src/Core/Nodes/Screens/Timeline/NEpochCard.cs")]
public class NEpochCard : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName SetToWigglyUnlockPreviewMode = "SetToWigglyUnlockPreviewMode";

		public static readonly StringName GlowFlash = "GlowFlash";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _glow = "_glow";

		public static readonly StringName _mask = "_mask";

		public static readonly StringName _portrait = "_portrait";

		public static readonly StringName _isHovered = "_isHovered";

		public static readonly StringName _isHoverable = "_isHoverable";

		public static readonly StringName _isHeld = "_isHeld";

		public static readonly StringName _isWigglyUnlockPreviewMode = "_isWigglyUnlockPreviewMode";

		public static readonly StringName _glowTween = "_glowTween";

		public static readonly StringName _targetScale = "_targetScale";

		public static readonly StringName _time = "_time";

		public static readonly StringName _noiseSpeed = "_noiseSpeed";

		public static readonly StringName _noise = "_noise";

		public static readonly StringName _denyTween = "_denyTween";

		public static readonly StringName _transparencyTween = "_transparencyTween";

		public static readonly StringName _scaleTween = "_scaleTween";

		public static readonly StringName _blueGlowColor = "_blueGlowColor";

		public static readonly StringName _goldGlowColor = "_goldGlowColor";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private TextureRect _glow;

	private TextureRect _mask;

	private TextureRect _portrait;

	private bool _isHovered;

	private bool _isHoverable = true;

	private bool _isHeld;

	private bool _isWigglyUnlockPreviewMode;

	private Tween? _glowTween;

	private Vector2 _targetScale;

	private float _time;

	private float _noiseSpeed = 0.25f;

	private FastNoiseLite _noise;

	private Tween? _denyTween;

	private Tween? _transparencyTween;

	private Tween? _scaleTween;

	private Color _blueGlowColor = new Color("2de5ff80");

	private Color _goldGlowColor = new Color("ffd92e80");

	public void Init(EpochModel epochModel)
	{
		_glow = GetNode<TextureRect>("%GlowPlaceholder");
		_portrait = GetNode<TextureRect>("%Portrait");
		_portrait.Texture = epochModel.BigPortrait;
		base.Scale = Vector2.One;
	}

	public override void _Ready()
	{
		_mask = GetNode<TextureRect>("%Mask");
	}

	public override void _Process(double delta)
	{
		if (_isWigglyUnlockPreviewMode)
		{
			_time += _noiseSpeed * (float)delta;
			float x = 42f * _noise.GetNoise2D(_time, 0f);
			float y = 42f * _noise.GetNoise2D(0f, _time);
			base.Position = new Vector2(x, y) - GetPivotOffset();
		}
	}

	public void SetToWigglyUnlockPreviewMode()
	{
		_noise = new FastNoiseLite();
		_noise.Frequency = 0.2f;
		_noise.SetSeed(Rng.Chaotic.NextInt());
		_isWigglyUnlockPreviewMode = true;
		base.MouseFilter = MouseFilterEnum.Ignore;
		base.Scale = Vector2.One * 1.2f;
	}

	private void GlowFlash()
	{
		base.Modulate = new Color(1f, 1f, 1f, 0.75f);
		_glow.Modulate = Colors.Gold;
		_glowTween?.Kill();
		_glowTween = CreateTween().SetParallel();
		_glowTween.TweenProperty(_glow, "modulate:a", 0.5f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_glowTween.TweenProperty(_glow, "scale", Vector2.One, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(Vector2.One * 1.5f);
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
		list.Add(new MethodInfo(MethodName.SetToWigglyUnlockPreviewMode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GlowFlash, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SetToWigglyUnlockPreviewMode && args.Count == 0)
		{
			SetToWigglyUnlockPreviewMode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GlowFlash && args.Count == 0)
		{
			GlowFlash();
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
		if (method == MethodName.SetToWigglyUnlockPreviewMode)
		{
			return true;
		}
		if (method == MethodName.GlowFlash)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._glow)
		{
			_glow = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._mask)
		{
			_mask = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._portrait)
		{
			_portrait = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._isHovered)
		{
			_isHovered = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isHoverable)
		{
			_isHoverable = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isHeld)
		{
			_isHeld = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isWigglyUnlockPreviewMode)
		{
			_isWigglyUnlockPreviewMode = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._glowTween)
		{
			_glowTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._targetScale)
		{
			_targetScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._time)
		{
			_time = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._noiseSpeed)
		{
			_noiseSpeed = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._noise)
		{
			_noise = VariantUtils.ConvertTo<FastNoiseLite>(in value);
			return true;
		}
		if (name == PropertyName._denyTween)
		{
			_denyTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._transparencyTween)
		{
			_transparencyTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._scaleTween)
		{
			_scaleTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._blueGlowColor)
		{
			_blueGlowColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._goldGlowColor)
		{
			_goldGlowColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._glow)
		{
			value = VariantUtils.CreateFrom(in _glow);
			return true;
		}
		if (name == PropertyName._mask)
		{
			value = VariantUtils.CreateFrom(in _mask);
			return true;
		}
		if (name == PropertyName._portrait)
		{
			value = VariantUtils.CreateFrom(in _portrait);
			return true;
		}
		if (name == PropertyName._isHovered)
		{
			value = VariantUtils.CreateFrom(in _isHovered);
			return true;
		}
		if (name == PropertyName._isHoverable)
		{
			value = VariantUtils.CreateFrom(in _isHoverable);
			return true;
		}
		if (name == PropertyName._isHeld)
		{
			value = VariantUtils.CreateFrom(in _isHeld);
			return true;
		}
		if (name == PropertyName._isWigglyUnlockPreviewMode)
		{
			value = VariantUtils.CreateFrom(in _isWigglyUnlockPreviewMode);
			return true;
		}
		if (name == PropertyName._glowTween)
		{
			value = VariantUtils.CreateFrom(in _glowTween);
			return true;
		}
		if (name == PropertyName._targetScale)
		{
			value = VariantUtils.CreateFrom(in _targetScale);
			return true;
		}
		if (name == PropertyName._time)
		{
			value = VariantUtils.CreateFrom(in _time);
			return true;
		}
		if (name == PropertyName._noiseSpeed)
		{
			value = VariantUtils.CreateFrom(in _noiseSpeed);
			return true;
		}
		if (name == PropertyName._noise)
		{
			value = VariantUtils.CreateFrom(in _noise);
			return true;
		}
		if (name == PropertyName._denyTween)
		{
			value = VariantUtils.CreateFrom(in _denyTween);
			return true;
		}
		if (name == PropertyName._transparencyTween)
		{
			value = VariantUtils.CreateFrom(in _transparencyTween);
			return true;
		}
		if (name == PropertyName._scaleTween)
		{
			value = VariantUtils.CreateFrom(in _scaleTween);
			return true;
		}
		if (name == PropertyName._blueGlowColor)
		{
			value = VariantUtils.CreateFrom(in _blueGlowColor);
			return true;
		}
		if (name == PropertyName._goldGlowColor)
		{
			value = VariantUtils.CreateFrom(in _goldGlowColor);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mask, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._portrait, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isHovered, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isHoverable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isHeld, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isWigglyUnlockPreviewMode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glowTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._time, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._noiseSpeed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noise, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._denyTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._transparencyTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scaleTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._blueGlowColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._goldGlowColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._glow, Variant.From(in _glow));
		info.AddProperty(PropertyName._mask, Variant.From(in _mask));
		info.AddProperty(PropertyName._portrait, Variant.From(in _portrait));
		info.AddProperty(PropertyName._isHovered, Variant.From(in _isHovered));
		info.AddProperty(PropertyName._isHoverable, Variant.From(in _isHoverable));
		info.AddProperty(PropertyName._isHeld, Variant.From(in _isHeld));
		info.AddProperty(PropertyName._isWigglyUnlockPreviewMode, Variant.From(in _isWigglyUnlockPreviewMode));
		info.AddProperty(PropertyName._glowTween, Variant.From(in _glowTween));
		info.AddProperty(PropertyName._targetScale, Variant.From(in _targetScale));
		info.AddProperty(PropertyName._time, Variant.From(in _time));
		info.AddProperty(PropertyName._noiseSpeed, Variant.From(in _noiseSpeed));
		info.AddProperty(PropertyName._noise, Variant.From(in _noise));
		info.AddProperty(PropertyName._denyTween, Variant.From(in _denyTween));
		info.AddProperty(PropertyName._transparencyTween, Variant.From(in _transparencyTween));
		info.AddProperty(PropertyName._scaleTween, Variant.From(in _scaleTween));
		info.AddProperty(PropertyName._blueGlowColor, Variant.From(in _blueGlowColor));
		info.AddProperty(PropertyName._goldGlowColor, Variant.From(in _goldGlowColor));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._glow, out var value))
		{
			_glow = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._mask, out var value2))
		{
			_mask = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._portrait, out var value3))
		{
			_portrait = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._isHovered, out var value4))
		{
			_isHovered = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isHoverable, out var value5))
		{
			_isHoverable = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isHeld, out var value6))
		{
			_isHeld = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isWigglyUnlockPreviewMode, out var value7))
		{
			_isWigglyUnlockPreviewMode = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._glowTween, out var value8))
		{
			_glowTween = value8.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._targetScale, out var value9))
		{
			_targetScale = value9.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._time, out var value10))
		{
			_time = value10.As<float>();
		}
		if (info.TryGetProperty(PropertyName._noiseSpeed, out var value11))
		{
			_noiseSpeed = value11.As<float>();
		}
		if (info.TryGetProperty(PropertyName._noise, out var value12))
		{
			_noise = value12.As<FastNoiseLite>();
		}
		if (info.TryGetProperty(PropertyName._denyTween, out var value13))
		{
			_denyTween = value13.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._transparencyTween, out var value14))
		{
			_transparencyTween = value14.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._scaleTween, out var value15))
		{
			_scaleTween = value15.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._blueGlowColor, out var value16))
		{
			_blueGlowColor = value16.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._goldGlowColor, out var value17))
		{
			_goldGlowColor = value17.As<Color>();
		}
	}
}
