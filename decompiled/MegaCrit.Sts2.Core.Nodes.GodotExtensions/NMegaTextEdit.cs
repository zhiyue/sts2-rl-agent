using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.GodotExtensions;

[ScriptPath("res://src/Core/Nodes/GodotExtensions/NMegaTextEdit.cs")]
public class NMegaTextEdit : TextEdit
{
	public new class MethodName : TextEdit.MethodName
	{
		public static readonly StringName IsEditing = "IsEditing";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshFont = "RefreshFont";

		public static readonly StringName OnFocus = "OnFocus";

		public static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName _GuiInput = "_GuiInput";

		public static readonly StringName OpenKeyboard = "OpenKeyboard";

		public static readonly StringName StopEditing = "StopEditing";
	}

	public new class PropertyName : TextEdit.PropertyName
	{
		public static readonly StringName _selectionReticle = "_selectionReticle";

		public static readonly StringName _isEditing = "_isEditing";
	}

	public new class SignalName : TextEdit.SignalName
	{
	}

	private NSelectionReticle? _selectionReticle;

	private bool _isEditing;

	public bool IsEditing()
	{
		return _isEditing;
	}

	public override void _Ready()
	{
		RefreshFont();
		Connect(Control.SignalName.FocusExited, Callable.From(StopEditing));
		if (HasNode("SelectionReticle"))
		{
			_selectionReticle = GetNode<NSelectionReticle>("SelectionReticle");
		}
		Connect(Control.SignalName.FocusEntered, Callable.From(OnFocus));
		Connect(Control.SignalName.FocusExited, Callable.From(OnUnfocus));
		Connect(Control.SignalName.MouseEntered, Callable.From(OnFocus));
		Connect(Control.SignalName.MouseExited, Callable.From(OnUnfocus));
	}

	public void RefreshFont()
	{
		this.ApplyLocaleFontSubstitution(FontType.Regular, ThemeConstants.TextEdit.font);
	}

	private void OnFocus()
	{
		if (NControllerManager.Instance.IsUsingController)
		{
			_selectionReticle?.OnSelect();
		}
	}

	private void OnUnfocus()
	{
		_selectionReticle?.OnDeselect();
	}

	public override void _GuiInput(InputEvent inputEvent)
	{
		base._GuiInput(inputEvent);
		if (inputEvent is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left && inputEventMouseButton.IsPressed())
		{
			OpenKeyboard();
		}
		if (inputEvent.IsActionPressed(MegaInput.select))
		{
			OpenKeyboard();
		}
		if (inputEvent.IsActionPressed(MegaInput.cancel) && IsEditing())
		{
			StopEditing();
			GetViewport()?.SetInputAsHandled();
		}
	}

	private void OpenKeyboard()
	{
		this.TryGrabFocus();
		_isEditing = true;
		PlatformUtil.OpenVirtualKeyboard();
	}

	private void StopEditing()
	{
		_isEditing = false;
		PlatformUtil.CloseVirtualKeyboard();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName.IsEditing, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshFont, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenKeyboard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StopEditing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.IsEditing && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsEditing());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshFont && args.Count == 0)
		{
			RefreshFont();
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
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenKeyboard && args.Count == 0)
		{
			OpenKeyboard();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopEditing && args.Count == 0)
		{
			StopEditing();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.IsEditing)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.RefreshFont)
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
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName.OpenKeyboard)
		{
			return true;
		}
		if (method == MethodName.StopEditing)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._selectionReticle)
		{
			_selectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._isEditing)
		{
			_isEditing = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._selectionReticle)
		{
			value = VariantUtils.CreateFrom(in _selectionReticle);
			return true;
		}
		if (name == PropertyName._isEditing)
		{
			value = VariantUtils.CreateFrom(in _isEditing);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isEditing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
		info.AddProperty(PropertyName._isEditing, Variant.From(in _isEditing));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value))
		{
			_selectionReticle = value.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._isEditing, out var value2))
		{
			_isEditing = value2.As<bool>();
		}
	}
}
