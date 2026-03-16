using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NSoulFyshVfx.cs")]
public class NSoulFyshVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName StartSoundwave = "StartSoundwave";

		public static readonly StringName EndSoundwave = "EndSoundwave";

		public static readonly StringName StartBeckon = "StartBeckon";

		public static readonly StringName EndBeckon = "EndBeckon";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _soundShaderMat = "_soundShaderMat";

		public static readonly StringName _beckonShaderMat = "_beckonShaderMat";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private static readonly StringName _amount = new StringName("amount");

	private ShaderMaterial? _soundShaderMat;

	private MegaSlotNode _soundSlotNode;

	private ShaderMaterial? _beckonShaderMat;

	private MegaSlotNode _beckonSlotNode;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_soundSlotNode = new MegaSlotNode(_parent.GetNode("Soundwave"));
		_soundShaderMat = _soundSlotNode.GetNormalMaterial() as ShaderMaterial;
		_soundShaderMat?.SetShaderParameter(_amount, 0.3f);
		_beckonSlotNode = new MegaSlotNode(_parent.GetNode("Beckonwave"));
		_beckonShaderMat = _beckonSlotNode.GetNormalMaterial() as ShaderMaterial;
		_beckonShaderMat?.SetShaderParameter(_amount, 0.3f);
		_animController.GetAnimationState().SetAnimation("attack_debuff");
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "soundwave_start":
			StartSoundwave();
			break;
		case "soundwave_end":
			EndSoundwave();
			break;
		case "beckon_start":
			StartBeckon();
			break;
		case "beckon_end":
			EndBeckon();
			break;
		}
	}

	private void StartSoundwave()
	{
		Tween tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out);
		tween.SetTrans(Tween.TransitionType.Quad);
		tween.TweenProperty(_soundShaderMat, "shader_parameter/amount", 1f, 0.44999998807907104);
	}

	private void EndSoundwave()
	{
		Tween tween = CreateTween();
		tween.SetEase(Tween.EaseType.In);
		tween.SetTrans(Tween.TransitionType.Quad);
		tween.TweenProperty(_soundShaderMat, "shader_parameter/amount", 0.3f, 0.5);
	}

	private void StartBeckon()
	{
		Tween tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out);
		tween.SetTrans(Tween.TransitionType.Quad);
		tween.TweenProperty(_beckonShaderMat, "shader_parameter/amount", 1f, 0.25);
	}

	private void EndBeckon()
	{
		Tween tween = CreateTween();
		tween.SetEase(Tween.EaseType.In);
		tween.SetTrans(Tween.TransitionType.Quad);
		tween.TweenProperty(_beckonShaderMat, "shader_parameter/amount", 0.3f, 0.5);
	}

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
		list.Add(new MethodInfo(MethodName.StartSoundwave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndSoundwave, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartBeckon, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndBeckon, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.StartSoundwave && args.Count == 0)
		{
			StartSoundwave();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndSoundwave && args.Count == 0)
		{
			EndSoundwave();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartBeckon && args.Count == 0)
		{
			StartBeckon();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndBeckon && args.Count == 0)
		{
			EndBeckon();
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
		if (method == MethodName.StartSoundwave)
		{
			return true;
		}
		if (method == MethodName.EndSoundwave)
		{
			return true;
		}
		if (method == MethodName.StartBeckon)
		{
			return true;
		}
		if (method == MethodName.EndBeckon)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._soundShaderMat)
		{
			_soundShaderMat = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._beckonShaderMat)
		{
			_beckonShaderMat = VariantUtils.ConvertTo<ShaderMaterial>(in value);
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
		if (name == PropertyName._soundShaderMat)
		{
			value = VariantUtils.CreateFrom(in _soundShaderMat);
			return true;
		}
		if (name == PropertyName._beckonShaderMat)
		{
			value = VariantUtils.CreateFrom(in _beckonShaderMat);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._soundShaderMat, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._beckonShaderMat, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._soundShaderMat, Variant.From(in _soundShaderMat));
		info.AddProperty(PropertyName._beckonShaderMat, Variant.From(in _beckonShaderMat));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._soundShaderMat, out var value))
		{
			_soundShaderMat = value.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._beckonShaderMat, out var value2))
		{
			_beckonShaderMat = value2.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value3))
		{
			_parent = value3.As<Node2D>();
		}
	}
}
