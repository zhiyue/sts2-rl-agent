using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NGasBombVfx.cs")]
public class NGasBombVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName OnBurst = "OnBurst";

		public static readonly StringName OnIdleParticles = "OnIdleParticles";

		public static readonly StringName OnDissipate = "OnDissipate";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _explodePuffParticles = "_explodePuffParticles";

		public static readonly StringName _puffParticles = "_puffParticles";

		public static readonly StringName _dotParticles = "_dotParticles";

		public static readonly StringName _bitParticles = "_bitParticles";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private MegaSprite _megaSprite;

	private GpuParticles2D _explodePuffParticles;

	private GpuParticles2D _puffParticles;

	private GpuParticles2D _dotParticles;

	private GpuParticles2D _bitParticles;

	public override void _Ready()
	{
		_explodePuffParticles = GetNode<GpuParticles2D>("../SmokeBallSlot/ExplodePuffParticles");
		_puffParticles = GetNode<GpuParticles2D>("../SmokeBallSlot/PuffParticles");
		_dotParticles = GetNode<GpuParticles2D>("../SmokeBallSlot/DotParticles");
		_bitParticles = GetNode<GpuParticles2D>("../SmokeBallSlot/BitParticles");
		_megaSprite = new MegaSprite(GetParent<Node2D>());
		_megaSprite.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_explodePuffParticles.Emitting = false;
		_explodePuffParticles.OneShot = true;
		_bitParticles.Emitting = false;
		_bitParticles.OneShot = true;
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "burst":
			OnBurst();
			break;
		case "idle_particles":
			OnIdleParticles();
			break;
		case "dissipate":
			OnDissipate();
			break;
		}
	}

	private void OnBurst()
	{
		_dotParticles.Emitting = false;
		_puffParticles.Emitting = false;
		_puffParticles.SetVisible(visible: false);
		_explodePuffParticles.Restart();
		_bitParticles.Restart();
	}

	private void OnIdleParticles()
	{
		_dotParticles.Emitting = true;
		_puffParticles.Emitting = true;
		_puffParticles.SetVisible(visible: true);
	}

	private void OnDissipate()
	{
		_dotParticles.Emitting = false;
		_puffParticles.Emitting = false;
	}

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
		list.Add(new MethodInfo(MethodName.OnBurst, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnIdleParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDissipate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnBurst && args.Count == 0)
		{
			OnBurst();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnIdleParticles && args.Count == 0)
		{
			OnIdleParticles();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDissipate && args.Count == 0)
		{
			OnDissipate();
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
		if (method == MethodName.OnBurst)
		{
			return true;
		}
		if (method == MethodName.OnIdleParticles)
		{
			return true;
		}
		if (method == MethodName.OnDissipate)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._explodePuffParticles)
		{
			_explodePuffParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._puffParticles)
		{
			_puffParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._dotParticles)
		{
			_dotParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._bitParticles)
		{
			_bitParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._explodePuffParticles)
		{
			value = VariantUtils.CreateFrom(in _explodePuffParticles);
			return true;
		}
		if (name == PropertyName._puffParticles)
		{
			value = VariantUtils.CreateFrom(in _puffParticles);
			return true;
		}
		if (name == PropertyName._dotParticles)
		{
			value = VariantUtils.CreateFrom(in _dotParticles);
			return true;
		}
		if (name == PropertyName._bitParticles)
		{
			value = VariantUtils.CreateFrom(in _bitParticles);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._explodePuffParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._puffParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dotParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bitParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._explodePuffParticles, Variant.From(in _explodePuffParticles));
		info.AddProperty(PropertyName._puffParticles, Variant.From(in _puffParticles));
		info.AddProperty(PropertyName._dotParticles, Variant.From(in _dotParticles));
		info.AddProperty(PropertyName._bitParticles, Variant.From(in _bitParticles));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._explodePuffParticles, out var value))
		{
			_explodePuffParticles = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._puffParticles, out var value2))
		{
			_puffParticles = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._dotParticles, out var value3))
		{
			_dotParticles = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._bitParticles, out var value4))
		{
			_bitParticles = value4.As<GpuParticles2D>();
		}
	}
}
