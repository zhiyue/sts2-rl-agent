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

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NLanguageDropdown.cs")]
public class NLanguageDropdown : NSettingsDropdown
{
	public new class MethodName : NSettingsDropdown.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName PopulateOptions = "PopulateOptions";

		public static readonly StringName OnDropdownItemSelected = "OnDropdownItemSelected";

		public static readonly StringName GetLanguageNameForCode = "GetLanguageNameForCode";
	}

	public new class PropertyName : NSettingsDropdown.PropertyName
	{
		public static readonly StringName CurrentLanguage = "CurrentLanguage";

		public static readonly StringName _dropdownItemScene = "_dropdownItemScene";
	}

	public new class SignalName : NSettingsDropdown.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private PackedScene _dropdownItemScene;

	private static readonly Dictionary<string, string> _languageCodeToName = new Dictionary<string, string>
	{
		{ "ARA", "العربية" },
		{ "BEN", "ব\u09be\u0982ল\u09be" },
		{ "CZE", "Čeština" },
		{ "DEU", "Deutsch" },
		{ "DUT", "Nederlands" },
		{ "ENG", "English" },
		{ "ESP", "Español (Latinoamérica)" },
		{ "FIL", "Filipino" },
		{ "FIN", "Suomi" },
		{ "FRA", "Français" },
		{ "GRE", "Ελληνικά" },
		{ "HIN", "ह\u093fन\u094dद\u0940" },
		{ "IND", "Bahasa Indonesia" },
		{ "ITA", "Italiano" },
		{ "JPN", "日本語" },
		{ "KOR", "한국어" },
		{ "MAL", "Bahasa Melayu" },
		{ "NOR", "Norsk" },
		{ "POL", "Polski" },
		{ "POR", "Português" },
		{ "PTB", "Português Brasileiro" },
		{ "RUS", "Русский" },
		{ "SPA", "Español (Castellano)" },
		{ "SWE", "Svenska" },
		{ "THA", "ไทย" },
		{ "TUR", "Türkçe" },
		{ "UKR", "Українська" },
		{ "VIE", "Tiếng Việt" },
		{ "ZHS", "中文" },
		{ "ZHT", "繁體中文" }
	};

	private string CurrentLanguage => LocManager.Instance.Language;

	public override void _Ready()
	{
		ConnectSignals();
		PopulateOptions();
		_currentOptionLabel.SetTextAutoSize(GetLanguageNameForCode(CurrentLanguage));
	}

	private void PopulateOptions()
	{
		ClearDropdownItems();
		foreach (string language in LocManager.Languages)
		{
			NLanguageDropdownItem nLanguageDropdownItem = _dropdownItemScene.Instantiate<NLanguageDropdownItem>(PackedScene.GenEditState.Disabled);
			_dropdownItems.AddChildSafely(nLanguageDropdownItem);
			nLanguageDropdownItem.Connect(NDropdownItem.SignalName.Selected, Callable.From<NDropdownItem>(OnDropdownItemSelected));
			nLanguageDropdownItem.Init(language);
		}
		_dropdownItems.GetParent<NDropdownContainer>().RefreshLayout();
	}

	private void OnDropdownItemSelected(NDropdownItem nDropdownItem)
	{
		NLanguageDropdownItem nLanguageDropdownItem = (NLanguageDropdownItem)nDropdownItem;
		if (!(nLanguageDropdownItem.LanguageCode == CurrentLanguage))
		{
			CloseDropdown();
			_currentOptionLabel.SetTextAutoSize(GetLanguageNameForCode(nLanguageDropdownItem.LanguageCode));
			SaveManager.Instance.SettingsSave.Language = nLanguageDropdownItem.LanguageCode;
			LocManager.Instance.SetLanguage(nLanguageDropdownItem.LanguageCode);
			NGame.Instance.Relocalize();
			NGame.Instance.MainMenu.OpenSettingsMenu();
		}
	}

	public static string GetLanguageNameForCode(string languageCode)
	{
		if (!_languageCodeToName.TryGetValue(languageCode.ToUpperInvariant(), out string value))
		{
			throw new InvalidOperationException("Tried to get language name for code " + languageCode + " but it doesn't exist!");
		}
		return value;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PopulateOptions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDropdownItemSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "nDropdownItem", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetLanguageNameForCode, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "languageCode", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.PopulateOptions && args.Count == 0)
		{
			PopulateOptions();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDropdownItemSelected && args.Count == 1)
		{
			OnDropdownItemSelected(VariantUtils.ConvertTo<NDropdownItem>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetLanguageNameForCode && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetLanguageNameForCode(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.GetLanguageNameForCode && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetLanguageNameForCode(VariantUtils.ConvertTo<string>(in args[0])));
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
		if (method == MethodName.PopulateOptions)
		{
			return true;
		}
		if (method == MethodName.OnDropdownItemSelected)
		{
			return true;
		}
		if (method == MethodName.GetLanguageNameForCode)
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
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.CurrentLanguage)
		{
			value = VariantUtils.CreateFrom<string>(CurrentLanguage);
			return true;
		}
		if (name == PropertyName._dropdownItemScene)
		{
			value = VariantUtils.CreateFrom(in _dropdownItemScene);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dropdownItemScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.CurrentLanguage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._dropdownItemScene, Variant.From(in _dropdownItemScene));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._dropdownItemScene, out var value))
		{
			_dropdownItemScene = value.As<PackedScene>();
		}
	}
}
