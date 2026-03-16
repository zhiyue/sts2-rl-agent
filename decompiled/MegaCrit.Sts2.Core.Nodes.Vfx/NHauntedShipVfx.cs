using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NHauntedShipVfx.cs")]
public class NHauntedShipVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName OnEyeBubblesStart = "OnEyeBubblesStart";

		public static readonly StringName OnEyeBubblesEnd = "OnEyeBubblesEnd";

		public static readonly StringName OnHeadBubblesStart = "OnHeadBubblesStart";

		public static readonly StringName OnHeadBubblesEnd = "OnHeadBubblesEnd";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _eyeParticles1 = "_eyeParticles1";

		public static readonly StringName _eyeParticles2 = "_eyeParticles2";

		public static readonly StringName _eyeParticles3 = "_eyeParticles3";

		public static readonly StringName _headParticles1 = "_headParticles1";

		public static readonly StringName _headParticles2 = "_headParticles2";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private MegaSprite _megaSprite;

	private GpuParticles2D _eyeParticles1;

	private GpuParticles2D _eyeParticles2;

	private GpuParticles2D _eyeParticles3;

	private GpuParticles2D _headParticles1;

	private GpuParticles2D _headParticles2;

	public override void _Ready()
	{
		_eyeParticles1 = GetNode<GpuParticles2D>("../EyeBone1/BubbleParticles");
		_eyeParticles2 = GetNode<GpuParticles2D>("../EyeBone2/BubbleParticles");
		_eyeParticles3 = GetNode<GpuParticles2D>("../EyeBone3/BubbleParticles");
		_headParticles1 = GetNode<GpuParticles2D>("../HeadSlot/BubbleParticles");
		_headParticles2 = GetNode<GpuParticles2D>("../HeadSlot/BubbleParticles2");
		_eyeParticles1.Emitting = false;
		_eyeParticles2.Emitting = false;
		_eyeParticles3.Emitting = false;
		_megaSprite = new MegaSprite(GetParent<Node2D>());
		_megaSprite.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "eye_bubbles_start":
			OnEyeBubblesStart();
			break;
		case "eye_bubbles_end":
			OnEyeBubblesEnd();
			break;
		case "head_bubbles_start":
			OnHeadBubblesStart();
			break;
		case "head_bubbles_end":
			OnHeadBubblesEnd();
			break;
		}
	}

	private void OnEyeBubblesStart()
	{
		_eyeParticles1.Emitting = true;
		_eyeParticles2.Emitting = true;
		_eyeParticles3.Emitting = true;
	}

	private void OnEyeBubblesEnd()
	{
		_eyeParticles1.Emitting = false;
		_eyeParticles2.Emitting = false;
		_eyeParticles3.Emitting = false;
	}

	private void OnHeadBubblesStart()
	{
		_headParticles1.Emitting = true;
		_headParticles2.Emitting = true;
	}

	private void OnHeadBubblesEnd()
	{
		_headParticles1.Emitting = false;
		_headParticles2.Emitting = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnEyeBubblesStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEyeBubblesEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHeadBubblesStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHeadBubblesEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnAnimationEvent && args.Count == 4)
		{
			OnAnimationEvent(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]), VariantUtils.ConvertTo<GodotObject>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEyeBubblesStart && args.Count == 0)
		{
			OnEyeBubblesStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEyeBubblesEnd && args.Count == 0)
		{
			OnEyeBubblesEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHeadBubblesStart && args.Count == 0)
		{
			OnHeadBubblesStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHeadBubblesEnd && args.Count == 0)
		{
			OnHeadBubblesEnd();
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
		if (method == MethodName.OnAnimationEvent)
		{
			return true;
		}
		if (method == MethodName.OnEyeBubblesStart)
		{
			return true;
		}
		if (method == MethodName.OnEyeBubblesEnd)
		{
			return true;
		}
		if (method == MethodName.OnHeadBubblesStart)
		{
			return true;
		}
		if (method == MethodName.OnHeadBubblesEnd)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._eyeParticles1)
		{
			_eyeParticles1 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._eyeParticles2)
		{
			_eyeParticles2 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._eyeParticles3)
		{
			_eyeParticles3 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._headParticles1)
		{
			_headParticles1 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._headParticles2)
		{
			_headParticles2 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._eyeParticles1)
		{
			value = VariantUtils.CreateFrom(in _eyeParticles1);
			return true;
		}
		if (name == PropertyName._eyeParticles2)
		{
			value = VariantUtils.CreateFrom(in _eyeParticles2);
			return true;
		}
		if (name == PropertyName._eyeParticles3)
		{
			value = VariantUtils.CreateFrom(in _eyeParticles3);
			return true;
		}
		if (name == PropertyName._headParticles1)
		{
			value = VariantUtils.CreateFrom(in _headParticles1);
			return true;
		}
		if (name == PropertyName._headParticles2)
		{
			value = VariantUtils.CreateFrom(in _headParticles2);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._eyeParticles1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._eyeParticles2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._eyeParticles3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._headParticles1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._headParticles2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._eyeParticles1, Variant.From(in _eyeParticles1));
		info.AddProperty(PropertyName._eyeParticles2, Variant.From(in _eyeParticles2));
		info.AddProperty(PropertyName._eyeParticles3, Variant.From(in _eyeParticles3));
		info.AddProperty(PropertyName._headParticles1, Variant.From(in _headParticles1));
		info.AddProperty(PropertyName._headParticles2, Variant.From(in _headParticles2));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._eyeParticles1, out var value))
		{
			_eyeParticles1 = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._eyeParticles2, out var value2))
		{
			_eyeParticles2 = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._eyeParticles3, out var value3))
		{
			_eyeParticles3 = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._headParticles1, out var value4))
		{
			_headParticles1 = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._headParticles2, out var value5))
		{
			_headParticles2 = value5.As<GpuParticles2D>();
		}
	}
}
