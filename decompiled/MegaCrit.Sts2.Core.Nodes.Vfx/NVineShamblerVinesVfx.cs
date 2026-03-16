using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NVineShamblerVinesVfx.cs")]
public class NVineShamblerVinesVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnFrontEvent = "OnFrontEvent";

		public static readonly StringName AnimationEnded = "AnimationEnded";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _frontVinesNode = "_frontVinesNode";

		public static readonly StringName _backVinesNode = "_backVinesNode";

		public static readonly StringName _dirtBlast1 = "_dirtBlast1";

		public static readonly StringName _dirtBlast2 = "_dirtBlast2";

		public static readonly StringName _dirtBlast3 = "_dirtBlast3";

		public static readonly StringName _dirtBlast4 = "_dirtBlast4";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private Node2D _frontVinesNode;

	private MegaSprite _frontVinesAnimController;

	private Node2D _backVinesNode;

	private MegaSprite _backVinesAnimController;

	private GpuParticles2D _dirtBlast1;

	private GpuParticles2D _dirtBlast2;

	private GpuParticles2D _dirtBlast3;

	private GpuParticles2D _dirtBlast4;

	public override void _Ready()
	{
		_frontVinesNode = GetNode<Node2D>("VinesFront");
		_frontVinesAnimController = new MegaSprite(_frontVinesNode);
		_frontVinesAnimController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnFrontEvent));
		_frontVinesAnimController.ConnectAnimationCompleted(Callable.From<GodotObject, GodotObject, GodotObject>(AnimationEnded));
		_backVinesNode = GetNode<Node2D>("VinesBackScene/VinesBack");
		_backVinesAnimController = new MegaSprite(_backVinesNode);
		_dirtBlast1 = GetNode<GpuParticles2D>("DirtBlast1");
		_dirtBlast3 = GetNode<GpuParticles2D>("DirtBlast3");
		_dirtBlast2 = GetNode<GpuParticles2D>("VinesBackScene/DirtBlast2");
		_dirtBlast4 = GetNode<GpuParticles2D>("VinesBackScene/DirtBlast4");
		_dirtBlast1.Emitting = false;
		_dirtBlast1.OneShot = true;
		_dirtBlast2.Emitting = false;
		_dirtBlast2.OneShot = true;
		_dirtBlast3.Emitting = false;
		_dirtBlast3.OneShot = true;
		_dirtBlast4.Emitting = false;
		_dirtBlast4.OneShot = true;
		Vector2 backVineOffset = _backVinesNode.GlobalPosition - _frontVinesNode.GlobalPosition;
		_backVinesNode.Reparent(NCombatRoom.Instance.BackCombatVfxContainer);
		Callable.From(delegate
		{
			_backVinesNode.GlobalPosition = _frontVinesNode.GlobalPosition + backVineOffset;
		}).CallDeferred();
		_frontVinesAnimController.GetAnimationState().SetAnimation("animation");
		_backVinesAnimController.GetAnimationState().SetAnimation("animation");
	}

	private void OnFrontEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "dirt_1":
			_dirtBlast1.Restart();
			break;
		case "dirt_2":
			_dirtBlast2.Restart();
			break;
		case "dirt_3":
			_dirtBlast3.Restart();
			break;
		case "dirt_4":
			_dirtBlast4.Restart();
			break;
		}
	}

	private void AnimationEnded(GodotObject _, GodotObject __, GodotObject ___)
	{
		this.QueueFreeSafely();
		_backVinesNode.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFrontEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimationEnded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
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
		if (method == MethodName.OnFrontEvent && args.Count == 4)
		{
			OnFrontEvent(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]), VariantUtils.ConvertTo<GodotObject>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimationEnded && args.Count == 3)
		{
			AnimationEnded(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]));
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
		if (method == MethodName.OnFrontEvent)
		{
			return true;
		}
		if (method == MethodName.AnimationEnded)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._frontVinesNode)
		{
			_frontVinesNode = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._backVinesNode)
		{
			_backVinesNode = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._dirtBlast1)
		{
			_dirtBlast1 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._dirtBlast2)
		{
			_dirtBlast2 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._dirtBlast3)
		{
			_dirtBlast3 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._dirtBlast4)
		{
			_dirtBlast4 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._frontVinesNode)
		{
			value = VariantUtils.CreateFrom(in _frontVinesNode);
			return true;
		}
		if (name == PropertyName._backVinesNode)
		{
			value = VariantUtils.CreateFrom(in _backVinesNode);
			return true;
		}
		if (name == PropertyName._dirtBlast1)
		{
			value = VariantUtils.CreateFrom(in _dirtBlast1);
			return true;
		}
		if (name == PropertyName._dirtBlast2)
		{
			value = VariantUtils.CreateFrom(in _dirtBlast2);
			return true;
		}
		if (name == PropertyName._dirtBlast3)
		{
			value = VariantUtils.CreateFrom(in _dirtBlast3);
			return true;
		}
		if (name == PropertyName._dirtBlast4)
		{
			value = VariantUtils.CreateFrom(in _dirtBlast4);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._frontVinesNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backVinesNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dirtBlast1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dirtBlast2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dirtBlast3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dirtBlast4, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._frontVinesNode, Variant.From(in _frontVinesNode));
		info.AddProperty(PropertyName._backVinesNode, Variant.From(in _backVinesNode));
		info.AddProperty(PropertyName._dirtBlast1, Variant.From(in _dirtBlast1));
		info.AddProperty(PropertyName._dirtBlast2, Variant.From(in _dirtBlast2));
		info.AddProperty(PropertyName._dirtBlast3, Variant.From(in _dirtBlast3));
		info.AddProperty(PropertyName._dirtBlast4, Variant.From(in _dirtBlast4));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._frontVinesNode, out var value))
		{
			_frontVinesNode = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._backVinesNode, out var value2))
		{
			_backVinesNode = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._dirtBlast1, out var value3))
		{
			_dirtBlast1 = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._dirtBlast2, out var value4))
		{
			_dirtBlast2 = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._dirtBlast3, out var value5))
		{
			_dirtBlast3 = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._dirtBlast4, out var value6))
		{
			_dirtBlast4 = value6.As<GpuParticles2D>();
		}
	}
}
