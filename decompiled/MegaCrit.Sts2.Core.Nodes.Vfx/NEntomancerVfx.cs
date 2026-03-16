using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NEntomancerVfx.cs")]
public class NEntomancerVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName LaunchSwarm = "LaunchSwarm";

		public static readonly StringName CompleteSwarmAttack = "CompleteSwarmAttack";

		public static readonly StringName CancelSwarmAttack = "CancelSwarmAttack";

		public static readonly StringName TurnOffSwarm = "TurnOffSwarm";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _swarmParticles = "_swarmParticles";

		public static readonly StringName _attackingBugParticles = "_attackingBugParticles";

		public static readonly StringName _swarmTargetNode = "_swarmTargetNode";

		public static readonly StringName _basePosition = "_basePosition";

		public static readonly StringName _swarmTween = "_swarmTween";

		public static readonly StringName _swarming = "_swarming";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private GpuParticles2D _swarmParticles;

	private GpuParticles2D _attackingBugParticles;

	private Node2D _swarmTargetNode;

	private Vector2 _basePosition;

	private Tween? _swarmTween;

	private bool _swarming;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_swarmParticles = _parent.GetNode<GpuParticles2D>("SwarmParticles");
		_attackingBugParticles = _parent.GetNode<GpuParticles2D>("SwarmParticles/AttackingBugParticles");
		_swarmTargetNode = _parent.GetNode<Node2D>("SwarmTargetNode");
		_basePosition = _swarmParticles.Position;
		_attackingBugParticles.Emitting = false;
	}

	private void LaunchSwarm()
	{
		_swarming = true;
		_swarmTween = CreateTween();
		_attackingBugParticles.Emitting = true;
		_swarmTween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_swarmTween.TweenProperty(_swarmParticles, "position", _swarmTargetNode.Position, 1.0);
		_swarmTween.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
		_swarmTween.TweenProperty(_swarmParticles, "position", _basePosition, 1.5).SetDelay(0.0);
		_swarmTween.TweenCallback(Callable.From(CompleteSwarmAttack)).SetDelay(0.009999999776482582);
	}

	private void CompleteSwarmAttack()
	{
		_attackingBugParticles.Emitting = false;
		_swarming = false;
	}

	private void CancelSwarmAttack()
	{
		if (_swarming)
		{
			_swarmTween.Kill();
			_swarmParticles.Position = _basePosition;
			CompleteSwarmAttack();
		}
	}

	private void TurnOffSwarm()
	{
		_swarmParticles.Emitting = false;
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		string eventName = new MegaEvent(spineEvent).GetData().GetEventName();
		if (!(eventName == "launch_swarm"))
		{
			if (eventName == "turn_off_swarm")
			{
				TurnOffSwarm();
			}
		}
		else
		{
			LaunchSwarm();
		}
	}

	public override void _ExitTree()
	{
		_swarmTween?.Kill();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.LaunchSwarm, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CompleteSwarmAttack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CancelSwarmAttack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffSwarm, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.LaunchSwarm && args.Count == 0)
		{
			LaunchSwarm();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CompleteSwarmAttack && args.Count == 0)
		{
			CompleteSwarmAttack();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CancelSwarmAttack && args.Count == 0)
		{
			CancelSwarmAttack();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOffSwarm && args.Count == 0)
		{
			TurnOffSwarm();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAnimationEvent && args.Count == 4)
		{
			OnAnimationEvent(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]), VariantUtils.ConvertTo<GodotObject>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName.LaunchSwarm)
		{
			return true;
		}
		if (method == MethodName.CompleteSwarmAttack)
		{
			return true;
		}
		if (method == MethodName.CancelSwarmAttack)
		{
			return true;
		}
		if (method == MethodName.TurnOffSwarm)
		{
			return true;
		}
		if (method == MethodName.OnAnimationEvent)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._swarmParticles)
		{
			_swarmParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._attackingBugParticles)
		{
			_attackingBugParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._swarmTargetNode)
		{
			_swarmTargetNode = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._basePosition)
		{
			_basePosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._swarmTween)
		{
			_swarmTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._swarming)
		{
			_swarming = VariantUtils.ConvertTo<bool>(in value);
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
		if (name == PropertyName._swarmParticles)
		{
			value = VariantUtils.CreateFrom(in _swarmParticles);
			return true;
		}
		if (name == PropertyName._attackingBugParticles)
		{
			value = VariantUtils.CreateFrom(in _attackingBugParticles);
			return true;
		}
		if (name == PropertyName._swarmTargetNode)
		{
			value = VariantUtils.CreateFrom(in _swarmTargetNode);
			return true;
		}
		if (name == PropertyName._basePosition)
		{
			value = VariantUtils.CreateFrom(in _basePosition);
			return true;
		}
		if (name == PropertyName._swarmTween)
		{
			value = VariantUtils.CreateFrom(in _swarmTween);
			return true;
		}
		if (name == PropertyName._swarming)
		{
			value = VariantUtils.CreateFrom(in _swarming);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._swarmParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._attackingBugParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._swarmTargetNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._basePosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._swarmTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._swarming, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._swarmParticles, Variant.From(in _swarmParticles));
		info.AddProperty(PropertyName._attackingBugParticles, Variant.From(in _attackingBugParticles));
		info.AddProperty(PropertyName._swarmTargetNode, Variant.From(in _swarmTargetNode));
		info.AddProperty(PropertyName._basePosition, Variant.From(in _basePosition));
		info.AddProperty(PropertyName._swarmTween, Variant.From(in _swarmTween));
		info.AddProperty(PropertyName._swarming, Variant.From(in _swarming));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._swarmParticles, out var value))
		{
			_swarmParticles = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._attackingBugParticles, out var value2))
		{
			_attackingBugParticles = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._swarmTargetNode, out var value3))
		{
			_swarmTargetNode = value3.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._basePosition, out var value4))
		{
			_basePosition = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._swarmTween, out var value5))
		{
			_swarmTween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._swarming, out var value6))
		{
			_swarming = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value7))
		{
			_parent = value7.As<Node2D>();
		}
	}
}
