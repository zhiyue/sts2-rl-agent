using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Debug;

[ScriptPath("res://src/Core/Nodes/Debug/NFpsVisualizer.cs")]
public class NFpsVisualizer : TextureRect
{
	[Signal]
	public delegate void MouseReleasedEventHandler(InputEvent inputEvent);

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : TextureRect.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'HandleMouseRelease' method.
		/// </summary>
		public static readonly StringName HandleMouseRelease = "HandleMouseRelease";

		/// <summary>
		/// Cached name for the '_GuiInput' method.
		/// </summary>
		public new static readonly StringName _GuiInput = "_GuiInput";

		/// <summary>
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : TextureRect.PropertyName
	{
		/// <summary>
		/// Cached name for the '_label' field.
		/// </summary>
		public static readonly StringName _label = "_label";

		/// <summary>
		/// Cached name for the '_happy' field.
		/// </summary>
		public static readonly StringName _happy = "_happy";

		/// <summary>
		/// Cached name for the '_content' field.
		/// </summary>
		public static readonly StringName _content = "_content";

		/// <summary>
		/// Cached name for the '_neutral' field.
		/// </summary>
		public static readonly StringName _neutral = "_neutral";

		/// <summary>
		/// Cached name for the '_sad' field.
		/// </summary>
		public static readonly StringName _sad = "_sad";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : TextureRect.SignalName
	{
		/// <summary>
		/// Cached name for the 'MouseReleased' signal.
		/// </summary>
		public static readonly StringName MouseReleased = "MouseReleased";
	}

	private Label? _label;

	[Export(PropertyHint.None, "")]
	private Texture2D _happy;

	[Export(PropertyHint.None, "")]
	private Texture2D _content;

	[Export(PropertyHint.None, "")]
	private Texture2D _neutral;

	[Export(PropertyHint.None, "")]
	private Texture2D _sad;

	private MouseReleasedEventHandler backing_MouseReleased;

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Debug.NFpsVisualizer.MouseReleasedEventHandler" />
	public event MouseReleasedEventHandler MouseReleased
	{
		add
		{
			backing_MouseReleased = (MouseReleasedEventHandler)Delegate.Combine(backing_MouseReleased, value);
		}
		remove
		{
			backing_MouseReleased = (MouseReleasedEventHandler)Delegate.Remove(backing_MouseReleased, value);
		}
	}

	public override void _Ready()
	{
		if (!OS.HasFeature("editor"))
		{
			this.QueueFreeSafely();
			return;
		}
		_label = GetNode<Label>("Label");
		Connect(SignalName.MouseReleased, Callable.From<InputEvent>(HandleMouseRelease));
	}

	private void HandleMouseRelease(InputEvent inputEvent)
	{
		this.QueueFreeSafely();
	}

	public override void _GuiInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left && inputEventMouseButton.IsReleased())
		{
			EmitSignal(SignalName.MouseReleased, inputEvent);
		}
	}

	public override void _Process(double delta)
	{
		if (_label != null)
		{
			double framesPerSecond = Engine.GetFramesPerSecond();
			Texture2D texture = ((framesPerSecond >= 58.0) ? _happy : ((framesPerSecond >= 50.0) ? _content : ((!(framesPerSecond >= 30.0)) ? _sad : _neutral)));
			base.Texture = texture;
			_label.Text = framesPerSecond.ToString(CultureInfo.InvariantCulture);
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HandleMouseRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.HandleMouseRelease && args.Count == 1)
		{
			HandleMouseRelease(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
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
		if (method == MethodName.HandleMouseRelease)
		{
			return true;
		}
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._happy)
		{
			_happy = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._content)
		{
			_content = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._neutral)
		{
			_neutral = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._sad)
		{
			_sad = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._happy)
		{
			value = VariantUtils.CreateFrom(in _happy);
			return true;
		}
		if (name == PropertyName._content)
		{
			value = VariantUtils.CreateFrom(in _content);
			return true;
		}
		if (name == PropertyName._neutral)
		{
			value = VariantUtils.CreateFrom(in _neutral);
			return true;
		}
		if (name == PropertyName._sad)
		{
			value = VariantUtils.CreateFrom(in _sad);
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
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._happy, PropertyHint.ResourceType, "Texture2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._content, PropertyHint.ResourceType, "Texture2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._neutral, PropertyHint.ResourceType, "Texture2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sad, PropertyHint.ResourceType, "Texture2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._happy, Variant.From(in _happy));
		info.AddProperty(PropertyName._content, Variant.From(in _content));
		info.AddProperty(PropertyName._neutral, Variant.From(in _neutral));
		info.AddProperty(PropertyName._sad, Variant.From(in _sad));
		info.AddSignalEventDelegate(SignalName.MouseReleased, backing_MouseReleased);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._label, out var value))
		{
			_label = value.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._happy, out var value2))
		{
			_happy = value2.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._content, out var value3))
		{
			_content = value3.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._neutral, out var value4))
		{
			_neutral = value4.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._sad, out var value5))
		{
			_sad = value5.As<Texture2D>();
		}
		if (info.TryGetSignalEventDelegate<MouseReleasedEventHandler>(SignalName.MouseReleased, out var value6))
		{
			backing_MouseReleased = value6;
		}
	}

	/// <summary>
	/// Get the signal information for all the signals declared in this class.
	/// This method is used by Godot to register the available signals in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.MouseReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalMouseReleased(InputEvent inputEvent)
	{
		EmitSignal(SignalName.MouseReleased, inputEvent);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.MouseReleased && args.Count == 1)
		{
			backing_MouseReleased?.Invoke(VariantUtils.ConvertTo<InputEvent>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.MouseReleased)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
