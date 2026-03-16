using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;

[ScriptPath("res://src/Core/Nodes/Screens/CustomRun/NRunModifierTickbox.cs")]
public class NRunModifierTickbox : NTickbox
{
	public new class MethodName : NTickbox.MethodName
	{
		public new static readonly StringName OnEnable = "OnEnable";

		public new static readonly StringName OnDisable = "OnDisable";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	public new class PropertyName : NTickbox.PropertyName
	{
		public static readonly StringName _label = "_label";

		public static readonly StringName _highlight = "_highlight";
	}

	public new class SignalName : NTickbox.SignalName
	{
	}

	public const string scenePath = "res://scenes/screens/custom_run/modifier_tickbox.tscn";

	private static readonly LocString _descriptionLoc = new LocString("main_menu_ui", "CUSTOM_RUN_SCREEN.MODIFIER_LABEL");

	private MegaRichTextLabel _label;

	private Control _highlight;

	public ModifierModel? Modifier { get; private set; }

	public static NRunModifierTickbox? Create(ModifierModel model)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NRunModifierTickbox nRunModifierTickbox = PreloadManager.Cache.GetScene("res://scenes/screens/custom_run/modifier_tickbox.tscn").Instantiate<NRunModifierTickbox>(PackedScene.GenEditState.Disabled);
		nRunModifierTickbox.Modifier = model;
		return nRunModifierTickbox;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.Modulate = Colors.White;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.Modulate = Colors.Gray;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_label = GetNode<MegaRichTextLabel>("%Description");
		_highlight = GetNode<Control>("Highlight");
		if (Modifier != null)
		{
			string variable = ((ModelDb.GoodModifiers.FirstOrDefault((ModifierModel m) => m.GetType() == Modifier.GetType()) != null) ? "green" : ((ModelDb.BadModifiers.FirstOrDefault((ModifierModel m) => m.GetType() == Modifier.GetType()) != null) ? "red" : "blue"));
			_descriptionLoc.Add("color", variable);
			_descriptionLoc.Add("modifier_title", Modifier.Title.GetFormattedText());
			_descriptionLoc.Add("modifier_description", Modifier.Description.GetFormattedText());
			_label.Text = _descriptionLoc.GetFormattedText();
		}
		base.IsTicked = false;
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		if (NControllerManager.Instance.IsUsingController)
		{
			_highlight.Visible = true;
		}
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_highlight.Visible = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.OnEnable && args.Count == 0)
		{
			OnEnable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDisable && args.Count == 0)
		{
			OnDisable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.OnEnable)
		{
			return true;
		}
		if (method == MethodName.OnDisable)
		{
			return true;
		}
		if (method == MethodName._Ready)
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._highlight)
		{
			_highlight = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._highlight)
		{
			value = VariantUtils.CreateFrom(in _highlight);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._highlight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._highlight, Variant.From(in _highlight));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._label, out var value))
		{
			_label = value.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._highlight, out var value2))
		{
			_highlight = value2.As<Control>();
		}
	}
}
