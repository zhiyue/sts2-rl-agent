using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NSkulkingColonyVfx.cs")]
public class NSkulkingColonyVfx : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
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
		/// Cached name for the 'DamageHandler' method.
		/// </summary>
		public static readonly StringName DamageHandler = "DamageHandler";

		/// <summary>
		/// Cached name for the 'DeathHandler' method.
		/// </summary>
		public static readonly StringName DeathHandler = "DeathHandler";

		/// <summary>
		/// Cached name for the 'PoofHandler' method.
		/// </summary>
		public static readonly StringName PoofHandler = "PoofHandler";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the '_particles1' field.
		/// </summary>
		public static readonly StringName _particles1 = "_particles1";

		/// <summary>
		/// Cached name for the '_wideParticles' field.
		/// </summary>
		public static readonly StringName _wideParticles = "_wideParticles";

		/// <summary>
		/// Cached name for the '_poofParticles' field.
		/// </summary>
		public static readonly StringName _poofParticles = "_poofParticles";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	private MegaSprite _megaSprite;

	private GpuParticles2D _particles1;

	private GpuParticles2D _wideParticles;

	private GpuParticles2D _poofParticles;

	public override void _Ready()
	{
		_particles1 = GetNode<GpuParticles2D>("../ParticleSlot1/Particles");
		_wideParticles = GetNode<GpuParticles2D>("../ParticleSlot2/ParticlesWide");
		_poofParticles = GetNode<GpuParticles2D>("../ParticleSlot3/Particles");
		_particles1.Emitting = false;
		_poofParticles.Emitting = false;
		_wideParticles.Emitting = false;
		_particles1.OneShot = true;
		_poofParticles.OneShot = true;
		_wideParticles.OneShot = true;
		_megaSprite = new MegaSprite(GetParent<Node2D>());
		_megaSprite.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "take_damage":
			DamageHandler();
			break;
		case "take_fatal_damage":
			DeathHandler();
			break;
		case "final_poof":
			PoofHandler();
			break;
		}
	}

	private void DamageHandler()
	{
		_particles1.Restart();
	}

	private void DeathHandler()
	{
		_wideParticles.Restart();
	}

	private void PoofHandler()
	{
		_poofParticles.Restart();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DamageHandler, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DeathHandler, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PoofHandler, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.DamageHandler && args.Count == 0)
		{
			DamageHandler();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DeathHandler && args.Count == 0)
		{
			DeathHandler();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PoofHandler && args.Count == 0)
		{
			PoofHandler();
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
		if (method == MethodName.DamageHandler)
		{
			return true;
		}
		if (method == MethodName.DeathHandler)
		{
			return true;
		}
		if (method == MethodName.PoofHandler)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._particles1)
		{
			_particles1 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._wideParticles)
		{
			_wideParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._poofParticles)
		{
			_poofParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._particles1)
		{
			value = VariantUtils.CreateFrom(in _particles1);
			return true;
		}
		if (name == PropertyName._wideParticles)
		{
			value = VariantUtils.CreateFrom(in _wideParticles);
			return true;
		}
		if (name == PropertyName._poofParticles)
		{
			value = VariantUtils.CreateFrom(in _poofParticles);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._particles1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._wideParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._poofParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._particles1, Variant.From(in _particles1));
		info.AddProperty(PropertyName._wideParticles, Variant.From(in _wideParticles));
		info.AddProperty(PropertyName._poofParticles, Variant.From(in _poofParticles));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._particles1, out var value))
		{
			_particles1 = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._wideParticles, out var value2))
		{
			_wideParticles = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._poofParticles, out var value3))
		{
			_poofParticles = value3.As<GpuParticles2D>();
		}
	}
}
