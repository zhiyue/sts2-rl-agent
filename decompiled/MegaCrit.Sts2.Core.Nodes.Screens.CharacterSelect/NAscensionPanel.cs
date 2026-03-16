using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

[ScriptPath("res://src/Core/Nodes/Screens/CharacterSelect/NAscensionPanel.cs")]
public class NAscensionPanel : Control
{
	[Signal]
	public delegate void AscensionLevelChangedEventHandler();

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Initialize = "Initialize";

		public static readonly StringName SetFireBlue = "SetFireBlue";

		public static readonly StringName SetFireRed = "SetFireRed";

		public static readonly StringName Cleanup = "Cleanup";

		public static readonly StringName SetAscensionLevel = "SetAscensionLevel";

		public static readonly StringName IncrementAscension = "IncrementAscension";

		public static readonly StringName DecrementAscension = "DecrementAscension";

		public static readonly StringName RefreshArrowVisibility = "RefreshArrowVisibility";

		public static readonly StringName SetMaxAscension = "SetMaxAscension";

		public static readonly StringName RefreshAscensionText = "RefreshAscensionText";

		public static readonly StringName AnimIn = "AnimIn";

		public static readonly StringName UpdateControllerButton = "UpdateControllerButton";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName Ascension = "Ascension";

		public static readonly StringName _maxAscension = "_maxAscension";

		public static readonly StringName _leftArrow = "_leftArrow";

		public static readonly StringName _rightArrow = "_rightArrow";

		public static readonly StringName _ascensionLevel = "_ascensionLevel";

		public static readonly StringName _info = "_info";

		public static readonly StringName _leftTriggerIcon = "_leftTriggerIcon";

		public static readonly StringName _rightTriggerIcon = "_rightTriggerIcon";

		public static readonly StringName _iconHsv = "_iconHsv";

		public static readonly StringName _arrowsVisible = "_arrowsVisible";

		public static readonly StringName _mode = "_mode";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Control.SignalName
	{
		public static readonly StringName AscensionLevelChanged = "AscensionLevelChanged";
	}

	private static readonly StringName _tabLeftHotkey = MegaInput.viewDeckAndTabLeft;

	private static readonly StringName _tabRightHotkey = MegaInput.viewExhaustPileAndTabRight;

	private static readonly StringName _fontOutlineTheme = "font_outline_color";

	private static readonly StringName _h = new StringName("h");

	private static readonly StringName _v = new StringName("v");

	private static readonly Color _redLabelOutline = new Color("593400");

	private static readonly Color _blueLabelOutline = new Color("004759");

	private int _maxAscension;

	private NButton _leftArrow;

	private NButton _rightArrow;

	private MegaLabel _ascensionLevel;

	private MegaRichTextLabel _info;

	private TextureRect _leftTriggerIcon;

	private TextureRect _rightTriggerIcon;

	private ShaderMaterial _iconHsv;

	private bool _arrowsVisible = true;

	private MultiplayerUiMode _mode = MultiplayerUiMode.Singleplayer;

	private Tween? _tween;

	private AscensionLevelChangedEventHandler backing_AscensionLevelChanged;

	public int Ascension { get; private set; }

	public event AscensionLevelChangedEventHandler AscensionLevelChanged
	{
		add
		{
			backing_AscensionLevelChanged = (AscensionLevelChangedEventHandler)Delegate.Combine(backing_AscensionLevelChanged, value);
		}
		remove
		{
			backing_AscensionLevelChanged = (AscensionLevelChangedEventHandler)Delegate.Remove(backing_AscensionLevelChanged, value);
		}
	}

