using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

/// <summary>
/// The buttons which are in the Action list for the monsters in the Bestiary.
/// Click on these buttons to play the associated animation: Attack, Hurt, Die, etc.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/Bestiary/NBestiaryMoveButton.cs")]
public class NBestiaryMoveButton : NButton
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
		/// Cached name for the '_UnhandledInput' method.
		/// </summary>
		public new static readonly StringName _UnhandledInput = "_UnhandledInput";

		/// <summary>
		/// Cached name for the 'OnRelease' method.
		/// </summary>
		public new static readonly StringName OnRelease = "OnRelease";

		/// <summary>
		/// Cached name for the 'OnPress' method.
		/// </summary>
		public new static readonly StringName OnPress = "OnPress";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'ClickedSfx' property.
		/// </summary>
		public new static readonly StringName ClickedSfx = "ClickedSfx";

		/// <summary>
		/// Cached name for the 'HoveredSfx' property.
		/// </summary>
		public new static readonly StringName HoveredSfx = "HoveredSfx";

		/// <summary>
		/// Cached name for the '_buttonAnimator' field.
		/// </summary>
		public static readonly StringName _buttonAnimator = "_buttonAnimator";

		/// <summary>
		/// Cached name for the '_label' field.
		/// </summary>
		public static readonly StringName _label = "_label";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";

		/// <summary>
		/// Cached name for the '_clickTween' field.
		/// </summary>
		public static readonly StringName _clickTween = "_clickTween";

		/// <summary>
		/// Cached name for the '_hotkey' field.
		/// </summary>
		public static readonly StringName _hotkey = "_hotkey";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/bestiary/bestiary_move_button");

	private Control _buttonAnimator;

	private MegaLabel _label;

	private Tween? _tween;

	private Tween? _clickTween;

	private StringName _hotkey;

	protected override string? ClickedSfx => null;

	protected override string? HoveredSfx => null;

	public BestiaryMonsterMove Move { get; private set; }

	public override void _Ready()
	{
		ConnectSignals();
		_label = GetNode<MegaLabel>("%Label");
		_buttonAnimator = GetNode<Control>("%ButtonAnimator");
		_label.SetTextAutoSize(Move.displayName);
		_label.PivotOffset = new Vector2(0f, _label.Size.Y * 0.5f);
		_buttonAnimator.PivotOffset = new Vector2(0f, _buttonAnimator.Size.Y * 0.5f);
	}

	public override void _UnhandledInput(InputEvent input)
	{
		if ((!(NControllerManager.Instance?.IsUsingController)) ?? true)
		{
			if (input.IsActionPressed(_hotkey))
			{
				OnPress();
			}
			else if (input.IsActionReleased(_hotkey))
			{
				EmitSignal(NClickableControl.SignalName.Released, this);
				OnRelease();
			}
		}
	}

	public static NBestiaryMoveButton Create(BestiaryMonsterMove move, StringName setHotkey)
	{
		NBestiaryMoveButton nBestiaryMoveButton = PreloadManager.Cache.GetAsset<PackedScene>(_scenePath).Instantiate<NBestiaryMoveButton>(PackedScene.GenEditState.Disabled);
		nBestiaryMoveButton._hotkey = setHotkey;
		nBestiaryMoveButton.Move = move;
		return nBestiaryMoveButton;
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		_label.Modulate = StsColors.green;
		_clickTween?.Kill();
		_clickTween = CreateTween().SetParallel();
		_clickTween.TweenProperty(_buttonAnimator, "scale", Vector2.One, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Bounce);
		_clickTween.TweenProperty(_label, "modulate", StsColors.cream, 0.2);
	}

	protected override void OnPress()
	{
		base.OnPress();
		_clickTween?.Kill();
		_clickTween = CreateTween().SetParallel();
		_clickTween.TweenProperty(_buttonAnimator, "scale", Vector2.One * 0.9f, 0.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_clickTween.TweenProperty(_label, "modulate", StsColors.lightGray, 0.05);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_label, "scale", Vector2.One * 1.05f, 0.05);
	}

	protected override void OnUnfocus()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_label, "scale", Vector2.One, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_clickTween?.Kill();
		_clickTween = CreateTween().SetParallel();
		_clickTween.TweenProperty(_buttonAnimator, "scale", Vector2.One, 0.1).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_clickTween.TweenProperty(_label, "modulate", StsColors.cream, 0.1);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._UnhandledInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "input", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._UnhandledInput && args.Count == 1)
		{
			_UnhandledInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
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
		if (method == MethodName._UnhandledInput)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.OnPress)
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
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._buttonAnimator)
		{
			_buttonAnimator = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._clickTween)
		{
			_clickTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._hotkey)
		{
			_hotkey = VariantUtils.ConvertTo<StringName>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		string from;
		if (name == PropertyName.ClickedSfx)
		{
			from = ClickedSfx;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HoveredSfx)
		{
			from = HoveredSfx;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._buttonAnimator)
		{
			value = VariantUtils.CreateFrom(in _buttonAnimator);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._clickTween)
		{
			value = VariantUtils.CreateFrom(in _clickTween);
			return true;
		}
		if (name == PropertyName._hotkey)
		{
			value = VariantUtils.CreateFrom(in _hotkey);
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
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.ClickedSfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.HoveredSfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._buttonAnimator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._clickTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.StringName, PropertyName._hotkey, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._buttonAnimator, Variant.From(in _buttonAnimator));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._clickTween, Variant.From(in _clickTween));
		info.AddProperty(PropertyName._hotkey, Variant.From(in _hotkey));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._buttonAnimator, out var value))
		{
			_buttonAnimator = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value2))
		{
			_label = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value3))
		{
			_tween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._clickTween, out var value4))
		{
			_clickTween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._hotkey, out var value5))
		{
			_hotkey = value5.As<StringName>();
		}
	}
}
