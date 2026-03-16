using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NSettingsTabManager.cs")]
public class NSettingsTabManager : Control
{
	[Signal]
	public delegate void TabChangedEventHandler();

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ResetTabs = "ResetTabs";

		public static readonly StringName Enable = "Enable";

		public static readonly StringName Disable = "Disable";

		public static readonly StringName TabLeft = "TabLeft";

		public static readonly StringName TabRight = "TabRight";

		public static readonly StringName SwitchTabTo = "SwitchTabTo";

		public static readonly StringName UpdateControllerButton = "UpdateControllerButton";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName CurrentlyDisplayedPanel = "CurrentlyDisplayedPanel";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _currentTab = "_currentTab";

		public static readonly StringName _scrollContainer = "_scrollContainer";

		public static readonly StringName _leftTriggerIcon = "_leftTriggerIcon";

		public static readonly StringName _rightTriggerIcon = "_rightTriggerIcon";

		public static readonly StringName _scrollbarTween = "_scrollbarTween";
	}

	public new class SignalName : Control.SignalName
	{
		public static readonly StringName TabChanged = "TabChanged";
	}

	private const float _scrollPaddingTop = 20f;

	private const float _scrollPaddingBottom = 30f;

	private static readonly StringName _tabLeftHotkey = MegaInput.viewDeckAndTabLeft;

	private static readonly StringName _tabRightHotkey = MegaInput.viewExhaustPileAndTabRight;

	private NSettingsTab? _currentTab;

	private NScrollableContainer _scrollContainer;

	private readonly Dictionary<NSettingsTab, NSettingsPanel> _tabs = new Dictionary<NSettingsTab, NSettingsPanel>();

	private TextureRect _leftTriggerIcon;

	private TextureRect _rightTriggerIcon;

	private Tween? _scrollbarTween;

	private TabChangedEventHandler backing_TabChanged;

	private NSettingsPanel CurrentlyDisplayedPanel => _tabs[_currentTab];

	public Control? DefaultFocusedControl
	{
		get
		{
			if (_currentTab == null)
			{
				return null;
			}
			return _tabs[_currentTab].DefaultFocusedControl;
		}
	}

	public event TabChangedEventHandler TabChanged
	{
		add
		{
			backing_TabChanged = (TabChangedEventHandler)Delegate.Combine(backing_TabChanged, value);
		}
		remove
		{
			backing_TabChanged = (TabChangedEventHandler)Delegate.Remove(backing_TabChanged, value);
		}
	}

	public override void _Ready()
	{
		_leftTriggerIcon = GetNode<TextureRect>("LeftTriggerIcon");
		_rightTriggerIcon = GetNode<TextureRect>("RightTriggerIcon");
		_scrollContainer = GetNode<NScrollableContainer>("%ScrollContainer");
		_scrollContainer.DisableScrollingIfContentFits();
		NSettingsTab node = GetNode<NSettingsTab>("General");
		node.SetLabel(new LocString("settings_ui", "TAB_GENERAL").GetFormattedText());
		_tabs.Add(node, GetNode<NSettingsPanel>("%GeneralSettings"));
		node = GetNode<NSettingsTab>("Graphics");
		node.SetLabel(new LocString("settings_ui", "TAB_GRAPHICS").GetFormattedText());
		_tabs.Add(node, GetNode<NSettingsPanel>("%GraphicsSettings"));
		node = GetNode<NSettingsTab>("Sound");
		node.SetLabel(new LocString("settings_ui", "TAB_SOUND").GetFormattedText());
		_tabs.Add(node, GetNode<NSettingsPanel>("%SoundSettings"));
		node = GetNode<NSettingsTab>("Input");
		node.SetLabel(new LocString("settings_ui", "TAB_INPUT").GetFormattedText());
		_tabs.Add(node, GetNode<NSettingsPanel>("%InputSettings"));
		foreach (NSettingsTab tab in _tabs.Keys)
		{
			tab.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
			{
				SwitchTabTo(tab);
			}));
		}
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateControllerButton));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateControllerButton));
		NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(UpdateControllerButton));
		UpdateControllerButton();
	}

	public void ResetTabs()
	{
		_tabs.First().Key.Select();
		SwitchTabTo(_tabs.First().Key);
	}

	public void Enable()
	{
		NHotkeyManager.Instance.PushHotkeyPressedBinding(_tabLeftHotkey, TabLeft);
		NHotkeyManager.Instance.PushHotkeyPressedBinding(_tabRightHotkey, TabRight);
	}

	public void Disable()
	{
		NHotkeyManager.Instance.RemoveHotkeyPressedBinding(_tabLeftHotkey, TabLeft);
		NHotkeyManager.Instance.RemoveHotkeyPressedBinding(_tabRightHotkey, TabRight);
	}

	private void TabLeft()
	{
		List<NSettingsTab> list = _tabs.Keys.ToList();
		int num = list.IndexOf(_currentTab) - 1;
		if (num >= 0)
		{
			SwitchTabTo(list[num]);
		}
	}

	private void TabRight()
	{
		List<NSettingsTab> list = _tabs.Keys.ToList();
		int num = Math.Min(list.Count - 1, list.IndexOf(_currentTab) + 1);
		if (num < list.Count)
		{
			SwitchTabTo(list[num]);
		}
	}

	private void SwitchTabTo(NSettingsTab selectedTab)
	{
		if (selectedTab != _currentTab)
		{
			foreach (NSettingsTab key in _tabs.Keys)
			{
				key.Deselect();
				_tabs[key].Visible = false;
			}
			selectedTab.Select();
			_tabs[selectedTab].Visible = true;
			_currentTab = selectedTab;
			_scrollContainer.SetContent(CurrentlyDisplayedPanel, 20f, 30f);
			_scrollContainer.InstantlyScrollToTop();
			_scrollbarTween?.Kill();
			_scrollbarTween = CreateTween().SetParallel();
			_scrollbarTween.TweenProperty(_scrollContainer.Scrollbar, "modulate", Colors.White, 0.5).From(StsColors.transparentBlack).SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Cubic);
		}
		ActiveScreenContext.Instance.Update();
	}

	private void UpdateControllerButton()
	{
		_leftTriggerIcon.Visible = NControllerManager.Instance.IsUsingController;
		_rightTriggerIcon.Visible = NControllerManager.Instance.IsUsingController;
		_leftTriggerIcon.Texture = NInputManager.Instance.GetHotkeyIcon(MegaInput.viewDeckAndTabLeft);
		_rightTriggerIcon.Texture = NInputManager.Instance.GetHotkeyIcon(MegaInput.viewExhaustPileAndTabRight);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ResetTabs, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Enable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Disable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TabLeft, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TabRight, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SwitchTabTo, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "selectedTab", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
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
		if (method == MethodName.ResetTabs && args.Count == 0)
		{
			ResetTabs();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Enable && args.Count == 0)
		{
			Enable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Disable && args.Count == 0)
		{
			Disable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TabLeft && args.Count == 0)
		{
			TabLeft();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TabRight && args.Count == 0)
		{
			TabRight();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SwitchTabTo && args.Count == 1)
		{
			SwitchTabTo(VariantUtils.ConvertTo<NSettingsTab>(in args[0]));
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
		if (method == MethodName.ResetTabs)
		{
			return true;
		}
		if (method == MethodName.Enable)
		{
			return true;
		}
		if (method == MethodName.Disable)
		{
			return true;
		}
		if (method == MethodName.TabLeft)
		{
			return true;
		}
		if (method == MethodName.TabRight)
		{
			return true;
		}
		if (method == MethodName.SwitchTabTo)
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
		if (name == PropertyName._currentTab)
		{
			_currentTab = VariantUtils.ConvertTo<NSettingsTab>(in value);
			return true;
		}
		if (name == PropertyName._scrollContainer)
		{
			_scrollContainer = VariantUtils.ConvertTo<NScrollableContainer>(in value);
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
		if (name == PropertyName._scrollbarTween)
		{
			_scrollbarTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.CurrentlyDisplayedPanel)
		{
			value = VariantUtils.CreateFrom<NSettingsPanel>(CurrentlyDisplayedPanel);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._currentTab)
		{
			value = VariantUtils.CreateFrom(in _currentTab);
			return true;
		}
		if (name == PropertyName._scrollContainer)
		{
			value = VariantUtils.CreateFrom(in _scrollContainer);
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
		if (name == PropertyName._scrollbarTween)
		{
			value = VariantUtils.CreateFrom(in _scrollbarTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentTab, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scrollContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CurrentlyDisplayedPanel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leftTriggerIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rightTriggerIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scrollbarTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._currentTab, Variant.From(in _currentTab));
		info.AddProperty(PropertyName._scrollContainer, Variant.From(in _scrollContainer));
		info.AddProperty(PropertyName._leftTriggerIcon, Variant.From(in _leftTriggerIcon));
		info.AddProperty(PropertyName._rightTriggerIcon, Variant.From(in _rightTriggerIcon));
		info.AddProperty(PropertyName._scrollbarTween, Variant.From(in _scrollbarTween));
		info.AddSignalEventDelegate(SignalName.TabChanged, backing_TabChanged);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._currentTab, out var value))
		{
			_currentTab = value.As<NSettingsTab>();
		}
		if (info.TryGetProperty(PropertyName._scrollContainer, out var value2))
		{
			_scrollContainer = value2.As<NScrollableContainer>();
		}
		if (info.TryGetProperty(PropertyName._leftTriggerIcon, out var value3))
		{
			_leftTriggerIcon = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._rightTriggerIcon, out var value4))
		{
			_rightTriggerIcon = value4.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._scrollbarTween, out var value5))
		{
			_scrollbarTween = value5.As<Tween>();
		}
		if (info.TryGetSignalEventDelegate<TabChangedEventHandler>(SignalName.TabChanged, out var value6))
		{
			backing_TabChanged = value6;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.TabChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalTabChanged()
	{
		EmitSignal(SignalName.TabChanged);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.TabChanged && args.Count == 0)
		{
			backing_TabChanged?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.TabChanged)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
