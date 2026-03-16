using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Localization;

[ScriptPath("res://src/Core/Localization/LocTextLabel.cs")]
public class LocTextLabel : RichTextLabel
{
	public new class MethodName : RichTextLabel.MethodName
	{
		public static readonly StringName UpdateLocalization = "UpdateLocalization";

		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : RichTextLabel.PropertyName
	{
		public static readonly StringName LocalizationTable = "LocalizationTable";

		public static readonly StringName LocalizationKey = "LocalizationKey";

		public static readonly StringName _localizationTable = "_localizationTable";

		public static readonly StringName _localizationKey = "_localizationKey";
	}

	public new class SignalName : RichTextLabel.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private string? _localizationTable;

	[Export(PropertyHint.None, "")]
	private string? _localizationKey;

	private LocString? _locString;

	public string? LocalizationTable
	{
		get
		{
			return _localizationTable;
		}
		set
		{
			if (!(_localizationTable == value))
			{
				_localizationTable = value;
				_locString = null;
				UpdateLocalization();
			}
		}
	}

	public string? LocalizationKey
	{
		get
		{
			return _localizationKey;
		}
		set
		{
			if (!(_localizationKey == value))
			{
				_localizationKey = value;
				_locString = null;
				UpdateLocalization();
			}
		}
	}

	private void UpdateLocalization()
	{
		if (_localizationTable == null)
		{
			throw new InvalidOperationException("_localizationTable is null.");
		}
		if (_localizationKey == null)
		{
			throw new InvalidOperationException("_localizationKey is null.");
		}
		if (_locString == null)
		{
			_locString = new LocString(_localizationTable, _localizationKey);
		}
		base.Text = _locString.GetFormattedText();
	}

	public override void _Ready()
	{
		UpdateLocalization();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName.UpdateLocalization, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.UpdateLocalization && args.Count == 0)
		{
			UpdateLocalization();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.UpdateLocalization)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.LocalizationTable)
		{
			LocalizationTable = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.LocalizationKey)
		{
			LocalizationKey = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._localizationTable)
		{
			_localizationTable = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._localizationKey)
		{
			_localizationKey = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		string from;
		if (name == PropertyName.LocalizationTable)
		{
			from = LocalizationTable;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.LocalizationKey)
		{
			from = LocalizationKey;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._localizationTable)
		{
			value = VariantUtils.CreateFrom(in _localizationTable);
			return true;
		}
		if (name == PropertyName._localizationKey)
		{
			value = VariantUtils.CreateFrom(in _localizationKey);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._localizationTable, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._localizationKey, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.LocalizationTable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.LocalizationKey, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.LocalizationTable, Variant.From<string>(LocalizationTable));
		info.AddProperty(PropertyName.LocalizationKey, Variant.From<string>(LocalizationKey));
		info.AddProperty(PropertyName._localizationTable, Variant.From(in _localizationTable));
		info.AddProperty(PropertyName._localizationKey, Variant.From(in _localizationKey));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.LocalizationTable, out var value))
		{
			LocalizationTable = value.As<string>();
		}
		if (info.TryGetProperty(PropertyName.LocalizationKey, out var value2))
		{
			LocalizationKey = value2.As<string>();
		}
		if (info.TryGetProperty(PropertyName._localizationTable, out var value3))
		{
			_localizationTable = value3.As<string>();
		}
		if (info.TryGetProperty(PropertyName._localizationKey, out var value4))
		{
			_localizationKey = value4.As<string>();
		}
	}
}
