using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace MegaCrit.Sts2.Core.Nodes.Screens.StatsScreen;

[ScriptPath("res://src/Core/Nodes/Screens/StatsScreen/NStatsTabManager.cs")]
public class NStatsTabManager : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ResetTabs = "ResetTabs";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName SwitchToTab = "SwitchToTab";

		public static readonly StringName UpdateControllerButton = "UpdateControllerButton";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _leftTriggerIcon = "_leftTriggerIcon";

		public static readonly StringName _rightTriggerIcon = "_rightTriggerIcon";

		public static readonly StringName _tabContainer = "_tabContainer";

		public static readonly StringName _currentTab = "_currentTab";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly StringName _tabLeftHotkey = MegaInput.viewDeckAndTabLeft;

	private static readonly StringName _tabRightHotkey = MegaInput.viewExhaustPileAndTabRight;

	private Control _leftTriggerIcon;

	private Control _rightTriggerIcon;

	private Control _tabContainer;

	private List<NSettingsTab> _tabs;

	private NSettingsTab? _currentTab;

	public override void _Ready()
	{
		_leftTriggerIcon = GetNode<Control>("LeftTriggerIcon");
		_rightTriggerIcon = GetNode<Control>("RightTriggerIcon");
		_tabContainer = GetNode<Control>("TabContainer");
		_tabs = _tabContainer.GetChildren().OfType<NSettingsTab>().ToList();
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateControllerButton));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateControllerButton));
		NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(UpdateControllerButton));
		foreach (NSettingsTab nSettingsTab in _tabs)
		{
			nSettingsTab.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(delegate
			{
				SwitchToTab(nSettingsTab);
			}));
		}
		UpdateControllerButton();
	}

	public void ResetTabs()
	{
		SwitchToTab(_tabContainer.GetChild<NSettingsTab>(0));
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (!IsVisibleInTree() || NDevConsole.Instance.Visible)
		{
			return;
		}
		Control control = GetViewport().GuiGetFocusOwner();
		if ((control is TextEdit || control is LineEdit) ? true : false)
		{
			return;
		}
		if (inputEvent.IsActionPressed(_tabLeftHotkey))
		{
			int num = _tabs.IndexOf(_currentTab) - 1;
			if (num >= 0)
			{
				_tabs[num].ForceTabPressed();
				SwitchToTab(_tabs[num]);
			}
		}
		if (inputEvent.IsActionPressed(_tabRightHotkey))
		{
			int num2 = Math.Min(_tabs.Count - 1, _tabs.IndexOf(_currentTab) + 1);
			if (num2 < _tabs.Count)
			{
				_tabs[num2].ForceTabPressed();
			}
		}
	}

	private void SwitchToTab(NSettingsTab tab)
	{
		_currentTab = tab;
		foreach (NSettingsTab tab2 in _tabs)
		{
			if (tab2 != _currentTab)
			{
				tab2.Deselect();
			}
			else
			{
				tab2.Select();
			}
		}
	}

	private void UpdateControllerButton()
	{
		_leftTriggerIcon.Visible = NControllerManager.Instance.IsUsingController;
		_rightTriggerIcon.Visible = NControllerManager.Instance.IsUsingController;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ResetTabs, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SwitchToTab, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tab", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
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
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SwitchToTab && args.Count == 1)
		{
			SwitchToTab(VariantUtils.ConvertTo<NSettingsTab>(in args[0]));
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
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.SwitchToTab)
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
		if (name == PropertyName._leftTriggerIcon)
		{
			_leftTriggerIcon = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._rightTriggerIcon)
		{
			_rightTriggerIcon = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._tabContainer)
		{
			_tabContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._currentTab)
		{
			_currentTab = VariantUtils.ConvertTo<NSettingsTab>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
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
		if (name == PropertyName._tabContainer)
		{
			value = VariantUtils.CreateFrom(in _tabContainer);
			return true;
		}
		if (name == PropertyName._currentTab)
		{
			value = VariantUtils.CreateFrom(in _currentTab);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leftTriggerIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rightTriggerIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tabContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentTab, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._leftTriggerIcon, Variant.From(in _leftTriggerIcon));
		info.AddProperty(PropertyName._rightTriggerIcon, Variant.From(in _rightTriggerIcon));
		info.AddProperty(PropertyName._tabContainer, Variant.From(in _tabContainer));
		info.AddProperty(PropertyName._currentTab, Variant.From(in _currentTab));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._leftTriggerIcon, out var value))
		{
			_leftTriggerIcon = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._rightTriggerIcon, out var value2))
		{
			_rightTriggerIcon = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._tabContainer, out var value3))
		{
			_tabContainer = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._currentTab, out var value4))
		{
			_currentTab = value4.As<NSettingsTab>();
		}
	}
}
