using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NNecrobinderVfx.cs")]
public class NNecrobinderVfx : Node
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
		/// Cached name for the 'UpdateFlameVisibility' method.
		/// </summary>
		public static readonly StringName UpdateFlameVisibility = "UpdateFlameVisibility";

		/// <summary>
		/// Cached name for the 'OnScytheFlame1' method.
		/// </summary>
		public static readonly StringName OnScytheFlame1 = "OnScytheFlame1";

		/// <summary>
		/// Cached name for the 'OnScytheFlame2' method.
		/// </summary>
		public static readonly StringName OnScytheFlame2 = "OnScytheFlame2";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the '_parent' field.
		/// </summary>
		public static readonly StringName _parent = "_parent";

		/// <summary>
		/// Cached name for the '_headRef' field.
		/// </summary>
		public static readonly StringName _headRef = "_headRef";

		/// <summary>
		/// Cached name for the '_scytheFireParticles1' field.
		/// </summary>
		public static readonly StringName _scytheFireParticles1 = "_scytheFireParticles1";

		/// <summary>
		/// Cached name for the '_scytheFireParticles2' field.
		/// </summary>
		public static readonly StringName _scytheFireParticles2 = "_scytheFireParticles2";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	private Node2D _parent;

	private MegaSprite _animController;

	private Node2D _headRef;

	private GpuParticles2D? _scytheFireParticles1;

	private GpuParticles2D? _scytheFireParticles2;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_headRef = _parent.GetNode<Node2D>("HeadBoneNode");
		_scytheFireParticles1 = _parent.GetNodeOrNull<GpuParticles2D>("ScytheVfxSlot1/ScytheParticles");
		_scytheFireParticles2 = _parent.GetNodeOrNull<GpuParticles2D>("ScytheVfxSlot2/ScytheParticles");
		_scytheFireParticles1?.SetEmitting(emitting: false);
		_scytheFireParticles2?.SetEmitting(emitting: false);
		_scytheFireParticles1?.SetOneShot(secs: true);
		_scytheFireParticles2?.SetOneShot(secs: true);
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationStarted(Callable.From<GodotObject, GodotObject, GodotObject>(UpdateFlameVisibility));
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		string eventName = new MegaEvent(spineEvent).GetData().GetEventName();
		if (!(eventName == "scythe_fx1"))
		{
			if (eventName == "scythe_fx2")
			{
				OnScytheFlame2();
			}
		}
		else
		{
			OnScytheFlame1();
		}
	}

	private void UpdateFlameVisibility(GodotObject spineSprite, GodotObject animationState, GodotObject trackEntry)
	{
		_headRef.Visible = new MegaAnimationState(animationState).GetCurrentAnimationName() != "die";
	}

	private void OnScytheFlame1()
	{
		_scytheFireParticles1?.Restart();
	}

	private void OnScytheFlame2()
	{
		_scytheFireParticles2?.Restart();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
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
		list.Add(new MethodInfo(MethodName.UpdateFlameVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "spineSprite", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "animationState", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "trackEntry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnScytheFlame1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnScytheFlame2, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.UpdateFlameVisibility && args.Count == 3)
		{
			UpdateFlameVisibility(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnScytheFlame1 && args.Count == 0)
		{
			OnScytheFlame1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnScytheFlame2 && args.Count == 0)
		{
			OnScytheFlame2();
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
		if (method == MethodName.UpdateFlameVisibility)
		{
			return true;
		}
		if (method == MethodName.OnScytheFlame1)
		{
			return true;
		}
		if (method == MethodName.OnScytheFlame2)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._headRef)
		{
			_headRef = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._scytheFireParticles1)
		{
			_scytheFireParticles1 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._scytheFireParticles2)
		{
			_scytheFireParticles2 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._parent)
		{
			value = VariantUtils.CreateFrom(in _parent);
			return true;
		}
		if (name == PropertyName._headRef)
		{
			value = VariantUtils.CreateFrom(in _headRef);
			return true;
		}
		if (name == PropertyName._scytheFireParticles1)
		{
			value = VariantUtils.CreateFrom(in _scytheFireParticles1);
			return true;
		}
		if (name == PropertyName._scytheFireParticles2)
		{
			value = VariantUtils.CreateFrom(in _scytheFireParticles2);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._headRef, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scytheFireParticles1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scytheFireParticles2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
		info.AddProperty(PropertyName._headRef, Variant.From(in _headRef));
		info.AddProperty(PropertyName._scytheFireParticles1, Variant.From(in _scytheFireParticles1));
		info.AddProperty(PropertyName._scytheFireParticles2, Variant.From(in _scytheFireParticles2));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._parent, out var value))
		{
			_parent = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._headRef, out var value2))
		{
			_headRef = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._scytheFireParticles1, out var value3))
		{
			_scytheFireParticles1 = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._scytheFireParticles2, out var value4))
		{
			_scytheFireParticles2 = value4.As<GpuParticles2D>();
		}
	}
}
