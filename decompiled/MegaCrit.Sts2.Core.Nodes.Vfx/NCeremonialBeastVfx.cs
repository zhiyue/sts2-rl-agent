using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NCeremonialBeastVfx.cs")]
public class NCeremonialBeastVfx : Node, IDeathDelayer
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName TurnOnDeathParticles = "TurnOnDeathParticles";

		public static readonly StringName TurnOnEnergyParticles = "TurnOnEnergyParticles";

		public static readonly StringName TurnOffEnergyParticles = "TurnOffEnergyParticles";

		public static readonly StringName OnPlowStart = "OnPlowStart";

		public static readonly StringName OnPlowEnd = "OnPlowEnd";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _deathParticles = "_deathParticles";

		public static readonly StringName _energyParticlesFront = "_energyParticlesFront";

		public static readonly StringName _energyParticlesBack = "_energyParticlesBack";

		public static readonly StringName _plowStartTarget = "_plowStartTarget";

		public static readonly StringName _plowEndTarget = "_plowEndTarget";

		public static readonly StringName _parent = "_parent";

		public static readonly StringName _globalPlowTarget = "_globalPlowTarget";

		public static readonly StringName _globalPlowEndTarget = "_globalPlowEndTarget";
	}

	public new class SignalName : Node.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private GpuParticles2D _deathParticles;

	[Export(PropertyHint.None, "")]
	private CpuParticles2D _energyParticlesFront;

	[Export(PropertyHint.None, "")]
	private CpuParticles2D _energyParticlesBack;

	[Export(PropertyHint.None, "")]
	private Node2D _plowStartTarget;

	[Export(PropertyHint.None, "")]
	private Node2D _plowEndTarget;

	private Node2D _parent;

	private MegaSprite _animController;

	private Vector2 _globalPlowTarget;

	private Vector2 _globalPlowEndTarget;

	private readonly TaskCompletionSource _deathTask = new TaskCompletionSource();

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_deathParticles.OneShot = true;
		_deathParticles.Emitting = false;
		_energyParticlesBack.Emitting = true;
		_energyParticlesFront.Emitting = true;
		_globalPlowTarget = _plowStartTarget.GlobalPosition;
		_globalPlowEndTarget = _plowEndTarget.GlobalPosition;
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "turnOffEnergy":
			TurnOffEnergyParticles();
			break;
		case "turnOnEnergy":
			TurnOnEnergyParticles();
			break;
		case "deathParticles":
			TurnOnDeathParticles();
			break;
		case "plowStart":
			OnPlowStart();
			break;
		case "plowEnd":
			OnPlowEnd();
			break;
		}
	}

	public Task GetDelayTask()
	{
		return _deathTask.Task;
	}

	private void TurnOnDeathParticles()
	{
		_deathParticles.Restart();
		TaskHelper.RunSafely(FinishTaskWhenDeathParticlesFinished());
	}

	private async Task FinishTaskWhenDeathParticlesFinished()
	{
		await ToSignal(_deathParticles, CpuParticles2D.SignalName.Finished);
		_deathTask.SetResult();
	}

	private void TurnOnEnergyParticles()
	{
		_energyParticlesFront.Emitting = true;
		_energyParticlesBack.Emitting = true;
	}

	private void TurnOffEnergyParticles()
	{
		_energyParticlesFront.Emitting = false;
		_energyParticlesBack.Emitting = false;
	}

	private void OnPlowStart()
	{
		_plowStartTarget.GlobalPosition = _globalPlowTarget;
	}

	private void OnPlowEnd()
	{
		_plowEndTarget.GlobalPosition = _globalPlowEndTarget;
	}

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
		list.Add(new MethodInfo(MethodName.TurnOnDeathParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOnEnergyParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffEnergyParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPlowStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPlowEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.TurnOnDeathParticles && args.Count == 0)
		{
			TurnOnDeathParticles();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOnEnergyParticles && args.Count == 0)
		{
			TurnOnEnergyParticles();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOffEnergyParticles && args.Count == 0)
		{
			TurnOffEnergyParticles();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPlowStart && args.Count == 0)
		{
			OnPlowStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPlowEnd && args.Count == 0)
		{
			OnPlowEnd();
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
		if (method == MethodName.TurnOnDeathParticles)
		{
			return true;
		}
		if (method == MethodName.TurnOnEnergyParticles)
		{
			return true;
		}
		if (method == MethodName.TurnOffEnergyParticles)
		{
			return true;
		}
		if (method == MethodName.OnPlowStart)
		{
			return true;
		}
		if (method == MethodName.OnPlowEnd)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._deathParticles)
		{
			_deathParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._energyParticlesFront)
		{
			_energyParticlesFront = VariantUtils.ConvertTo<CpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._energyParticlesBack)
		{
			_energyParticlesBack = VariantUtils.ConvertTo<CpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._plowStartTarget)
		{
			_plowStartTarget = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._plowEndTarget)
		{
			_plowEndTarget = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._globalPlowTarget)
		{
			_globalPlowTarget = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._globalPlowEndTarget)
		{
			_globalPlowEndTarget = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._deathParticles)
		{
			value = VariantUtils.CreateFrom(in _deathParticles);
			return true;
		}
		if (name == PropertyName._energyParticlesFront)
		{
			value = VariantUtils.CreateFrom(in _energyParticlesFront);
			return true;
		}
		if (name == PropertyName._energyParticlesBack)
		{
			value = VariantUtils.CreateFrom(in _energyParticlesBack);
			return true;
		}
		if (name == PropertyName._plowStartTarget)
		{
			value = VariantUtils.CreateFrom(in _plowStartTarget);
			return true;
		}
		if (name == PropertyName._plowEndTarget)
		{
			value = VariantUtils.CreateFrom(in _plowEndTarget);
			return true;
		}
		if (name == PropertyName._parent)
		{
			value = VariantUtils.CreateFrom(in _parent);
			return true;
		}
		if (name == PropertyName._globalPlowTarget)
		{
			value = VariantUtils.CreateFrom(in _globalPlowTarget);
			return true;
		}
		if (name == PropertyName._globalPlowEndTarget)
		{
			value = VariantUtils.CreateFrom(in _globalPlowEndTarget);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deathParticles, PropertyHint.NodeType, "GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._energyParticlesFront, PropertyHint.NodeType, "CPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._energyParticlesBack, PropertyHint.NodeType, "CPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._plowStartTarget, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._plowEndTarget, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._globalPlowTarget, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._globalPlowEndTarget, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._deathParticles, Variant.From(in _deathParticles));
		info.AddProperty(PropertyName._energyParticlesFront, Variant.From(in _energyParticlesFront));
		info.AddProperty(PropertyName._energyParticlesBack, Variant.From(in _energyParticlesBack));
		info.AddProperty(PropertyName._plowStartTarget, Variant.From(in _plowStartTarget));
		info.AddProperty(PropertyName._plowEndTarget, Variant.From(in _plowEndTarget));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
		info.AddProperty(PropertyName._globalPlowTarget, Variant.From(in _globalPlowTarget));
		info.AddProperty(PropertyName._globalPlowEndTarget, Variant.From(in _globalPlowEndTarget));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._deathParticles, out var value))
		{
			_deathParticles = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._energyParticlesFront, out var value2))
		{
			_energyParticlesFront = value2.As<CpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._energyParticlesBack, out var value3))
		{
			_energyParticlesBack = value3.As<CpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._plowStartTarget, out var value4))
		{
			_plowStartTarget = value4.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._plowEndTarget, out var value5))
		{
			_plowEndTarget = value5.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value6))
		{
			_parent = value6.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._globalPlowTarget, out var value7))
		{
			_globalPlowTarget = value7.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._globalPlowEndTarget, out var value8))
		{
			_globalPlowEndTarget = value8.As<Vector2>();
		}
	}
}
