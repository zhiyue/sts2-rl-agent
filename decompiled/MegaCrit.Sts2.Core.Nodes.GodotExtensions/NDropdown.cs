using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.GodotExtensions;

[ScriptPath("res://src/Core/Nodes/GodotExtensions/NDropdown.cs")]
public class NDropdown : NClickableControl
{
	public new class MethodName : NClickableControl.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName ConnectSignals = "ConnectSignals";

		public static readonly StringName ClearDropdownItems = "ClearDropdownItems";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName OnDismisserClicked = "OnDismisserClicked";

		public new static readonly StringName OnRelease = "OnRelease";

		public static readonly StringName OpenDropdown = "OpenDropdown";

		public static readonly StringName CloseDropdown = "CloseDropdown";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName _dropdownContainer = "_dropdownContainer";

		public static readonly StringName _dropdownItems = "_dropdownItems";

		public static readonly StringName _dismisser = "_dismisser";

		public static readonly StringName _currentOptionLabel = "_currentOptionLabel";

		public static readonly StringName _currentOptionHighlight = "_currentOptionHighlight";

		public new static readonly StringName _isHovered = "_isHovered";

		public static readonly StringName _isOpen = "_isOpen";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	private Control _dropdownContainer;

	protected Control _dropdownItems;

	private NButton _dismisser;

	protected MegaLabel _currentOptionLabel;

	protected Control _currentOptionHighlight;

	private bool _isHovered;

	private bool _isOpen;

	public override void _Ready()
	{
		if (GetType() != typeof(NDropdown))
		{
			throw new InvalidOperationException("Don't call base._Ready(). Use ConnectSignals() instead");
		}
		ConnectSignals();
	}

	protected override void ConnectSignals()
	{
		base.ConnectSignals();
		_currentOptionHighlight = GetNode<Control>("%Highlight");
		_currentOptionLabel = GetNode<MegaLabel>("%Label");
		_dropdownContainer = GetNode<Control>("%DropdownContainer");
		_dropdownItems = _dropdownContainer.GetNode<Control>("VBoxContainer");
		_dismisser = GetNode<NButton>("%Dismisser");
		_dismisser.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnDismisserClicked));
	}

	protected void ClearDropdownItems()
	{
		foreach (Node child in _dropdownItems.GetChildren())
		{
			_dropdownItems.RemoveChildSafely(child);
			child.QueueFreeSafely();
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (IsVisibleInTree() && _isEnabled && !NDevConsole.Instance.Visible)
		{
			Control control = GetViewport().GuiGetFocusOwner();
			bool flag = ((control is TextEdit || control is LineEdit) ? true : false);
			if (!flag && inputEvent.IsActionPressed(MegaInput.cancel))
			{
				CloseDropdown();
			}
		}
	}

	private void OnDismisserClicked(NButton obj)
	{
		CloseDropdown();
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		if (_isOpen)
		{
			Log.Info("Closing dropdown because you clicked on the main dropdown button.");
			CloseDropdown();
		}
		else
		{
			OpenDropdown();
		}
	}

	private void OpenDropdown()
	{
		_dropdownContainer.Visible = true;
		_dismisser.Visible = true;
		_isOpen = true;
		GetParent().MoveChild(this, GetParent().GetChildCount());
		List<NDropdownItem> list = _dropdownItems.GetChildren().OfType<NDropdownItem>().ToList();
		for (int i = 0; i < list.Count; i++)
		{
			list[i].UnhoverSelection();
			list[i].FocusNeighborLeft = list[i].GetPath();
			list[i].FocusNeighborRight = list[i].GetPath();
			list[i].FocusNeighborTop = ((i > 0) ? list[i - 1].GetPath() : list[i].GetPath());
			list[i].FocusNeighborBottom = ((i < list.Count - 1) ? list[i + 1].GetPath() : list[i].GetPath());
			list[i].FocusMode = FocusModeEnum.All;
		}
		list.FirstOrDefault()?.TryGrabFocus();
	}

	protected void CloseDropdown()
	{
		_dismisser.Visible = false;
		_dropdownContainer.Visible = false;
		_isOpen = false;
		this.TryGrabFocus();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ClearDropdownItems, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnDismisserClicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "obj", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenDropdown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CloseDropdown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ConnectSignals && args.Count == 0)
		{
			ConnectSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearDropdownItems && args.Count == 0)
		{
			ClearDropdownItems();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDismisserClicked && args.Count == 1)
		{
			OnDismisserClicked(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenDropdown && args.Count == 0)
		{
			OpenDropdown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CloseDropdown && args.Count == 0)
		{
			CloseDropdown();
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
		if (method == MethodName.ConnectSignals)
		{
			return true;
		}
		if (method == MethodName.ClearDropdownItems)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.OnDismisserClicked)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.OpenDropdown)
		{
			return true;
		}
		if (method == MethodName.CloseDropdown)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._dropdownContainer)
		{
			_dropdownContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._dropdownItems)
		{
			_dropdownItems = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._dismisser)
		{
			_dismisser = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._currentOptionLabel)
		{
			_currentOptionLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._currentOptionHighlight)
		{
			_currentOptionHighlight = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._isHovered)
		{
			_isHovered = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isOpen)
		{
			_isOpen = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._dropdownContainer)
		{
			value = VariantUtils.CreateFrom(in _dropdownContainer);
			return true;
		}
		if (name == PropertyName._dropdownItems)
		{
			value = VariantUtils.CreateFrom(in _dropdownItems);
			return true;
		}
		if (name == PropertyName._dismisser)
		{
			value = VariantUtils.CreateFrom(in _dismisser);
			return true;
		}
		if (name == PropertyName._currentOptionLabel)
		{
			value = VariantUtils.CreateFrom(in _currentOptionLabel);
			return true;
		}
		if (name == PropertyName._currentOptionHighlight)
		{
			value = VariantUtils.CreateFrom(in _currentOptionHighlight);
			return true;
		}
		if (name == PropertyName._isHovered)
		{
			value = VariantUtils.CreateFrom(in _isHovered);
			return true;
		}
		if (name == PropertyName._isOpen)
		{
			value = VariantUtils.CreateFrom(in _isOpen);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dropdownContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dropdownItems, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dismisser, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentOptionLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentOptionHighlight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isHovered, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isOpen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._dropdownContainer, Variant.From(in _dropdownContainer));
		info.AddProperty(PropertyName._dropdownItems, Variant.From(in _dropdownItems));
		info.AddProperty(PropertyName._dismisser, Variant.From(in _dismisser));
		info.AddProperty(PropertyName._currentOptionLabel, Variant.From(in _currentOptionLabel));
		info.AddProperty(PropertyName._currentOptionHighlight, Variant.From(in _currentOptionHighlight));
		info.AddProperty(PropertyName._isHovered, Variant.From(in _isHovered));
		info.AddProperty(PropertyName._isOpen, Variant.From(in _isOpen));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._dropdownContainer, out var value))
		{
			_dropdownContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._dropdownItems, out var value2))
		{
			_dropdownItems = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._dismisser, out var value3))
		{
			_dismisser = value3.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._currentOptionLabel, out var value4))
		{
			_currentOptionLabel = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._currentOptionHighlight, out var value5))
		{
			_currentOptionHighlight = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._isHovered, out var value6))
		{
			_isHovered = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isOpen, out var value7))
		{
			_isOpen = value7.As<bool>();
		}
	}
}
