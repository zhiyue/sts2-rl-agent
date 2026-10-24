using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NVantomVfx.cs")]
public class NVantomVfx : Node
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
		/// Cached name for the 'DissolveTail' method.
		/// </summary>
		public static readonly StringName DissolveTail = "DissolveTail";

		/// <summary>
		/// Cached name for the 'StartSpray' method.
		/// </summary>
		public static readonly StringName StartSpray = "StartSpray";

		/// <summary>
		/// Cached name for the 'EndSpray' method.
		/// </summary>
		public static readonly StringName EndSpray = "EndSpray";

		/// <summary>
		/// Cached name for the 'StartDeathSpray' method.
		/// </summary>
		public static readonly StringName StartDeathSpray = "StartDeathSpray";

		/// <summary>
		/// Cached name for the 'EndDeathSpray' method.
		/// </summary>
		public static readonly StringName EndDeathSpray = "EndDeathSpray";

		/// <summary>
		/// Cached name for the 'DeathExplode' method.
		/// </summary>
		public static readonly StringName DeathExplode = "DeathExplode";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the '_tailShaderMat' field.
		/// </summary>
		public static readonly StringName _tailShaderMat = "_tailShaderMat";

		/// <summary>
		/// Cached name for the '_sprayParticles' field.
		/// </summary>
		public static readonly StringName _sprayParticles = "_sprayParticles";

		/// <summary>
		/// Cached name for the '_deathSprayParticles' field.
		/// </summary>
		public static readonly StringName _deathSprayParticles = "_deathSprayParticles";

		/// <summary>
		/// Cached name for the '_deathSprayParticlesBack' field.
		/// </summary>
		public static readonly StringName _deathSprayParticlesBack = "_deathSprayParticlesBack";

		/// <summary>
		/// Cached name for the '_deathExplosionParticles' field.
		/// </summary>
		public static readonly StringName _deathExplosionParticles = "_deathExplosionParticles";

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

	private static readonly StringName _step = new StringName("step");

	private ShaderMaterial? _tailShaderMat;

	private GpuParticles2D _sprayParticles;

	private GpuParticles2D _deathSprayParticles;

	private GpuParticles2D _deathSprayParticlesBack;

	private GpuParticles2D _deathExplosionParticles;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_animController.ConnectAnimationStarted(Callable.From<GodotObject, GodotObject, GodotObject>(OnAnimationStart));
		_tailShaderMat = new MegaSlotNode(_parent.GetNode("TailSlotNode")).GetNormalMaterial() as ShaderMaterial;
		_sprayParticles = _parent.GetNode<GpuParticles2D>("SprayBoneNode/SprayParticles");
		_deathSprayParticles = _parent.GetNode<GpuParticles2D>("DeathSpraySlotNode/DeathSprayParticles");
		_deathSprayParticlesBack = _parent.GetNode<GpuParticles2D>("DeathSprayBackSlotNode/DeathSprayParticlesBack");
		_deathExplosionParticles = _parent.GetNode<GpuParticles2D>("DeathExplosionSlotNode/DeathExplosionParticles");
		_tailShaderMat?.SetShaderParameter(_step, -0.1f);
		this.RunWhenSpineReady(_animController, delegate(MegaAnimationState animState)
		{
			animState.SetAnimation("idle_loop");
			animState.SetAnimation("_tracks/charged_0", loop: true, 1);
		});
		_sprayParticles.Emitting = false;
		_deathSprayParticles.Emitting = false;
		_deathSprayParticlesBack.Emitting = false;
		_deathExplosionParticles.Emitting = false;
		_deathExplosionParticles.OneShot = true;
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "dissolve_tail":
			DissolveTail();
			break;
		case "spray_on":
			StartSpray();
			break;
		case "spray_off":
			EndSpray();
			break;
		case "death_spray_on":
			StartDeathSpray();
			break;
		case "death_spray_off":
			EndDeathSpray();
			break;
		case "death_explosion":
			DeathExplode();
			break;
		}
	}

	private void OnAnimationStart(GodotObject spineSprite, GodotObject animationState, GodotObject trackEntry)
	{
		if (new MegaAnimationState(animationState).GetCurrentAnimationName() != "die")
		{
			_tailShaderMat?.SetShaderParameter(_step, 0f);
		}
	}

	private void DissolveTail()
	{
		if (_tailShaderMat != null)
		{
			Tween tween = CreateTween();
			tween.SetEase(Tween.EaseType.In);
			tween.SetTrans(Tween.TransitionType.Quad);
			tween.TweenProperty(_tailShaderMat, "shader_parameter/step", 1f, 1.0);
			tween.TweenCallback(Callable.From(delegate
			{
				_animController.GetAnimationState().SetAnimation("_tracks/charge_up_1", loop: false, 1);
				_animController.GetAnimationState().AddAnimation("_tracks/charged_1", 0f, loop: true, 1);
			}));
		}
	}

	private void StartSpray()
	{
		_sprayParticles.Emitting = true;
	}

	private void EndSpray()
	{
		_sprayParticles.Emitting = false;
	}

	private void StartDeathSpray()
	{
		_deathSprayParticles.Emitting = true;
		_deathSprayParticlesBack.Emitting = true;
	}

	private void EndDeathSpray()
	{
		_deathSprayParticles.Emitting = false;
		_deathSprayParticlesBack.Emitting = false;
	}

	private void DeathExplode()
	{
		_deathExplosionParticles.Restart();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
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
		list.Add(new MethodInfo(MethodName.OnAnimationStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "spineSprite", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "animationState", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "trackEntry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DissolveTail, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartSpray, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndSpray, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartDeathSpray, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndDeathSpray, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DeathExplode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.DissolveTail && args.Count == 0)
		{
			DissolveTail();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartSpray && args.Count == 0)
		{
			StartSpray();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndSpray && args.Count == 0)
		{
			EndSpray();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartDeathSpray && args.Count == 0)
		{
			StartDeathSpray();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndDeathSpray && args.Count == 0)
		{
			EndDeathSpray();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DeathExplode && args.Count == 0)
		{
			DeathExplode();
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
		if (method == MethodName.DissolveTail)
		{
			return true;
		}
		if (method == MethodName.StartSpray)
		{
			return true;
		}
		if (method == MethodName.EndSpray)
		{
			return true;
		}
		if (method == MethodName.StartDeathSpray)
		{
			return true;
		}
		if (method == MethodName.EndDeathSpray)
		{
			return true;
		}
		if (method == MethodName.DeathExplode)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._tailShaderMat)
		{
			_tailShaderMat = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._sprayParticles)
		{
			_sprayParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._deathSprayParticles)
		{
			_deathSprayParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._deathSprayParticlesBack)
		{
			_deathSprayParticlesBack = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._deathExplosionParticles)
		{
			_deathExplosionParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
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
		if (name == PropertyName._tailShaderMat)
		{
			value = VariantUtils.CreateFrom(in _tailShaderMat);
			return true;
		}
		if (name == PropertyName._sprayParticles)
		{
			value = VariantUtils.CreateFrom(in _sprayParticles);
			return true;
		}
		if (name == PropertyName._deathSprayParticles)
		{
			value = VariantUtils.CreateFrom(in _deathSprayParticles);
			return true;
		}
		if (name == PropertyName._deathSprayParticlesBack)
		{
			value = VariantUtils.CreateFrom(in _deathSprayParticlesBack);
			return true;
		}
		if (name == PropertyName._deathExplosionParticles)
		{
			value = VariantUtils.CreateFrom(in _deathExplosionParticles);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tailShaderMat, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sprayParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deathSprayParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deathSprayParticlesBack, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deathExplosionParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tailShaderMat, Variant.From(in _tailShaderMat));
		info.AddProperty(PropertyName._sprayParticles, Variant.From(in _sprayParticles));
		info.AddProperty(PropertyName._deathSprayParticles, Variant.From(in _deathSprayParticles));
		info.AddProperty(PropertyName._deathSprayParticlesBack, Variant.From(in _deathSprayParticlesBack));
		info.AddProperty(PropertyName._deathExplosionParticles, Variant.From(in _deathExplosionParticles));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tailShaderMat, out var value))
		{
			_tailShaderMat = value.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._sprayParticles, out var value2))
		{
			_sprayParticles = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._deathSprayParticles, out var value3))
		{
			_deathSprayParticles = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._deathSprayParticlesBack, out var value4))
		{
			_deathSprayParticlesBack = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._deathExplosionParticles, out var value5))
		{
			_deathExplosionParticles = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value6))
		{
			_parent = value6.As<Node2D>();
		}
	}
}
