using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NTheInsatiableVfx.cs")]
public class NTheInsatiableVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName TurnOnSaliva = "TurnOnSaliva";

		public static readonly StringName TurnOffSaliva = "TurnOffSaliva";

		public static readonly StringName TurnOnDrool = "TurnOnDrool";

		public static readonly StringName TurnOffDrool = "TurnOffDrool";

		public static readonly StringName TurnOnBaseBlast = "TurnOnBaseBlast";

		public static readonly StringName TurnOffBaseBlast = "TurnOffBaseBlast";

		public static readonly StringName TurnOffContinuousParticles = "TurnOffContinuousParticles";

		public static readonly StringName OnAnimationStart = "OnAnimationStart";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _continuousParticles = "_continuousParticles";

		public static readonly StringName _salivaFountainParticles = "_salivaFountainParticles";

		public static readonly StringName _salivaDroolParticles = "_salivaDroolParticles";

		public static readonly StringName _salivaCloudParticles = "_salivaCloudParticles";

		public static readonly StringName _baseBlastParticles = "_baseBlastParticles";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private CpuParticles2D[] _continuousParticles;

	private CpuParticles2D _salivaFountainParticles;

	private CpuParticles2D _salivaDroolParticles;

	private CpuParticles2D _salivaCloudParticles;

	private GpuParticles2D _baseBlastParticles;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_animController.ConnectAnimationStarted(Callable.From<GodotObject, GodotObject, GodotObject>(OnAnimationStart));
		_salivaFountainParticles = _parent.GetNode<CpuParticles2D>("SalivaSlotNode/SalivaFountainParticles");
		_salivaDroolParticles = _parent.GetNode<CpuParticles2D>("SalivaSlotNode/SalivaDroolParticles");
		_salivaCloudParticles = _parent.GetNode<CpuParticles2D>("SalivaSlotNode/SalivaCloudParticles");
		_baseBlastParticles = _parent.GetNode<GpuParticles2D>("BaseBlastSlot/BaseBlastParticles");
		_salivaFountainParticles.Emitting = false;
		_salivaDroolParticles.Emitting = false;
		_salivaCloudParticles.Emitting = false;
		_baseBlastParticles.Emitting = false;
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		string eventName = new MegaEvent(spineEvent).GetData().GetEventName();
		if (eventName == null)
		{
			return;
		}
		switch (eventName.Length)
		{
		case 9:
			switch (eventName[1])
			{
			case 'r':
				if (eventName == "drool_end")
				{
					TurnOffDrool();
				}
				break;
			case 'e':
				if (eventName == "death_end")
				{
					TurnOffContinuousParticles();
				}
				break;
			}
			break;
		case 12:
			if (eventName == "saliva_start")
			{
				TurnOnSaliva();
			}
			break;
		case 10:
			if (eventName == "saliva_end")
			{
				TurnOffSaliva();
			}
			break;
		case 11:
			if (eventName == "drool_start")
			{
				TurnOnDrool();
			}
			break;
		case 16:
			if (eventName == "base_blast_start")
			{
				TurnOnBaseBlast();
			}
			break;
		case 14:
			if (eventName == "base_blast_end")
			{
				TurnOffBaseBlast();
			}
			break;
		case 13:
		case 15:
			break;
		}
	}

	private void TurnOnSaliva()
	{
		_salivaFountainParticles.Restart();
		_salivaCloudParticles.Restart();
	}

	private void TurnOffSaliva()
	{
		_salivaFountainParticles.Emitting = false;
		_salivaCloudParticles.Emitting = false;
	}

	private void TurnOnDrool()
	{
		_salivaDroolParticles.Restart();
	}

	private void TurnOffDrool()
	{
		_salivaDroolParticles.Emitting = false;
	}

	private void TurnOnBaseBlast()
	{
		_baseBlastParticles.Emitting = true;
	}

	private void TurnOffBaseBlast()
	{
		_baseBlastParticles.Emitting = false;
	}

	private void TurnOffContinuousParticles()
	{
		CpuParticles2D[] continuousParticles = _continuousParticles;
		foreach (CpuParticles2D cpuParticles2D in continuousParticles)
		{
			cpuParticles2D.Emitting = false;
		}
	}

	private void OnAnimationStart(GodotObject spineSprite, GodotObject animationState, GodotObject trackEntry)
	{
		if (new MegaAnimationState(animationState).GetCurrent(0).GetAnimation().GetName() != "attack_thrash")
		{
			TurnOffBaseBlast();
		}
		if (new MegaAnimationState(animationState).GetCurrent(0).GetAnimation().GetName() != "salivate")
		{
			TurnOffSaliva();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.TurnOnSaliva, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffSaliva, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOnDrool, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffDrool, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOnBaseBlast, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffBaseBlast, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffContinuousParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "spineSprite", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "animationState", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "trackEntry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
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
		if (method == MethodName.OnAnimationEvent && args.Count == 4)
		{
			OnAnimationEvent(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]), VariantUtils.ConvertTo<GodotObject>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOnSaliva && args.Count == 0)
		{
			TurnOnSaliva();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOffSaliva && args.Count == 0)
		{
			TurnOffSaliva();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOnDrool && args.Count == 0)
		{
			TurnOnDrool();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOffDrool && args.Count == 0)
		{
			TurnOffDrool();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOnBaseBlast && args.Count == 0)
		{
			TurnOnBaseBlast();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOffBaseBlast && args.Count == 0)
		{
			TurnOffBaseBlast();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOffContinuousParticles && args.Count == 0)
		{
			TurnOffContinuousParticles();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAnimationStart && args.Count == 3)
		{
			OnAnimationStart(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]));
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
		if (method == MethodName.TurnOnSaliva)
		{
			return true;
		}
		if (method == MethodName.TurnOffSaliva)
		{
			return true;
		}
		if (method == MethodName.TurnOnDrool)
		{
			return true;
		}
		if (method == MethodName.TurnOffDrool)
		{
			return true;
		}
		if (method == MethodName.TurnOnBaseBlast)
		{
			return true;
		}
		if (method == MethodName.TurnOffBaseBlast)
		{
			return true;
		}
		if (method == MethodName.TurnOffContinuousParticles)
		{
			return true;
		}
		if (method == MethodName.OnAnimationStart)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._continuousParticles)
		{
			_continuousParticles = VariantUtils.ConvertToSystemArrayOfGodotObject<CpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._salivaFountainParticles)
		{
			_salivaFountainParticles = VariantUtils.ConvertTo<CpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._salivaDroolParticles)
		{
			_salivaDroolParticles = VariantUtils.ConvertTo<CpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._salivaCloudParticles)
		{
			_salivaCloudParticles = VariantUtils.ConvertTo<CpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._baseBlastParticles)
		{
			_baseBlastParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
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
		if (name == PropertyName._continuousParticles)
		{
			GodotObject[] continuousParticles = _continuousParticles;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(continuousParticles);
			return true;
		}
		if (name == PropertyName._salivaFountainParticles)
		{
			value = VariantUtils.CreateFrom(in _salivaFountainParticles);
			return true;
		}
		if (name == PropertyName._salivaDroolParticles)
		{
			value = VariantUtils.CreateFrom(in _salivaDroolParticles);
			return true;
		}
		if (name == PropertyName._salivaCloudParticles)
		{
			value = VariantUtils.CreateFrom(in _salivaCloudParticles);
			return true;
		}
		if (name == PropertyName._baseBlastParticles)
		{
			value = VariantUtils.CreateFrom(in _baseBlastParticles);
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
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._continuousParticles, PropertyHint.TypeString, "24/34:CPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._salivaFountainParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._salivaDroolParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._salivaCloudParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._baseBlastParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		StringName continuousParticles = PropertyName._continuousParticles;
		GodotObject[] continuousParticles2 = _continuousParticles;
		info.AddProperty(continuousParticles, Variant.CreateFrom(continuousParticles2));
		info.AddProperty(PropertyName._salivaFountainParticles, Variant.From(in _salivaFountainParticles));
		info.AddProperty(PropertyName._salivaDroolParticles, Variant.From(in _salivaDroolParticles));
		info.AddProperty(PropertyName._salivaCloudParticles, Variant.From(in _salivaCloudParticles));
		info.AddProperty(PropertyName._baseBlastParticles, Variant.From(in _baseBlastParticles));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._continuousParticles, out var value))
		{
			_continuousParticles = value.AsGodotObjectArray<CpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._salivaFountainParticles, out var value2))
		{
			_salivaFountainParticles = value2.As<CpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._salivaDroolParticles, out var value3))
		{
			_salivaDroolParticles = value3.As<CpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._salivaCloudParticles, out var value4))
		{
			_salivaCloudParticles = value4.As<CpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._baseBlastParticles, out var value5))
		{
			_baseBlastParticles = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value6))
		{
			_parent = value6.As<Node2D>();
		}
	}
}
