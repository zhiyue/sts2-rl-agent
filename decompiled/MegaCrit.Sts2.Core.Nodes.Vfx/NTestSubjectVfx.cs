using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NTestSubjectVfx.cs")]
public class NTestSubjectVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName PlayAnim1 = "PlayAnim1";

		public static readonly StringName SquirtNeck = "SquirtNeck";

		public static readonly StringName StartDizzies = "StartDizzies";

		public static readonly StringName EndDizzies = "EndDizzies";

		public static readonly StringName StartEmbers = "StartEmbers";

		public static readonly StringName StartFlames = "StartFlames";

		public static readonly StringName EndFlames = "EndFlames";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _neckParticles = "_neckParticles";

		public static readonly StringName _dizzyParticles = "_dizzyParticles";

		public static readonly StringName _emberParticles = "_emberParticles";

		public static readonly StringName _flameParticles = "_flameParticles";

		public static readonly StringName _parent = "_parent";

		public static readonly StringName _keyDown = "_keyDown";

		public static readonly StringName _doingThing = "_doingThing";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private GpuParticles2D _neckParticles;

	private GpuParticles2D _dizzyParticles;

	private GpuParticles2D _emberParticles;

	private GpuParticles2D _flameParticles;

	private Node2D _parent;

	private MegaSprite _animController;

	private bool _keyDown;

	private bool _doingThing;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_neckParticles = _parent.GetNode<GpuParticles2D>("NeckParticlesSlot/NeckParticles");
		_dizzyParticles = _parent.GetNode<GpuParticles2D>("NeckParticlesSlot/DizzyPaticles");
		_emberParticles = _parent.GetNode<GpuParticles2D>("EmberParticles");
		_flameParticles = _parent.GetNode<GpuParticles2D>("../../FlameParticles");
		_neckParticles.OneShot = true;
		_neckParticles.Emitting = false;
		_dizzyParticles.Emitting = false;
		_emberParticles.OneShot = true;
		_emberParticles.Emitting = false;
		_flameParticles.Emitting = false;
		_animController.GetAnimationState().SetAnimation("idle_loop3");
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "neck_explode":
			SquirtNeck();
			break;
		case "start_dizzies":
			StartDizzies();
			break;
		case "end_dizzies":
			EndDizzies();
			break;
		case "start_embers":
			StartEmbers();
			break;
		case "start_flames":
			StartFlames();
			break;
		case "end_flames":
			EndFlames();
			break;
		}
	}

	private void PlayAnim1()
	{
		_animController.GetAnimationState().SetAnimation("die3", loop: false);
		_animController.GetAnimationState().AddAnimation("idle_loop3");
	}

	private void SquirtNeck()
	{
		_neckParticles.Restart();
	}

	private void StartDizzies()
	{
		if (!_dizzyParticles.Emitting)
		{
			_dizzyParticles.Emitting = true;
		}
	}

	private void EndDizzies()
	{
		_dizzyParticles.Emitting = false;
	}

	private void StartEmbers()
	{
		_emberParticles.Restart();
	}

	private void StartFlames()
	{
		_flameParticles.Emitting = true;
	}

	private void EndFlames()
	{
		_flameParticles.Emitting = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PlayAnim1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SquirtNeck, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartDizzies, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndDizzies, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartEmbers, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartFlames, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndFlames, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.PlayAnim1 && args.Count == 0)
		{
			PlayAnim1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SquirtNeck && args.Count == 0)
		{
			SquirtNeck();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartDizzies && args.Count == 0)
		{
			StartDizzies();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndDizzies && args.Count == 0)
		{
			EndDizzies();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartEmbers && args.Count == 0)
		{
			StartEmbers();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartFlames && args.Count == 0)
		{
			StartFlames();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndFlames && args.Count == 0)
		{
			EndFlames();
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
		if (method == MethodName.PlayAnim1)
		{
			return true;
		}
		if (method == MethodName.SquirtNeck)
		{
			return true;
		}
		if (method == MethodName.StartDizzies)
		{
			return true;
		}
		if (method == MethodName.EndDizzies)
		{
			return true;
		}
		if (method == MethodName.StartEmbers)
		{
			return true;
		}
		if (method == MethodName.StartFlames)
		{
			return true;
		}
		if (method == MethodName.EndFlames)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._neckParticles)
		{
			_neckParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._dizzyParticles)
		{
			_dizzyParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._emberParticles)
		{
			_emberParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._flameParticles)
		{
			_flameParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._keyDown)
		{
			_keyDown = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._doingThing)
		{
			_doingThing = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._neckParticles)
		{
			value = VariantUtils.CreateFrom(in _neckParticles);
			return true;
		}
		if (name == PropertyName._dizzyParticles)
		{
			value = VariantUtils.CreateFrom(in _dizzyParticles);
			return true;
		}
		if (name == PropertyName._emberParticles)
		{
			value = VariantUtils.CreateFrom(in _emberParticles);
			return true;
		}
		if (name == PropertyName._flameParticles)
		{
			value = VariantUtils.CreateFrom(in _flameParticles);
			return true;
		}
		if (name == PropertyName._parent)
		{
			value = VariantUtils.CreateFrom(in _parent);
			return true;
		}
		if (name == PropertyName._keyDown)
		{
			value = VariantUtils.CreateFrom(in _keyDown);
			return true;
		}
		if (name == PropertyName._doingThing)
		{
			value = VariantUtils.CreateFrom(in _doingThing);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._neckParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dizzyParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._emberParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._flameParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._keyDown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._doingThing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._neckParticles, Variant.From(in _neckParticles));
		info.AddProperty(PropertyName._dizzyParticles, Variant.From(in _dizzyParticles));
		info.AddProperty(PropertyName._emberParticles, Variant.From(in _emberParticles));
		info.AddProperty(PropertyName._flameParticles, Variant.From(in _flameParticles));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
		info.AddProperty(PropertyName._keyDown, Variant.From(in _keyDown));
		info.AddProperty(PropertyName._doingThing, Variant.From(in _doingThing));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._neckParticles, out var value))
		{
			_neckParticles = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._dizzyParticles, out var value2))
		{
			_dizzyParticles = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._emberParticles, out var value3))
		{
			_emberParticles = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._flameParticles, out var value4))
		{
			_flameParticles = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value5))
		{
			_parent = value5.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._keyDown, out var value6))
		{
			_keyDown = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._doingThing, out var value7))
		{
			_doingThing = value7.As<bool>();
		}
	}
}
