using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NSlimedBerserkerVfx.cs")]
public class NSlimedBerserkerVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName StartGooParticles = "StartGooParticles";

		public static readonly StringName StopGooParticles = "StopGooParticles";

		public static readonly StringName StopVomitParticles = "StopVomitParticles";

		public static readonly StringName StartVomitParticles = "StartVomitParticles";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _gooParticlesR = "_gooParticlesR";

		public static readonly StringName _gooParticlesL = "_gooParticlesL";

		public static readonly StringName _gooParticlesVomit = "_gooParticlesVomit";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private GpuParticles2D _gooParticlesR;

	private GpuParticles2D _gooParticlesL;

	private GpuParticles2D _gooParticlesVomit;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_gooParticlesR = _parent.GetNode<GpuParticles2D>("ParticleSlotNodeR/GooParticles");
		_gooParticlesL = _parent.GetNode<GpuParticles2D>("ParticleSlotNodeL/GooParticles");
		_gooParticlesVomit = _parent.GetNode<GpuParticles2D>("ParticleSlotNodeVomit/GooParticles");
		StopGooParticles();
		StopVomitParticles();
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "goo_start":
			StartGooParticles();
			break;
		case "goo_stop":
			StopGooParticles();
			break;
		case "vomit_start":
			StartVomitParticles();
			break;
		case "vomit_stop":
			StopVomitParticles();
			break;
		}
	}

	private void StartGooParticles()
	{
		_gooParticlesR.Emitting = true;
		_gooParticlesL.Emitting = true;
	}

	private void StopGooParticles()
	{
		_gooParticlesR.Emitting = false;
		_gooParticlesL.Emitting = false;
	}

	private void StopVomitParticles()
	{
		_gooParticlesVomit.Emitting = false;
	}

	private void StartVomitParticles()
	{
		_gooParticlesVomit.Emitting = true;
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
		list.Add(new MethodInfo(MethodName.StartGooParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StopGooParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StopVomitParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartVomitParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.StartGooParticles && args.Count == 0)
		{
			StartGooParticles();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopGooParticles && args.Count == 0)
		{
			StopGooParticles();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopVomitParticles && args.Count == 0)
		{
			StopVomitParticles();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartVomitParticles && args.Count == 0)
		{
			StartVomitParticles();
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
		if (method == MethodName.StartGooParticles)
		{
			return true;
		}
		if (method == MethodName.StopGooParticles)
		{
			return true;
		}
		if (method == MethodName.StopVomitParticles)
		{
			return true;
		}
		if (method == MethodName.StartVomitParticles)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._gooParticlesR)
		{
			_gooParticlesR = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._gooParticlesL)
		{
			_gooParticlesL = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._gooParticlesVomit)
		{
			_gooParticlesVomit = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._gooParticlesR)
		{
			value = VariantUtils.CreateFrom(in _gooParticlesR);
			return true;
		}
		if (name == PropertyName._gooParticlesL)
		{
			value = VariantUtils.CreateFrom(in _gooParticlesL);
			return true;
		}
		if (name == PropertyName._gooParticlesVomit)
		{
			value = VariantUtils.CreateFrom(in _gooParticlesVomit);
			return true;
		}
		if (name == PropertyName._parent)
		{
			value = VariantUtils.CreateFrom(in _parent);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._gooParticlesR, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._gooParticlesL, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._gooParticlesVomit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._gooParticlesR, Variant.From(in _gooParticlesR));
		info.AddProperty(PropertyName._gooParticlesL, Variant.From(in _gooParticlesL));
		info.AddProperty(PropertyName._gooParticlesVomit, Variant.From(in _gooParticlesVomit));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._gooParticlesR, out var value))
		{
			_gooParticlesR = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._gooParticlesL, out var value2))
		{
			_gooParticlesL = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._gooParticlesVomit, out var value3))
		{
			_gooParticlesVomit = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value4))
		{
			_parent = value4.As<Node2D>();
		}
	}
}
