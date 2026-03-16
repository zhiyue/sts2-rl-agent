using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

[ScriptPath("res://src/Core/Nodes/Screens/Timeline/NResetProgressButton.cs")]
public class NResetProgressButton : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnMouseExited = "OnMouseExited";

		public static readonly StringName OnMouseEntered = "OnMouseEntered";

		public static readonly StringName OnGuiInput = "OnGuiInput";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _disclaimer = "_disclaimer";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private MegaLabel _disclaimer;

	public override void _Ready()
	{
		Connect(Control.SignalName.MouseEntered, Callable.From(OnMouseEntered));
		Connect(Control.SignalName.MouseExited, Callable.From(OnMouseExited));
		Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(OnGuiInput));
		_disclaimer = GetNode<MegaLabel>("%CloseScreenDisclaimer");
	}

	private void OnMouseExited()
	{
		base.Scale = Vector2.One;
	}

	private void OnMouseEntered()
	{
		base.Scale = Vector2.One * 1.1f;
	}

	private void OnGuiInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left && !inputEventMouseButton.Pressed)
		{
			SaveManager.Instance.ResetTimelineProgress();
			_disclaimer.Visible = true;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnMouseExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnMouseEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnGuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
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
		if (method == MethodName.OnMouseExited && args.Count == 0)
		{
			OnMouseExited();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMouseEntered && args.Count == 0)
		{
			OnMouseEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnGuiInput && args.Count == 1)
		{
			OnGuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
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
		if (method == MethodName.OnMouseExited)
		{
			return true;
		}
		if (method == MethodName.OnMouseEntered)
		{
			return true;
		}
		if (method == MethodName.OnGuiInput)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._disclaimer)
		{
			_disclaimer = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._disclaimer)
		{
			value = VariantUtils.CreateFrom(in _disclaimer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._disclaimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._disclaimer, Variant.From(in _disclaimer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._disclaimer, out var value))
		{
			_disclaimer = value.As<MegaLabel>();
		}
	}
}
