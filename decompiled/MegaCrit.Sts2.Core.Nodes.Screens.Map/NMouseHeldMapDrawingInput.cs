using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

[ScriptPath("res://src/Core/Nodes/Screens/Map/NMouseHeldMapDrawingInput.cs")]
public class NMouseHeldMapDrawingInput : NMapDrawingInput
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NMapDrawingInput.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_Input' method.
		/// </summary>
		public new static readonly StringName _Input = "_Input";

		/// <summary>
		/// Cached name for the 'ProcessMouseDrawingEvent' method.
		/// </summary>
		public static readonly StringName ProcessMouseDrawingEvent = "ProcessMouseDrawingEvent";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NMapDrawingInput.PropertyName
	{
		/// <summary>
		/// Cached name for the 'ListeningButton' property.
		/// </summary>
		public static readonly StringName ListeningButton = "ListeningButton";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NMapDrawingInput.SignalName
	{
	}

	private MouseButton ListeningButton
	{
		get
		{
			if (base.DrawingMode != DrawingMode.Drawing)
			{
				return MouseButton.Middle;
			}
			return MouseButton.Right;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		_drawings.BeginLineLocal(_drawings.GetGlobalTransform().Inverse() * GetGlobalMousePosition(), null);
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (IsVisibleInTree())
		{
			ProcessMouseDrawingEvent(inputEvent);
			if (inputEvent is InputEventMouseMotion inputEventMouseMotion && _drawings.IsLocalDrawing())
			{
				_drawings.UpdateCurrentLinePositionLocal(_drawings.GetGlobalTransform().Inverse() * inputEventMouseMotion.GlobalPosition);
			}
		}
	}

	private void ProcessMouseDrawingEvent(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.ButtonIndex == ListeningButton && !inputEventMouseButton.IsPressed())
		{
			StopDrawing();
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessMouseDrawingEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
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
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessMouseDrawingEvent && args.Count == 1)
		{
			ProcessMouseDrawingEvent(VariantUtils.ConvertTo<InputEvent>(in args[0]));
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
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.ProcessMouseDrawingEvent)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ListeningButton)
		{
			value = VariantUtils.CreateFrom<MouseButton>(ListeningButton);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ListeningButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
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