	public override void _Ready()
	{
		_leftTriggerIcon = GetNode<TextureRect>("%LeftTriggerIcon");
		_rightTriggerIcon = GetNode<TextureRect>("%RightTriggerIcon");
		_leftArrow = GetNode<NButton>("HBoxContainer/LeftArrowContainer/LeftArrow");
		_rightArrow = GetNode<NButton>("HBoxContainer/RightArrowContainer/RightArrow");
		_ascensionLevel = GetNode<MegaLabel>("HBoxContainer/AscensionIconContainer/AscensionIcon/AscensionLevel");
		_info = GetNode<MegaRichTextLabel>("HBoxContainer/AscensionDescription/Description");
		_iconHsv = (ShaderMaterial)GetNode<Control>("%AscensionIcon").Material;
		_leftArrow.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
		{
			DecrementAscension();
		}));
		_rightArrow.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
		{
			IncrementAscension();
		}));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateControllerButton));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateControllerButton));
		NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(UpdateControllerButton));
		UpdateControllerButton();
	}

	public void Initialize(MultiplayerUiMode mode)
	{
		_mode = mode;
		if (_mode == MultiplayerUiMode.Host)
		{
			SetFireBlue();
			_arrowsVisible = true;
			SetMaxAscension(SaveManager.Instance.Progress.MaxMultiplayerAscension);
			SetAscensionLevel(Math.Min(_maxAscension, SaveManager.Instance.Progress.PreferredMultiplayerAscension));
			NHotkeyManager.Instance.PushHotkeyPressedBinding(_tabLeftHotkey, DecrementAscension);
			NHotkeyManager.Instance.PushHotkeyPressedBinding(_tabRightHotkey, IncrementAscension);
		}
		else if (_mode == MultiplayerUiMode.Singleplayer)
		{
			SetFireRed();
			_arrowsVisible = true;
			SetMaxAscension(0);
			SetAscensionLevel(0);
			NHotkeyManager.Instance.PushHotkeyPressedBinding(_tabLeftHotkey, DecrementAscension);
			NHotkeyManager.Instance.PushHotkeyPressedBinding(_tabRightHotkey, IncrementAscension);
		}
		else
		{
			MultiplayerUiMode mode2 = _mode;
			if ((uint)(mode2 - 3) <= 1u)
			{
				SetFireBlue();
				_arrowsVisible = false;
				SetMaxAscension(0);
			}
		}
	}

	private void SetFireBlue()
	{
		_iconHsv.SetShaderParameter(_h, 0.52f);
		_iconHsv.SetShaderParameter(_v, 1.2f);
		_ascensionLevel.AddThemeColorOverride(_fontOutlineTheme, _blueLabelOutline);
	}

	private void SetFireRed()
	{
		_iconHsv.SetShaderParameter(_h, 1f);
		_iconHsv.SetShaderParameter(_v, 1f);
		_ascensionLevel.AddThemeColorOverride(_fontOutlineTheme, _redLabelOutline);
	}

	public void Cleanup()
	{
		MultiplayerUiMode mode = _mode;
		if ((uint)(mode - 1) <= 1u)
		{
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(_tabLeftHotkey, DecrementAscension);
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(_tabRightHotkey, IncrementAscension);
		}
	}

	public void SetAscensionLevel(int ascension)
	{
		if (Ascension != ascension)
		{
			Ascension = ascension;
			EmitSignal(SignalName.AscensionLevelChanged);
		}
		RefreshAscensionText();
		RefreshArrowVisibility();
	}

	private void IncrementAscension()
	{
		if (Ascension < _maxAscension)
		{
			SetAscensionLevel(Ascension + 1);
		}
	}

	private void DecrementAscension()
	{
		if (Ascension > 0)
		{
			SetAscensionLevel(Ascension - 1);
		}
	}

	private void RefreshArrowVisibility()
	{
		_leftArrow.Visible = _arrowsVisible && Ascension != 0;
		_rightArrow.Visible = _arrowsVisible && Ascension != _maxAscension;
	}

	public void SetMaxAscension(int maxAscension)
	{
		Log.Info($"Max ascension changed to {maxAscension}");
		_maxAscension = maxAscension;
		if (Ascension >= _maxAscension)
		{
			SetAscensionLevel(_maxAscension);
		}
		base.Visible = _maxAscension > 0;
		RefreshArrowVisibility();
	}

	private void RefreshAscensionText()
	{
		_ascensionLevel.SetTextAutoSize(Ascension.ToString());
		string formattedText = AscensionHelper.GetTitle(Ascension).GetFormattedText();
		string formattedText2 = AscensionHelper.GetDescription(Ascension).GetFormattedText();
		_info.Text = "[b][gold]" + formattedText + "[/gold][/b]\n" + formattedText2;
	}

	public void AnimIn()
	{
		if (base.Visible)
		{
			Color modulate = base.Modulate;
			modulate.A = 0f;
			base.Modulate = modulate;
			_tween?.FastForwardToCompletion();
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(this, "modulate:a", 1f, 0.2);
			_tween.TweenProperty(this, "position:y", base.Position.Y, 0.3).From(base.Position.Y + 30f).SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Back);
		}
	}

	private void UpdateControllerButton()
	{
		MultiplayerUiMode mode = _mode;
		if ((uint)(mode - 1) <= 1u)
		{
			_leftTriggerIcon.Visible = NControllerManager.Instance.IsUsingController;
			_rightTriggerIcon.Visible = NControllerManager.Instance.IsUsingController;
			_leftTriggerIcon.Texture = NInputManager.Instance.GetHotkeyIcon(MegaInput.viewDeckAndTabLeft);
			_rightTriggerIcon.Texture = NInputManager.Instance.GetHotkeyIcon(MegaInput.viewExhaustPileAndTabRight);
		}
		else
		{
			_leftTriggerIcon.Visible = false;
			_rightTriggerIcon.Visible = false;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(13);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Initialize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "mode", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetFireBlue, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetFireRed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Cleanup, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetAscensionLevel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "ascension", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.IncrementAscension, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DecrementAscension, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshArrowVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetMaxAscension, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "maxAscension", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshAscensionText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateControllerButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Initialize && args.Count == 1)
		{
			Initialize(VariantUtils.ConvertTo<MultiplayerUiMode>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetFireBlue && args.Count == 0)
		{
			SetFireBlue();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetFireRed && args.Count == 0)
		{
			SetFireRed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Cleanup && args.Count == 0)
		{
			Cleanup();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetAscensionLevel && args.Count == 1)
		{
			SetAscensionLevel(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IncrementAscension && args.Count == 0)
		{
			IncrementAscension();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DecrementAscension && args.Count == 0)
		{
			DecrementAscension();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshArrowVisibility && args.Count == 0)
		{
			RefreshArrowVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetMaxAscension && args.Count == 1)
		{
			SetMaxAscension(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshAscensionText && args.Count == 0)
		{
			RefreshAscensionText();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimIn && args.Count == 0)
		{
			AnimIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateControllerButton && args.Count == 0)
		{
			UpdateControllerButton();
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
		if (method == MethodName.Initialize)
		{
			return true;
		}
		if (method == MethodName.SetFireBlue)
		{
			return true;
		}
		if (method == MethodName.SetFireRed)
		{
			return true;
		}
		if (method == MethodName.Cleanup)
		{
			return true;
		}
		if (method == MethodName.SetAscensionLevel)
		{
			return true;
		}
		if (method == MethodName.IncrementAscension)
		{
			return true;
		}
		if (method == MethodName.DecrementAscension)
		{
			return true;
		}
		if (method == MethodName.RefreshArrowVisibility)
		{
			return true;
		}
		if (method == MethodName.SetMaxAscension)
		{
			return true;
		}
		if (method == MethodName.RefreshAscensionText)
		{
			return true;
		}
		if (method == MethodName.AnimIn)
		{
			return true;
		}
		if (method == MethodName.UpdateControllerButton)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Ascension)
		{
			Ascension = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._maxAscension)
		{
			_maxAscension = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._leftArrow)
		{
			_leftArrow = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._rightArrow)
		{
			_rightArrow = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._ascensionLevel)
		{
			_ascensionLevel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._info)
		{
			_info = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._leftTriggerIcon)
		{
			_leftTriggerIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._rightTriggerIcon)
		{
			_rightTriggerIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._iconHsv)
		{
			_iconHsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._arrowsVisible)
		{
			_arrowsVisible = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._mode)
		{
			_mode = VariantUtils.ConvertTo<MultiplayerUiMode>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Ascension)
		{
			value = VariantUtils.CreateFrom<int>(Ascension);
			return true;
		}
		if (name == PropertyName._maxAscension)
		{
			value = VariantUtils.CreateFrom(in _maxAscension);
			return true;
		}
		if (name == PropertyName._leftArrow)
		{
			value = VariantUtils.CreateFrom(in _leftArrow);
			return true;
		}
		if (name == PropertyName._rightArrow)
		{
			value = VariantUtils.CreateFrom(in _rightArrow);
			return true;
		}
		if (name == PropertyName._ascensionLevel)
		{
			value = VariantUtils.CreateFrom(in _ascensionLevel);
			return true;
		}
		if (name == PropertyName._info)
		{
			value = VariantUtils.CreateFrom(in _info);
			return true;
		}
		if (name == PropertyName._leftTriggerIcon)
		{
			value = VariantUtils.CreateFrom(in _leftTriggerIcon);
			return true;
		}
		if (name == PropertyName._rightTriggerIcon)
		{
			value = VariantUtils.CreateFrom(in _rightTriggerIcon);
			return true;
		}
		if (name == PropertyName._iconHsv)
		{
			value = VariantUtils.CreateFrom(in _iconHsv);
			return true;
		}
		if (name == PropertyName._arrowsVisible)
		{
			value = VariantUtils.CreateFrom(in _arrowsVisible);
			return true;
		}
		if (name == PropertyName._mode)
		{
			value = VariantUtils.CreateFrom(in _mode);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.Ascension, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._maxAscension, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leftArrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rightArrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionLevel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._info, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leftTriggerIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rightTriggerIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._iconHsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._arrowsVisible, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._mode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Ascension, Variant.From<int>(Ascension));
		info.AddProperty(PropertyName._maxAscension, Variant.From(in _maxAscension));
		info.AddProperty(PropertyName._leftArrow, Variant.From(in _leftArrow));
		info.AddProperty(PropertyName._rightArrow, Variant.From(in _rightArrow));
		info.AddProperty(PropertyName._ascensionLevel, Variant.From(in _ascensionLevel));
		info.AddProperty(PropertyName._info, Variant.From(in _info));
		info.AddProperty(PropertyName._leftTriggerIcon, Variant.From(in _leftTriggerIcon));
		info.AddProperty(PropertyName._rightTriggerIcon, Variant.From(in _rightTriggerIcon));
		info.AddProperty(PropertyName._iconHsv, Variant.From(in _iconHsv));
		info.AddProperty(PropertyName._arrowsVisible, Variant.From(in _arrowsVisible));
		info.AddProperty(PropertyName._mode, Variant.From(in _mode));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddSignalEventDelegate(SignalName.AscensionLevelChanged, backing_AscensionLevelChanged);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Ascension, out var value))
		{
			Ascension = value.As<int>();
		}
		if (info.TryGetProperty(PropertyName._maxAscension, out var value2))
		{
			_maxAscension = value2.As<int>();
		}
		if (info.TryGetProperty(PropertyName._leftArrow, out var value3))
		{
			_leftArrow = value3.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._rightArrow, out var value4))
		{
			_rightArrow = value4.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._ascensionLevel, out var value5))
		{
			_ascensionLevel = value5.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._info, out var value6))
		{
			_info = value6.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._leftTriggerIcon, out var value7))
		{
			_leftTriggerIcon = value7.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._rightTriggerIcon, out var value8))
		{
			_rightTriggerIcon = value8.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._iconHsv, out var value9))
		{
			_iconHsv = value9.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._arrowsVisible, out var value10))
		{
			_arrowsVisible = value10.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._mode, out var value11))
		{
			_mode = value11.As<MultiplayerUiMode>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value12))
		{
			_tween = value12.As<Tween>();
		}
		if (info.TryGetSignalEventDelegate<AscensionLevelChangedEventHandler>(SignalName.AscensionLevelChanged, out var value13))
		{
			backing_AscensionLevelChanged = value13;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.AscensionLevelChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalAscensionLevelChanged()
	{
		EmitSignal(SignalName.AscensionLevelChanged);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.AscensionLevelChanged && args.Count == 0)
		{
			backing_AscensionLevelChanged?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.AscensionLevelChanged)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
