using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NPhrogParasiteVfx.cs")]
public class NPhrogParasiteVfx : Node
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'OnAnimationEvent' method.
		/// </summary>
		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		/// <summary>
		/// Cached name for the 'OnAnimationStart' method.
		/// </summary>
		public static readonly StringName OnAnimationStart = "OnAnimationStart";

		/// <summary>
		/// Cached name for the 'TurnOnInfect' method.
		/// </summary>
		public static readonly StringName TurnOnInfect = "TurnOnInfect";

		/// <summary>
		/// Cached name for the 'TurnOffInfect' method.
		/// </summary>
		public static readonly StringName TurnOffInfect = "TurnOffInfect";

		/// <summary>
		/// Cached name for the 'StartExplode' method.
		/// </summary>
		public static readonly StringName StartExplode = "StartExplode";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the '_bubbleParticlesA' field.
		/// </summary>
		public static readonly StringName _bubbleParticlesA = "_bubbleParticlesA";

		/// <summary>
		/// Cached name for the '_bubbleParticlesB' field.
		/// </summary>
		public static readonly StringName _bubbleParticlesB = "_bubbleParticlesB";

		/// <summary>
		/// Cached name for the '_bubbleParticlesC' field.
		/// </summary>
		public static readonly StringName _bubbleParticlesC = "_bubbleParticlesC";

		/// <summary>
		/// Cached name for the '_gooParticlesDeath' field.
		/// </summary>
		public static readonly StringName _gooParticlesDeath = "_gooParticlesDeath";

		/// <summary>
		/// Cached name for the '_wormParticlesDeath' field.
		/// </summary>
		public static readonly StringName _wormParticlesDeath = "_wormParticlesDeath";

		/// <summary>
		/// Cached name for the '_parent' field.
		/// </summary>
		public static readonly StringName _parent = "_parent";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	private GpuParticles2D _bubbleParticlesA;

	private GpuParticles2D _bubbleParticlesB;

	private GpuParticles2D _bubbleParticlesC;

	private GpuParticles2D _gooParticlesDeath;

	private GpuParticles2D _wormParticlesDeath;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_bubbleParticlesA = _parent.GetNode<GpuParticles2D>("BubbleABoneNode/WormParticlesA");
		_bubbleParticlesB = _parent.GetNode<GpuParticles2D>("BubbleBSlotNode/WormParticlesB");
		_bubbleParticlesC = _parent.GetNode<GpuParticles2D>("BubbleCBoneNode/WormParticlesC");
		_gooParticlesDeath = _parent.GetNode<GpuParticles2D>("DeathParticles");
		_wormParticlesDeath = _parent.GetNode<GpuParticles2D>("DeathWormParticles");
		_bubbleParticlesA.Emitting = false;
		_bubbleParticlesB.Emitting = false;
		_bubbleParticlesC.Emitting = false;
		_gooParticlesDeath.Emitting = false;
		_wormParticlesDeath.Emitting = false;
		_gooParticlesDeath.OneShot = true;
		_wormParticlesDeath.OneShot = true;
		this.RunWhenSpineReady(_animController, delegate(MegaAnimationState animState)
		{
			animState.SetAnimation("die");
		});
		_animController.ConnectAnimationStarted(Callable.From<GodotObject, GodotObject, GodotObject>(OnAnimationStart));
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "infect":
			TurnOnInfect();
			break;
		case "stop_infect":
			TurnOffInfect();
			break;
		case "explode":
			StartExplode();
			break;
		}
	}

	private void OnAnimationStart(GodotObject spineSprite, GodotObject animationState, GodotObject trackEntry)
	{
		TurnOffInfect();
	}

	private void TurnOnInfect()
	{
		_bubbleParticlesA.Emitting = true;
		_bubbleParticlesB.Emitting = true;
		_bubbleParticlesC.Emitting = true;
	}

	private void TurnOffInfect()
	{
		_bubbleParticlesA.Emitting = false;
		_bubbleParticlesB.Emitting = false;
		_bubbleParticlesC.Emitting = false;
	}

	private void StartExplode()
	{
		_gooParticlesDeath.Restart();
		_wormParticlesDeath.Restart();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
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
		list.Add(new MethodInfo(MethodName.OnAnimationStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "spineSprite", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "animationState", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "trackEntry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.TurnOnInfect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffInfect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartExplode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnAnimationEvent && args.Count == 4)
		{
			OnAnimationEvent(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]), VariantUtils.ConvertTo<GodotObject>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAnimationStart && args.Count == 3)
		{
			OnAnimationStart(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOnInfect && args.Count == 0)
		{
			TurnOnInfect();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOffInfect && args.Count == 0)
		{
			TurnOffInfect();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartExplode && args.Count == 0)
		{
			StartExplode();
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
		if (method == MethodName.OnAnimationEvent)
		{
			return true;
		}
		if (method == MethodName.OnAnimationStart)
		{
			return true;
		}
		if (method == MethodName.TurnOnInfect)
		{
			return true;
		}
		if (method == MethodName.TurnOffInfect)
		{
			return true;
		}
		if (method == MethodName.StartExplode)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._bubbleParticlesA)
		{
			_bubbleParticlesA = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._bubbleParticlesB)
		{
			_bubbleParticlesB = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._bubbleParticlesC)
		{
			_bubbleParticlesC = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._gooParticlesDeath)
		{
			_gooParticlesDeath = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._wormParticlesDeath)
		{
			_wormParticlesDeath = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._bubbleParticlesA)
		{
			value = VariantUtils.CreateFrom(in _bubbleParticlesA);
			return true;
		}
		if (name == PropertyName._bubbleParticlesB)
		{
			value = VariantUtils.CreateFrom(in _bubbleParticlesB);
			return true;
		}
		if (name == PropertyName._bubbleParticlesC)
		{
			value = VariantUtils.CreateFrom(in _bubbleParticlesC);
			return true;
		}
		if (name == PropertyName._gooParticlesDeath)
		{
			value = VariantUtils.CreateFrom(in _gooParticlesDeath);
			return true;
		}
		if (name == PropertyName._wormParticlesDeath)
		{
			value = VariantUtils.CreateFrom(in _wormParticlesDeath);
			return true;
		}
		if (name == PropertyName._parent)
		{
			value = VariantUtils.CreateFrom(in _parent);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bubbleParticlesA, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bubbleParticlesB, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bubbleParticlesC, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._gooParticlesDeath, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._wormParticlesDeath, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._bubbleParticlesA, Variant.From(in _bubbleParticlesA));
		info.AddProperty(PropertyName._bubbleParticlesB, Variant.From(in _bubbleParticlesB));
		info.AddProperty(PropertyName._bubbleParticlesC, Variant.From(in _bubbleParticlesC));
		info.AddProperty(PropertyName._gooParticlesDeath, Variant.From(in _gooParticlesDeath));
		info.AddProperty(PropertyName._wormParticlesDeath, Variant.From(in _wormParticlesDeath));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._bubbleParticlesA, out var value))
		{
			_bubbleParticlesA = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._bubbleParticlesB, out var value2))
		{
			_bubbleParticlesB = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._bubbleParticlesC, out var value3))
		{
			_bubbleParticlesC = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._gooParticlesDeath, out var value4))
		{
			_gooParticlesDeath = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._wormParticlesDeath, out var value5))
		{
			_wormParticlesDeath = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value6))
		{
			_parent = value6.As<Node2D>();
		}
	}
}
