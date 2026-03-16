using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

[ScriptPath("res://src/Core/Nodes/Screens/Map/NMapDrawingInput.cs")]
public abstract class NMapDrawingInput : Control
{
	[Signal]
	public delegate void FinishedEventHandler();

	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName StopDrawing = "StopDrawing";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName DrawingMode = "DrawingMode";

		public static readonly StringName _drawings = "_drawings";
	}

	public new class SignalName : Control.SignalName
	{
		public static readonly StringName Finished = "Finished";
	}

	protected NMapDrawings _drawings;

	private FinishedEventHandler backing_Finished;

	public DrawingMode DrawingMode { get; private set; }

	public event FinishedEventHandler Finished
	{
		add
		{
			backing_Finished = (FinishedEventHandler)Delegate.Combine(backing_Finished, value);
		}
		remove
		{
			backing_Finished = (FinishedEventHandler)Delegate.Remove(backing_Finished, value);
		}
	}

	public static NMapDrawingInput Create(NMapDrawings drawings, DrawingMode drawingMode, bool stopOnMouseRelease = false)
	{
		NMapDrawingInput nMapDrawingInput = (stopOnMouseRelease ? new NMouseHeldMapDrawingInput() : ((!NControllerManager.Instance.IsUsingController) ? new NMouseModeMapDrawingInput() : NControllerMapDrawingInput.Create()));
		nMapDrawingInput._drawings = drawings;
		nMapDrawingInput.DrawingMode = drawingMode;
		nMapDrawingInput._drawings.SetDrawingModeLocal(drawingMode);
		return nMapDrawingInput;
	}

	public override void _Ready()
	{
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(StopDrawing));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(StopDrawing));
	}

	public override void _EnterTree()
	{
		ActiveScreenContext.Instance.Updated += StopDrawing;
	}

	public override void _ExitTree()
	{
		ActiveScreenContext.Instance.Updated -= StopDrawing;
	}

	public void StopDrawing()
	{
		if (_drawings.IsLocalDrawing())
		{
			_drawings.StopLineLocal();
		}
		_drawings.SetDrawingModeLocal(DrawingMode.None);
		EmitSignalFinished();
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "drawings", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Int, "drawingMode", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "stopOnMouseRelease", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StopDrawing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NMapDrawingInput>(Create(VariantUtils.ConvertTo<NMapDrawings>(in args[0]), VariantUtils.ConvertTo<DrawingMode>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopDrawing && args.Count == 0)
		{
			StopDrawing();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NMapDrawingInput>(Create(VariantUtils.ConvertTo<NMapDrawings>(in args[0]), VariantUtils.ConvertTo<DrawingMode>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.StopDrawing)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.DrawingMode)
		{
			DrawingMode = VariantUtils.ConvertTo<DrawingMode>(in value);
			return true;
		}
		if (name == PropertyName._drawings)
		{
			_drawings = VariantUtils.ConvertTo<NMapDrawings>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DrawingMode)
		{
			value = VariantUtils.CreateFrom<DrawingMode>(DrawingMode);
			return true;
		}
		if (name == PropertyName._drawings)
		{
			value = VariantUtils.CreateFrom(in _drawings);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._drawings, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.DrawingMode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.DrawingMode, Variant.From<DrawingMode>(DrawingMode));
		info.AddProperty(PropertyName._drawings, Variant.From(in _drawings));
		info.AddSignalEventDelegate(SignalName.Finished, backing_Finished);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.DrawingMode, out var value))
		{
			DrawingMode = value.As<DrawingMode>();
		}
		if (info.TryGetProperty(PropertyName._drawings, out var value2))
		{
			_drawings = value2.As<NMapDrawings>();
		}
		if (info.TryGetSignalEventDelegate<FinishedEventHandler>(SignalName.Finished, out var value3))
		{
			backing_Finished = value3;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.Finished, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalFinished()
	{
		EmitSignal(SignalName.Finished);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Finished && args.Count == 0)
		{
			backing_Finished?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.Finished)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
