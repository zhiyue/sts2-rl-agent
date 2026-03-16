using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NAspectRatioDropdown.cs")]
public class NAspectRatioDropdown : NSettingsDropdown, IResettableSettingNode
{
	public new class MethodName : NSettingsDropdown.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetFromSettings = "SetFromSettings";

		public static readonly StringName AddDropdownItem = "AddDropdownItem";

		public static readonly StringName OnDropdownItemSelected = "OnDropdownItemSelected";

		public static readonly StringName GetAspectRatioSettingString = "GetAspectRatioSettingString";
	}

	public new class PropertyName : NSettingsDropdown.PropertyName
	{
		public static readonly StringName _currentAspectRatioSetting = "_currentAspectRatioSetting";
	}

	public new class SignalName : NSettingsDropdown.SignalName
	{
	}

	private AspectRatioSetting _currentAspectRatioSetting;

	private static string AspectRatioDropdownItemScenePath => SceneHelper.GetScenePath("ui/aspect_ratio_dropdown_item");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(AspectRatioDropdownItemScenePath);

	public override void _Ready()
	{
		ConnectSignals();
		ClearDropdownItems();
		AddDropdownItem(AspectRatioSetting.Auto);
		AddDropdownItem(AspectRatioSetting.FourByThree);
		AddDropdownItem(AspectRatioSetting.SixteenByTen);
		AddDropdownItem(AspectRatioSetting.SixteenByNine);
		AddDropdownItem(AspectRatioSetting.TwentyOneByNine);
		_dropdownItems.GetParent<NDropdownContainer>().RefreshLayout();
		SetFromSettings();
	}

	public void SetFromSettings()
	{
		_currentAspectRatioSetting = SaveManager.Instance.SettingsSave.AspectRatioSetting;
		_currentOptionLabel.SetTextAutoSize(GetAspectRatioSettingString(_currentAspectRatioSetting));
	}

	private void AddDropdownItem(AspectRatioSetting aspectRatioSetting)
	{
		NAspectRatioDropdownItem nAspectRatioDropdownItem = ResourceLoader.Load<PackedScene>(AspectRatioDropdownItemScenePath, null, ResourceLoader.CacheMode.Reuse).Instantiate<NAspectRatioDropdownItem>(PackedScene.GenEditState.Disabled);
		_dropdownItems.AddChildSafely(nAspectRatioDropdownItem);
		nAspectRatioDropdownItem.Connect(NDropdownItem.SignalName.Selected, Callable.From<NDropdownItem>(OnDropdownItemSelected));
		nAspectRatioDropdownItem.Init(aspectRatioSetting);
	}

	private void OnDropdownItemSelected(NDropdownItem nDropdownItem)
	{
		NAspectRatioDropdownItem nAspectRatioDropdownItem = (NAspectRatioDropdownItem)nDropdownItem;
		if (nAspectRatioDropdownItem.aspectRatioSetting != _currentAspectRatioSetting)
		{
			CloseDropdown();
			SaveManager.Instance.SettingsSave.AspectRatioSetting = nAspectRatioDropdownItem.aspectRatioSetting;
			SetFromSettings();
			NGame.Instance.ApplyDisplaySettings();
		}
	}

	private static string GetAspectRatioSettingString(AspectRatioSetting aspectRatioSettingString)
	{
		return aspectRatioSettingString switch
		{
			AspectRatioSetting.Auto => new LocString("settings_ui", "ASPECT_RATIO_AUTO").GetFormattedText(), 
			AspectRatioSetting.FourByThree => new LocString("settings_ui", "ASPECT_RATIO_FOUR_BY_THREE").GetFormattedText(), 
			AspectRatioSetting.SixteenByTen => new LocString("settings_ui", "ASPECT_RATIO_SIXTEEN_BY_TEN").GetFormattedText(), 
			AspectRatioSetting.SixteenByNine => new LocString("settings_ui", "ASPECT_RATIO_SIXTEEN_BY_NINE").GetFormattedText(), 
			AspectRatioSetting.TwentyOneByNine => new LocString("settings_ui", "ASPECT_RATIO_TWENTY_ONE_BY_NINE").GetFormattedText(), 
			_ => throw new ArgumentOutOfRangeException($"Invalid Aspect Ratio: {aspectRatioSettingString}"), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetFromSettings, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AddDropdownItem, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "aspectRatioSetting", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnDropdownItemSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "nDropdownItem", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetAspectRatioSettingString, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "aspectRatioSettingString", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.SetFromSettings && args.Count == 0)
		{
			SetFromSettings();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddDropdownItem && args.Count == 1)
		{
			AddDropdownItem(VariantUtils.ConvertTo<AspectRatioSetting>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDropdownItemSelected && args.Count == 1)
		{
			OnDropdownItemSelected(VariantUtils.ConvertTo<NDropdownItem>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetAspectRatioSettingString && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetAspectRatioSettingString(VariantUtils.ConvertTo<AspectRatioSetting>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.GetAspectRatioSettingString && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetAspectRatioSettingString(VariantUtils.ConvertTo<AspectRatioSetting>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.SetFromSettings)
		{
			return true;
		}
		if (method == MethodName.AddDropdownItem)
		{
			return true;
		}
		if (method == MethodName.OnDropdownItemSelected)
		{
			return true;
		}
		if (method == MethodName.GetAspectRatioSettingString)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._currentAspectRatioSetting)
		{
			_currentAspectRatioSetting = VariantUtils.ConvertTo<AspectRatioSetting>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._currentAspectRatioSetting)
		{
			value = VariantUtils.CreateFrom(in _currentAspectRatioSetting);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentAspectRatioSetting, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._currentAspectRatioSetting, Variant.From(in _currentAspectRatioSetting));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._currentAspectRatioSetting, out var value))
		{
			_currentAspectRatioSetting = value.As<AspectRatioSetting>();
		}
	}
}
