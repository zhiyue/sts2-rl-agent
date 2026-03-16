using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NSewerClamVfx.cs")]
public class NSewerClamVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ScaleCoralTo = "ScaleCoralTo";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName OnDeathStart = "OnDeathStart";

		public static readonly StringName OnDeathEnd = "OnDeathEnd";

		public static readonly StringName OnDarknessStart = "OnDarknessStart";

		public static readonly StringName OnDarknessEnd = "OnDarknessEnd";

		public static readonly StringName OnChomp = "OnChomp";

		public static readonly StringName OnGrow = "OnGrow";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _deathParticles = "_deathParticles";

		public static readonly StringName _buffParticles = "_buffParticles";

		public static readonly StringName _chompParticles = "_chompParticles";

		public static readonly StringName _scaleNode = "_scaleNode";

		public static readonly StringName _keyDown = "_keyDown";

		public static readonly StringName _onState = "_onState";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private const float _coralScaleAmount = 0.2f;

	private const float _maxCoralScale = 1.5f;

	private const float _coralTweenDelay = 0.5f;

	private MegaSprite _megaSprite;

	private GpuParticles2D _deathParticles;

	private GpuParticles2D _buffParticles;

	private GpuParticles2D _chompParticles;

	private Node2D _scaleNode;

	private bool _keyDown;

	private bool _onState;

	public override void _Ready()
	{
		_megaSprite = new MegaSprite(GetParent<Node2D>());
		_megaSprite.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_deathParticles = GetParent().GetNode<GpuParticles2D>("MouthSlot/DeathParticles");
		_buffParticles = GetParent().GetNode<GpuParticles2D>("MouthSlot/BuffParticles");
		_chompParticles = GetParent().GetNode<GpuParticles2D>("MouthSlot/ChompParticles");
		_scaleNode = GetParent().GetNode<Node2D>("CoralScaleBone");
		_deathParticles.Emitting = false;
		_deathParticles.OneShot = true;
		_buffParticles.Emitting = false;
		_chompParticles.OneShot = true;
		_chompParticles.Emitting = false;
		_scaleNode.Scale = new Vector2(0.1f, 0.1f);
	}

	private void ScaleCoralTo(float targetScale)
	{
		Vector2 vector = new Vector2(targetScale - 0.2f, targetScale - 0.2f);
		Vector2 vector2 = new Vector2(targetScale, targetScale);
		Tween tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
		tween.TweenProperty(_scaleNode, "scale", vector2, 0.5).From(vector).SetDelay(0.5);
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "death_explode":
			OnDeathStart();
			break;
		case "darkness_start":
			OnDarknessStart();
			break;
		case "darkness_end":
			OnDarknessEnd();
			break;
		case "chomp":
			OnChomp();
			break;
		case "grow":
			OnGrow();
			break;
		}
	}

	private void OnDeathStart()
	{
		_deathParticles.Restart();
	}

	private void OnDeathEnd()
	{
		_deathParticles.Emitting = false;
	}

	private void OnDarknessStart()
	{
		_buffParticles.Restart();
	}

	private void OnDarknessEnd()
	{
		_buffParticles.Emitting = false;
	}

	private void OnChomp()
	{
		_chompParticles.Restart();
	}

	private void OnGrow()
	{
		float num = _scaleNode.Scale.X + 0.2f;
		if (num >= 1.5f)
		{
			num = 1.5f;
		}
		ScaleCoralTo(num);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ScaleCoralTo, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "targetScale", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnDeathStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDeathEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDarknessStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDarknessEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnChomp, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnGrow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ScaleCoralTo && args.Count == 1)
		{
			ScaleCoralTo(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAnimationEvent && args.Count == 4)
		{
			OnAnimationEvent(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]), VariantUtils.ConvertTo<GodotObject>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDeathStart && args.Count == 0)
		{
			OnDeathStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDeathEnd && args.Count == 0)
		{
			OnDeathEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDarknessStart && args.Count == 0)
		{
			OnDarknessStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDarknessEnd && args.Count == 0)
		{
			OnDarknessEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnChomp && args.Count == 0)
		{
			OnChomp();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnGrow && args.Count == 0)
		{
			OnGrow();
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
		if (method == MethodName.ScaleCoralTo)
		{
			return true;
		}
		if (method == MethodName.OnAnimationEvent)
		{
			return true;
		}
		if (method == MethodName.OnDeathStart)
		{
			return true;
		}
		if (method == MethodName.OnDeathEnd)
		{
			return true;
		}
		if (method == MethodName.OnDarknessStart)
		{
			return true;
		}
		if (method == MethodName.OnDarknessEnd)
		{
			return true;
		}
		if (method == MethodName.OnChomp)
		{
			return true;
		}
		if (method == MethodName.OnGrow)
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
		if (name == PropertyName._buffParticles)
		{
			_buffParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._chompParticles)
		{
			_chompParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._scaleNode)
		{
			_scaleNode = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._keyDown)
		{
			_keyDown = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._onState)
		{
			_onState = VariantUtils.ConvertTo<bool>(in value);
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
		if (name == PropertyName._buffParticles)
		{
			value = VariantUtils.CreateFrom(in _buffParticles);
			return true;
		}
		if (name == PropertyName._chompParticles)
		{
			value = VariantUtils.CreateFrom(in _chompParticles);
			return true;
		}
		if (name == PropertyName._scaleNode)
		{
			value = VariantUtils.CreateFrom(in _scaleNode);
			return true;
		}
		if (name == PropertyName._keyDown)
		{
			value = VariantUtils.CreateFrom(in _keyDown);
			return true;
		}
		if (name == PropertyName._onState)
		{
			value = VariantUtils.CreateFrom(in _onState);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deathParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._buffParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._chompParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scaleNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._keyDown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._onState, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._deathParticles, Variant.From(in _deathParticles));
		info.AddProperty(PropertyName._buffParticles, Variant.From(in _buffParticles));
		info.AddProperty(PropertyName._chompParticles, Variant.From(in _chompParticles));
		info.AddProperty(PropertyName._scaleNode, Variant.From(in _scaleNode));
		info.AddProperty(PropertyName._keyDown, Variant.From(in _keyDown));
		info.AddProperty(PropertyName._onState, Variant.From(in _onState));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._deathParticles, out var value))
		{
			_deathParticles = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._buffParticles, out var value2))
		{
			_buffParticles = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._chompParticles, out var value3))
		{
			_chompParticles = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._scaleNode, out var value4))
		{
			_scaleNode = value4.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._keyDown, out var value5))
		{
			_keyDown = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._onState, out var value6))
		{
			_onState = value6.As<bool>();
		}
	}
}
