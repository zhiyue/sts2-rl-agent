using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NFogmogVfx.cs")]
public class NFogmogVfx : Node
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
		/// Cached name for the 'StartThrust' method.
		/// </summary>
		public static readonly StringName StartThrust = "StartThrust";

		/// <summary>
		/// Cached name for the 'EndThrust' method.
		/// </summary>
		public static readonly StringName EndThrust = "EndThrust";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the '_thrustParicles' field.
		/// </summary>
		public static readonly StringName _thrustParicles = "_thrustParicles";

		/// <summary>
		/// Cached name for the '_dustLeftParticles' field.
		/// </summary>
		public static readonly StringName _dustLeftParticles = "_dustLeftParticles";

		/// <summary>
		/// Cached name for the '_dustRightParticles' field.
		/// </summary>
		public static readonly StringName _dustRightParticles = "_dustRightParticles";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	private GpuParticles2D _thrustParicles;

	private GpuParticles2D _dustLeftParticles;

	private GpuParticles2D _dustRightParticles;

	private MegaSprite _megaSprite;

	public override void _Ready()
	{
		_thrustParicles = GetNode<GpuParticles2D>("../ThrustSlotNode/ThrustParticles");
		_dustLeftParticles = GetNode<GpuParticles2D>("../DustSlotNode/DustLeftParticles");
		_dustRightParticles = GetNode<GpuParticles2D>("../DustSlotNode/DustRightParticles");
		_thrustParicles.Emitting = false;
		_dustLeftParticles.Emitting = false;
		_dustRightParticles.Emitting = false;
		_megaSprite = new MegaSprite(GetParent<Node2D>());
		_megaSprite.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		string eventName = new MegaEvent(spineEvent).GetData().GetEventName();
		if (!(eventName == "thrust_start"))
		{
			if (eventName == "thrust_end")
			{
				EndThrust();
			}
		}
		else
		{
			StartThrust();
		}
	}

	private void StartThrust()
	{
		_thrustParicles.Restart();
		_dustRightParticles.Restart();
		_dustLeftParticles.Restart();
	}

	private void EndThrust()
	{
		_thrustParicles.Emitting = false;
		_dustLeftParticles.Emitting = false;
		_dustRightParticles.Emitting = false;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StartThrust, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndThrust, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.StartThrust && args.Count == 0)
		{
			StartThrust();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndThrust && args.Count == 0)
		{
			EndThrust();
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
		if (method == MethodName.StartThrust)
		{
			return true;
		}
		if (method == MethodName.EndThrust)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._thrustParicles)
		{
			_thrustParicles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._dustLeftParticles)
		{
			_dustLeftParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._dustRightParticles)
		{
			_dustRightParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._thrustParicles)
		{
			value = VariantUtils.CreateFrom(in _thrustParicles);
			return true;
		}
		if (name == PropertyName._dustLeftParticles)
		{
			value = VariantUtils.CreateFrom(in _dustLeftParticles);
			return true;
		}
		if (name == PropertyName._dustRightParticles)
		{
			value = VariantUtils.CreateFrom(in _dustRightParticles);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._thrustParicles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dustLeftParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dustRightParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._thrustParicles, Variant.From(in _thrustParicles));
		info.AddProperty(PropertyName._dustLeftParticles, Variant.From(in _dustLeftParticles));
		info.AddProperty(PropertyName._dustRightParticles, Variant.From(in _dustRightParticles));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._thrustParicles, out var value))
		{
			_thrustParicles = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._dustLeftParticles, out var value2))
		{
			_dustLeftParticles = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._dustRightParticles, out var value3))
		{
			_dustRightParticles = value3.As<GpuParticles2D>();
		}
	}
}
