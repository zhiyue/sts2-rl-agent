using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.FeedbackScreen;

[ScriptPath("res://src/Core/Nodes/Screens/FeedbackScreen/NFeedbackCategoryDropdown.cs")]
public class NFeedbackCategoryDropdown : NDropdown
{
	public new class MethodName : NDropdown.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName PopulateOptions = "PopulateOptions";

		public static readonly StringName OnDropdownItemSelected = "OnDropdownItemSelected";
	}

	public new class PropertyName : NDropdown.PropertyName
	{
		public static readonly StringName CurrentCategory = "CurrentCategory";

		public static readonly StringName _dropdownItemScene = "_dropdownItemScene";

		public static readonly StringName _selectionReticle = "_selectionReticle";

		public static readonly StringName _currentCategoryIndex = "_currentCategoryIndex";
	}

	public new class SignalName : NDropdown.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private PackedScene _dropdownItemScene;

	private NSelectionReticle _selectionReticle;

	private static readonly string[] _categories = new string[3] { "bug", "balance", "feedback" };

	private static readonly LocString[] _categoryLoc = new LocString[3]
	{
		new LocString("settings_ui", "FEEDBACK_CATEGORY.bug"),
		new LocString("settings_ui", "FEEDBACK_CATEGORY.balance"),
		new LocString("settings_ui", "FEEDBACK_CATEGORY.feedback")
	};

	private int _currentCategoryIndex = _categories.IndexOf("feedback");

	public string CurrentCategory => _categories[_currentCategoryIndex];

	public override void _Ready()
	{
		ConnectSignals();
		_currentOptionHighlight = GetNode<Panel>("%Highlight");
		_currentOptionLabel = GetNode<MegaLabel>("%Label");
		PopulateOptions();
		_currentOptionLabel.SetTextAutoSize(_categoryLoc[_currentCategoryIndex].GetFormattedText());
		_selectionReticle = GetNode<NSelectionReticle>("SelectionReticle");
	}

	protected override void OnFocus()
	{
		_currentOptionHighlight.Modulate = new Color("afcdde");
		if (NControllerManager.Instance.IsUsingController)
		{
			_selectionReticle.OnSelect();
		}
	}

	protected override void OnUnfocus()
	{
		_selectionReticle.OnDeselect();
		_currentOptionHighlight.Modulate = Colors.White;
	}

	private void PopulateOptions()
	{
		Control node = GetNode<Control>("DropdownContainer/VBoxContainer");
		foreach (Node child in node.GetChildren())
		{
			node.RemoveChildSafely(child);
			child.QueueFreeSafely();
		}
		for (int i = 0; i < _categories.Length; i++)
		{
			NFeedbackCategoryDropdownItem nFeedbackCategoryDropdownItem = _dropdownItemScene.Instantiate<NFeedbackCategoryDropdownItem>(PackedScene.GenEditState.Disabled);
			node.AddChildSafely(nFeedbackCategoryDropdownItem);
			nFeedbackCategoryDropdownItem.Connect(NDropdownItem.SignalName.Selected, Callable.From<NDropdownItem>(OnDropdownItemSelected));
			nFeedbackCategoryDropdownItem.Init(i, _categoryLoc[i].GetFormattedText());
		}
		node.GetParent<NDropdownContainer>().RefreshLayout();
	}

	private void OnDropdownItemSelected(NDropdownItem item)
	{
		NFeedbackCategoryDropdownItem nFeedbackCategoryDropdownItem = (NFeedbackCategoryDropdownItem)item;
		if (nFeedbackCategoryDropdownItem.CategoryIndex != _currentCategoryIndex)
		{
			CloseDropdown();
			_currentCategoryIndex = nFeedbackCategoryDropdownItem.CategoryIndex;
			_currentOptionLabel.SetTextAutoSize(_categoryLoc[_currentCategoryIndex].GetFormattedText());
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PopulateOptions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDropdownItemSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "item", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
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
		if (method == MethodName.PopulateOptions)
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
		if (name == PropertyName._selectionReticle)
		{
			_selectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._currentCategoryIndex)
		{
			_currentCategoryIndex = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.CurrentCategory)
		{
			value = VariantUtils.CreateFrom<string>(CurrentCategory);
			return true;
		}
		if (name == PropertyName._dropdownItemScene)
		{
			value = VariantUtils.CreateFrom(in _dropdownItemScene);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			value = VariantUtils.CreateFrom(in _selectionReticle);
			return true;
		}
		if (name == PropertyName._currentCategoryIndex)
		{
			value = VariantUtils.CreateFrom(in _currentCategoryIndex);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dropdownItemScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentCategoryIndex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.CurrentCategory, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._dropdownItemScene, Variant.From(in _dropdownItemScene));
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
		info.AddProperty(PropertyName._currentCategoryIndex, Variant.From(in _currentCategoryIndex));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._dropdownItemScene, out var value))
		{
			_dropdownItemScene = value.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value2))
		{
			_selectionReticle = value2.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._currentCategoryIndex, out var value3))
		{
			_currentCategoryIndex = value3.As<int>();
		}
	}
}
