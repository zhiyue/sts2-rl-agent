using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NDropdownItem.cs")]
public class NDropdownItem : NButton
{
	[Signal]
	public delegate void SelectedEventHandler(NDropdownItem cardHolder);

	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnRelease = "OnRelease";

		public static readonly StringName UnhoverSelection = "UnhoverSelection";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName Text = "Text";

		public static readonly StringName _highlight = "_highlight";

		public static readonly StringName _label = "_label";

		public static readonly StringName _richLabel = "_richLabel";
	}

	public new class SignalName : NButton.SignalName
	{
		public static readonly StringName Selected = "Selected";
	}

	private ColorRect _highlight;

	protected MegaLabel _label;

	protected MegaRichTextLabel? _richLabel;

	private SelectedEventHandler backing_Selected;

	public string Text
	{
		get
		{
			return _label.Text;
		}
		set
		{
			_label.SetTextAutoSize(value);
		}
	}

	public event SelectedEventHandler Selected
	{
		add
		{
			backing_Selected = (SelectedEventHandler)Delegate.Combine(backing_Selected, value);
		}
		remove
		{
			backing_Selected = (SelectedEventHandler)Delegate.Remove(backing_Selected, value);
		}
	}

	public override void _Ready()
	{
		ConnectSignals();
		_highlight = GetNode<ColorRect>("Highlight");
		_label = GetNodeOrNull<MegaLabel>("Label");
		_richLabel = GetNodeOrNull<MegaRichTextLabel>("RichLabel");
	}

	protected override void OnFocus()
	{
		_highlight.Visible = true;
	}

	protected override void OnUnfocus()
	{
		_highlight.Visible = false;
	}

	protected override void OnPress()
	{
		_highlight.Visible = false;
	}

	protected sealed override void OnRelease()
	{
		_highlight.Visible = true;
		EmitSignal(SignalName.Selected, this);
	}

	public void UnhoverSelection()
	{
		OnUnfocus();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UnhoverSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UnhoverSelection && args.Count == 0)
		{
			UnhoverSelection();
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
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.UnhoverSelection)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Text)
		{
			Text = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._highlight)
		{
			_highlight = VariantUtils.ConvertTo<ColorRect>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._richLabel)
		{
			_richLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Text)
		{
			value = VariantUtils.CreateFrom<string>(Text);
			return true;
		}
		if (name == PropertyName._highlight)
		{
			value = VariantUtils.CreateFrom(in _highlight);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._richLabel)
		{
			value = VariantUtils.CreateFrom(in _richLabel);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._highlight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._richLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.Text, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Text, Variant.From<string>(Text));
		info.AddProperty(PropertyName._highlight, Variant.From(in _highlight));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._richLabel, Variant.From(in _richLabel));
		info.AddSignalEventDelegate(SignalName.Selected, backing_Selected);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Text, out var value))
		{
			Text = value.As<string>();
		}
		if (info.TryGetProperty(PropertyName._highlight, out var value2))
		{
			_highlight = value2.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value3))
		{
			_label = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._richLabel, out var value4))
		{
			_richLabel = value4.As<MegaRichTextLabel>();
		}
		if (info.TryGetSignalEventDelegate<SelectedEventHandler>(SignalName.Selected, out var value5))
		{
			backing_Selected = value5;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.Selected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardHolder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalSelected(NDropdownItem cardHolder)
	{
		EmitSignal(SignalName.Selected, cardHolder);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Selected && args.Count == 1)
		{
			backing_Selected?.Invoke(VariantUtils.ConvertTo<NDropdownItem>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.Selected)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
