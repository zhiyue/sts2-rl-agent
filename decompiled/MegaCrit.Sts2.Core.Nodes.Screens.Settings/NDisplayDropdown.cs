using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NDisplayDropdown.cs")]
public class NDisplayDropdown : NSettingsDropdown
{
	public new class MethodName : NSettingsDropdown.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Notification = "_Notification";

		public static readonly StringName OnWindowChange = "OnWindowChange";

		public static readonly StringName OnDropdownItemSelected = "OnDropdownItemSelected";
	}

	public new class PropertyName : NSettingsDropdown.PropertyName
	{
		public static readonly StringName _dropdownItemScene = "_dropdownItemScene";

		public static readonly StringName _currentDisplayIndex = "_currentDisplayIndex";
	}

	public new class SignalName : NSettingsDropdown.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private PackedScene _dropdownItemScene;

	private static readonly LocString _optionString = new LocString("settings_ui", "DISPLAY_DROPDOWN_OPTION");

	private int _currentDisplayIndex = -1;

	public override void _Ready()
	{
		ConnectSignals();
		NGame.Instance.Connect(NGame.SignalName.WindowChange, Callable.From<bool>(OnWindowChange));
		OnWindowChange(SaveManager.Instance.SettingsSave.AspectRatioSetting == AspectRatioSetting.Auto);
		ClearDropdownItems();
		for (int i = 0; i < DisplayServer.GetScreenCount(); i++)
		{
			NDisplayDropdownItem nDisplayDropdownItem = _dropdownItemScene.Instantiate<NDisplayDropdownItem>(PackedScene.GenEditState.Disabled);
			_dropdownItems.AddChildSafely(nDisplayDropdownItem);
			nDisplayDropdownItem.Connect(NDropdownItem.SignalName.Selected, Callable.From<NDropdownItem>(OnDropdownItemSelected));
			nDisplayDropdownItem.Init(i);
		}
		_dropdownItems.GetParent<NDropdownContainer>().RefreshLayout();
	}

	public override void _Notification(int what)
	{
		if ((long)what == 1012 && IsNodeReady())
		{
			OnWindowChange(SaveManager.Instance.SettingsSave.AspectRatioSetting == AspectRatioSetting.Auto);
		}
	}

	private void OnWindowChange(bool _)
	{
		long num = GetWindow().CurrentScreen;
		if (num != _currentDisplayIndex)
		{
			_currentDisplayIndex = (int)num;
			_optionString.Add("MonitorIndex", _currentDisplayIndex);
			_currentOptionLabel.SetTextAutoSize(_optionString.GetFormattedText());
			SaveManager.Instance.SettingsSave.TargetDisplay = _currentDisplayIndex;
			NResolutionDropdown? instance = NResolutionDropdown.Instance;
			if (instance != null && instance.IsNodeReady())
			{
				NResolutionDropdown.Instance.RefreshCurrentlySelectedResolution();
				NResolutionDropdown.Instance.PopulateDropdownItems();
			}
		}
	}

	private void OnDropdownItemSelected(NDropdownItem nDropdownItem)
	{
		SettingsSave settingsSave = SaveManager.Instance.SettingsSave;
		NDisplayDropdownItem nDisplayDropdownItem = (NDisplayDropdownItem)nDropdownItem;
		if (nDisplayDropdownItem.displayIndex != _currentDisplayIndex)
		{
			CloseDropdown();
			settingsSave.TargetDisplay = nDisplayDropdownItem.displayIndex;
			if (!settingsSave.Fullscreen && !PlatformUtil.GetSupportedWindowMode().ShouldForceFullscreen())
			{
				Vector2I vector2I = DisplayServer.ScreenGetSize(nDisplayDropdownItem.displayIndex);
				settingsSave.WindowPosition = vector2I / 8;
				settingsSave.WindowSize = new Vector2I((int)((float)vector2I.X * 0.75f), (int)((float)vector2I.Y * 0.75f));
			}
			NResolutionDropdown.Instance.RefreshCurrentlySelectedResolution();
			NResolutionDropdown.Instance.PopulateDropdownItems();
			NGame.Instance.ApplyDisplaySettings();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Notification, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "what", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnDropdownItemSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "nDropdownItem", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
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
		if (method == MethodName._Notification && args.Count == 1)
		{
			_Notification(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnWindowChange && args.Count == 1)
		{
			OnWindowChange(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDropdownItemSelected && args.Count == 1)
		{
			OnDropdownItemSelected(VariantUtils.ConvertTo<NDropdownItem>(in args[0]));
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
		if (method == MethodName._Notification)
		{
			return true;
		}
		if (method == MethodName.OnWindowChange)
		{
			return true;
		}
		if (method == MethodName.OnDropdownItemSelected)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._dropdownItemScene)
		{
			_dropdownItemScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._currentDisplayIndex)
		{
			_currentDisplayIndex = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._dropdownItemScene)
		{
			value = VariantUtils.CreateFrom(in _dropdownItemScene);
			return true;
		}
		if (name == PropertyName._currentDisplayIndex)
		{
			value = VariantUtils.CreateFrom(in _currentDisplayIndex);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dropdownItemScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentDisplayIndex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._dropdownItemScene, Variant.From(in _dropdownItemScene));
		info.AddProperty(PropertyName._currentDisplayIndex, Variant.From(in _currentDisplayIndex));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._dropdownItemScene, out var value))
		{
			_dropdownItemScene = value.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._currentDisplayIndex, out var value2))
		{
			_currentDisplayIndex = value2.As<int>();
		}
	}
}
