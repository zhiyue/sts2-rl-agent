using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NKinFollowerVfx.cs")]
public class NKinFollowerVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName StartTrail1 = "StartTrail1";

		public static readonly StringName StartTrail2 = "StartTrail2";

		public static readonly StringName EndTrail1 = "EndTrail1";

		public static readonly StringName EndTrail2 = "EndTrail2";

		public static readonly StringName StartHay = "StartHay";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _trail1 = "_trail1";

		public static readonly StringName _trail2 = "_trail2";

		public static readonly StringName _hay = "_hay";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private NBasicTrail _trail1;

	private NBasicTrail _trail2;

	private GpuParticles2D _hay;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_trail1 = _parent.GetNode<NBasicTrail>("Boomerang1Slot/Trail");
		_trail2 = _parent.GetNode<NBasicTrail>("Boomerang2Slot/Trail");
		_hay = _parent.GetNode<GpuParticles2D>("HaySlot/HayParticles");
		_trail1.Visible = false;
		_trail2.Visible = false;
		_hay.Emitting = false;
		_hay.OneShot = true;
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "start_trail1":
			StartTrail1();
			break;
		case "end_trail1":
			EndTrail1();
			break;
		case "start_trail2":
			StartTrail2();
			break;
		case "end_trail2":
			EndTrail2();
			break;
		case "start_hay":
			StartHay();
			break;
		}
	}

	private void StartTrail1()
	{
		_trail1.ClearPoints();
		_trail1.Visible = true;
	}

	private void StartTrail2()
	{
		_trail2.ClearPoints();
		_trail2.Visible = true;
	}

	private void EndTrail1()
	{
		_trail1.Visible = false;
	}

	private void EndTrail2()
	{
		_trail2.Visible = false;
	}

	private void StartHay()
	{
		_hay.Restart();
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
		list.Add(new MethodInfo(MethodName.StartTrail1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartTrail2, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndTrail1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndTrail2, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartHay, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.StartTrail1 && args.Count == 0)
		{
			StartTrail1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartTrail2 && args.Count == 0)
		{
			StartTrail2();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndTrail1 && args.Count == 0)
		{
			EndTrail1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndTrail2 && args.Count == 0)
		{
			EndTrail2();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartHay && args.Count == 0)
		{
			StartHay();
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
		if (method == MethodName.StartTrail1)
		{
			return true;
		}
		if (method == MethodName.StartTrail2)
		{
			return true;
		}
		if (method == MethodName.EndTrail1)
		{
			return true;
		}
		if (method == MethodName.EndTrail2)
		{
			return true;
		}
		if (method == MethodName.StartHay)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._trail1)
		{
			_trail1 = VariantUtils.ConvertTo<NBasicTrail>(in value);
			return true;
		}
		if (name == PropertyName._trail2)
		{
			_trail2 = VariantUtils.ConvertTo<NBasicTrail>(in value);
			return true;
		}
		if (name == PropertyName._hay)
		{
			_hay = VariantUtils.ConvertTo<GpuParticles2D>(in value);
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
		if (name == PropertyName._trail1)
		{
			value = VariantUtils.CreateFrom(in _trail1);
			return true;
		}
		if (name == PropertyName._trail2)
		{
			value = VariantUtils.CreateFrom(in _trail2);
			return true;
		}
		if (name == PropertyName._hay)
		{
			value = VariantUtils.CreateFrom(in _hay);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._trail1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._trail2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._trail1, Variant.From(in _trail1));
		info.AddProperty(PropertyName._trail2, Variant.From(in _trail2));
		info.AddProperty(PropertyName._hay, Variant.From(in _hay));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._trail1, out var value))
		{
			_trail1 = value.As<NBasicTrail>();
		}
		if (info.TryGetProperty(PropertyName._trail2, out var value2))
		{
			_trail2 = value2.As<NBasicTrail>();
		}
		if (info.TryGetProperty(PropertyName._hay, out var value3))
		{
			_hay = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value4))
		{
			_parent = value4.As<Node2D>();
		}
	}
}
