using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Ftue;

[ScriptPath("res://src/Core/Nodes/Ftue/NRestSiteFtue.cs")]
public class NRestSiteFtue : NFtue
{
	public new class MethodName : NFtue.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Create = "Create";

		public new static readonly StringName CloseFtue = "CloseFtue";
	}

	public new class PropertyName : NFtue.PropertyName
	{
		public static readonly StringName _confirmButton = "_confirmButton";

		public static readonly StringName _header = "_header";

		public static readonly StringName _description = "_description";

		public static readonly StringName _arrow = "_arrow";

		public static readonly StringName _defaultZIndex = "_defaultZIndex";

		public static readonly StringName _restSiteOptionContainer = "_restSiteOptionContainer";
	}

	public new class SignalName : NFtue.SignalName
	{
	}

	public const string id = "rest_site_ftue";

	private static readonly string _scenePath = SceneHelper.GetScenePath("ftue/rest_site_ftue");

	private NButton _confirmButton;

	private MegaLabel _header;

	private MegaRichTextLabel _description;

	private Control _arrow;

	private int _defaultZIndex;

	private Control _restSiteOptionContainer;

	public override void _Ready()
	{
		_header = GetNode<MegaLabel>("FtuePopup/Header");
		_header.SetTextAutoSize(new LocString("ftues", "REST_SITE_FTUE_TITLE").GetFormattedText());
		_description = GetNode<MegaRichTextLabel>("FtuePopup/DescriptionContainer/Description");
		_description.Text = new LocString("ftues", "REST_SITE_FTUE_DESCRIPTION").GetFormattedText();
		_confirmButton = GetNode<NButton>("FtuePopup/FtueConfirmButton");
		_confirmButton.Connect(NClickableControl.SignalName.Released, Callable.From((Action<NButton>)CloseFtue));
		_defaultZIndex = _restSiteOptionContainer.ZIndex;
		_restSiteOptionContainer.ZIndex++;
	}

	public static NRestSiteFtue? Create(Control restSiteOptionContainer)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NRestSiteFtue nRestSiteFtue = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NRestSiteFtue>(PackedScene.GenEditState.Disabled);
		nRestSiteFtue._restSiteOptionContainer = restSiteOptionContainer;
		return nRestSiteFtue;
	}

	private void CloseFtue(NButton _)
	{
		_restSiteOptionContainer.ZIndex = _defaultZIndex;
		CloseFtue();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "restSiteOptionContainer", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CloseFtue, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
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
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NRestSiteFtue>(Create(VariantUtils.ConvertTo<Control>(in args[0])));
			return true;
		}
		if (method == MethodName.CloseFtue && args.Count == 1)
		{
			CloseFtue(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NRestSiteFtue>(Create(VariantUtils.ConvertTo<Control>(in args[0])));
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
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.CloseFtue)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._confirmButton)
		{
			_confirmButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._header)
		{
			_header = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._description)
		{
			_description = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._arrow)
		{
			_arrow = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._defaultZIndex)
		{
			_defaultZIndex = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._restSiteOptionContainer)
		{
			_restSiteOptionContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._confirmButton)
		{
			value = VariantUtils.CreateFrom(in _confirmButton);
			return true;
		}
		if (name == PropertyName._header)
		{
			value = VariantUtils.CreateFrom(in _header);
			return true;
		}
		if (name == PropertyName._description)
		{
			value = VariantUtils.CreateFrom(in _description);
			return true;
		}
		if (name == PropertyName._arrow)
		{
			value = VariantUtils.CreateFrom(in _arrow);
			return true;
		}
		if (name == PropertyName._defaultZIndex)
		{
			value = VariantUtils.CreateFrom(in _defaultZIndex);
			return true;
		}
		if (name == PropertyName._restSiteOptionContainer)
		{
			value = VariantUtils.CreateFrom(in _restSiteOptionContainer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._confirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._header, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._description, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._arrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._defaultZIndex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._restSiteOptionContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._confirmButton, Variant.From(in _confirmButton));
		info.AddProperty(PropertyName._header, Variant.From(in _header));
		info.AddProperty(PropertyName._description, Variant.From(in _description));
		info.AddProperty(PropertyName._arrow, Variant.From(in _arrow));
		info.AddProperty(PropertyName._defaultZIndex, Variant.From(in _defaultZIndex));
		info.AddProperty(PropertyName._restSiteOptionContainer, Variant.From(in _restSiteOptionContainer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._confirmButton, out var value))
		{
			_confirmButton = value.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._header, out var value2))
		{
			_header = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._description, out var value3))
		{
			_description = value3.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._arrow, out var value4))
		{
			_arrow = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._defaultZIndex, out var value5))
		{
			_defaultZIndex = value5.As<int>();
		}
		if (info.TryGetProperty(PropertyName._restSiteOptionContainer, out var value6))
		{
			_restSiteOptionContainer = value6.As<Control>();
		}
	}
}
