using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NAspectRatioDropdownItem.cs")]
public class NAspectRatioDropdownItem : NDropdownItem
{
	public new class MethodName : NDropdownItem.MethodName
	{
		public static readonly StringName Init = "Init";
	}

	public new class PropertyName : NDropdownItem.PropertyName
	{
		public static readonly StringName aspectRatioSetting = "aspectRatioSetting";
	}

	public new class SignalName : NDropdownItem.SignalName
	{
	}

	public AspectRatioSetting aspectRatioSetting;

	public void Init(AspectRatioSetting setAspectRatioSetting)
	{
		aspectRatioSetting = setAspectRatioSetting;
		switch (aspectRatioSetting)
		{
		case AspectRatioSetting.Auto:
			_label.SetTextAutoSize(new LocString("settings_ui", "ASPECT_RATIO_AUTO").GetFormattedText());
			break;
		case AspectRatioSetting.FourByThree:
			_label.SetTextAutoSize(new LocString("settings_ui", "ASPECT_RATIO_FOUR_BY_THREE").GetFormattedText());
			break;
		case AspectRatioSetting.SixteenByTen:
			_label.SetTextAutoSize(new LocString("settings_ui", "ASPECT_RATIO_SIXTEEN_BY_TEN").GetFormattedText());
			break;
		case AspectRatioSetting.SixteenByNine:
			_label.SetTextAutoSize(new LocString("settings_ui", "ASPECT_RATIO_SIXTEEN_BY_NINE").GetFormattedText());
			break;
		case AspectRatioSetting.TwentyOneByNine:
			_label.SetTextAutoSize(new LocString("settings_ui", "ASPECT_RATIO_TWENTY_ONE_BY_NINE").GetFormattedText());
			break;
		default:
			throw new ArgumentOutOfRangeException($"Invalid Aspect Ratio: {aspectRatioSetting}");
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName.Init, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "setAspectRatioSetting", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Init && args.Count == 1)
		{
			Init(VariantUtils.ConvertTo<AspectRatioSetting>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Init)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.aspectRatioSetting)
		{
			aspectRatioSetting = VariantUtils.ConvertTo<AspectRatioSetting>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.aspectRatioSetting)
		{
			value = VariantUtils.CreateFrom(in aspectRatioSetting);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.aspectRatioSetting, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.aspectRatioSetting, Variant.From(in aspectRatioSetting));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.aspectRatioSetting, out var value))
		{
			aspectRatioSetting = value.As<AspectRatioSetting>();
		}
	}
}
