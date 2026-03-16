using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NOilSpillVfx.cs")]
public class NOilSpillVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName OnAnimationStart = "OnAnimationStart";

		public static readonly StringName TurnOnSprayAttack = "TurnOnSprayAttack";

		public static readonly StringName TurnOffSprayAttack = "TurnOffSprayAttack";

		public static readonly StringName TurnOnSlamSpray = "TurnOnSlamSpray";

		public static readonly StringName TurnOffSlamSpray = "TurnOffSlamSpray";

		public static readonly StringName TurnOnDeathSpray = "TurnOnDeathSpray";

		public static readonly StringName TurnOffDeathSpray = "TurnOffDeathSpray";

		public static readonly StringName TurnOnDrool = "TurnOnDrool";

		public static readonly StringName TurnOffDrool = "TurnOffDrool";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _droolParticles = "_droolParticles";

		public static readonly StringName _sprayParticles = "_sprayParticles";

		public static readonly StringName _rainDropParticles = "_rainDropParticles";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private const int _slamSprayAmount = 800;

	private const int _sprayAttackAmount = 2000;

	private const int _deathSprayAmount = 500;

	private const float _slamSprayLifetime = 0.75f;

	private GpuParticles2D _droolParticles;

	private GpuParticles2D _sprayParticles;

	private GpuParticles2D _rainDropParticles;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_animController.ConnectAnimationStarted(Callable.From<GodotObject, GodotObject, GodotObject>(OnAnimationStart));
		_droolParticles = _parent.GetNode<GpuParticles2D>("MouthDribbleBoneNode/DribbleParticles");
		_sprayParticles = _parent.GetNode<GpuParticles2D>("MouthSpraySlot/SprayParticles");
		_rainDropParticles = _parent.GetNode<GpuParticles2D>("MouthSpraySlot/RainDropParticles");
		_rainDropParticles.OneShot = true;
		_droolParticles.Restart();
		_rainDropParticles.Restart();
		_sprayParticles.Restart();
		TurnOffSprayAttack();
		TurnOffSlamSpray();
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
		case 11:
			switch (eventName[0])
			{
			case 's':
				if (eventName == "spray_start")
				{
					TurnOnSprayAttack();
				}
				break;
			case 'd':
				if (eventName == "drool_start")
				{
					TurnOnDrool();
				}
				break;
			}
			break;
		case 9:
			switch (eventName[0])
			{
			case 's':
				if (eventName == "spray_end")
				{
					TurnOffSprayAttack();
				}
				break;
			case 'd':
				if (eventName == "drool_end")
				{
					TurnOffDrool();
				}
				break;
			}
			break;
		case 16:
			if (eventName == "slam_spray_start")
			{
				TurnOnSlamSpray();
			}
			break;
		case 14:
			if (eventName == "slam_spray_end")
			{
				TurnOffSlamSpray();
			}
			break;
		case 17:
			if (eventName == "death_spray_start")
			{
				TurnOnDeathSpray();
			}
			break;
		case 15:
			if (eventName == "death_spray_end")
			{
				TurnOffDeathSpray();
			}
			break;
		case 10:
		case 12:
		case 13:
			break;
		}
	}

	private void OnAnimationStart(GodotObject spineSprite, GodotObject animationState, GodotObject trackEntry)
	{
		string name = new MegaAnimationState(animationState).GetCurrent(0).GetAnimation().GetName();
		if (name != "slam")
		{
			TurnOffSprayAttack();
		}
		if (name != "spray")
		{
			TurnOffSlamSpray();
		}
	}

	private void TurnOnSprayAttack()
	{
		_sprayParticles.Amount = 2000;
		_sprayParticles.Emitting = true;
	}

	private void TurnOffSprayAttack()
	{
		_sprayParticles.Emitting = false;
	}

	private void TurnOnSlamSpray()
	{
		_rainDropParticles.OneShot = false;
		_rainDropParticles.Amount = 800;
		_rainDropParticles.Explosiveness = 0f;
		_rainDropParticles.Lifetime = 0.75;
		_rainDropParticles.Restart();
	}

	private void TurnOffSlamSpray()
	{
		_rainDropParticles.Emitting = false;
	}

	private void TurnOnDeathSpray()
	{
		_sprayParticles.Amount = 500;
		_sprayParticles.Emitting = true;
	}

	private void TurnOffDeathSpray()
	{
		_sprayParticles.Emitting = false;
	}

	private void TurnOnDrool()
	{
		_droolParticles.Emitting = true;
	}

	private void TurnOffDrool()
	{
		_droolParticles.Emitting = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
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
		list.Add(new MethodInfo(MethodName.TurnOnSprayAttack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffSprayAttack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOnSlamSpray, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffSlamSpray, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOnDeathSpray, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffDeathSpray, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOnDrool, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffDrool, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnAnimationStart && args.Count == 3)
		{
			OnAnimationStart(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOnSprayAttack && args.Count == 0)
		{
			TurnOnSprayAttack();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOffSprayAttack && args.Count == 0)
		{
			TurnOffSprayAttack();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOnSlamSpray && args.Count == 0)
		{
			TurnOnSlamSpray();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOffSlamSpray && args.Count == 0)
		{
			TurnOffSlamSpray();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOnDeathSpray && args.Count == 0)
		{
			TurnOnDeathSpray();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOffDeathSpray && args.Count == 0)
		{
			TurnOffDeathSpray();
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
		if (method == MethodName.OnAnimationStart)
		{
			return true;
		}
		if (method == MethodName.TurnOnSprayAttack)
		{
			return true;
		}
		if (method == MethodName.TurnOffSprayAttack)
		{
			return true;
		}
		if (method == MethodName.TurnOnSlamSpray)
		{
			return true;
		}
		if (method == MethodName.TurnOffSlamSpray)
		{
			return true;
		}
		if (method == MethodName.TurnOnDeathSpray)
		{
			return true;
		}
		if (method == MethodName.TurnOffDeathSpray)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._droolParticles)
		{
			_droolParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._sprayParticles)
		{
			_sprayParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._rainDropParticles)
		{
			_rainDropParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
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
		if (name == PropertyName._droolParticles)
		{
			value = VariantUtils.CreateFrom(in _droolParticles);
			return true;
		}
		if (name == PropertyName._sprayParticles)
		{
			value = VariantUtils.CreateFrom(in _sprayParticles);
			return true;
		}
		if (name == PropertyName._rainDropParticles)
		{
			value = VariantUtils.CreateFrom(in _rainDropParticles);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._droolParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sprayParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rainDropParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._droolParticles, Variant.From(in _droolParticles));
		info.AddProperty(PropertyName._sprayParticles, Variant.From(in _sprayParticles));
		info.AddProperty(PropertyName._rainDropParticles, Variant.From(in _rainDropParticles));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._droolParticles, out var value))
		{
			_droolParticles = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._sprayParticles, out var value2))
		{
			_sprayParticles = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._rainDropParticles, out var value3))
		{
			_rainDropParticles = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value4))
		{
			_parent = value4.As<Node2D>();
		}
	}
}
