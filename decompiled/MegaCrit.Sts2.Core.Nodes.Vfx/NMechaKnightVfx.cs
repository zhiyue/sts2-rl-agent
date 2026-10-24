using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NMechaKnightVfx.cs")]
public class NMechaKnightVfx : Node
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
		/// Cached name for the 'TurnOnFlameThrower' method.
		/// </summary>
		public static readonly StringName TurnOnFlameThrower = "TurnOnFlameThrower";

		/// <summary>
		/// Cached name for the 'TurnOffFlameThrower' method.
		/// </summary>
		public static readonly StringName TurnOffFlameThrower = "TurnOffFlameThrower";

		/// <summary>
		/// Cached name for the 'TurnOnEngine' method.
		/// </summary>
		public static readonly StringName TurnOnEngine = "TurnOnEngine";

		/// <summary>
		/// Cached name for the 'TurnOffEngine' method.
		/// </summary>
		public static readonly StringName TurnOffEngine = "TurnOffEngine";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the '_flameThrowerParticlesDark' field.
		/// </summary>
		public static readonly StringName _flameThrowerParticlesDark = "_flameThrowerParticlesDark";

		/// <summary>
		/// Cached name for the '_flameThrowerParticlesLight' field.
		/// </summary>
		public static readonly StringName _flameThrowerParticlesLight = "_flameThrowerParticlesLight";

		/// <summary>
		/// Cached name for the '_cinderParticles' field.
		/// </summary>
		public static readonly StringName _cinderParticles = "_cinderParticles";

		/// <summary>
		/// Cached name for the '_glowParticles' field.
		/// </summary>
		public static readonly StringName _glowParticles = "_glowParticles";

		/// <summary>
		/// Cached name for the '_engineParticles' field.
		/// </summary>
		public static readonly StringName _engineParticles = "_engineParticles";

		/// <summary>
		/// Cached name for the '_engineParticlesDark' field.
		/// </summary>
		public static readonly StringName _engineParticlesDark = "_engineParticlesDark";

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

	private GpuParticles2D _flameThrowerParticlesDark;

	private GpuParticles2D _flameThrowerParticlesLight;

	private GpuParticles2D _cinderParticles;

	private GpuParticles2D _glowParticles;

	private GpuParticles2D _engineParticles;

	private GpuParticles2D _engineParticlesDark;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_animController.ConnectAnimationStarted(Callable.From<GodotObject, GodotObject, GodotObject>(OnAnimationStart));
		_flameThrowerParticlesDark = _parent.GetNode<GpuParticles2D>("FlameParticlesBone/FlameParticlesDark");
		_flameThrowerParticlesLight = _parent.GetNode<GpuParticles2D>("FlameParticlesBone/FlameParticlesLight");
		_cinderParticles = _parent.GetNode<GpuParticles2D>("FlameParticlesBone/CinderParticles");
		_glowParticles = _parent.GetNode<GpuParticles2D>("FlameParticlesBone/GlowParticles");
		_engineParticles = _parent.GetNode<GpuParticles2D>("EngineSlot/EngineBone/EngineParticles");
		_engineParticlesDark = _parent.GetNode<GpuParticles2D>("EngineSlot/EngineBone/EngineParticlesDark");
		TurnOffFlameThrower();
		TurnOffEngine();
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "flame_start":
			TurnOnFlameThrower();
			break;
		case "flame_end":
			TurnOffFlameThrower();
			break;
		case "engine_start":
			TurnOnEngine();
			break;
		case "engine_stop":
			TurnOffEngine();
			break;
		}
	}

	private void OnAnimationStart(GodotObject spineSprite, GodotObject animationState, GodotObject trackEntry)
	{
		string currentAnimationName = new MegaAnimationState(animationState).GetCurrentAnimationName();
		if (currentAnimationName != "attack")
		{
			TurnOffFlameThrower();
		}
		bool flag = currentAnimationName == "hurt";
		_flameThrowerParticlesDark.Visible = !flag;
		_flameThrowerParticlesLight.Visible = !flag;
		_cinderParticles.Visible = !flag;
		_glowParticles.Visible = !flag;
	}

	private void TurnOnFlameThrower()
	{
		_flameThrowerParticlesDark.Restart();
		_flameThrowerParticlesLight.Restart();
		_cinderParticles.Restart();
		_glowParticles.Restart();
	}

	private void TurnOffFlameThrower()
	{
		_flameThrowerParticlesDark.Emitting = false;
		_flameThrowerParticlesLight.Emitting = false;
		_cinderParticles.Emitting = false;
		_glowParticles.Emitting = false;
	}

	private void TurnOnEngine()
	{
		_engineParticles.Restart();
		_engineParticlesDark.Restart();
	}

	private void TurnOffEngine()
	{
		_engineParticles.Emitting = false;
		_engineParticlesDark.Emitting = false;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
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
		list.Add(new MethodInfo(MethodName.TurnOnFlameThrower, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffFlameThrower, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOnEngine, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffEngine, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.TurnOnFlameThrower && args.Count == 0)
		{
			TurnOnFlameThrower();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOffFlameThrower && args.Count == 0)
		{
			TurnOffFlameThrower();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOnEngine && args.Count == 0)
		{
			TurnOnEngine();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOffEngine && args.Count == 0)
		{
			TurnOffEngine();
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
		if (method == MethodName.TurnOnFlameThrower)
		{
			return true;
		}
		if (method == MethodName.TurnOffFlameThrower)
		{
			return true;
		}
		if (method == MethodName.TurnOnEngine)
		{
			return true;
		}
		if (method == MethodName.TurnOffEngine)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._flameThrowerParticlesDark)
		{
			_flameThrowerParticlesDark = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._flameThrowerParticlesLight)
		{
			_flameThrowerParticlesLight = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._cinderParticles)
		{
			_cinderParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._glowParticles)
		{
			_glowParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._engineParticles)
		{
			_engineParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._engineParticlesDark)
		{
			_engineParticlesDark = VariantUtils.ConvertTo<GpuParticles2D>(in value);
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
		if (name == PropertyName._flameThrowerParticlesDark)
		{
			value = VariantUtils.CreateFrom(in _flameThrowerParticlesDark);
			return true;
		}
		if (name == PropertyName._flameThrowerParticlesLight)
		{
			value = VariantUtils.CreateFrom(in _flameThrowerParticlesLight);
			return true;
		}
		if (name == PropertyName._cinderParticles)
		{
			value = VariantUtils.CreateFrom(in _cinderParticles);
			return true;
		}
		if (name == PropertyName._glowParticles)
		{
			value = VariantUtils.CreateFrom(in _glowParticles);
			return true;
		}
		if (name == PropertyName._engineParticles)
		{
			value = VariantUtils.CreateFrom(in _engineParticles);
			return true;
		}
		if (name == PropertyName._engineParticlesDark)
		{
			value = VariantUtils.CreateFrom(in _engineParticlesDark);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._flameThrowerParticlesDark, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._flameThrowerParticlesLight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cinderParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glowParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._engineParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._engineParticlesDark, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._flameThrowerParticlesDark, Variant.From(in _flameThrowerParticlesDark));
		info.AddProperty(PropertyName._flameThrowerParticlesLight, Variant.From(in _flameThrowerParticlesLight));
		info.AddProperty(PropertyName._cinderParticles, Variant.From(in _cinderParticles));
		info.AddProperty(PropertyName._glowParticles, Variant.From(in _glowParticles));
		info.AddProperty(PropertyName._engineParticles, Variant.From(in _engineParticles));
		info.AddProperty(PropertyName._engineParticlesDark, Variant.From(in _engineParticlesDark));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._flameThrowerParticlesDark, out var value))
		{
			_flameThrowerParticlesDark = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._flameThrowerParticlesLight, out var value2))
		{
			_flameThrowerParticlesLight = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._cinderParticles, out var value3))
		{
			_cinderParticles = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._glowParticles, out var value4))
		{
			_glowParticles = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._engineParticles, out var value5))
		{
			_engineParticles = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._engineParticlesDark, out var value6))
		{
			_engineParticlesDark = value6.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value7))
		{
			_parent = value7.As<Node2D>();
		}
	}
}
