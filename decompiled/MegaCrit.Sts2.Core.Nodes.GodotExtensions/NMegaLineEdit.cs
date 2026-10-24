using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.GodotExtensions;

/// <summary>
/// A base LineEdit class that is used to access the virtual keyboard
/// for various devices/apis (ie Steam Deck)
/// </summary>
[ScriptPath("res://src/Core/Nodes/GodotExtensions/NMegaLineEdit.cs")]
public class NMegaLineEdit : LineEdit
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : LineEdit.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_GuiInput' method.
		/// </summary>
		public new static readonly StringName _GuiInput = "_GuiInput";

		/// <summary>
		/// Cached name for the 'OpenKeyboard' method.
		/// </summary>
		public static readonly StringName OpenKeyboard = "OpenKeyboard";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : LineEdit.PropertyName
	{
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : LineEdit.SignalName
	{
	}

	public override void _Ready()
	{
		this.ApplyLocaleFontSubstitution(FontType.Regular, ThemeConstants.LineEdit.Font);
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
			Unedit();
			GetViewport()?.SetInputAsHandled();
			PlatformUtil.CloseVirtualKeyboard();
		}
	}

	private void OpenKeyboard()
	{
		Edit();
		PlatformUtil.OpenVirtualKeyboard();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenKeyboard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
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
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
