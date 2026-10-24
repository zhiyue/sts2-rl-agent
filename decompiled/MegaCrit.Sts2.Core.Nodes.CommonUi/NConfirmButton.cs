using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

/// <summary>
/// Confirm Button used in various places to confirm choices, embark on a run, etc.
/// Very useful but not as cool as Back Button but that's life.
/// Use the ButtonReleased Action to handle events.
/// </summary>
[ScriptPath("res://src/Core/Nodes/CommonUi/NConfirmButton.cs")]
public class NConfirmButton : NButton
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
		/// Cached name for the 'OnWindowChange' method.
		/// </summary>
		public static readonly StringName OnWindowChange = "OnWindowChange";

		/// <summary>
		/// Cached name for the 'OnEnable' method.
		/// </summary>
		public new static readonly StringName OnEnable = "OnEnable";

		/// <summary>
		/// Cached name for the 'OnDisable' method.
		/// </summary>
		public new static readonly StringName OnDisable = "OnDisable";

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
		/// Cached name for the 'OverrideHotkeys' method.
		/// </summary>
		public static readonly StringName OverrideHotkeys = "OverrideHotkeys";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Hotkeys' property.
		/// </summary>
		public new static readonly StringName Hotkeys = "Hotkeys";

		/// <summary>
		/// Cached name for the '_outline' field.
		/// </summary>
		public static readonly StringName _outline = "_outline";

		/// <summary>
		/// Cached name for the '_buttonImage' field.
		/// </summary>
		public static readonly StringName _buttonImage = "_buttonImage";

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
		/// Cached name for the '_viewport' field.
		/// </summary>
		public static readonly StringName _viewport = "_viewport";

		/// <summary>
		/// Cached name for the '_hotkeys' field.
		/// </summary>
		public static readonly StringName _hotkeys = "_hotkeys";

		/// <summary>
		/// Cached name for the '_posOffset' field.
		/// </summary>
		public static readonly StringName _posOffset = "_posOffset";

		/// <summary>
		/// Cached name for the '_showPos' field.
		/// </summary>
		public static readonly StringName _showPos = "_showPos";

		/// <summary>
		/// Cached name for the '_hidePos' field.
		/// </summary>
		public static readonly StringName _hidePos = "_hidePos";

		/// <summary>
		/// Cached name for the '_moveTween' field.
		/// </summary>
		public static readonly StringName _moveTween = "_moveTween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private Control _outline;

	private Control _buttonImage;

	private Color _defaultOutlineColor = StsColors.cream;

	private Color _hoveredOutlineColor = StsColors.gold;

	private Color _downColor = Colors.Gray;

	private Color _outlineColor = new Color("F0B400");

	private Color _outlineTransparentColor = new Color("00FFFF00");

	private Viewport _viewport;

	private string[] _hotkeys = new string[1] { MegaInput.accept };

	private static readonly Vector2 _hoverScale = new Vector2(1.05f, 1.05f);

	private static readonly Vector2 _downScale = new Vector2(0.95f, 0.95f);

	private const float _pressDownDur = 0.25f;

	private const float _unhoverAnimDur = 0.5f;

	private const float _animInOutDur = 0.35f;

	private Vector2 _posOffset;

	private Vector2 _showPos;

	private Vector2 _hidePos;

	private static readonly Vector2 _hideOffset = new Vector2(180f, 0f);

	private Tween? _moveTween;

	private CancellationTokenSource? _pressDownCancelToken;

	private CancellationTokenSource? _unhoverAnimCancelToken;

	protected override string[] Hotkeys => _hotkeys;

	public override void _Ready()
	{
		ConnectSignals();
		_isEnabled = false;
		_outline = GetNode<Control>("Outline");
		_buttonImage = GetNode<Control>("Image");
		_viewport = GetViewport();
		_posOffset = new Vector2(base.OffsetRight + 120f, 0f - base.OffsetBottom + 110f);
		GetTree().Root.Connect(Viewport.SignalName.SizeChanged, Callable.From(OnWindowChange));
		OnWindowChange();
		OnDisable();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_pressDownCancelToken?.Cancel();
		_unhoverAnimCancelToken?.Cancel();
	}

	private void OnWindowChange()
	{
		_showPos = NGame.Instance.Size - _posOffset;
		_hidePos = _showPos + _hideOffset;
		base.Position = (_isEnabled ? _showPos : _hidePos);
	}

	/// <summary>
	/// Call when we want this button to animate in.
	/// </summary>
	protected override void OnEnable()
	{
		base.OnEnable();
		_isEnabled = true;
		_outline.Modulate = Colors.Transparent;
		_buttonImage.Modulate = Colors.White;
		_moveTween?.Kill();
		_moveTween = CreateTween();
		_moveTween.TweenProperty(this, "position", _showPos, 0.3499999940395355).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.FromCurrent();
	}

	/// <summary>
	/// Call when we want this button to hide this button (Disables clickability/hotkeys)
	/// </summary>
	protected override void OnDisable()
	{
		base.OnDisable();
		_isEnabled = false;
		_moveTween?.Kill();
		_moveTween = CreateTween();
		_moveTween.TweenProperty(this, "position", _hidePos, 0.3499999940395355).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.FromCurrent();
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_unhoverAnimCancelToken?.Cancel();
		base.Scale = _hoverScale;
		_outline.Modulate = _outlineColor;
		_buttonImage.Modulate = Colors.White;
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_pressDownCancelToken?.Cancel();
		_unhoverAnimCancelToken = new CancellationTokenSource();
		TaskHelper.RunSafely(AnimUnhover(_unhoverAnimCancelToken));
	}

	private async Task AnimUnhover(CancellationTokenSource cancelToken)
	{
		float num = 0f;
		Vector2 startScale = base.Scale;
		Color startButtonColor = _buttonImage.Modulate;
		Color startColor = _outline.Modulate;
		while (num < 0.5f)
		{
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}
			base.Scale = startScale.Lerp(Vector2.One, Ease.ExpoOut(num / 0.5f));
			_outline.Modulate = startColor.Lerp(_outlineTransparentColor, Ease.ExpoOut(num / 0.5f));
			_buttonImage.Modulate = startButtonColor.Lerp(Colors.White, Ease.ExpoOut(num / 0.5f));
			float num2 = num;
			num = num2 + await this.AwaitProcessFrame();
		}
		base.Scale = Vector2.One;
		_outline.Modulate = _outlineTransparentColor;
		_buttonImage.Modulate = Colors.White;
	}

	protected override void OnPress()
	{
		base.OnPress();
		_pressDownCancelToken = new CancellationTokenSource();
		TaskHelper.RunSafely(AnimPressDown(_pressDownCancelToken));
	}

	private async Task AnimPressDown(CancellationTokenSource cancelToken)
	{
		float num = 0f;
		_buttonImage.Modulate = Colors.White;
		_outline.Modulate = _outlineColor;
		base.Scale = _hoverScale;
		while (num < 0.25f)
		{
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}
			base.Scale = _hoverScale.Lerp(_downScale, Ease.CubicOut(num / 0.25f));
			_buttonImage.Modulate = Colors.White.Lerp(_downColor, Ease.CubicOut(num / 0.25f));
			_outline.Modulate = _outlineColor.Lerp(_outlineTransparentColor, Ease.CubicOut(num / 0.25f));
			float num2 = num;
			num = num2 + await this.AwaitProcessFrame();
		}
		base.Scale = _downScale;
		_buttonImage.Modulate = _downColor;
		_outline.Modulate = _outlineTransparentColor;
	}

	public void OverrideHotkeys(string[] hotkeys)
	{
		_hotkeys = hotkeys;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OverrideHotkeys, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.PackedStringArray, "hotkeys", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.OnWindowChange && args.Count == 0)
		{
			OnWindowChange();
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
		if (method == MethodName.OverrideHotkeys && args.Count == 1)
		{
			OverrideHotkeys(VariantUtils.ConvertTo<string[]>(in args[0]));
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
		if (method == MethodName.OnWindowChange)
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
		if (method == MethodName.OverrideHotkeys)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
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
		if (name == PropertyName._viewport)
		{
			_viewport = VariantUtils.ConvertTo<Viewport>(in value);
			return true;
		}
		if (name == PropertyName._hotkeys)
		{
			_hotkeys = VariantUtils.ConvertTo<string[]>(in value);
			return true;
		}
		if (name == PropertyName._posOffset)
		{
			_posOffset = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._showPos)
		{
			_showPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._hidePos)
		{
			_hidePos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._moveTween)
		{
			_moveTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Hotkeys)
		{
			value = VariantUtils.CreateFrom<string[]>(Hotkeys);
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
		if (name == PropertyName._viewport)
		{
			value = VariantUtils.CreateFrom(in _viewport);
			return true;
		}
		if (name == PropertyName._hotkeys)
		{
			value = VariantUtils.CreateFrom(in _hotkeys);
			return true;
		}
		if (name == PropertyName._posOffset)
		{
			value = VariantUtils.CreateFrom(in _posOffset);
			return true;
		}
		if (name == PropertyName._showPos)
		{
			value = VariantUtils.CreateFrom(in _showPos);
			return true;
		}
		if (name == PropertyName._hidePos)
		{
			value = VariantUtils.CreateFrom(in _hidePos);
			return true;
		}
		if (name == PropertyName._moveTween)
		{
			value = VariantUtils.CreateFrom(in _moveTween);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._buttonImage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._defaultOutlineColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._hoveredOutlineColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._downColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._outlineColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._outlineTransparentColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._viewport, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName._hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._posOffset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._showPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._hidePos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._moveTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._outline, Variant.From(in _outline));
		info.AddProperty(PropertyName._buttonImage, Variant.From(in _buttonImage));
		info.AddProperty(PropertyName._defaultOutlineColor, Variant.From(in _defaultOutlineColor));
		info.AddProperty(PropertyName._hoveredOutlineColor, Variant.From(in _hoveredOutlineColor));
		info.AddProperty(PropertyName._downColor, Variant.From(in _downColor));
		info.AddProperty(PropertyName._outlineColor, Variant.From(in _outlineColor));
		info.AddProperty(PropertyName._outlineTransparentColor, Variant.From(in _outlineTransparentColor));
		info.AddProperty(PropertyName._viewport, Variant.From(in _viewport));
		info.AddProperty(PropertyName._hotkeys, Variant.From(in _hotkeys));
		info.AddProperty(PropertyName._posOffset, Variant.From(in _posOffset));
		info.AddProperty(PropertyName._showPos, Variant.From(in _showPos));
		info.AddProperty(PropertyName._hidePos, Variant.From(in _hidePos));
		info.AddProperty(PropertyName._moveTween, Variant.From(in _moveTween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._outline, out var value))
		{
			_outline = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._buttonImage, out var value2))
		{
			_buttonImage = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._defaultOutlineColor, out var value3))
		{
			_defaultOutlineColor = value3.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._hoveredOutlineColor, out var value4))
		{
			_hoveredOutlineColor = value4.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._downColor, out var value5))
		{
			_downColor = value5.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._outlineColor, out var value6))
		{
			_outlineColor = value6.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._outlineTransparentColor, out var value7))
		{
			_outlineTransparentColor = value7.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._viewport, out var value8))
		{
			_viewport = value8.As<Viewport>();
		}
		if (info.TryGetProperty(PropertyName._hotkeys, out var value9))
		{
			_hotkeys = value9.As<string[]>();
		}
		if (info.TryGetProperty(PropertyName._posOffset, out var value10))
		{
			_posOffset = value10.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._showPos, out var value11))
		{
			_showPos = value11.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._hidePos, out var value12))
		{
			_hidePos = value12.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._moveTween, out var value13))
		{
			_moveTween = value13.As<Tween>();
		}
	}
}
