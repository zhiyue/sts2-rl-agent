using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

[ScriptPath("res://src/Core/Nodes/Screens/Map/NMouseModeMapDrawingInput.cs")]
public class NMouseModeMapDrawingInput : NMapDrawingInput
{
	public new class MethodName : NMapDrawingInput.MethodName
	{
		public new static readonly StringName _Input = "_Input";

		public static readonly StringName ProcessMouseDrawingEvent = "ProcessMouseDrawingEvent";
	}

	public new class PropertyName : NMapDrawingInput.PropertyName
	{
	}

	public new class SignalName : NMapDrawingInput.SignalName
	{
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
		if (!(inputEvent is InputEventMouseButton inputEventMouseButton))
		{
			return;
		}
		if (inputEventMouseButton.ButtonIndex == MouseButton.Left)
		{
			if (inputEventMouseButton.Pressed && !_drawings.IsLocalDrawing())
			{
				_drawings.BeginLineLocal(_drawings.GetGlobalTransform().Inverse() * inputEventMouseButton.GlobalPosition, null);
			}
			else if (!inputEventMouseButton.Pressed && _drawings.IsLocalDrawing())
			{
				_drawings.StopLineLocal();
			}
		}
		else
		{
			MouseButton buttonIndex = inputEventMouseButton.ButtonIndex;
			bool flag = (((ulong)(buttonIndex - 2) <= 1uL) ? true : false);
			if (flag && inputEventMouseButton.Pressed)
			{
				StopDrawing();
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
