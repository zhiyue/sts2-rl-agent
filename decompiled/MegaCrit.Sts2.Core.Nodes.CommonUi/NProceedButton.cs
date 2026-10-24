using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

/// <summary>
/// Proceed Button. The Man. The Myth. The Legend. A Button.
/// </summary>
[ScriptPath("res://src/Core/Nodes/CommonUi/NProceedButton.cs")]
public class NProceedButton : NButton
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NButton.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'OnEnable' method.
		/// </summary>
		public new static readonly StringName OnEnable = "OnEnable";

		/// <summary>
		/// Cached name for the 'OnDisable' method.
		/// </summary>
		public new static readonly StringName OnDisable = "OnDisable";

		/// <summary>
		/// Cached name for the 'OnRelease' method.
		/// </summary>
		public new static readonly StringName OnRelease = "OnRelease";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";

		/// <summary>
		/// Cached name for the 'OnPress' method.
		/// </summary>
		public new static readonly StringName OnPress = "OnPress";

		/// <summary>
		/// Cached name for the 'DebugToggleVisibility' method.
		/// </summary>
		public static readonly StringName DebugToggleVisibility = "DebugToggleVisibility";

		/// <summary>
		/// Cached name for the 'SetPulseState' method.
		/// </summary>
		public static readonly StringName SetPulseState = "SetPulseState";

		/// <summary>
		/// Cached name for the 'StartGlowTween' method.
		/// </summary>
		public static readonly StringName StartGlowTween = "StartGlowTween";

		/// <summary>
		/// Cached name for the 'StopGlowTween' method.
		/// </summary>
		public static readonly StringName StopGlowTween = "StopGlowTween";

		/// <summary>
		/// Cached name for the 'UpdateShaderS' method.
		/// </summary>
		public static readonly StringName UpdateShaderS = "UpdateShaderS";

		/// <summary>
		/// Cached name for the 'UpdateShaderV' method.
		/// </summary>
		public static readonly StringName UpdateShaderV = "UpdateShaderV";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'IsSkip' property.
		/// </summary>
		public static readonly StringName IsSkip = "IsSkip";

		/// <summary>
		/// Cached name for the 'Hotkeys' property.
		/// </summary>
		public new static readonly StringName Hotkeys = "Hotkeys";

		/// <summary>
		/// Cached name for the 'ShowPos' property.
		/// </summary>
		public static readonly StringName ShowPos = "ShowPos";

		/// <summary>
		/// Cached name for the 'HidePos' property.
		/// </summary>
		public static readonly StringName HidePos = "HidePos";

		/// <summary>
		/// Cached name for the '_outline' field.
		/// </summary>
		public static readonly StringName _outline = "_outline";

		/// <summary>
		/// Cached name for the '_buttonImage' field.
		/// </summary>
		public static readonly StringName _buttonImage = "_buttonImage";

		/// <summary>
		/// Cached name for the '_label' field.
		/// </summary>
		public static readonly StringName _label = "_label";

		/// <summary>
		/// Cached name for the '_hsv' field.
		/// </summary>
		public static readonly StringName _hsv = "_hsv";

		/// <summary>
		/// Cached name for the '_viewport' field.
		/// </summary>
		public static readonly StringName _viewport = "_viewport";

		/// <summary>
		/// Cached name for the '_defaultOutlineColor' field.
		/// </summary>
		public static readonly StringName _defaultOutlineColor = "_defaultOutlineColor";

		/// <summary>
		/// Cached name for the '_hoveredOutlineColor' field.
		/// </summary>
		public static readonly StringName _hoveredOutlineColor = "_hoveredOutlineColor";

		/// <summary>
		/// Cached name for the '_downColor' field.
		/// </summary>
		public static readonly StringName _downColor = "_downColor";

		/// <summary>
		/// Cached name for the '_outlineColor' field.
		/// </summary>
		public static readonly StringName _outlineColor = "_outlineColor";

		/// <summary>
		/// Cached name for the '_outlineTransparentColor' field.
		/// </summary>
		public static readonly StringName _outlineTransparentColor = "_outlineTransparentColor";

		/// <summary>
		/// Cached name for the '_animTween' field.
		/// </summary>
		public static readonly StringName _animTween = "_animTween";

		/// <summary>
		/// Cached name for the '_glowTween' field.
		/// </summary>
		public static readonly StringName _glowTween = "_glowTween";

		/// <summary>
		/// Cached name for the '_hoverTween' field.
		/// </summary>
		public static readonly StringName _hoverTween = "_hoverTween";

		/// <summary>
		/// Cached name for the '_elapsedTime' field.
		/// </summary>
		public static readonly StringName _elapsedTime = "_elapsedTime";

		/// <summary>
		/// Cached name for the '_shouldPulse' field.
		/// </summary>
		public static readonly StringName _shouldPulse = "_shouldPulse";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private Control _outline;

	private Control _buttonImage;

	private MegaLabel _label;

	private ShaderMaterial _hsv;

	private Viewport _viewport;

	private Color _defaultOutlineColor = StsColors.cream;

	private Color _hoveredOutlineColor = StsColors.gold;

	private Color _downColor = Colors.Gray;

	private Color _outlineColor = new Color("FFCC00C0");

	private Color _outlineTransparentColor = new Color("FF000000");

	private Tween? _animTween;

	private Tween? _glowTween;

	private Tween? _hoverTween;

	private static readonly Vector2 _hoverScale = Vector2.One * 1.05f;

	private float _elapsedTime;

	private bool _shouldPulse = true;

	private static readonly Vector2 _showPosRatio = new Vector2(1583f, 764f) / NGame.devResolution;

	private static readonly Vector2 _hidePosRatio = _showPosRatio + new Vector2(400f, 0f) / NGame.devResolution;

	public static LocString ProceedLoc => new LocString("gameplay_ui", "PROCEED_BUTTON");

	public static LocString SkipLoc => new LocString("gameplay_ui", "CHOOSE_CARD_SKIP_BUTTON");

	public bool IsSkip { get; private set; }

	protected override string[] Hotkeys => new string[1] { MegaInput.accept };

	private Vector2 ShowPos => _showPosRatio * _viewport.GetVisibleRect().Size;

	private Vector2 HidePos => _hidePosRatio * _viewport.GetVisibleRect().Size;

	public override void _Ready()
	{
		ConnectSignals();
		_outline = GetNode<Control>("%Outline");
		_buttonImage = GetNode<Control>("%Image");
		_hsv = (ShaderMaterial)_buttonImage.GetMaterial();
		_label = GetNode<MegaLabel>("%Label");
		_viewport = GetViewport();
		base.Position = HidePos;
		Disable();
		NGame.Instance.DebugToggleProceedButton += DebugToggleVisibility;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		NGame.Instance.DebugToggleProceedButton -= DebugToggleVisibility;
	}

	/// <summary>
	/// Call when we want this button to animate in.
	/// We refresh a lot of values here as the screen may not have been disposed and we reuse the button.
	/// May need to separate this logic into a Refresh() call for organization.
	/// </summary>
	protected override void OnEnable()
	{
		base.OnEnable();
		if (NGame.IsDebugHidingProceedButton)
		{
			base.Modulate = Colors.Transparent;
		}
		base.Scale = Vector2.One;
		_buttonImage.SelfModulate = StsColors.gray;
		UpdateShaderS(1f);
		UpdateShaderV(1f);
		_animTween?.Kill();
		_animTween = CreateTween().SetParallel();
		_animTween.TweenProperty(this, "position", ShowPos, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_animTween.TweenProperty(_buttonImage, "self_modulate", Colors.White, 0.5);
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenProperty(_outline, "modulate", Colors.White, 0.5);
		if (_shouldPulse)
		{
			StartGlowTween();
		}
	}

	/// <summary>
	/// Call when we want this button to hide this button (Disables clickability/hotkeys)
	/// </summary>
	protected override void OnDisable()
	{
		base.OnDisable();
		_animTween?.Kill();
		_animTween = CreateTween().SetParallel();
		_animTween.TweenProperty(this, "position", HidePos, 0.8).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_animTween.TweenProperty(_buttonImage, "self_modulate", StsColors.gray, 0.8);
		_animTween.TweenProperty(_outline, "modulate", StsColors.transparentWhite, 0.8);
		_glowTween?.Kill();
	}

	protected override void OnRelease()
	{
		if (_isEnabled)
		{
			_hoverTween?.Kill();
			_hoverTween = CreateTween().SetParallel();
			_hoverTween.TweenProperty(this, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_hoverTween.TweenProperty(_buttonImage, "modulate", Colors.White, 0.5);
			_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1f, 0.05);
			_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), 1f, 0.05);
		}
	}

	protected override void OnFocus()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenProperty(this, "scale", _hoverScale, 0.05);
		_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1.4f, 0.05);
		_glowTween?.Kill();
		_glowTween = CreateTween();
		_glowTween.TweenProperty(_outline, "self_modulate:a", 1f, 0.05);
	}

	protected override void OnUnfocus()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenProperty(this, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_hoverTween.TweenProperty(_buttonImage, "modulate", Colors.White, 0.5);
		_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1f, 0.05);
		_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), 1f, 0.05);
		if (_shouldPulse)
		{
			StartGlowTween();
		}
		else
		{
			StopGlowTween();
		}
	}

	protected override void OnPress()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenProperty(this, "scale", Vector2.One * 0.95f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_hoverTween.TweenProperty(_buttonImage, "modulate", StsColors.gray, 0.5);
		_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1f, 0.25);
		_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), 0.8f, 0.25);
		StopGlowTween();
	}

	public void UpdateText(LocString loc)
	{
		_label.SetTextAutoSize(loc.GetFormattedText());
		IsSkip = loc.LocEntryKey == SkipLoc.LocEntryKey;
	}

	private void DebugToggleVisibility()
	{
		base.Modulate = (NGame.IsDebugHidingProceedButton ? Colors.Transparent : Colors.White);
	}

	public void SetPulseState(bool isPulsing)
	{
		_shouldPulse = isPulsing;
		if (isPulsing)
		{
			StartGlowTween();
		}
		else
		{
			StopGlowTween();
		}
	}

	private void StartGlowTween()
	{
		_glowTween?.Kill();
		_glowTween = CreateTween().SetLoops();
		_glowTween.TweenProperty(_outline, "self_modulate:a", 0.25f, 0.5);
		_glowTween.TweenProperty(_outline, "self_modulate:a", 0.75f, 0.5);
	}

	private void StopGlowTween()
	{
		_glowTween?.Kill();
		_glowTween = CreateTween();
		_glowTween.TweenProperty(_outline, "self_modulate:a", 0f, 0.5);
	}

	private void UpdateShaderS(float value)
	{
		_hsv.SetShaderParameter(_s, value);
	}

	private void UpdateShaderV(float value)
	{
		_hsv.SetShaderParameter(_v, value);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(14);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DebugToggleVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetPulseState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isPulsing", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StartGlowTween, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StopGlowTween, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderS, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderV, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEnable && args.Count == 0)
		{
			OnEnable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDisable && args.Count == 0)
		{
			OnDisable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DebugToggleVisibility && args.Count == 0)
		{
			DebugToggleVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetPulseState && args.Count == 1)
		{
			SetPulseState(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartGlowTween && args.Count == 0)
		{
			StartGlowTween();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopGlowTween && args.Count == 0)
		{
			StopGlowTween();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderS && args.Count == 1)
		{
			UpdateShaderS(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderV && args.Count == 1)
		{
			UpdateShaderV(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.OnEnable)
		{
			return true;
		}
		if (method == MethodName.OnDisable)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName.DebugToggleVisibility)
		{
			return true;
		}
		if (method == MethodName.SetPulseState)
		{
			return true;
		}
		if (method == MethodName.StartGlowTween)
		{
			return true;
		}
		if (method == MethodName.StopGlowTween)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderS)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderV)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsSkip)
		{
			IsSkip = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._outline)
		{
			_outline = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._buttonImage)
		{
			_buttonImage = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._viewport)
		{
			_viewport = VariantUtils.ConvertTo<Viewport>(in value);
			return true;
		}
		if (name == PropertyName._defaultOutlineColor)
		{
			_defaultOutlineColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._hoveredOutlineColor)
		{
			_hoveredOutlineColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._downColor)
		{
			_downColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._outlineColor)
		{
			_outlineColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._outlineTransparentColor)
		{
			_outlineTransparentColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._animTween)
		{
			_animTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._glowTween)
		{
			_glowTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._elapsedTime)
		{
			_elapsedTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._shouldPulse)
		{
			_shouldPulse = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsSkip)
		{
			value = VariantUtils.CreateFrom<bool>(IsSkip);
			return true;
		}
		if (name == PropertyName.Hotkeys)
		{
			value = VariantUtils.CreateFrom<string[]>(Hotkeys);
			return true;
		}
		Vector2 from;
		if (name == PropertyName.ShowPos)
		{
			from = ShowPos;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HidePos)
		{
			from = HidePos;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._outline)
		{
			value = VariantUtils.CreateFrom(in _outline);
			return true;
		}
		if (name == PropertyName._buttonImage)
		{
			value = VariantUtils.CreateFrom(in _buttonImage);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._viewport)
		{
			value = VariantUtils.CreateFrom(in _viewport);
			return true;
		}
		if (name == PropertyName._defaultOutlineColor)
		{
			value = VariantUtils.CreateFrom(in _defaultOutlineColor);
			return true;
		}
		if (name == PropertyName._hoveredOutlineColor)
		{
			value = VariantUtils.CreateFrom(in _hoveredOutlineColor);
			return true;
		}
		if (name == PropertyName._downColor)
		{
			value = VariantUtils.CreateFrom(in _downColor);
			return true;
		}
		if (name == PropertyName._outlineColor)
		{
			value = VariantUtils.CreateFrom(in _outlineColor);
			return true;
		}
		if (name == PropertyName._outlineTransparentColor)
		{
			value = VariantUtils.CreateFrom(in _outlineTransparentColor);
			return true;
		}
		if (name == PropertyName._animTween)
		{
			value = VariantUtils.CreateFrom(in _animTween);
			return true;
		}
		if (name == PropertyName._glowTween)
		{
			value = VariantUtils.CreateFrom(in _glowTween);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		if (name == PropertyName._elapsedTime)
		{
			value = VariantUtils.CreateFrom(in _elapsedTime);
			return true;
		}
		if (name == PropertyName._shouldPulse)
		{
			value = VariantUtils.CreateFrom(in _shouldPulse);
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
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsSkip, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._buttonImage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._viewport, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._defaultOutlineColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._hoveredOutlineColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._downColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._outlineColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._outlineTransparentColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._animTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glowTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._elapsedTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._shouldPulse, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.ShowPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.HidePos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsSkip, Variant.From<bool>(IsSkip));
		info.AddProperty(PropertyName._outline, Variant.From(in _outline));
		info.AddProperty(PropertyName._buttonImage, Variant.From(in _buttonImage));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._viewport, Variant.From(in _viewport));
		info.AddProperty(PropertyName._defaultOutlineColor, Variant.From(in _defaultOutlineColor));
		info.AddProperty(PropertyName._hoveredOutlineColor, Variant.From(in _hoveredOutlineColor));
		info.AddProperty(PropertyName._downColor, Variant.From(in _downColor));
		info.AddProperty(PropertyName._outlineColor, Variant.From(in _outlineColor));
		info.AddProperty(PropertyName._outlineTransparentColor, Variant.From(in _outlineTransparentColor));
		info.AddProperty(PropertyName._animTween, Variant.From(in _animTween));
		info.AddProperty(PropertyName._glowTween, Variant.From(in _glowTween));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
		info.AddProperty(PropertyName._elapsedTime, Variant.From(in _elapsedTime));
		info.AddProperty(PropertyName._shouldPulse, Variant.From(in _shouldPulse));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsSkip, out var value))
		{
			IsSkip = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._outline, out var value2))
		{
			_outline = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._buttonImage, out var value3))
		{
			_buttonImage = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value4))
		{
			_label = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value5))
		{
			_hsv = value5.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._viewport, out var value6))
		{
			_viewport = value6.As<Viewport>();
		}
		if (info.TryGetProperty(PropertyName._defaultOutlineColor, out var value7))
		{
			_defaultOutlineColor = value7.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._hoveredOutlineColor, out var value8))
		{
			_hoveredOutlineColor = value8.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._downColor, out var value9))
		{
			_downColor = value9.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._outlineColor, out var value10))
		{
			_outlineColor = value10.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._outlineTransparentColor, out var value11))
		{
			_outlineTransparentColor = value11.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._animTween, out var value12))
		{
			_animTween = value12.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._glowTween, out var value13))
		{
			_glowTween = value13.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value14))
		{
			_hoverTween = value14.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._elapsedTime, out var value15))
		{
			_elapsedTime = value15.As<float>();
		}
		if (info.TryGetProperty(PropertyName._shouldPulse, out var value16))
		{
			_shouldPulse = value16.As<bool>();
		}
	}
}
