using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

[ScriptPath("res://src/Core/Nodes/Screens/Bestiary/NBestiaryEntry.cs")]
public class NBestiaryEntry : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Select = "Select";

		public static readonly StringName Deselect = "Deselect";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName IsLocked = "IsLocked";

		public static readonly StringName _nameLabel = "_nameLabel";

		public static readonly StringName _highlight = "_highlight";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private MegaRichTextLabel _nameLabel;

	private Control _highlight;

	private static string ScenePath => SceneHelper.GetScenePath("screens/bestiary/bestiary_entry");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public MonsterModel Monster { get; private set; }

	public bool IsLocked { get; private set; }

	public static NBestiaryEntry Create(MonsterModel monster, bool isLocked)
	{
		NBestiaryEntry nBestiaryEntry = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NBestiaryEntry>(PackedScene.GenEditState.Disabled);
		nBestiaryEntry.Monster = monster;
		nBestiaryEntry.IsLocked = isLocked;
		return nBestiaryEntry;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_nameLabel = GetNode<MegaRichTextLabel>("Label");
		_highlight = GetNode<Control>("Highlight");
		if (IsLocked)
		{
			_nameLabel.Text = "[Locked]";
			_nameLabel.Modulate = StsColors.gray;
		}
		else
		{
			_nameLabel.Text = Monster.Title.GetFormattedText();
			_nameLabel.Modulate = StsColors.cream;
		}
	}

	public void Select()
	{
		_nameLabel.Modulate = StsColors.gold;
	}

	public void Deselect()
	{
		_nameLabel.Modulate = (IsLocked ? StsColors.gray : StsColors.cream);
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
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Select, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Deselect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Select && args.Count == 0)
		{
			Select();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Deselect && args.Count == 0)
		{
			Deselect();
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
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Select)
		{
			return true;
		}
		if (method == MethodName.Deselect)
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
		if (name == PropertyName.IsLocked)
		{
			IsLocked = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._nameLabel)
		{
			_nameLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
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
		if (name == PropertyName.IsLocked)
		{
			value = VariantUtils.CreateFrom<bool>(IsLocked);
			return true;
		}
		if (name == PropertyName._nameLabel)
		{
			value = VariantUtils.CreateFrom(in _nameLabel);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._nameLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._highlight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsLocked, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsLocked, Variant.From<bool>(IsLocked));
		info.AddProperty(PropertyName._nameLabel, Variant.From(in _nameLabel));
		info.AddProperty(PropertyName._highlight, Variant.From(in _highlight));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsLocked, out var value))
		{
			IsLocked = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._nameLabel, out var value2))
		{
			_nameLabel = value2.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._highlight, out var value3))
		{
			_highlight = value3.As<Control>();
		}
	}
}
